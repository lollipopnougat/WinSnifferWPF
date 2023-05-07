using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WinSnifferWPF.Command;
using WinSnifferWPF.Model;
using WinSnifferWPF.CapUtils;
using System.Windows.Threading;
using System.Windows.Data;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace WinSnifferWPF.ViewModel
{
    class MainWindowViewModel : NotificationObject
    {

        #region 字段区
        /// <summary>
        /// ui线程的Dispatcher
        /// </summary>
        private Dispatcher uiThreadDispatcher;

        /// <summary>
        /// 是否已经启动
        /// </summary>
        private bool _isStart;

        /// <summary>
        /// 状态栏文本存储值
        /// </summary>
        private string statusText = "完毕";

        /// <summary>
        /// 数据包列表
        /// </summary>
        private ObservableCollection<PacketItem> packetsList;

        /// <summary>
        /// 数据包列表视图
        /// </summary>
        private CollectionView packetsListView;

        /// <summary>
        /// 设备列表
        /// </summary>
        private List<string> deviceList;

        /// <summary>
        /// 选中设备索引
        /// </summary>
        private int selectedDeviceIndex = 0;

        /// <summary>
        /// 选中的数据包
        /// </summary>
        private PacketItem selectedItem;

        /// <summary>
        /// 未选择数据包时TextBox的默认显示文本
        /// </summary>
        private readonly string packetData = "还没有东西哦";

        /// <summary>
        /// 筛选器TextBox原始值
        /// </summary>
        private string filter = "ptl=*";

        /// <summary>
        /// 协议正则
        /// </summary>
        private readonly Regex ptlRegex = new Regex(@"^ptl=((tcp)|(udp)|(icmp4)|(icmp6)|(icmp)|(arp)|(\*))$");

        /// <summary>
        /// 源地址正则
        /// </summary>
        private readonly Regex srcRegex = new Regex(@"^src=([^=]+)$");

        /// <summary>
        /// 目的地址正则
        /// </summary>
        private readonly Regex dstRegex = new Regex(@"^dst=([^=]+)$");

        /// <summary>
        /// 过滤谓词表达式
        /// </summary>
        private readonly Predicate<object> filterPredicate;

        #endregion

        #region 视图绑定用属性


        /// <summary>
        /// 是否已经开始捕获数据包(使能界面的某些按钮)
        /// </summary>
        public bool Started
        {
            get { return _isStart; }
            set
            {
                _isStart = value;
                RaisePropertyChange(nameof(Started));
                RaisePropertyChange(nameof(Stopped));
            }
        }

        /// <summary>
        /// 是否停止了数据包的捕获(使能界面的某些按钮)
        /// </summary>
        public bool Stopped => !_isStart;

        /// <summary>
        /// 状态栏文本(绑定TextBlock)
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                RaisePropertyChange(nameof(StatusText));
            }
        }
        
        /// <summary>
        /// 数据包列表的视图(界面ListView显示用)
        /// </summary>
        public CollectionView PacketListView
        {
            get 
            { 
                if(packetsListView == null)
                {
                    packetsListView = CollectionViewSource.GetDefaultView(PacketList) as CollectionView;
                    packetsListView.Filter = filterPredicate;
                }
                return packetsListView; 
            }
            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// 数据包详细信息(用于界面TextBox显示)
        /// </summary>
        public string PacketData
        {
            get 
            { 
                if(SelectedPacketItem == null)
                {
                    return packetData; 
                }
                return SelectedPacketItem.ToString();
            }
            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// 筛选器输入文本(绑定TextBox)
        /// </summary>
        public string FilterText
        {
            get { return filter; }
            set
            {

                filter = value;
                RaisePropertyChange(nameof(FilterText));
            }
        }



        /// <summary>
        /// 设备列表(绑定ComboBox)
        /// </summary>
        public List<string> DeviceList
        {
            get { return deviceList; }
            set
            {
                deviceList = value;
                RaisePropertyChange(nameof(DeviceList));
            }
        }

        /// <summary>
        /// 当前选中的设备(绑定ComboBox)
        /// </summary>
        public string SelectedDevice
        {
            get { return DeviceList[selectedDeviceIndex]; }
            set
            {
                SelectedDeviceIndex = DeviceList.IndexOf(value);
                RaisePropertyChange(nameof(SelectedDevice));
            }
        }

        /// <summary>
        /// 当前选中的数据包(绑定ListView)
        /// </summary>
        public PacketItem SelectedPacketItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                RaisePropertyChange(nameof(SelectedPacketItem));
                RaisePropertyChange(nameof(PacketData));
            }
        }
        #endregion

        #region 普通属性
        /// <summary>
        /// 所有的数据包列表
        /// </summary>
        public ObservableCollection<PacketItem> PacketList
        {
            get { return packetsList; }
            set
            {
                packetsList = value;
                RaisePropertyChange(nameof(PacketList));
                RaisePropertyChange(nameof(PacketListView));
            }
        }

        /// <summary>
        /// 筛选器筛选具体值
        /// </summary>
        public string Filter
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterText) && FilterText.Contains("="))
                {
                    return FilterText.Split('=')[1];
                }
                return string.Empty;
            }
        }


        /// <summary>
        /// 当前选中的设备在列表中的索引
        /// </summary>
        public int SelectedDeviceIndex
        {
            get { return selectedDeviceIndex; }
            set
            {
                selectedDeviceIndex = value;//DeviceList[value];
                RaisePropertyChange(nameof(SelectedDeviceIndex));
            }
        }
        #endregion

        #region 绑定的Command

        /// <summary>
        /// 网卡ComboBox选中元素改变事件Command
        /// </summary>
        public DelegateCommand AdapterComboBoxSelectionChangedCommand { get; set; }
        
        /// <summary>
        /// 开始捕获数据包事件Command
        /// </summary>
        public DelegateCommand StartCaptureBtnCommand { get; set; }

        /// <summary>
        /// 停止捕获数据包事件Command
        /// </summary>
        public DelegateCommand StopCaptureBtnCommand { get; set; }

        /// <summary>
        /// 清除捕获数据包事件Command
        /// </summary>
        public DelegateCommand ClearCaptureBtnCommand { get; set; }

        /// <summary>
        /// 保存捕获数据包事件Command
        /// </summary>
        public DelegateCommand SaveCaptureBtnCommand { get; set; }

        /// <summary>
        /// 导入捕获数据包事件Command
        /// </summary>
        public DelegateCommand ImportCaptureBtnCommand { get; set; }

        /// <summary>
        /// 过滤捕获数据包事件Command
        /// </summary>
        public DelegateCommand FilterCaptureBtnCommand { get; set; }

        /// <summary>
        /// 显示关于事件Command
        /// </summary>
        public DelegateCommand InfoBtnCommand { get; set; }

        #endregion

        #region 其他方法
        /// <summary>
        /// 数据包添加事件处理方法
        /// </summary>
        /// <param name="sender">触发事件的对象</param>
        /// <param name="e">数据包添加事件</param>
        public void OnPacketAdd(object sender, PacketAddEventArgs e)
        {
            //collections.Add(e.PacketItem);
            uiThreadDispatcher.Invoke(() =>
            {
                PacketList.Add(e.PacketItem);
                StatusText = $"捕获中...捕获到 {PacketList.Count} 个数据包";
            });
        }


        /// <summary>
        /// 获取所有数据包
        /// </summary>
        /// <returns>所有数据包List</returns>
        public List<PacketItem> GetPacketsList()
        {
            var list = new List<PacketItem>();
            foreach (var i in PacketList)
            {
                list.Add(i);
            }
            return list;
        }

        /// <summary>
        /// 在开始捕获前要完成的工作
        /// </summary>
        public void BeforeStart()
        {
            Started = true;
        }

        /// <summary>
        /// 在结束捕获后要完成的工作
        /// </summary>
        public void AfterStop()
        {
            Started = false;
        }

        /// <summary>
        /// 外部导入数据包
        /// </summary>
        /// <param name="packets">承载PackItem数据包的枚举容器</param>
        public void ImportPackets(IEnumerable<PacketItem> packets)
        {
            PacketList.Clear();
            foreach(var i in packets)
            {
                PacketList.Add(i);
            }
        }
        #endregion

        public MainWindowViewModel(Dispatcher uiDispatcher)
        {
            AdapterComboBoxSelectionChangedCommand = new DelegateCommand();
            StartCaptureBtnCommand = new DelegateCommand();
            StopCaptureBtnCommand = new DelegateCommand();
            ClearCaptureBtnCommand = new DelegateCommand();
            SaveCaptureBtnCommand = new DelegateCommand();
            ImportCaptureBtnCommand = new DelegateCommand();
            FilterCaptureBtnCommand = new DelegateCommand();
            InfoBtnCommand = new DelegateCommand();
            PacketList = new ObservableCollection<PacketItem>();
            filterPredicate = (e) =>
            {
                var el = e as PacketItem;
                if (ptlRegex.IsMatch(FilterText))
                {
                    if (Filter == "*")
                    {
                        return true;
                    }
                    return el.Protocol.ToLower() == Filter.ToLower();
                }
                else if (srcRegex.IsMatch(FilterText))
                {
                    return el.Source.ToLower() == Filter.ToLower();
                }
                else if (dstRegex.IsMatch(FilterText))
                {
                    return el.Destination.ToLower() == Filter.ToLower();
                }
                else
                {
                    return true;
                }
            };
            this.uiThreadDispatcher = uiDispatcher;
        }


    }
}
