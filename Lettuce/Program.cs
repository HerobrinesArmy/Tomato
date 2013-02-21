using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lettuce.Config;
using Tomato.Hardware;
using Tomato;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Globalization;
using System.Drawing;
using System.Security.Permissions;
using System.ComponentModel;
using System.Drawing.Design;

namespace Lettuce
{
    public static class Program
    {
        public static DCPU CPU;
        public static DateTime LastTick;
        public static Debugger debugger;
        
        public static string lastbinFilepath = "";
        public static string lastlistingFilepath = "";
        public static bool lastlittleEndian = false;
        public static Configuration Configuration;
        public static string ConfigFilePath;

        public static Dictionary<Device, Form> Windows = new Dictionary<Device, Form>();
        
        private static System.Threading.Timer timer;
        private static Point screenLocation = new Point();
        private static bool EnableAutomaticArrangement = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        [STAThread]
        static void Main(string[] args)
        {
            ConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ConfigFilePath = Path.Combine(ConfigFilePath, ".lettuce");
            Configuration = ConfigurationManager.LoadConfiguration(ConfigFilePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, true);
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            }

            // Enumerate loaded devices from plugins and Tomato
            List<Device> PossibleDevices = new List<Device>();
            GenericKeyboard kb = new GenericKeyboard();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes().Where(t => typeof(Device).IsAssignableFrom(t) && t.IsAbstract == false);
                foreach (var type in types)
                {
                    PossibleDevices.Add((Device)Activator.CreateInstance(type));
                }
            }

