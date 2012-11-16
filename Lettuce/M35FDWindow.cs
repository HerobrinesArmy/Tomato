using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tomato.Hardware;
using Tomato;
using System.IO;

namespace Lettuce
{
    public partial class M35FDWindow : DeviceHostForm
    {
        static M35FDWindow()
        {
            DiskImages = new List<DiskImage>();
        }

        public override Device[] ManagedDevices { get { return new Device[] { M35FD }; } }

        public override bool OpenByDefault { get { return false; } }

        protected static List<DiskImage> DiskImages { get; set; }
        protected static event EventHandler ReloadImages;

        private bool disableReverseEndianness = false;

        public M35FD M35FD { get; set; }
        public DCPU CPU { get; set; }

        public M35FDWindow(M35FD M35FD, DCPU CPU)
        {
            InitializeComponent();
            this.M35FD = M35FD;
            this.CPU = CPU;
            this.Text = "M35FD #" + CPU.Devices.IndexOf(M35FD);
            ReloadImageList();
            ReloadImages += (s, e) => 
            {
                if (InvokeRequired)
                    this.Invoke(new Action(ReloadImageList));
                else
                    ReloadImageList();
            };
            if (M35FD.Disk != null)
            {
                imageNameLabel.Text = "[Unknown disk]";
                ejectButton.Enabled = true;
                insertDiskButton.Enabled = false;
            }
            else
            {
                imageNameLabel.Text = "[None loaded]";
                ejectButton.Enabled = false;
                insertDiskButton.Enabled = listBox1.SelectedIndex != -1;
            }
        }

        private void ReloadImageList()
        {
            int index = listBox1.SelectedIndex;
            listBox1.Items.Clear();
            foreach (var image in DiskImages)
                listBox1.Items.Add(Path.GetFileName(image.Name));
            if (index < listBox1.Items.Count)
                listBox1.SelectedIndex = index;
        }

        public static void AddImage(ushort[] data, string name)
        {
            DiskImages.Add(new DiskImage
            {
                Data = data,
                Name = name,
                LittleEndian = true,
                Writable = true,
                IsBlank = false
            });
            if (ReloadImages != null)
                ReloadImages(null, null);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            DiskImages.RemoveAt(listBox1.SelectedIndex);
            if (ReloadImages != null)
                ReloadImages(this, null);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Disk Images (*.img)|*.img|All Files (*.*)|*.*";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            using (Stream stream = File.Open(ofd.FileName, FileMode.Open))
            {
                ushort[] data = new ushort[737280];
                byte[] temp = new byte[2];
                for (int i = 0; i < Math.Min((stream.Length/2), data.Length); i++)
                {
                    stream.Read(temp, 0, 2);
                    data[i] = BitConverter.ToUInt16(temp, 0);
                }
                for (int i = ((int)stream.Length / 2) + 1; i < data.Length; i++)
                {
                    data[i] = 0;
                }
                AddImage(data, Path.GetFileName(ofd.FileName));
            }
        }

        private void ejectButton_Click(object sender, EventArgs e)
        {
            M35FD.Eject();
            if (M35FD.Disk != null)
            {
                imageNameLabel.Text = "[Unknown disk]";
                ejectButton.Enabled = true;
                insertDiskButton.Enabled = false;
            }
            else
            {
                imageNameLabel.Text = "[None loaded]";
                ejectButton.Enabled = false;
                insertDiskButton.Enabled = listBox1.SelectedIndex != -1;
            }
        }

        private void insertDiskButton_Click(object sender, EventArgs e)
        {
            var image = DiskImages[listBox1.SelectedIndex];
            M35FD.InsertDisk(ref image.Data, image.Writable);
            if (M35FD.Disk != null)
            {
                imageNameLabel.Text = image.Name;
                ejectButton.Enabled = true;
                insertDiskButton.Enabled = false;
            }
            else
            {
                imageNameLabel.Text = "[None loaded]";
                ejectButton.Enabled = false;
                insertDiskButton.Enabled = listBox1.SelectedIndex != -1;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            disableReverseEndianness = true;
            if (listBox1.SelectedIndex != -1)
            {
                removeButton.Enabled = writeProtectedCheckBox.Enabled = bigEndianCheckBox.Enabled = saveSelectedDiskButton.Enabled = true;
                if (M35FD.Disk == null)
                    insertDiskButton.Enabled = true;
                writeProtectedCheckBox.Checked = !DiskImages[listBox1.SelectedIndex].Writable;
                bigEndianCheckBox.Checked = !DiskImages[listBox1.SelectedIndex].LittleEndian;
            }
            else
                removeButton.Enabled = insertDiskButton.Enabled = writeProtectedCheckBox.Enabled = bigEndianCheckBox.Enabled = saveSelectedDiskButton.Enabled = false;
            disableReverseEndianness = false;
        }

        private void addBlankDisk_Click(object sender, EventArgs e)
        {
            DiskImages.Add(new DiskImage()
            {
                Name = "Blank Disk",
                Data = new ushort[737280],
                IsBlank = true,
                Writable = true,
                LittleEndian = true
            });
            if (ReloadImages != null)
                ReloadImages(this, null);
        }

        private void writeProtectedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DiskImages[listBox1.SelectedIndex].Writable = !writeProtectedCheckBox.Checked;
        }

        private void bigEndianCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (disableReverseEndianness)
                return;
            var image = DiskImages[listBox1.SelectedIndex];
            image.LittleEndian = !bigEndianCheckBox.Checked;
            for (int i = 0; i < image.Data.Length; i++)
                image.Data[i] = BitConverter.ToUInt16(BitConverter.GetBytes(image.Data[i]).Reverse().ToArray(), 0);
        }

        private void saveSelectedDiskButton_Click(object sender, EventArgs e)
        {
            var disk = DiskImages[listBox1.SelectedIndex];

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Disk Images (*.img)|*.img|All Files (*.*)|*.*";
            sfd.FileName = disk.Name;
            if (disk.IsBlank)
                sfd.FileName += ".img";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            using (Stream stream = File.Create(sfd.FileName))
            {
                for (int i = 0; i < disk.Data.Length; i++)
                {
                    if (disk.LittleEndian)
                        stream.Write(BitConverter.GetBytes(disk.Data[i]), 0, 2);
                    else
                        stream.Write(BitConverter.GetBytes(disk.Data[i]).Reverse().ToArray(), 0, 2);
                }
            }
        }

        protected class DiskImage
        {
            public ushort[] Data;
            public string Name { get; set; }
            public bool LittleEndian { get; set; }
            public bool Writable { get; set; }
            public bool IsBlank { get; set; }
        }
    }
}
