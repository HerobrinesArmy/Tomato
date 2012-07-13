﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tomato;
using Tomato.Hardware;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Lettuce
{
    public partial class LEM1802Window : DeviceHost
    {
        private Device[] managedDevices;
        public override Device[] ManagedDevices
        {
            get { return managedDevices; }
        }

        public static List<int> AssignedKeyboards = new List<int>();
        public LEM1802 Screen;
        public GenericKeyboard Keyboard;
        public DCPU CPU;
        protected int ScreenIndex, KeyboardIndex;
        
        protected virtual void InitClientSize()
        {
            this.ClientSize = new Size(LEM1802.Width * 4 + 20, LEM1802.Height * 4 + 35);
        }
        
        /// <summary>
        /// Assigns a LEM to the window.  If AssignKeyboard is true, it will search for a keyboard
        /// in the given CPU and assign it to this window as well.
        /// </summary>
        public LEM1802Window(LEM1802 LEM1802, DCPU CPU, bool AssignKeyboard) : base()
        {
            InitializeComponent();
            // Set up drawing
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint | ControlStyles.Opaque, true);
            
            // Take a screen
            this.CPU = CPU;
            Screen = LEM1802;
            managedDevices = new Device[] { Screen };
            ScreenIndex = CPU.Devices.IndexOf(Screen);
            
            Keyboard = null;
            
            // Take a keyboard
            if (AssignKeyboard)
            {
                for (int i = 0; i < CPU.Devices.Count; i++)
                {
                    if (AssignedKeyboards.Contains(i))
                        continue;
                    if (CPU.Devices[i] is GenericKeyboard)
                    {
                        Keyboard = CPU.Devices[i] as GenericKeyboard;
                        managedDevices = managedDevices.Concat(new Device[] { Keyboard }).ToArray();
                        AssignedKeyboards.Add(i);
                        KeyboardIndex = i;
                        this.detatchKeyboardToolStripMenuItem.Visible = true;
                        break;
                    }
                }
            }
            this.KeyDown += new KeyEventHandler(LEM1802Window_KeyDown);
            this.KeyUp += new KeyEventHandler(LEM1802Window_KeyUp);
            if (this.Keyboard == null)
            {
                this.DragEnter += new DragEventHandler(LEM1802Window_DragEnter);
                this.DragDrop += new DragEventHandler(LEM1802Window_DragDrop);
                this.AllowDrop = true;
            }
            timer = new System.Threading.Timer(delegate(object o)
                {
                    InvalidateAsync();
                }, null, 16, 16); // 60 Hz
            InitClientSize();
        }

        void LEM1802Window_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GenericKeyboard)))
            {
                GenericKeyboard data = (GenericKeyboard)e.Data.GetData(typeof(GenericKeyboard));
                if (Program.Windows.ContainsKey(data))
                {
                    Program.Windows[data].Close();
                    Program.Windows.Remove(data);
                    Program.Windows.Add(data, this);
                    this.Keyboard = data;
                    this.KeyboardIndex = this.CPU.Devices.IndexOf(data);
                    managedDevices = managedDevices.Concat(new Device[] { Keyboard }).ToArray();
                    AssignedKeyboards.Add(KeyboardIndex);
                    this.DragEnter -= LEM1802Window_DragEnter;
                    this.DragDrop -= LEM1802Window_DragDrop;
                    this.detatchKeyboardToolStripMenuItem.Visible = true;
                    this.AllowDrop = false;
                }
            }
        }

        void LEM1802Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GenericKeyboard)))
            {
                GenericKeyboard data = (GenericKeyboard)e.Data.GetData(typeof(GenericKeyboard));
                e.Effect = DragDropEffects.Move;
            }
        }

        System.Threading.Timer timer;

        void LEM1802Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard != null)
                Keyboard.KeyUp(e.KeyCode);
        }

        void LEM1802Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard != null)
                Keyboard.KeyDown(e.KeyCode);
        }

        private delegate void InvalidateAsyncDelegate();
        private void InvalidateAsync()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    InvalidateAsyncDelegate iad = new InvalidateAsyncDelegate(InvalidateAsync);
                    this.Invoke(iad);
                }
                catch { }
            }
            else
            {
                this.Invalidate();
                this.Update();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            string title = "LEM1802 #" + ScreenIndex;
            if (Keyboard != null)
                title += ", Generic Keyboard #" + KeyboardIndex;

            // Border
            e.Graphics.FillRectangle(new SolidBrush(Screen.BorderColor), new Rectangle(0, 0, this.Width, this.Height));

            // Title bar
            e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, this.Width, 15));
            // Devices
            e.Graphics.DrawString(title, this.Font, Brushes.Black, new PointF(0, 0));
            e.Graphics.DrawLine(Pens.Black, new Point(0, 15), new Point(this.Width, 15));

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            // Screen
            e.Graphics.DrawImage(Screen.ScreenImage, 10, 25, this.ClientSize.Width - 20, this.ClientSize.Height - 35);
        }

        private void detatchKeyboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.Windows.Remove(this.Keyboard);
            GenericKeyboardWindow gkw = new GenericKeyboardWindow(this.Keyboard, this.CPU);
            Program.Windows.Add(this.Keyboard, gkw);
            gkw.Show();
            managedDevices = managedDevices.Where(d => d != this.Keyboard).ToArray();
            this.Keyboard = null;
            AssignedKeyboards.Remove(this.KeyboardIndex);
            this.KeyboardIndex = -1;
            this.AllowDrop = true;
            this.DragEnter += LEM1802Window_DragEnter;
            this.DragDrop += LEM1802Window_DragDrop;
            this.detatchKeyboardToolStripMenuItem.Visible = false;
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = (Bitmap)Screen.ScreenImage.Clone();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Image (*.bmp)|*.bmp|All Files (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            image.Save(sfd.FileName);
        }
    }
}
