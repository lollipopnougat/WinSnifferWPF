using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using WinSnifferWPF.Model;
//using System.Diagnostics;

namespace WinSnifferWPF.CapUtils
{
    /// <summary>
    /// 数据包捕获管理器
    /// </summary>
    class CaptureManager
    {
        public CaptureManager(LibPcapLiveDevice device)
        {
            this.device = device;
        }

        /// <summary>
        /// 当前使用的设备
        /// </summary>
        private LibPcapLiveDevice device;

        private int count = 0;
        private bool _isOpened;

        /// <summary>
        /// 是否已经开始捕获
        /// </summary>
        public bool Opened => _isOpened;

        /// <summary>
        /// 当前使用的设备
        /// </summary>
        public LibPcapLiveDevice Device => this.device;

        /// <summary>
        /// 添加数据包的事件委托
        /// </summary>
        /// <param name="sender">引发此事件的源对象</param>
        /// <param name="e">添加数据包事件参数</param>
        public delegate void PacketAddHandle(object sender, PacketAddEventArgs e);

        /// <summary>
        /// 添加数据包事件
        /// </summary>
        public event PacketAddHandle PacketAddEvent;


        /// <summary>
        /// 开始捕获数据包
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool StartCapture()
        {
            if(Opened)
            {
                return false;
            }
            _isOpened = true;
            RemoveOnPacketArrival(OnRecvPacket);
            AddOnPacketArrival(OnRecvPacket);
            device.Open(DeviceModes.Promiscuous, 1000);
            device.StartCapture();
            //Debug.WriteLine("st");
            return true;
        }

        /// <summary>
        /// 停止捕获数据包
        /// </summary>
        /// <returns>是否成功停止</returns>
        public bool StopCapture()
        {
            if (!Opened)
            {
                return false;
            }
            device.StopCapture();
            device.Close();
            //Debug.WriteLine("end");
            _isOpened = false;
            return true;
        }

        /// <summary>
        /// 清空记录
        /// </summary>
        /// <returns>是否清除成功</returns>
        public bool Clear()
        {
            if(Opened)
            {
                return false;
            }
            count = 0;
            return true;
        }

        /// <summary>
        /// 获取当前的网卡易于阅读的名称
        /// </summary>
        /// <returns>当前的网卡易于阅读的名称(如果是空则返回默认名称)</returns>
        public string GetCurrentAdapterName()
        {
            if(string.IsNullOrEmpty(device.Interface.FriendlyName))
            {
                return device.Interface.Name;
            }
            return device.Interface.FriendlyName;
        }

        /// <summary>
        /// 切换设备
        /// </summary>
        /// <param name="device">要切换的设备</param>
        /// <returns>是否切换成功</returns>
        public bool ChangeDevice(LibPcapLiveDevice device)
        {
            if (this.device.Opened)
            {
                return false;
            }
            this.device = device;
            return true;
        }

        /// <summary>
        /// 捕获到数据包事件处理方法
        /// </summary>
        /// <param name="sender">触发此事件的对象</param>
        /// <param name="packetc">数据包</param>
        private void OnRecvPacket(object sender, PacketCapture packetc)
        {
            var rawPacket = packetc.GetPacket();
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data); //解析出基本包
            var item = PacketConvert.ParsePacket(packet, count, packetc.Header.Timeval.Date);
            RaisePacketAddEvent(item);
            count++;
        }

        
        /// <summary>
        /// 触发数据包添加事件的方法
        /// </summary>
        /// <param name="packet">要添加的数据包</param>
        public void RaisePacketAddEvent(PacketItem packet)
        {
            if (PacketAddEvent == null) return;
            PacketAddEvent(this, new PacketAddEventArgs(packet));
        }

        /// <summary>
        /// 添加数据包添加事件处理方法
        /// </summary>
        /// <param name="handler">数据包添加事件的处理方法委托</param>
        public void AddOnPacketArrival(PacketArrivalEventHandler handler)
        {
            device.OnPacketArrival += handler;

        }

        /// <summary>
        /// 移除数据包添加事件处理方法
        /// </summary>
        /// <param name="handler">要移除的数据包添加事件处理方法委托</param>
        public void RemoveOnPacketArrival(PacketArrivalEventHandler handler)
        {
            device.OnPacketArrival -= handler;
        }

    }
}
