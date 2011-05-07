//

//

//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY

//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE

//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR

//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 

//  REMAINS UNCHANGED.

//

//  Email:  yetiicb@hotmail.com

//

//  Copyright (C) 2002-2003 Idael Cardoso. 

//


using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Propaganda.CDFunctions
{
    public class DataReadEventArgs : EventArgs
    {
        private readonly byte[] m_Data;
        private readonly uint m_DataSize;

        public DataReadEventArgs(byte[] data, uint size)
        {
            m_Data = data;
            m_DataSize = size;
        }

        public byte[] Data
        {
            get { return m_Data; }
        }

        public uint DataSize
        {
            get { return m_DataSize; }
        }
    }

    public class ReadProgressEventArgs : EventArgs
    {
        private readonly uint m_Bytes2Read;
        private readonly uint m_BytesRead;

        public ReadProgressEventArgs(uint bytes2read, uint bytesread)
        {
            m_Bytes2Read = bytes2read;
            m_BytesRead = bytesread;
        }

        public uint Bytes2Read
        {
            get { return m_Bytes2Read; }
        }

        public uint BytesRead
        {
            get { return m_BytesRead; }
        }

        public bool CancelRead { get; set; }
    }

    internal enum DeviceChangeEventType
    {
        DeviceInserted,
        DeviceRemoved
    } ;

    internal class DeviceChangeEventArgs : EventArgs
    {
        private readonly char m_Drive;
        private readonly DeviceChangeEventType m_Type;

        public DeviceChangeEventArgs(char drive, DeviceChangeEventType type)
        {
            m_Drive = drive;
            m_Type = type;
        }

        public char Drive
        {
            get { return m_Drive; }
        }

        public DeviceChangeEventType ChangeType
        {
            get { return m_Type; }
        }
    }

    public delegate void CdDataReadEventHandler(object sender, DataReadEventArgs ea);

    public delegate void CdReadProgressEventHandler(object sender, ReadProgressEventArgs ea);

    internal delegate void DeviceChangeEventHandler(object sender, DeviceChangeEventArgs ea);

    internal enum DeviceType : uint
    {
        DBT_DEVTYP_OEM = 0x00000000, // oem-defined device type
        DBT_DEVTYP_DEVNODE = 0x00000001, // devnode number
        DBT_DEVTYP_VOLUME = 0x00000002, // logical volume
        DBT_DEVTYP_PORT = 0x00000003, // serial, parallel
        DBT_DEVTYP_NET = 0x00000004 // network resource
    }

    internal enum VolumeChangeFlags : ushort
    {
        DBTF_MEDIA = 0x0001, // media comings and goings
        DBTF_NET = 0x0002 // network volume
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_HDR
    {
        public uint dbch_size;
        public DeviceType dbch_devicetype;
        private uint dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_VOLUME
    {
        public uint dbcv_size;
        public DeviceType dbcv_devicetype;
        private uint dbcv_reserved;
        private uint dbcv_unitmask;

        public char[] Drives
        {
            get
            {
                string drvs = "";
                for (char c = 'A'; c <= 'Z'; c++)
                {
                    if ((dbcv_unitmask & (1 << (c - 'A'))) != 0)
                    {
                        drvs += c;
                    }
                }
                return drvs.ToCharArray();
            }
        }

        public VolumeChangeFlags dbcv_flags;
    }

    internal class DeviceChangeNotificationWindow : NativeWindow
    {
        private const int DBT_APPYBEGIN = 0x0000;
        private const int DBT_APPYEND = 0x0001;
        private const int DBT_CONFIGCHANGECANCELED = 0x0019;
        private const int DBT_CONFIGCHANGED = 0x0018;
        private const int DBT_CONFIGMGAPI32 = 0x0022;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEQUERYREMOVE = 0x8001;
        private const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;
        private const int DBT_DEVICETYPESPECIFIC = 0x8005;
        private const int DBT_DEVNODES_CHANGED = 0x0007;
        private const int DBT_MONITORCHANGE = 0x001B;
        private const int DBT_QUERYCHANGECONFIG = 0x0017;
        private const int DBT_SHELLLOGGEDON = 0x0020;
        private const int DBT_VOLLOCKLOCKFAILED = 0x8043;
        private const int DBT_VOLLOCKLOCKRELEASED = 0x8045;
        private const int DBT_VOLLOCKLOCKTAKEN = 0x8042;
        private const int DBT_VOLLOCKQUERYLOCK = 0x8041;
        private const int DBT_VOLLOCKQUERYUNLOCK = 0x8044;
        private const int DBT_VOLLOCKUNLOCKFAILED = 0x8046;
        private const int DBT_VXDINITCOMPLETE = 0x0023;
        private const int WM_DEVICECHANGE = 0x0219;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_POPUP = unchecked((int) 0x80000000);

        public DeviceChangeNotificationWindow()
        {
            var Params = new CreateParams();
            Params.ExStyle = WS_EX_TOOLWINDOW;
            Params.Style = WS_POPUP;
            CreateHandle(Params);
        }

        public event DeviceChangeEventHandler DeviceChange;

        private void OnCDChange(DeviceChangeEventArgs ea)
        {
            if (DeviceChange != null)
            {
                DeviceChange(this, ea);
            }
        }

        private void OnDeviceChange(DEV_BROADCAST_VOLUME DevDesc, DeviceChangeEventType EventType)
        {
            if (DeviceChange != null)
            {
                foreach (char ch in DevDesc.Drives)
                {
                    var a = new DeviceChangeEventArgs(ch, EventType);
                    DeviceChange(this, a);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                DEV_BROADCAST_HDR head;
                switch (m.WParam.ToInt32())
                {
                        /*case DBT_DEVNODES_CHANGED :
            break;
          case DBT_CONFIGCHANGED :
            break;*/
                    case DBT_DEVICEARRIVAL:
                        head = (DEV_BROADCAST_HDR) Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_HDR));
                        if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
                        {
                            var DevDesc =
                                (DEV_BROADCAST_VOLUME) Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_VOLUME));
                            if (DevDesc.dbcv_flags == VolumeChangeFlags.DBTF_MEDIA)
                            {
                                OnDeviceChange(DevDesc, DeviceChangeEventType.DeviceInserted);
                            }
                        }
                        break;
                        /*case DBT_DEVICEQUERYREMOVE :
            break;
          case DBT_DEVICEQUERYREMOVEFAILED :
            break;
          case DBT_DEVICEREMOVEPENDING :
            break;*/
                    case DBT_DEVICEREMOVECOMPLETE:
                        head = (DEV_BROADCAST_HDR) Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_HDR));
                        if (head.dbch_devicetype == DeviceType.DBT_DEVTYP_VOLUME)
                        {
                            var DevDesc =
                                (DEV_BROADCAST_VOLUME) Marshal.PtrToStructure(m.LParam, typeof (DEV_BROADCAST_VOLUME));
                            if (DevDesc.dbcv_flags == VolumeChangeFlags.DBTF_MEDIA)
                            {
                                OnDeviceChange(DevDesc, DeviceChangeEventType.DeviceRemoved);
                            }
                        }
                        break;
                        /*case DBT_DEVICETYPESPECIFIC :
            break;*/
                }
            }
            base.WndProc(ref m);
        }
    }
}