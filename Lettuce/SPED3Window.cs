using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gif.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Tomato.Hardware;
using Tomato;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.Diagnostics;

namespace Lettuce
{
    public partial class SPED3Window : DeviceHostForm
    {
        public SPED3 SPED3;
        public DCPU CPU;
        private int brightestIndex = 0;

        private Device[] managedDevices;
        public override Device[] ManagedDevices
        {
            get { return managedDevices; }
        }

        System.Threading.Timer timer;
        public SPED3Window(SPED3 SPED3, DCPU CPU)
        {
            InitializeComponent();
            Text = "SPED-3 Display #" + CPU.Devices.IndexOf(SPED3);
            managedDevices = new Device[] { SPED3 };
            this.CPU = CPU;
            this.SPED3 = SPED3;
        }

        private delegate void InvalidateAsyncDelegate(object discarded);
        private void InvalidateAsync(object discarded)
        {
            if (InvokeRequired)
            {
                try
                {
                    InvalidateAsyncDelegate iad = InvalidateAsync;
                    Invoke(iad, new object());
                }
                catch { }
            }
            else
            {
                Invalidate(true);
                Update();
                timer.Change(16, System.Threading.Timeout.Infinite); // 60 Hz
            }
        }

        private delegate void ResetTitleAsyncDelegate();
        private void ResetTitleAsync()
        {
            if (InvokeRequired)
            {
                try
                {
                    ResetTitleAsyncDelegate iad = ResetTitleAsync;
                    Invoke(iad);
                }
                catch { }
            }
            else
            {
                Text = "SPED-3 Display #" + CPU.Devices.IndexOf(SPED3);
            }
        }

        bool glControlLoaded = false;

        private void glControl1_Load(object sender, EventArgs e)
        {
            glControlLoaded = true;

            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            GL.ClearColor(Color4.Black);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (!glControlLoaded)
                return;
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            GL.ClearColor(Color4.Black);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        Random random = new Random();
        Stopwatch stopwatch = new Stopwatch();
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!glControlLoaded)
                return;
            stopwatch.Restart();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(new Vector3(0, -4, 0), Vector3.Zero, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            GL.Rotate(SPED3.CurrentRotation, Vector3.UnitZ);
            // Flicker
            double flicker = random.NextDouble() * (SPED3.TotalVerticies / 4096f);
            if (SPED3.EnableFlickering)
                GL.Translate(flicker, flicker, flicker);

            if (SPED3.TotalVerticies > 0)
            {
                GL.Begin(BeginMode.LineStrip);
                var verticies = SPED3.Verticies;
                Vector3 position;
                float flickerIntensity = 0.007f;

                float initialIntensity = 1 - (SPED3.TotalVerticies * flickerIntensity);
                Vector4 alpha = new Vector4(initialIntensity, initialIntensity, initialIntensity, initialIntensity);

                for (int i = brightestIndex; i < SPED3.TotalVerticies + brightestIndex; i++)
                {
                    if (SPED3.EnableFlickering)
                        GL.Color4(Vector4.Multiply(GetColor(verticies[i % SPED3.TotalVerticies]), alpha));
                    else
                        GL.Color4(GetColor(verticies[i % SPED3.TotalVerticies]));
                    alpha = new Vector4(alpha.X + flickerIntensity, alpha.Y + flickerIntensity,
                        alpha.Z + flickerIntensity, alpha.W + flickerIntensity);
                    position = new Vector3((float)(verticies[i % SPED3.TotalVerticies].X) / 256 * 2 - 1,
                        (float)(verticies[i % SPED3.TotalVerticies].Y) / 256 * 2 - 1,
                        (float)(verticies[i % SPED3.TotalVerticies].Z) / 256 * 2 - 1);
                    GL.Vertex3(position);
                }

                GL.End();

                if (SPED3.EnableFlickering)
                {
                    GL.Begin(BeginMode.Quads);
                    position = new Vector3((float)(verticies[brightestIndex].X) / 256 * 2 - 1,
                                           (float)(verticies[brightestIndex].Y) / 256 * 2 - 1,
                                           (float)(verticies[brightestIndex].Z) / 256 * 2 - 1);
                    GL.Color4(GetColor(verticies[brightestIndex]));
                    foreach (var point in cubeVerticies)
                        GL.Vertex3(point + position);
                    GL.End();
                }
                brightestIndex += (int)(3 * SPED3.FlickerMultipler);
                if (brightestIndex >= SPED3.TotalVerticies)
                    brightestIndex = 0;
            }

            if (CPU.IsRunning && gifEncoder != null && !gifEncoder.Finished)
                gifEncoder.AddFrame(GrabScreenshot());

            glControl1.SwapBuffers();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds < 16)
            {
                timer = new System.Threading.Timer(InvalidateAsync, null, 16 - stopwatch.ElapsedMilliseconds,
                    System.Threading.Timeout.Infinite); // ~60 Hz
            }
            else
            {
                timer = new System.Threading.Timer(InvalidateAsync, null, 16,
                    System.Threading.Timeout.Infinite); // ~60 Hz
            }
        }

