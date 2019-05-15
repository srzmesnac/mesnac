using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPLC
{
    public class LibnodavePLC
    {
        libnodave.daveOSserialType fds;
        libnodave.daveInterface di;
        internal libnodave.daveConnection dc;
        int _rack;
        int _slot;
        string _IP;
        object _async = new object();
        DateTime _closeTime = DateTime.Now;

        bool _closed = true;
        public bool IsClosed
        {
            get
            {
                return _closed;
            }
        }

        int _timeOut = 1000;
        public int TimeOut
        {
            get
            {
                return _timeOut;
            }
            set
            {
                _timeOut = value;
            }
        }

        /// <summary>
        /// libnodave中connectPLC方法的封装 srz
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            lock (_async)
            {
                if (!_closed) return true;
                double sec = (DateTime.Now - _closeTime).TotalMilliseconds;
                if (sec < 6000)
                    System.Threading.Thread.Sleep(6000 - (int)sec);
                fds.rfd = libnodave.openSocket(102, _IP);
                fds.wfd = fds.rfd;
                if (fds.rfd > 0)
                {
                    di = new libnodave.daveInterface(fds, "IF1", 0, libnodave.daveProtoISOTCP, libnodave.daveSpeed187k);
                    di.setTimeout(TimeOut);
                    //	    res=di.initAdapter();	// does nothing in ISO_TCP. But call it to keep your programs indpendent of protocols
                    //	    if(res==0) {
                    dc = new libnodave.daveConnection(di, 0, _rack, _slot);
                    if (0 == dc.connectPLC())
                    {
                        _closed = false;
                        return true;
                    }
                }
                if (dc != null) dc.disconnectPLC();
                libnodave.closeSocket(fds.rfd);
            }
            _closed = true;
            return false;
        }

        public bool Init(string ADD, string IP)
        {
            _IP = IP;
            Device.Area = libnodave.daveDB;
            Device.DBNumber = ushort.Parse(ADD.Substring(2, 1));
            Device.Start = 0;
            return Connect();
        }
        public bool Init(string IP)
        {
            _IP = IP;
            Device.Area = libnodave.daveDB;
            Device.DBNumber = 2;
            Device.Start = 0;
            return Connect();
        }

        public DeviceAddress Device;

        public byte[] ReadBytes( ushort len)//从PLC中读取自己数组
        {
            try
            {
                if (dc != null)
                {
                    byte[] buffer = new byte[len];
                    int res = -1;
                    lock (_async)
                    {
                        res = dc == null ? -1 : dc.readBytes(Device.Area, Device.DBNumber, Device.Start, len, buffer);
                    }
                    if (res == 0)
                        return buffer;
                    //_closed = true; dc = null; _closeTime = DateTime.Now;
                    //if (OnError != null)
                    //{
                    //    OnError(this, new IOErrorEventArgs(daveStrerror(res))); _closeTime = DateTime.Now;
                    //}
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }

        public bool ReadBit()
        {
            if (dc != null)
            {
                dc.readBits(Device.Area, Device.DBNumber, Device.Start * 8 + Device.Bit, 1, null);
                return dc.getS8() != 0;
            }
            return false;
        }
        public short ReadInt16()
        {
            if (dc != null)
            {
                dc.readBytes(Device.Area, Device.DBNumber, Device.Start, 2, null);
                return (short)dc.getU16();
            }
            return 0;
        }
        public ushort ReadUInt16()
        {
            if (dc != null)
            {
                dc.readBytes(Device.Area, Device.DBNumber, Device.Start, 2, null);
                return (ushort)dc.getU16();
            }
            return 0;
        }
        public int ReadInt32()
        {
            if (dc != null)
            {
                dc.readBytes(Device.Area, Device.DBNumber, Device.Start, 4, null);
                return (int)dc.getU32();
            }
            return 0;
        }
        public uint ReadUInt32()
        {
            if (dc != null)
            {
                dc.readBytes(Device.Area, Device.DBNumber, Device.Start, 4, null);
                return (uint)dc.getU32();
            }
            return 0;
        }
        public float ReadFloat()
        {
            if (dc != null)
            {
                dc.readBytes(Device.Area, Device.DBNumber, Device.Start, 4, null);
                return dc.getFloat();

            }
            return 0;
        }

        public int WriteBytes( byte[] bit)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, bit.Length, bit);
            }
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBits(address.Area, address.DBNumber, address.Start * 8 + address.Bit, 1, bit ? new byte[] { 0x1 } : new byte[] { 0x00 });
            }
        }

        public int WriteBits(byte value)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBits(Device.Area, Device.DBNumber, Device.Start, 1, new byte[] { value });
            }
        }

        public int WriteInt16(short value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, 2, b);
            }
        }

        public int WriteUInt16( ushort value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, 2, b);
            }
        }

        public int WriteUInt32( uint value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, 4, b);
            }
        }

        public int WriteInt32( int value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, 4, b);
            }
        }

        public int WriteFloat( float value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(Device.Area, Device.DBNumber, Device.Start, 4, b);
            }
        }

        public int WriteString(DeviceAddress address, string str)
        {
            lock (_async)
            {
                if (str.Length > address.DataSize)
                    str.Remove(address.DataSize - 1);
                var textArr = Encoding.ASCII.GetBytes(str);
                var buffer = new byte[2 + textArr.Length];
                buffer[0] = (byte)address.DataSize;
                buffer[1] = (byte)textArr.Length;
                textArr.CopyTo(buffer, 2);
                return dc == null ? -1 : dc.writeManyBytes(address.Area, address.DBNumber, address.Start,
                    buffer.Length, buffer); //1200前置空格
            }
        }

        //public int WriteValue(DeviceAddress address, object value)
        //{
        //    return this.WriteValueEx(address, value);
        //}

        public struct DeviceAddress
        {
            /// <summary>
            /// 区域号
            /// </summary>
            public int Area;
            /// <summary>
            /// 起始位置
            /// </summary>
            public int Start;
            /// <summary>
            /// 区块号
            /// </summary>
            public ushort DBNumber;
            /// <summary>
            /// 数据长度
            /// </summary>
            public ushort DataSize;
            /// <summary>
            /// 
            /// </summary>
            public ushort CacheIndex;
            /// <summary>
            /// 位号
            /// </summary>
            public byte Bit;
            ///// <summary>
            ///// 数据类型
            ///// </summary>
            //public DataType VarType;
            //public ByteOrder ByteOrder;

        }

        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress plcAddr = new DeviceAddress();
            if (string.IsNullOrEmpty(address) || address.Length < 2) return plcAddr;
            if (address.Substring(0, 2) == "DB")
            {
                int index = 2;
                for (int i = index; i < address.Length; i++)
                {
                    if (!char.IsDigit(address[i]))
                    {
                        index = i; break;
                    }
                }
                plcAddr.Area = libnodave.daveDB;
                plcAddr.DBNumber = ushort.Parse(address.Substring(2, index - 2));
                string str = address.Substring(index + 1);
                if (!char.IsDigit(str[0]))
                {
                    for (int i = 1; i < str.Length; i++)
                    {
                        if (char.IsDigit(str[i]))
                        {
                            index = i; break;
                        }
                    }
                    if (str[2] == 'W')
                    {
                        int index1 = str.IndexOf('.');
                        if (index1 > 0)
                        {
                            int start = int.Parse(str.Substring(3, index1 - 3));
                            byte bit = byte.Parse(RightFrom(str,index1));
                            plcAddr.Start = bit > 8 ? start : start + 1;
                            plcAddr.Bit = (byte)(bit > 7 ? bit - 8 : bit);
                            return plcAddr;
                        }
                    }
                    str = str.Substring(index);
                }
                index = str.IndexOf('.');
                if (index < 0)
                    plcAddr.Start = int.Parse(str);
                else
                {
                    plcAddr.Start = int.Parse(str.Substring(0, index));
                    plcAddr.Bit = byte.Parse(RightFrom(str,index));
                }
            }
            return plcAddr;
        }

        /// <summary>
        /// string RightFrom
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public  string RightFrom( string text, int index)
        {
            return text.Substring(index + 1, text.Length - index - 1);
        }
    }
}
