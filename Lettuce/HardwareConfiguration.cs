using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tomato.Hardware;
using System.Reflection;
using System.IO;

namespace Lettuce
{
    public partial class HardwareConfiguration : Form
    {
        private List<Device> possibleDevices;

        public List<Device> SelectedDevices
        {
            get
            {
                var devices = new List<Device>();
                foreach (var device in selectedListBox.Items)
                    devices.Add((Device)device);
                return devices;
            }
        }

        public HardwareConfiguration()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            possibleDevices = new List<Device>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes().Where(t => typeof(Device).IsAssignableFrom(t) && t.IsAbstract == false);
                foreach (var type in types)
                {
                    var device = (Device)Activator.CreateInstance(type);
                    possibleDevices.Add(device);
                }
            }
            foreach (var device in possibleDevices)
                availableListBox.Items.Add(device);
            // Load previous configuration
            if (!(saveDevicesCheckBox.Checked = LoadSavedDevices()))
            {
                selectedListBox.Items.Clear();
                foreach (var device in possibleDevices)
                {
                    if (device.SelectedByDefault)
                        selectedListBox.Items.Add(device);
                }
            }
            DialogResult = DialogResult.Cancel;
        }

        private bool LoadSavedDevices()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".lettuce", "devices.config");
            if (!File.Exists(configPath))
                return false;
            try
            {
                // File config is a simple format, just device IDs in hex delimited by spaces
                var text = File.ReadAllText(configPath);
                var parts = text.Split(' ');
                foreach (var part in parts)
                {
                    var device = possibleDevices.FirstOrDefault(d =>
                        d.DeviceID == int.Parse(part, NumberStyles.HexNumber));
                    if (device == null)
                        throw new Exception();
                    selectedListBox.Items.Add(device);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    ".lettuce", "devices.config");
            if (saveDevicesCheckBox.Checked)
            {
                var sb = new StringBuilder();
                foreach (var _device in selectedListBox.Items)
                {
                    var device = _device as Device;
                    sb.Append(device.DeviceID.ToString("X8") + " ");
                }
                if (!Directory.Exists(Path.GetDirectoryName(configPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                var text = sb.ToString();
                text = text.Remove(text.Length - 1);
                File.WriteAllText(configPath, text);
            }
            else
            {
                if (File.Exists(configPath))
                    File.Delete(configPath);
            }
        }

        private void availableListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            addDeviceButton.Enabled = availableListBox.SelectedIndex != -1;
        }

        private void addDeviceButton_Click(object sender, EventArgs e)
        {
            selectedListBox.Items.Add(Activator.CreateInstance(availableListBox.SelectedItem.GetType()));
        }

        private void selectedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            removeDeviceButton.Enabled = selectedListBox.SelectedIndex != -1;
            moveUpButton.Enabled = selectedListBox.SelectedIndex != 0 && selectedListBox.Items.Count > 1;
            moveDownButton.Enabled = selectedListBox.SelectedIndex != selectedListBox.Items.Count - 1;
        }

        private void removeDeviceButton_Click(object sender, EventArgs e)
        {
            int index = selectedListBox.SelectedIndex;
            selectedListBox.Items.RemoveAt(selectedListBox.SelectedIndex);
            if (index != selectedListBox.Items.Count)
                selectedListBox.SelectedIndex = index;
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            var device = selectedListBox.SelectedItem;
            int index = selectedListBox.SelectedIndex;
            selectedListBox.Items.RemoveAt(selectedListBox.SelectedIndex);
            selectedListBox.Items.Insert(index + 1, device);
            selectedListBox.SelectedIndex = index + 1;
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            var device = selectedListBox.SelectedItem;
            int index = selectedListBox.SelectedIndex;
            selectedListBox.Items.RemoveAt(selectedListBox.SelectedIndex);
            selectedListBox.Items.Insert(index - 1, device);
            selectedListBox.SelectedIndex = index - 1;
        }
    }
}