        private Vector4 GetColor(SPED3Vertex vertex)
        {
            Vector4 color = new Vector4(0, 0, 0, 1);
            if (vertex.Color == SPED3Color.Black)
                color = new Vector4(0.25f, 0.25f, 0.25f, 1);
            else if (vertex.Color == SPED3Color.Green)
                color = new Vector4(0, 1, 0, 1);
            else if (vertex.Color == SPED3Color.Red)
                color = new Vector4(1, 0, 0, 1);
            else if (vertex.Color == SPED3Color.Blue)
                color = new Vector4(0, 0, 1, 1);
            if (vertex.Intensity == SPED3Intensity.Dim)
                color = Vector4.Multiply(color, new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            return color;
        }

        static SPED3Window()
        {
            for (int i = 0; i < cubeVerticies.Length; i++)
                cubeVerticies[i] = Vector3.Multiply(new Vector3(0.25f, 0.25f, 0.25f), cubeVerticies[i]);
        }

        private static Vector3[] cubeVerticies = new Vector3[]
            {
                // near
                new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-0.1f, 0.1f, 0.1f), new Vector3(-0.1f, 0.1f, -0.1f), new Vector3(0.1f, 0.1f, -0.1f), 
                // left
                new Vector3(-0.1f, 0.1f, 0.1f), new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(-0.1f, -0.1f, -0.1f), new Vector3(-0.1f, 0.1f, 0.1f),
                // far
                new Vector3(0.1f, -0.1f, 0.1f), new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(-0.1f, -0.1f, -0.1f), new Vector3(0.1f, -0.1f, -0.1f), 
                // right
                new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.1f, -0.1f, 0.1f), new Vector3(0.1f, -0.1f, -0.1f), new Vector3(0.1f, 0.1f, 0.1f),
                // top
                new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.1f, -0.1f, 0.1f), new Vector3(-0.1f, -0.1f, 0.1f), new Vector3(-0.1f, 0.1f, 0.1f),
                // bottom
                new Vector3(0.1f, 0.1f, -0.1f), new Vector3(0.1f, -0.1f, -0.1f), new Vector3(-0.1f, -0.1f, -0.1f), new Vector3(-0.1f, 0.1f, -0.1f)
            };

        private static Bitmap tempBmp;

        public Bitmap GrabScreenshot()
        {
            if (tempBmp == null)
                tempBmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            BitmapData data = tempBmp.LockBits(ClientRectangle, ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            tempBmp.UnlockBits(data);

            tempBmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return tempBmp;
        }

        private void takeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var image = GrabScreenshot();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Image (*.bmp)|*.bmp";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            image.Save(sfd.FileName);
        }

        private DeferredGifEncoder gifEncoder;
        private void recordGIFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (recordGIFToolStripMenuItem.Text != "Start Recording")
            {
                // End recording
                recordGIFToolStripMenuItem.Text = "Start Recording";
                gifEncoder.FinishAsync(ResetTitleAsync);
                Text += " (Encoding...)";
            }
            else
            {
                // Begin recording an animated gif
                var sfd = new SaveFileDialog();
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                sfd.Filter = "Animated Gif Image (*.gif)|*.gif";
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
                recordGIFToolStripMenuItem.Text = "Stop Recording";
                var _gifEncoder = new AnimatedGifEncoder();
                if (File.Exists(sfd.FileName))
                    File.Delete(sfd.FileName);
                gifEncoder = new DeferredGifEncoder(_gifEncoder, sfd.FileName);
            }
        }
    }
}