            CPU = new DCPU();
            string binFile = null;
            bool littleEndian = false, pairKeyboards = true;
            List<Device> devices = new List<Device>();
            CPU.IsRunning = false;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "--no-wait":
                        case "--nowait":
                            CPU.IsRunning = true;
                            break;
                        case "-c":
                        case "--connect":
                            if (i + 1 == args.Length || args[i + 1].StartsWith("-"))
                            {
                                Console.Error.WriteLine("Missing argument at --connect");
                                break;
                            }
                            bool gotWrongParam = false;
                            string deviceID = args[++i];
                            string[] ids = deviceID.Split(',');
                            foreach (var dID in ids)
                            {
                                uint id;
                                bool foundDevice = false;

                                if (uint.TryParse(dID, NumberStyles.HexNumber, null, out id))
                                {
                                    foreach (Device d in PossibleDevices)
                                    {
                                        if (d.DeviceID == id)
                                        {
                                            devices.Add((Device) Activator.CreateInstance(d.GetType()));
                                            foundDevice = true;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Device d in PossibleDevices)
                                    {
                                        if (d.GetType().Name.ToLower() == dID.ToLower())
                                        {
                                            devices.Add((Device)Activator.CreateInstance(d.GetType()));
                                            foundDevice = true;
                                        }
                                    }
                                }
                                if (!foundDevice)
                                {
                                    Console.Error.WriteLine("Device '" + dID + "' could not be found, continuing..");
                                    gotWrongParam = true;
                                }
                            }
                            if (gotWrongParam)
                                return;
                            break;
                        case "--skip-pairing":
                            pairKeyboards = false;
                            break;
                        case "--listing":
                            var file = args[++i];
                            if(!File.Exists(file))
                            {
                                Console.Error.WriteLine("Could not find listing-file: " + file);
                                return;
                            }
                            Debugger.LoadOrganicListing(file);
                            break;
                        case "--little-endian":
                            littleEndian = true;
                            break;
                        case "--list-devices":
                            Console.WriteLine("Got {0} devices:", PossibleDevices.Count);
                            foreach (var device in PossibleDevices)
                            {
                                Console.WriteLine("ID: 0x{0:X}, Name: {1}", device.DeviceID, device.GetType().Name);
                            }
                            return;
                        case "--disable-auto-arrange":
                            EnableAutomaticArrangement = false;
                            break;
                        case "--help":
                            Console.WriteLine("Lettuce - a graphical debugger for DCPU-16 programs");
                            Console.WriteLine("Options:");
                            Console.WriteLine("\t--no-wait             Starts debugging immediately.");
                            Console.WriteLine("\t--connect [Devices]   A comma-seperated list of devices to connect");
                            Console.WriteLine("\t                      For example: --connect 0x40E41D9D,M35FD");
                            Console.WriteLine("\t                      See also: --list-devices");
                            Console.WriteLine("\t--list-devices        Lists all available devices and exits.");
                            Console.WriteLine("\t--skip-pairing");
                            Console.WriteLine("\t--listing [File.lst]  Loads File.lst to make debugging easier.");
                            Console.WriteLine("\t--little-endian       Switches to little-endian mode.");
                            Console.WriteLine("\t--help                Outputs this and exits.");
                            return;
                    }
                }
                else
                {
                    if (binFile == null)
                        binFile = arg;
                    else
                        Debugger.LoadOrganicListing(args[i]);
                }
            }
            if (binFile == null)
            {
                var mc = new MemoryConfiguration();
                var result = mc.ShowDialog();
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.OK)
                {
                    binFile = mc.FileName;
                    littleEndian = mc.LittleEndian;
                }
            }
            if (devices.Count == 0)
            {
                var hwc = new HardwareConfiguration();
                var result = hwc.ShowDialog();
                if (result == DialogResult.Cancel)
                    return;
                foreach (var device in hwc.SelectedDevices)
                    devices.Add(device);
            }
            // Inject custom UITypeEditor into M35FD
            TypeDescriptor.AddAttributes(typeof(ushort[]),
                new EditorAttribute(typeof(M35FDTypeEditor), typeof(UITypeEditor)));

            if (!string.IsNullOrEmpty(binFile))
            {
                lastbinFilepath = binFile;
                // Load binary file
                List<ushort> data = new List<ushort>();
                using (Stream stream = File.OpenRead(binFile))
                {
                    for (int i = 0; i < stream.Length; i += 2)
                    {
                        byte a = (byte)stream.ReadByte();
                        byte b = (byte)stream.ReadByte();
                        if (littleEndian)
                            data.Add((ushort)(a | (b << 8)));
                        else
                            data.Add((ushort)(b | (a << 8)));
                    }
                }
                CPU.FlashMemory(data.ToArray());
            }
            else
                CPU.IsRunning = false;
            foreach (var device in devices)
                CPU.ConnectDevice(device);

            debugger = new Debugger(ref CPU);
            if (EnableAutomaticArrangement)
            {
                debugger.StartPosition = FormStartPosition.Manual;
                if (RuntimeInfo.IsMacOSX)
                    debugger.Location = new Point(0, 22);
                else
                    debugger.Location = new Point(0, 0);
            }
            debugger.ResetLayout();
            debugger.Show();
            
            screenLocation.Y = debugger.Location.Y + 4;
            screenLocation.X = debugger.Location.X + debugger.Width + 5;
            
            foreach (Device d in CPU.Devices)
            {
                if (d is LEM1802)
                    AddWindow(new LEM1802Window(d as LEM1802, CPU, pairKeyboards));
                else if (d is SPED3)
                    AddWindow(new SPED3Window(d as SPED3, CPU));
                else if (d is M35FD)
                    AddWindow(new M35FDWindow(d as M35FD, CPU));
            }
            
            // Run again for extra keyboards
            for (int i = 0; i < CPU.Devices.Count; i++)
            {
                if (CPU.Devices[i] is GenericKeyboard && !LEM1802Window.AssignedKeyboards.Contains(i))
                    AddWindow(new GenericKeyboardWindow(CPU.Devices[i] as GenericKeyboard, CPU));
            }
            
            debugger.Focus();
            LastTick = DateTime.Now;
            timer = new System.Threading.Timer(FetchExecute, null, 10, Timeout.Infinite);
            Application.Run(debugger);
            timer.Dispose();
        }
        
        static void AddWindow(DeviceHostForm window)
        {
            if (EnableAutomaticArrangement)
                window.StartPosition = FormStartPosition.Manual;
            if (screenLocation.Y + window.Height > Screen.PrimaryScreen.WorkingArea.Height) // Wrap excessive windows
            {
                screenLocation.Y = 25;
                screenLocation.X += 25;
            }
            if (window.OpenByDefault)
            {
                if (EnableAutomaticArrangement)
                    window.Location = screenLocation;
                screenLocation.Y += window.Height + 12;
                window.Show();
                window.Invalidate();
                window.Update();
                window.Focus();
            }
            foreach (var device in window.ManagedDevices)
                Windows.Add(device, window);
        }
        
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            CurrentDomain_UnhandledException(sender, new UnhandledExceptionEventArgs(e.Exception, false));
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledExceptionForm uef = new UnhandledExceptionForm((Exception)e.ExceptionObject, e.IsTerminating);
            uef.ShowDialog();
        }

        private static void FetchExecute(object o)
        {
            if (!CPU.IsRunning)
            {
                timer = new System.Threading.Timer(FetchExecute, null, 10, Timeout.Infinite);
                LastTick = DateTime.Now;
                return;
            }
            TimeSpan timeToEmulate = DateTime.Now - LastTick;
            LastTick = DateTime.Now;

            CPU.Execute((int)(timeToEmulate.TotalMilliseconds * (CPU.ClockSpeed / 1000)));
            debugger.ResetLayout();
            timer = new System.Threading.Timer(FetchExecute, null, 10, Timeout.Infinite);
        }
    }
}
