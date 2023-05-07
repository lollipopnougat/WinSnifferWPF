using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using WinSnifferWPF.CapUtils;
using WinSnifferWPF.ViewModel;

namespace WinSnifferWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 主窗体初始化方法
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitViewModel();
            InitCapture();
        }

        private readonly AdapterManager adapterManager = new AdapterManager();
        private CaptureManager captureManager;
        private MainWindowViewModel viewModel;

        #region 初始化管理器部分
        /// <summary>
        /// 初始化ViewModel
        /// </summary>
        private void InitViewModel()
        {
            viewModel = new MainWindowViewModel(Dispatcher);
            viewModel.DeviceList = adapterManager.GetAllAdapterDesc();//adapterManager.GetAllAdapterFriendlyNames();
            viewModel.StartCaptureBtnCommand.ExcuteAction = StartBtnClick;
            viewModel.StopCaptureBtnCommand.ExcuteAction = StopBtnClick;
            viewModel.ClearCaptureBtnCommand.ExcuteAction = ClearBtnClick;
            viewModel.ImportCaptureBtnCommand.ExcuteAction = ImportBtnClick;
            viewModel.SaveCaptureBtnCommand.ExcuteAction = SaveBtnClick;
            viewModel.FilterCaptureBtnCommand.ExcuteAction = FilteBtnClick;
            viewModel.AdapterComboBoxSelectionChangedCommand.ExcuteAction = AdapterBoxSelectionChanged;
            viewModel.InfoBtnCommand.ExcuteAction = InfoBtnClick;
            DataContext = viewModel;
        }

        /// <summary>
        /// 初始化抓包管理器
        /// </summary>
        private void InitCapture()
        {
            captureManager = new CaptureManager(adapterManager.DevicesList[viewModel.SelectedDeviceIndex]);
            captureManager.PacketAddEvent += viewModel.OnPacketAdd;
        }
        #endregion


        #region Command事件处理方法

        /// <summary>
        /// 开始捕获按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发开始捕获按钮按下的对象</param>
        private void StartBtnClick(object sender)
        {
            if(viewModel.PacketList.Count > 0)
            {
                var res = MessageBox.Show("确定要开始新的捕获吗?\n\r将会覆盖当前的结果.", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if(res == MessageBoxResult.Cancel)
                {
                    viewModel.StatusText = "取消捕获操作";
                    return;
                }
            }
            ClearBtnClick(sender);
            viewModel.BeforeStart();
            captureManager.StartCapture();
            viewModel.StatusText = "捕获数据包中...";
        }

        /// <summary>
        /// 停止捕获按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发停止捕获按钮按下事件的对象</param>
        private void StopBtnClick(object sender)
        {
            captureManager.StopCapture();
            viewModel.StatusText = $"结束, 共捕获 {viewModel.PacketList.Count} 个数据包";
            viewModel.AfterStop();
        }

        /// <summary>
        /// 网卡下拉框选中变更事件处理方法
        /// </summary>
        /// <param name="sender">触发网卡下拉框选中变更事件的对象</param>
        private void AdapterBoxSelectionChanged(object sender)
        {

            var res = captureManager.ChangeDevice(adapterManager.DevicesList[viewModel.SelectedDeviceIndex]);
            if(res)
            {
                viewModel.StatusText = $"切换成功, 选择了网卡: {captureManager.GetCurrentAdapterName()}";
            }
            else
            {
                viewModel.SelectedDevice = captureManager.GetCurrentAdapterName();
                viewModel.StatusText = $"切换失败, 请先关闭捕获";
            }

        }

        /// <summary>
        /// 清除按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发清除按钮按下事件的对象</param>
        private void ClearBtnClick(object sender)
        {
            if(viewModel.PacketList.Count > 0)
            {
                var res = MessageBox.Show("将会清除当前所有的数据包，是否确定？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if(res == MessageBoxResult.Cancel)
                {
                    viewModel.StatusText = "取消了清除操作";
                    return;
                }
            }
            captureManager.Clear();
            viewModel.PacketList.Clear();
            viewModel.StatusText = "清空完成";
        }

        /// <summary>
        /// 导入按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发导入按钮按下事件的对象</param>
        private void ImportBtnClick(object sender)
        {
            if (viewModel.PacketList.Count > 0)
            {
                var resDia = MessageBox.Show("将会覆盖掉当前所有的数据包，是否确定？", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (resDia == MessageBoxResult.Cancel)
                {
                    viewModel.StatusText = "取消了导入操作";
                    return;
                }
            }

            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "XML文件|*.xml",
                Title = "导入结果文件"
            };
            var res = (bool)dialog.ShowDialog();
            if(res && dialog.FileName.Contains(":"))
            {
                ClearBtnClick(sender);
                viewModel.ImportPackets(ResultDataLoader.GetPacketsFromFile(dialog.FileName));
                viewModel.StatusText = $"导入 {viewModel.PacketList.Count} 个数据包记录成功";
            }

        }

        /// <summary>
        /// 保存按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发保存按钮按下事件的对象</param>
        private void SaveBtnClick(object sender)
        {
            SaveFileDialog openFileDialog = new SaveFileDialog
            {
                FileName = "result.xml",
                Filter = "XML文件|*.xml",
                Title = "选择保存位置"
            };
            var res = (bool)openFileDialog.ShowDialog();
            if (res && openFileDialog.FileName.Contains(":"))
            {
                ResultDataLoader.SavePacketsToFile(viewModel.GetPacketsList(), openFileDialog.FileName);
                viewModel.StatusText = "保存文件成功";
            }
            else
            {
                viewModel.StatusText = "取消了保存";
            }
        }

        /// <summary>
        /// 筛选按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发筛选按钮按下事件的对象</param>
        private void FilteBtnClick(object sender)
        {
            viewModel.PacketListView.Refresh();
            viewModel.StatusText = $"应用筛选条件 {viewModel.FilterText}, 结果共 {viewModel.PacketListView.Count} 条";
        }


        /// <summary>
        /// 关于按钮按下事件处理方法
        /// </summary>
        /// <param name="sender">触发关于按钮按下事件的对象</param>
        private void InfoBtnClick(object sender)
        {
            var name = Application.ResourceAssembly.GetName().Name;
            var version = Application.ResourceAssembly.GetName().Version.ToString();
            var info = Application.ResourceAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false) as AssemblyDescriptionAttribute[];
            var time = System.IO.File.GetLastWriteTime(GetType().Assembly.Location);
            MessageBox.Show($"{name}\r\nVer {version}\r\nBy lnp. 2021202662\r\n\r\n{info[0].Description}\r\n\r\nnpcap版本\r\n{SharpPcap.Pcap.Version}\r\n\r\n编译日期: {time:G}", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
