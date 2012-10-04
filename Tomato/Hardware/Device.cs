﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tomato.Hardware
{
    public abstract class Device
    {
        public abstract uint DeviceID { get; }
        public abstract uint ManufacturerID { get; }
        public abstract ushort Version { get; }
        public abstract string FriendlyName { get; }
        public DCPU AttachedCPU;

        public virtual bool SelectedByDefault
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the number of cycles required to perform the interrupt
        /// </summary>
        /// <returns></returns>
        public abstract int HandleInterrupt();
        public abstract void Reset();

        public bool BreakOnInterrupt { get; set; }
    }
}
