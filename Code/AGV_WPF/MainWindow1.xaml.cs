using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.IO.Ports;
using System.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using DAL;
using COM;
using AGV_WPF_Global;
using AGV_WPF_DisMember;
using NS_SoftKey_Win;
using Casun_SoftKey;
using AGV_WPF.DLL.AGV;

namespace AGV_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    #region AGV结构体的定义
    public struct AGVDockAreaStr
    {
        public WorkMarkStr DockStartStop;   //停靠区起点
        public WorkMarkStr DockEndStop;     //停靠区终点 
        public int DockNum;         //停靠区路线
    }

    public struct WorkMarkStr
    {
        public int Line;    //AGV工作路线，流水线号
        public int Num; 	//对应流水线号管制区地标号
        public WorkMarkStr(int line, int num)
        {
            this.Line = line;
            this.Num = num;
        }
    };

    #endregion

    public partial class MainWindow : Window
    {
        #region 类变量
        #region AGV常变量的定义

        //常变量的定义

        //AGV运行控制
        public const bool AGVMODERUN = true;
        public const bool AGVMODESTOP = false;

        //AGV无线连接状态
        public const bool AGVWLLINK_OK = true;
        public const bool AGVWLLINK_ERROR = false;

        //实际的AGV数量
        public byte AGVNUM_MAX;// = Convert.ToByte(ConfigurationManager.AppSettings["AGVNUM_MAX"]);
        //管制区AGV等待数量
        //public byte TRAFFIC_CONAREA_WAITAGVNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["TRAFFIC_CONAREA_WAITAGVNUM_MAX"]);
        //管制区实际数量
        public int TRAFFIC_CONAREA;
        //停靠区实际数量
        public int DOCKAREA_NUM;

        //AGV地标功能
        public const byte AGVMARKFUN_OVER = 15;

        //帧校验结果
        public const byte VERIFY_NOERROR = 0;
        public const byte VERIFY_HEADERROR = 1;
        public const byte VERIFY_ENDERROR = 2;
        public const byte VERIFY_FUNERROR = 3;
        public const byte VERIFY_BCCERROR = 4;

        #endregion

        #region AGV信息定义
        public string[] MarkFuncOpt;
        public string[] StatusOpt;
        public string[] SpeedOpt = { "初始化...", "低速", "中速", "高速", "最高速", "减速" };
        public Brush[] ColorOpt = { Brushes.Yellow, Brushes.Tomato, Brushes.Purple, Brushes.Pink, Brushes.SkyBlue, Brushes.LightBlue, Brushes.GreenYellow, Brushes.Goldenrod };
        public string[] WirelessOpt = { "成功", "失败" };
        public string[] TrafficStateOpt = { "管制中", "非管制" };
        AGVCar[] AGVStatus;
        AGVTrafficList agvTrafficList;
        AGVDock agvDock;
        AGVCharge agvCharge;
        AGVCall agvCall;

        #endregion

        #region 串口信息定义

        #region AGV监控串口
        /// <summary>
        /// AGV监控串口
        /// </summary>
        private COM.SerialPortWrapper SPComControl;

        /// <summary>
        /// 读取AGV车辆信息buffer，默认分配1/4页1K内存，并始终限制不允许超过 
        /// </summary>
        private List<byte> buffer_readcontrol = new List<byte>(1024);

        /// <summary>
        /// 缓冲区中可读的字节数
        /// </summary>
        private int readcount_control = 0;

        /// <summary>
        /// AGV交通管制命令
        /// </summary>
        byte[] buf_trafficctl = { 0x10, 0x61, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };

        /// <summary>
        /// 交通管制返回AGV状态信息
        /// </summary>
        byte[] buf_rettraffic = { 0x10, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };

        /// <summary>
        /// AGV运行控制命令缓冲区，动态命令
        /// </summary>
        List<byte> buf_runctl = new List<byte>();
        
        /// <summary>
        /// AGV发送控制命令重复的次数
        /// </summary>
        List<byte> ctrlWaitNum = new List<byte>();

        /// <summary>
        /// 虚拟交通管制命令(用于判断是否断线，未接收到数据时虚拟发送数据，使agv状态断线)
        /// </summary>
        byte[] buf_virtualtrafficctl = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        
        /// <summary>
        /// 接收数据计数器，用于断线后自动更新数据
        /// </summary>
        byte[] data_total;

        /// <summary>
        /// 初试状态运行
        /// </summary>
        private Int64 SendD1D2 = 0L;

        //返回AGV运行控制
        //byte[] buf_retrun = { 0x10, 0x72, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };
        //public int iReceivedNum = 0;//接收数据的次数
        //public int iLastReceivedNum = 0;
        //public int IsSendCtrl = 0;
        //public int CtrlWaitNum = 0;

        #endregion

        #region AGV叫料串口

        /// <summary>
        /// AGV叫料串口
        /// </summary>
        private COM.SerialPortWrapper SPComCall;

        /// <summary>
        /// 叫料系统缓冲区中可读的字节数
        /// </summary>
        private int readcount_call = 0;

        /// <summary>
        /// 叫料系统信息buffer，默认分配1/8页0.5K内存，并始终限制不允许超过 
        /// </summary>
        private List<byte> buffer_readcall = new List<byte>(512);

        /// <summary>
        /// 叫料系统接收数据缓冲区
        /// </summary>
        private byte[] buf_callctl = { 0x10, 0x00, 0x00, 0x00, 0x00, 0x03 };

        /// <summary>
        /// 叫料系统发送数据缓冲区
        /// </summary>
        private byte[] buf_callret = { 0x10, 0x00, 0x00, 0x00, 0x00, 0x03 };
        #endregion

        #endregion

        #region 数据库定义
        public static DAL.ZSql TrafficPara = new DAL.ZSql();
        #endregion

        #region 动画定义
        AGV_DisMember[] DisMember;
        //定时器定时刷新界面
        DispatcherTimer t = new DispatcherTimer();
        DispatcherTimer DataTimer = new DispatcherTimer();
        ObservableCollection<AGV_DisMember> memberData = new ObservableCollection<AGV_DisMember>();
        #endregion

        #region 系统定义
        private bool IsOpenSystem = false;
        private bool disposed;
        #endregion
        #endregion

        #region 窗体创建与销毁

        public MainWindow()
        {
            NameScope.SetNameScope(this, new NameScope());
            InitializeComponent();
            ReadConfigFile();
            //变量初始化
            AGVStatus = new AGVCar[AGVNUM_MAX];
            DisMember = new AGV_DisMember[AGVNUM_MAX];
            data_total = new byte[AGVNUM_MAX];
            btn_OpenSystem.IsEnabled = true;
            btn_CloseSystem.IsEnabled = false;
        }

        private void ReadConfigFile()
        {
            try
            {
                AGVNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["AGVNUM_MAX"]);
                GlobalPara.Gcontrolcomname = ConfigurationManager.AppSettings["ControlCOMName"];
                GlobalPara.Gcallcomname = ConfigurationManager.AppSettings["CallCOMName"];
                GlobalPara.IsTrafficFun = Convert.ToBoolean(ConfigurationManager.AppSettings["TRAFFICFUN"]);
                GlobalPara.IsDockFun = Convert.ToBoolean(ConfigurationManager.AppSettings["DOCKFUN"]);
                GlobalPara.IsChargeFun = Convert.ToBoolean(ConfigurationManager.AppSettings["CHARGEFUN"]);
                GlobalPara.IsCallFun = Convert.ToBoolean(ConfigurationManager.AppSettings["CALLFUN"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //RegisterDeviceNotification();//注册加密锁事件插拨通知
            MenuBarInit();//菜单栏初始化
            StatusBarInit();//状态栏初始化
            MapInit(@"Image\background.png");//地图初始化
            AGVGridInit(AGVNUM_MAX);//界面表格初始化
            ControlAreaInit();//控制面板初始化
            CustomInit();//自定义参数获取初始化
            DrvWlConInit();//交通管制区初始化
            TimerInit();//定时器初始化
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Event arguments</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stop timer
            if (null != this.t)
            {
                this.t.Stop();
                this.t.Tick -= this.t_Tick;
            }

            // Stop timer
            if (null != this.DataTimer)
            {
                this.DataTimer.Stop();
                this.DataTimer.Tick -= this.SendDataTimerTick;
            }

            // Stop Com,Unregister SPComControl data received event
            if (null != SPComControl)
            {
                this.SPComControl.Close();
                this.SPComControl.OnDataReceived -= this.SPComControl_OnDataReceived;
            }

            //clear collection
            if (null != memberData)
            {
                memberData.Clear();
                memberData = null;
            }
        }

        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (null != this.SPComControl)
                    {
                        this.SPComControl.Dispose();
                    }
                    if (null != TrafficPara)
                    {
                        TrafficPara.Dispose();
                    }
                }
            }

            this.disposed = true;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 状态栏初始化设置
        /// </summary>
        private void StatusBarInit()
        {
            LaunchTimer();//状态栏本地系统时间
            lblusername.Content = GlobalPara.strName;//状态栏用户名
        }

        /// <summary>
        /// 菜单栏权限设置初始化
        /// </summary>
        private void MenuBarInit()
        {
            if (!GlobalPara.IsManager)
            {
                MenuAGVManager.Visibility = Visibility.Collapsed;
                MenuCallManage.Visibility = Visibility.Collapsed;
                MenuSettings.Visibility = Visibility.Collapsed;
                MenuSystemManage.Visibility = Visibility.Collapsed;
            }
            if (!GlobalPara.IsCallFun)
            {
                MenuCallManage.Visibility = Visibility.Collapsed;
                MCallCOM.Visibility = Visibility.Collapsed;
            }
            if (!GlobalPara.IsDockFun)
            {
                DockArea.Visibility = Visibility.Collapsed;
            }
            if (!GlobalPara.IsChargeFun)
            {
                ChargeArea.Visibility = Visibility.Collapsed;
            }
            if (!GlobalPara.IsTrafficFun)
            {
                Traffic.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 定时器初始化
        /// </summary>
        private void TimerInit()
        {
            //定时器定时刷新界面
            t.Interval = TimeSpan.FromMilliseconds(50);//设置执行间隔单位ms毫秒
            t.Tick += new EventHandler(t_Tick);//t_Tick是要执行的函数

            //PC与串口扩展板每次通信周期550+其他ms
            DataTimer.Interval = TimeSpan.FromMilliseconds(550);
            DataTimer.Tick += new EventHandler(this.SendDataTimerTick);
        }

        /// <summary>
        /// 自定义初始化
        /// </summary>
        private void CustomInit()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            //地标功能定义
            sql1.Open("select CmdFunction from T_Custom where CustomType=1 order by CmdNum");
            if (sql1.rowcount > 0)
            {
                MarkFuncOpt = new string[sql1.rowcount];
                for (int i = 0; i < sql1.rowcount; i++)
                {
                    MarkFuncOpt[i] = sql1.Rows[i]["CmdFunction"].ToString();
                }
            }
            else
            {
                MessageBox.Show("警告：请在“自定义设置”中设置地标功能！");
            }
            //运行状态定义
            sql1.Open("select CmdFunction from T_Custom where CustomType=2 order by CmdNum");
            if (sql1.rowcount > 0)
            {
                StatusOpt = new string[sql1.rowcount];
                for (int i = 0; i < sql1.rowcount; i++)
                {
                    StatusOpt[i] = sql1.Rows[i]["CmdFunction"].ToString();
                }
            }
            else
            {
                MessageBox.Show("警告：请在“自定义设置”中设置AGV运行状态！");
            }
            sql1.Close();
        }

        /// <summary>
        /// 控制面板中数据初始化
        /// </summary>
        private void ControlAreaInit()
        {
            //小车编号加载初始化
            for (int i = 0; i < AGVNUM_MAX; i++)
            {
                cb_AgvNum.Items.Add("AGV" + (i + 1).ToString());
            }
            //线路加载初始化
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT LineNum from T_Line order by LineNum");
            cb_LineNum.ItemsSource = sql1.m_table.DefaultView;
            cb_LineNum.DisplayMemberPath = "LineNum";
            cb_LineNum.SelectedValuePath = "LineNum";
            cb_LineNum.SelectedValue = "0";
            sql1.Close();
        }

        /// <summary>
        /// 状态栏定时器
        /// </summary>
        private void LaunchTimer()
        {
            DispatcherTimer innerTimer = new DispatcherTimer(TimeSpan.FromSeconds(1.0),
                    DispatcherPriority.Loaded, new EventHandler(this.TimerTick), this.Dispatcher);
            innerTimer.Start();
        }

        /// <summary>
        ///  状态栏定时器触发函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToString();
        }

        /// <summary>
        /// AGV监控串口初始化
        /// </summary>
        private void ControlCOMInit()
        {
            int controlcombaudrate = int.Parse(ConfigurationManager.AppSettings["ControlCOMBaudrate"]);
            int controlcomdatabits = int.Parse(ConfigurationManager.AppSettings["ControlCOMDataBits"]);
            string controlcomstopbits = ConfigurationManager.AppSettings["ControlCOMStopBits"];
            string controlcomparity = ConfigurationManager.AppSettings["ControlCOMParity"];
            try
            {
                SPComControl = new COM.SerialPortWrapper(GlobalPara.Gcontrolcomname, controlcombaudrate, controlcomparity, controlcomdatabits, controlcomstopbits);
                SPComControl.OnDataReceived += new SerialDataReceivedEventHandler(SPComControl_OnDataReceived);
                SPComControl.ReceivedBytesThreshold = buf_rettraffic.Length;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// AGV叫料串口初始化
        /// </summary>
        private void CallCOMInit()
        {
            int callcombaudrate = int.Parse(ConfigurationManager.AppSettings["CallCOMBaudrate"]);
            int callcomdatabits = int.Parse(ConfigurationManager.AppSettings["CallCOMDataBits"]);
            string callcomstopbits = ConfigurationManager.AppSettings["CallCOMStopBits"];
            string callcomparity = ConfigurationManager.AppSettings["CallCOMParity"];
            try
            {
                SPComCall = new COM.SerialPortWrapper(GlobalPara.Gcallcomname, callcombaudrate, callcomparity, callcomdatabits, callcomstopbits);
                SPComCall.OnDataReceived += new SerialDataReceivedEventHandler(SPComCall_OnDataReceived);
                SPComCall.ReceivedBytesThreshold = buf_callret.Length;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 加载电子地图背景图片
        /// </summary>
        /// <param name="struri">电子地图图片位置</param>
        public void MapInit(string struri)
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(struri, UriKind.RelativeOrAbsolute));
            imageBrush.Stretch = Stretch.Uniform;
            imageBrush.AlignmentX = AlignmentX.Left;
            imageBrush.AlignmentY = AlignmentY.Top;
            canvas.Background = imageBrush;
        }

        /// <summary>
        /// agv表格绑定数据初始化
        /// </summary>
        /// <param name="agvnum"></param>
        public void AGVGridInit(int agvnum)
        {
            for (int i = 0; i < agvnum; i++)
            {
                DisMember[i] = new AGV_DisMember() { txtAGVNum = "AGV" + (i + 1).ToString() };
                DisMember[i].PropertyChanged += new PropertyChangedEventHandler(Dis_PropertyChange);
                //memberData.Add(DisMember[i]);
            }
            dataGrid.DataContext = memberData;
        }

        #endregion

        #region 事件委托触发更新
        /// <summary>
        /// 串口类SerialPortWrapper交通管制的数据接收触发委托函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SPComControl_OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            readcount_control = SPComControl.BytesToRead;
            byte[] buf = new byte[readcount_control];
            SPComControl.Read(buf, 0, readcount_control);
            buffer_readcontrol.AddRange(buf);
            while (buffer_readcontrol.Count >= 11)
            {
                if (buffer_readcontrol[0] == 0x10 && buffer_readcontrol[1] == 0x62 && buffer_readcontrol[10] == 0x03)
                {
                    buffer_readcontrol.CopyTo(0, buf_rettraffic, 0, 11);
                    buffer_readcontrol.RemoveRange(0, 11);
                    ControlData_Processing(buf_rettraffic);
                }
                else
                {
                    buffer_readcontrol.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// AGV车辆返回信息处理过程
        /// </summary>
        /// <param name="buf">AGV车辆返回数据buffer</param>
        private void ControlData_Processing(byte[] buf)
        {
            if (DrvWlCon_AgvStatus(buf))
            {
                if (AGVStatus[buf[2] - 1].wlLink == AGVWLLINK_OK)
                {
                    if (buf[1] == 0x62)
                    {
                        data_total[buf[2] - 1]++;
                        if (agvDock != null)
                        {
                            agvDock.UpdateDock(ref AGVStatus[buf[2] - 1]); 
                        }
                        if (agvTrafficList != null)
                        {
                            bool runflag = agvTrafficList.DrvWlConUpdateRunPar(ref AGVStatus[buf[2] - 1]);
                            if (!runflag)
                            {
                                SendD1D2 |= Convert.ToInt64(1 << (buf[2] - 1));
                            }
                            else
                            {
                                SendD1D2 &= Convert.ToInt64(~(1 << (buf[2] - 1)));
                            }
                        }
                    }

                    //判断运行控制是否回信息
                    if (buf_runctl.Count > 0)
                    {
                        if (buf[1] == 0x72)
                        {
                            for (int i = 0; i < ctrlWaitNum.Count; i++)
                            {
                                if (buf[2] == buf_runctl[i * buf_runctl.Count / ctrlWaitNum.Count + 2])
                                {
                                    buf_runctl.RemoveRange(buf_runctl.Count / ctrlWaitNum.Count * i, buf_runctl.Count / ctrlWaitNum.Count);//移除11个字节
                                    ctrlWaitNum.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 串口类SerialPortWrapper叫料系统的数据接收触发委托函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SPComCall_OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            readcount_call = SPComCall.BytesToRead;
            byte[] buf = new byte[readcount_call];
            SPComCall.Read(buf, 0, readcount_call);
            buffer_readcall.AddRange(buf);
            while (buffer_readcall.Count >= 6)
            {
                if (buffer_readcall[0] == 0x10 && buffer_readcall[5] == 0x03)
                {
                    buffer_readcall.CopyTo(0, buf_callctl, 0, 6);
                    buffer_readcall.RemoveRange(0, 6);
                    CallData_Processing(buf_callctl);
                }
                else
                {
                    buffer_readcall.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// 叫料信息处理过程
        /// </summary>
        /// <param name="buf">叫料信息数据buffer</param>
        private void CallData_Processing(byte[] buf)
        {
            if (CallDataCheck(buf, buf.Length - 2) == VERIFY_NOERROR)
            {
                if (buf[1] == 0x41)//叫料
                {
                    if (agvCall.Add(buf[2], buf[3]))
                    {
                        buf_callret[0] = 0x10;
                        buf_callret[1] = 0x43;
                        buf_callret[2] = buf[2];
                        buf_callret[3] = buf[3];
                        buf_callret[4] = COM.SerialPortWrapper.GetXORCheckCode(buf_callret, buf_callctl.Length - 2);
                        buf_callret[5] = 0x03;
                        SPComCall.Write(buf_callret, 0, buf_callret.Length);
                        if (agvDock != null)
                        {
                            if (agvDock.dockingCount > 0)
                            {
                                byte agvnum = agvDock.GetDockCarNum();
                                if (agvnum > 0 && agvCall.lineNum.Count > 0)
                                {
                                    if (agvCall.lineNum[0] > 0)
                                    {
                                        agvDock.outDockLine[agvnum - 1] = agvCall.lineNum[0];
                                        AGVControlCommand(agvnum, 3, 0, 0);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //agvCall.memberData.
                             //先判断是否到达起点位置
                            //到达后再启动AGV
                            //没有达到时需要叫料排队，等到到达再启动
                        }
                    }
                }
                else if(buf[1] == 0x42)//取消叫料
                {
                    if(agvCall.Delete(buf[2], buf[3]))
                    {
                        buf_callret[0] = 0x10;
                        buf_callret[1] = 0x45;
                        buf_callret[2] = buf[2];
                        buf_callret[3] = buf[3];
                        buf_callret[4] = COM.SerialPortWrapper.GetXORCheckCode(buf_callret, buf_callctl.Length - 2);
                        buf_callret[5] = 0x03;
                        SPComCall.Write(buf_callret, 0, buf_callret.Length);
                    }
                }
            }
        }

        public void QueryCall(byte stationmum, byte materialnum)
        { 
        
        }

        /// <summary>
        /// 数据格式效验
        ///  0x10 功能码 工位号 异或效验 0x03
        ///  功能码（0x41 叫料 0x42 取消叫料 0x43 叫料成功 0x44 设置工位号成功 0x45 取消叫料成功））
        /// </summary>
        /// <param name="buf">接收帧数据</param>
        /// <returns>返回效验结果</returns>
        public static int CallDataCheck(byte[] buf, int length)
        {
            if (buf[0] != 0x10)
            {
                return VERIFY_HEADERROR;//头错误
            }
            else if ((buf[buf.Length - 1]) != 0x03)
            {
                return VERIFY_ENDERROR;//尾错误
            }
            else if (buf[1] != 0x41 && buf[1] != 0x42 && buf[1] != 0x43 && buf[1] != 0x44 && buf[1] != 0x45)
            {
                return VERIFY_FUNERROR;
            }
            else if (COM.SerialPortWrapper.GetXORCheckCode(buf, length) != buf[buf.Length - 2])
            {
                return VERIFY_BCCERROR;// 校验错误 
            }
            else
            {
                return VERIFY_NOERROR;
            }
        }

        /// <summary>
        /// 发送数据定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendDataTimerTick(object sender, EventArgs e)
        {
            if (ctrlWaitNum.Count > 0)
            {
                for (int i = 0; i < ctrlWaitNum.Count;i++ )
                {
                    if (ctrlWaitNum[i]++ < 3)
                    {
                        SPComControl.Write(buf_runctl.ToArray(), buf_runctl.Count / ctrlWaitNum.Count * i, buf_runctl.Count / ctrlWaitNum.Count);
                    }
                    else
                    {
                        buf_runctl.RemoveRange(buf_runctl.Count / ctrlWaitNum.Count * i,buf_runctl.Count / ctrlWaitNum.Count);//移除11个字节
                        ctrlWaitNum.RemoveAt(i);
                    }
                    Thread.Sleep(20);
                }
                //线程挂起100ms，等待接收数据中断（挂起时，其他线程运行）
                Thread.Sleep(80);
            }
            for (int i = 0; i < data_total.Length; i++)
            {
                if (data_total[i] == 0)
                {
                    Array.Clear(buf_virtualtrafficctl, 0, buf_virtualtrafficctl.Length);
                    buf_virtualtrafficctl[2] = Convert.ToByte(i + 1);
                    DrvWlCon_AgvStatus(buf_virtualtrafficctl);
                }
                data_total[i] = 0;
            }
            {
                buf_trafficctl[0] = 0x10;
                buf_trafficctl[1] = 0x61;
                buf_trafficctl[2] = Convert.ToByte(SendD1D2 & 0x00000000000000ff);
                buf_trafficctl[3] = Convert.ToByte((SendD1D2 & 0x000000000000ffff) >> 8);
                buf_trafficctl[4] = Convert.ToByte((SendD1D2 & 0x0000000000ffffff) >> 16);
                buf_trafficctl[5] = Convert.ToByte((SendD1D2 & 0x00000000ffffffff) >> 24);
                buf_trafficctl[6] = Convert.ToByte((SendD1D2 & 0x000000ffffffffff) >> 32);
                buf_trafficctl[7] = Convert.ToByte((SendD1D2 & 0x0000ffffffffffff) >> 40);
                buf_trafficctl[8] = Convert.ToByte((SendD1D2 & 0x00ffffffffffffff) >> 48);
                buf_trafficctl[9] = COM.SerialPortWrapper.GetXORCheckCode(buf_trafficctl, buf_trafficctl.Length - 2);
                buf_trafficctl[10] = 0x03;
                SPComControl.Write(buf_trafficctl, 0, buf_trafficctl.Length);
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// 更新屏幕定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void t_Tick(object sender, EventArgs e)
        {
            UpdateCurrentScreen();
        }

        /// <summary>
        /// 更新屏幕
        /// </summary>
        void UpdateCurrentScreen()
        {
            for (int i = 0; i < AGVNUM_MAX; i++)
            {
                DisMember[i].txtWorkLineValue = AGVStatus[i].worklineNum.ToString();
                DisMember[i].txtDockNumValue = AGVStatus[i].dockNum.ToString();
                DisMember[i].txtAGVNumValue = "AGV" + AGVStatus[i].agvNum.ToString();
                DisMember[i].txtTrafficNumValue = AGVStatus[i].trafficNum.ToString();
                DisMember[i].txtTrafficStateValue = (buf_trafficctl[i / 8 + 2] & (0x01 << i % 8)) != 0 ? "管制中" : "非管制";
                DisMember[i].txtLineNumValue = AGVStatus[i].lineNum.ToString();
                DisMember[i].txtMarkFunctionValue = MarkFuncOpt[AGVStatus[i].markFunction];
                DisMember[i].txtSpeedValue = SpeedOpt[AGVStatus[i].agvSpeed];
                DisMember[i].txtMarkNumValue = AGVStatus[i].markNum.ToString();
                DisMember[i].txtPowerValue = AGVStatus[i].agvPower;
                DisMember[i].txtagvChargeValue = AGVStatus[i].agvCharge;
                DisMember[i].txtWLValue = AGVStatus[i].wlLink ? "成功" : "失败";//1124
                switch (AGVStatus[i].agvStatus)
                {
                    case 0x40:
                        DisMember[i].txtStatusValue = "运行";
                        break;
                    case 0x41:
                        DisMember[i].txtStatusValue = "暂停";
                        break;
                    case 0x42:
                        DisMember[i].txtStatusValue = "结束地标停止";
                        break;
                    default:
                        if (AGVStatus[i].agvStatus < StatusOpt.Length)
                        {
                            DisMember[i].txtStatusValue = StatusOpt[AGVStatus[i].agvStatus];
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 界面显示类AGV_DisMember属性更改委托函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Dis_PropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("txtMarkNum"))
            {
                MarkChangeAction((AGV_DisMember)sender);
            }
            else if (e.PropertyName.Equals("txtStatus"))
            {
                StatusChangeAction((AGV_DisMember)sender);
            }
            else if (e.PropertyName.Equals("txtWL"))
            {
                WLChangeAction((AGV_DisMember)sender);
            }
            else if (e.PropertyName == "txtPower")
            {
                PowerChangeAction((AGV_DisMember)sender);
            }
        }

        /// <summary>
        /// 地标属性更改时触发的事件
        /// 移除上次动画，显示下一动画
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void MarkChangeAction(AGV_DisMember temp1)
        {
            byte iagvnum = Convert.ToByte(temp1.txtAGVNum.Remove(0, 3));
            if (iagvnum > 0 && iagvnum <= AGVNUM_MAX)
            {
                Point pStart = new Point(), pEnd = new Point(), pVirtual = new Point();
                //if (Convert.ToInt32(temp1.txtDockNum) > 0)
                //{
                //    TrafficPara.Open("Select RouteNum from T_DockSetting where DockNum=" + temp1.txtDockNum + " and AGVNum=" + iagvnum.ToString());
                //    if (TrafficPara.rowcount > 0)
                //    {
                //        temp1.txtLineNum = TrafficPara.Rows[0]["RouteNum"].ToString();
                //    }
                //}


                //待测试
                //if (agvDock != null && agvCharge != null && agvCall != null)
                //{
                //    if (Convert.ToInt32(temp1.txtDockNum) == 0)
                //    {
                //        if (agvDock.dockStartStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                //        {
                //            //充电返回后，清除状态
                //            if (temp1.txtagvCharge == 4) { AGVStatus[iagvnum - 1].agvCharge = 0; }
                //            CarLine carline = agvDock.Add(iagvnum);
                //            if (carline != null)
                //            {
                //                temp1.txtDockNum = carline.dockNum.ToString();
                //                temp1.txtLineNum = carline.lineNum.ToString();
                //                AGVStatus[iagvnum - 1].dockNum = carline.dockNum;
                //                //启动车辆，发送变更为待装停靠区路线
                //                //进入待装区时，起始地标的功能为暂停，结束地标不能为停止或暂停，一定为agv运行的地标功能，也不能在交通管制点内
                //                AGVControlCommand(iagvnum, 3, 0, Convert.ToByte(carline.lineNum));
                //            }
                //            else
                //            {
                //                //这是在排队队列中，此时需要车辆停止（发送停止命令）
                //                AGVControlCommand(iagvnum, 2, 0, 0);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (agvDock.dockEndStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                //        {
                //            //移除待装区排队
                //            CarLine carline = agvDock.Delete(iagvnum);
                //            if (carline != null)
                //            {
                //                //只有停靠区和管制区在这边需要软件更新，其他的更新可以发送控制命令，下位机更改AGV状态，上位机接收数据驱动更新
                //                AGVStatus[carline.agvNum - 1].dockNum = carline.dockNum;
                //                //如果在等待排队中有车辆，启动等待的车辆
                //                AGVControlCommand(carline.agvNum, 3, 0, Convert.ToByte(carline.lineNum));
                //            }
                //            temp1.txtDockNum = "0";
                //            AGVStatus[iagvnum - 1].dockNum = 0;
                //            //此时需要变换路线agvDock.outDockLine[iagvnum-1],发出控制命令，变更路线
                //            //1.当监测到temp1.txtagvCharge ==1时，outDockLine[iagvnum-1]为充电路线
                //            //2.当temp1.txtagvCharge ==0时，为执行任务的路线
                //            if (temp1.txtagvCharge == 1)
                //            {
                //                AGVStatus[iagvnum - 1].agvCharge = 2;
                //            }
                //            AGVControlCommand(iagvnum, 0, 0, Convert.ToByte(agvDock.outDockLine[iagvnum - 1]));
                //            agvDock.outDockLine[iagvnum - 1] = 0;
                //        }
                //    }

                //    if (temp1.txtagvCharge == 2)
                //    {
                //        if (agvCharge.dockStartStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                //        {
                //            CarLine carline = agvCharge.Add(iagvnum);
                //            if (carline != null)
                //            {
                //                temp1.txtagvCharge = 3;//充电状态
                //                AGVStatus[iagvnum - 1].agvCharge = 3;//充电状态
                //                temp1.txtLineNum = carline.lineNum.ToString();
                //                //启动车辆，发送变更路线
                //                AGVControlCommand(iagvnum, 3, 0, Convert.ToByte(carline.lineNum));
                //            }
                //            else
                //            {
                //                //这是在排队队列中，此时需要车辆停止（发送停止命令）
                //                AGVControlCommand(iagvnum, 2, 0, 0);
                //            }
                //        }
                //    }//注意：充电完成，车辆不会自行启动，需要人工拔掉充电线，然后启动车辆
                //    else if (temp1.txtagvCharge == 3)
                //    {
                //        if (agvCharge.dockEndStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                //        {
                //            //移除待装区排队
                //            CarLine carline = agvCharge.Delete(iagvnum);
                //            if (carline != null)
                //            {
                //                //只有停靠区和管制区在这边需要软件更新，其他的更新可以发送控制命令，下位机更改AGV状态，上位机接收数据驱动更新
                //                AGVStatus[carline.agvNum - 1].dockNum = carline.dockNum;
                //                //如果在等待排队(此时AGV车辆在待装区中排队，不是在充电区的起始地标排队)中有车辆，启动等待的车辆
                //                AGVControlCommand(carline.agvNum, 3, 0, 0);
                //                agvDock.outDockLine[iagvnum - 1] = agvCharge.chargeLine;//在出待装区的时候改变路线使用
                //            }
                //            temp1.txtagvCharge = 4;
                //            AGVStatus[iagvnum - 1].agvCharge = 4;
                //        }
                //    }
                //}

                #region 停靠区
                if (agvDock != null)
                {
                    // 判断是否进入停靠区
                    if (Convert.ToInt32(temp1.txtDockNum) == 0)
                    {
                        if (agvDock.dockStartStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                        {
                            //已经进入停靠区：1.更新AGV的停靠区参数 2.更新AGV的路线

                            //车辆进入停靠区
                            CarLine carline = agvDock.Add(iagvnum);
                            if (carline != null)
                            {
                                temp1.txtDockNum = carline.dockNum.ToString();
                                temp1.txtLineNum = carline.lineNum.ToString();
                                //更新停靠区参数
                                AGVStatus[iagvnum - 1].dockNum = carline.dockNum;
                                //启动车辆，发送变更为待装停靠区路线
                                //进入待装区时，起始地标的功能为暂停，结束地标不能为停止或暂停，一定为agv运行的地标功能，也不能在交通管制点内
                                AGVControlCommand(iagvnum, 3, 0, Convert.ToByte(carline.lineNum));
                            }
                            else
                            {
                                //这是在排队队列中，此时需要车辆停止（发送停止命令）
                                AGVControlCommand(iagvnum, 2, 0, 0);
                            }
                            //启用充电区
                            if (agvCharge != null)
                            {
                                //充电返回后，清除状态
                                if (temp1.txtagvCharge == 4) { AGVStatus[iagvnum - 1].agvCharge = 0; }
                            }
                            else//不启用充电区
                            {
                            }
                        }
                        else 
                        { 
                            //未进入停靠区，一直处于停靠区外面；暂不做处理
                        }
                    }
                    else//判断是否出停靠区
                    {
                        if (agvDock.dockEndStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                        {
                            //离开停靠区：1.更新AGV的停靠区参数 2.更新AGV的路线

                            //移除待装区排队
                            CarLine carline = agvDock.Delete(iagvnum);
                            //更新AGV停靠区参数
                            temp1.txtDockNum = "0";
                            AGVStatus[iagvnum - 1].dockNum = 0;
                            //更新AGV路线
                            //有车辆等待排队
                            if (carline != null)
                            {
                                //只有停靠区和管制区在这边需要软件更新，其他的更新可以发送控制命令，下位机更改AGV状态，上位机接收数据驱动更新
                                AGVStatus[carline.agvNum - 1].dockNum = carline.dockNum;
                                //如果在等待排队中有车辆，启动等待的车辆
                                AGVControlCommand(carline.agvNum, 3, 0, Convert.ToByte(carline.lineNum));
                            }
                            //启用充电区
                            if (agvCharge != null)
                            {
                                if (temp1.txtagvCharge == 1)
                                {
                                    byte linenum = agvCharge.GetChargeLine(Convert.ToByte(temp1.txtLineNum));
                                    //充电路线，当linenum=0,说明用户没有定义
                                    agvDock.outDockLine[iagvnum - 1] = linenum;//充电路线
                                    if (linenum > 0)
                                    {
                                        AGVStatus[iagvnum - 1].agvCharge = 2;
                                    }
                                }
                            }
                            else 
                            {
                                //不启动充电区
                            }

                            //车辆驶出停靠区后变更路线
                            //此时需要变换路线agvDock.outDockLine[iagvnum-1],发出控制命令，变更路线
                            //1.当监测到temp1.txtagvCharge ==1时，outDockLine[iagvnum-1]为充电路线
                            //2.当temp1.txtagvCharge ==0时，为执行任务的路线
                            if (agvDock.outDockLine[iagvnum - 1] > 0)
                            {
                                AGVControlCommand(iagvnum, 0, 0, Convert.ToByte(agvDock.outDockLine[iagvnum - 1]));
                                agvDock.outDockLine[iagvnum - 1] = 0;
                            }

                        }
                        else
                        {
                            //已经进入停靠区，但一直在停靠区内；暂不做处理
                        }
                    }
                }
                #endregion

                #region 充电区
                if (agvCharge != null)
                {
                    if (temp1.txtagvCharge == 2)
                    {
                        if (agvCharge.dockStartStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                        {
                            CarLine carline = agvCharge.Add(iagvnum,Convert.ToByte(temp1.txtLineNum));
                            if (carline != null)
                            {
                                temp1.txtagvCharge = 3;//充电状态
                                AGVStatus[iagvnum - 1].agvCharge = 3;//充电状态
                                temp1.txtLineNum = carline.lineNum.ToString();
                                //启动车辆，发送变更路线
                                AGVControlCommand(iagvnum, 3, 0, Convert.ToByte(carline.lineNum));
                            }
                            else
                            {
                                //这是在排队队列中，此时需要车辆停止（发送停止命令）
                                AGVControlCommand(iagvnum, 2, 0, 0);
                            }
                        }
                    }//注意：充电完成，车辆不会自行启动，需要人工拔掉充电线，然后启动车辆
                    else if (temp1.txtagvCharge == 3)
                    {
                        if (agvCharge.dockEndStop.Equals(new WorkMarkStr(Convert.ToInt32(temp1.txtWorkLine), Convert.ToInt32(temp1.txtMarkNum))))
                        {
                            //移除待装区排队
                            CarLine carline = agvCharge.Delete(iagvnum);
                            if (carline != null)
                            {
                                //只有停靠区和管制区在这边需要软件更新，其他的更新可以发送控制命令，下位机更改AGV状态，上位机接收数据驱动更新
                                AGVStatus[carline.agvNum - 1].dockNum = carline.dockNum;
                                //如果在等待排队(此时AGV车辆在待装区中排队，不是在充电区的起始地标排队)中有车辆，启动等待的车辆
                                temp1.txtLineNum = carline.lineNum.ToString();
                                AGVControlCommand(carline.agvNum, 3, 0, Convert.ToByte(carline.lineNum));
                            }
                            temp1.txtagvCharge = 4;
                            AGVStatus[iagvnum - 1].agvCharge = 4;
                            AGVControlCommand(iagvnum, 0, 0, agvCharge.beforeEnterLine[Convert.ToInt32(temp1.txtDockNum)]);
                        }
                    }
                }
                #endregion

                TrafficPara.Open("select T_Line.MarkOrder, T_Mark.XPos, T_Mark.YPos FROM T_Line LEFT OUTER JOIN T_Mark ON T_Mark.ID = T_Line.MarkID where WorkLine=" + temp1.txtWorkLine + " and Mark=" + temp1.txtMarkNum + " and LineNum=" + temp1.txtLineNum);
                if (TrafficPara.Rows.Count > 0)
                {
                    pStart.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                    pStart.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                    List<Point> pointcollection1 = new List<Point>();
                    pointcollection1.Add(pStart);
                    int currentOrder = Convert.ToInt16(TrafficPara.Rows[0]["MarkOrder"]) + 1;
                    //线路起点处检测电量
                    if (currentOrder == 2)
                    {
                        //AGV到达起点位置，空闲状态；当检测到AGV状态为运行时，为执行任务
                        AGVStatus[iagvnum - 1].agvTask = 0;
                        //启用充电区
                        if (agvCharge != null)
                        {
                            if (temp1.txtagvCharge == 1)
                            {
                                byte linenum = agvCharge.GetChargeLine(Convert.ToByte(temp1.txtLineNum));
                                //当linenum=0时，说明管理员没有设置充电路线，不会启动自动充电
                                if (linenum > 0)
                                {
                                     AGVControlCommand(iagvnum, 1, 0, linenum);
                                     AGVStatus[iagvnum - 1].agvCharge = 2;
                                }
                            }
                        }
                    }
                    bool isvirtualpoint = true;
                    double dMarkdistance = 0;
                    //while (isvirtualpoint)
                    //{
                    //    TrafficPara.Open("select XPos,YPos,T_Line.Distance from T_Mark Left join T_Line on T_Mark.ID = T_Line.MarkID where T_Line.MarkOrder=" + currentOrder.ToString() +
                    //        "and LineNum=" + temp1.txtLineNum + "and T_Mark.VirtualMark=1");
                    //    if (TrafficPara.Rows.Count > 0)
                    //    {
                    //        pVirtual.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                    //        pVirtual.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                    //        pointcollection1.Add(pVirtual);
                    //        dMarkdistance += Convert.ToDouble(TrafficPara.Rows[0]["Distance"]);
                    //        currentOrder++;
                    //    }
                    //    else { isvirtualpoint = false; break; }
                    //}
                    //TrafficPara.Open("select XPos,YPos,T_Line.Distance from T_Mark Left join T_Line on T_Mark.ID = T_Line.MarkID where T_Line.MarkOrder=" + currentOrder.ToString() +
                    //    "and LineNum=" + temp1.txtLineNum + "and T_Mark.VirtualMark=0");
                    //if (TrafficPara.Rows.Count > 0)
                    //{
                    //    pEnd.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                    //    pEnd.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                    //    pointcollection1.Add(pEnd);
                    //    dMarkdistance += Convert.ToDouble(TrafficPara.Rows[0]["Distance"]);
                    //}
                    //else return;
                    #region 待测试方法
                    
                    do
                    {
                        TrafficPara.Open("select XPos,YPos,T_Line.Distance,VirtualMark from T_Mark Left join T_Line on T_Mark.ID = T_Line.MarkID where T_Line.MarkOrder=" + currentOrder.ToString() +
                        "and LineNum=" + temp1.txtLineNum);
                        if (TrafficPara.Rows.Count > 0)
                        {
                            pVirtual.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                            pVirtual.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                            pointcollection1.Add(pVirtual);
                            dMarkdistance += Convert.ToDouble(TrafficPara.Rows[0]["Distance"]);
                            isvirtualpoint = Convert.ToBoolean(TrafficPara.Rows[0]["VirtualMark"]);
                            currentOrder++;
                        }
                    }
                    while (isvirtualpoint);
                    
                    
                    #endregion

                    if (pointcollection1.Count >= 2)
                    {
                        double dAgvspeed = 0;
                        TrafficPara.Open("select Speed from T_Speed where SpeedGrade='" + temp1.txtSpeedValue + "'");
                        if (TrafficPara.Rows.Count > 0)
                        {
                            dAgvspeed = Convert.ToDouble(TrafficPara.Rows[0]["Speed"]) / 60.0;
                        }
                        else return;
                        AGVStatus[iagvnum - 1].agvAnimation.DrawCarLine(pointcollection1, ColorOpt[iagvnum % ColorOpt.Length],dMarkdistance / dAgvspeed);
                    }
                }
                TrafficPara.Close();
            }
        }

        /// <summary>
        /// 运行状态属性更改时触发的事件
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void StatusChangeAction(AGV_DisMember temp1)
        {
            byte iagvnum = Convert.ToByte(temp1.txtAGVNum.Remove(0, 3));
            if (iagvnum > 0 && iagvnum <= AGVNUM_MAX)
            {
                AGVStatus[iagvnum - 1].agvAnimation.StatusChangeAnimation(temp1.txtStatus.Trim(),iagvnum.ToString(),temp1.txtDockNum.Trim());
                switch (temp1.txtStatus.Trim())
                {
                    case "运行":
                        AGVStatus[iagvnum - 1].agvTask = 1;
                        break;
                    case "暂停":
                        break;
                    case "结束地标停止":
                        //if (agvCall.lineNum.Count > 0)//有叫料信息，需要车辆运送
                        //{
                        //    if (agvCall.lineNum[0] > 0)
                        //    {
                        //        agvDock.outDockLine[iagvnum - 1] = agvCall.lineNum[0];
                        //        AGVControlCommand(iagvnum, 3, 0, 0);
                        //    }
                        //}
                        break;
                    default:
                        {
                            //将报警记录写入到数据库
                            string txttimer = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            TrafficPara.Open("Insert into T_Ex (CarID,ExTimer,ExType,ExRouteNum,ExMarkNum,ExWorkLine) VALUES (" + iagvnum.ToString() + ",'" + txttimer + "','" + temp1.txtStatus + "'," + temp1.txtLineNum + "," + temp1.txtMarkNum + "," + temp1.txtWorkLine + ")");
                            TrafficPara.Close();
                            break;
                        }
                }
            }    
        }

        /// <summary>
        /// 无线连接属性更改时触发的事件
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void WLChangeAction(AGV_DisMember temp1)
        {
            byte iagvnum = Convert.ToByte(temp1.txtAGVNum.Remove(0, 3));
            AGVStatus[iagvnum - 1].agvAnimation.WLChangeAnimation(temp1.txtWLValue);
            if (temp1.txtWLValue.Equals("失败"))
            {
                memberData.Remove(DisMember[iagvnum - 1]);//20140309从表格中移除无线连接失败的agv
                if (agvCharge != null)
                {
                    agvCharge.Delete(iagvnum);
                }
                if (agvDock != null)
                {
                    agvDock.Delete(iagvnum);
                }
                return;
            }
            else
            {
                memberData.Insert(0, DisMember[iagvnum - 1]);//20140309添加agv到表格中第一行
                MarkChangeAction(temp1);
            }
        }

        /// <summary>
        /// 电量属性更改时触发的事件
        /// 电量低于20%时进行充电
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void PowerChangeAction(AGV_DisMember temp1)
        {
            //20140312电量低于20%时进行充电，线路切换到充电线路
            byte iagvnum = Convert.ToByte(temp1.txtAGVNum.Remove(0, 3));
            if (iagvnum > 0 && iagvnum <= AGVNUM_MAX)
            {
                if (temp1.txtPower < 20)//电量低于20%时进行充电
                {
                    //设置充电标志位
                    AGVStatus[iagvnum - 1].agvCharge = 1;
                }
            }
        }

        #endregion

        #region AGV控制区

        void DrvWlConInit()
        {
            for (int i = 0; i < AGVNUM_MAX; i++)
            {
                AGVStatus[i] = new AGVCar(this,canvas);
            }
            DrvWlConWorkLineInit();  //生产区初始化,需要先初始化AGVStatus,然后再生产区初始化
            if (GlobalPara.IsTrafficFun)
            {
                agvTrafficList = new AGVTrafficList();
                agvTrafficList.Init();//交通管制路口初始化
            }
            if (GlobalPara.IsDockFun)
            {
                agvDock = new AGVDock();
                agvDock.Init();
            }
            if (GlobalPara.IsChargeFun)
            {
                agvCharge = new AGVCharge();
                agvCharge.Init();
            }
            if (GlobalPara.IsCallFun)
            {
                agvCall = new AGVCall();
            }
        }

        /// <summary>
        /// 生产区初始化
        /// </summary>
        void DrvWlConWorkLineInit()
        {
            try
            {
                DataSet dataset1 = TrafficPara.DSet("select * from T_WorkLine order by CarID");
                if (dataset1.Tables[0].Rows.Count < AGVNUM_MAX)
                {
                    MessageBox.Show("AGV车辆未设置生产区号，启动系统前请设置后再重启软件！");
                }
                for (int j = 0; j < dataset1.Tables[0].Rows.Count; j++)
                {
                    int carid = Convert.ToInt32(dataset1.Tables[0].Rows[j]["CarID"]);
                    if (carid > 0 && carid <= AGVStatus.Length)
                    {
                        AGVStatus[carid - 1].worklineNum = Convert.ToByte(dataset1.Tables[0].Rows[j]["WorkLine"]);
                    }
                }
                TrafficPara.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("数据库连接异常,请检查后再重启软件！");
                return;
            }
        }

        byte DrvWlConCheck(byte[] buf, int length)   //判断一帧数据正确性
        {
            if ((buf[0]) != 0x10)
            {
                return VERIFY_HEADERROR;  //头错误
            }
            else if ((buf[buf.Length - 1]) != 0x03)
            {
                return VERIFY_ENDERROR;    //尾错误
            }
            else if ((buf[1]) != 0x62 && (buf[1]) != 0x72)  //功能码错误
            {
                return VERIFY_FUNERROR;
            }
            else if (COM.SerialPortWrapper.GetXORCheckCode(buf, length) != buf[buf.Length - 2])
            {
                return VERIFY_BCCERROR;  // 校验错误 
            }
            else
            {
                return VERIFY_NOERROR;
            }
        }

        bool DrvWlCon_AgvStatus(byte[] buf)  //更新AGV运行参数
        {
            byte AgvNum = 0, SpeedGrade = 0;
            int bMarkNum = 0;
            AgvNum = buf[2];  //得到AGV编号，编号从0x01开始
            if (AgvNum > AGVNUM_MAX || AgvNum < 1)
            {
                return false;//AGV编号错误
            }
            if (DrvWlConCheck(buf, buf.Length - 2) == VERIFY_NOERROR)  //校验无错
            {
                AGVStatus[AgvNum - 1].wlLinkCount = 0;
                //由于生产区在运行过程中不会改变，故将此放置在AGV的初始化中，不需要每次更新
                AGVStatus[AgvNum - 1].agvNum = AgvNum;
                //当前地标号
                //修改时间：2013-11-28
                //修改说明：地标号扩展两位，速度等级的bit8,bit7作为地标号的bit9，bit10
                bMarkNum = buf[3] | ((buf[7] & 0xC0) << 2);
                if (bMarkNum > 0)
                {
                    AGVStatus[AgvNum - 1].markNum = bMarkNum;
                    //当前AGV运行速度等级，不会接收到保持现状的0
                    //修改时间：2013-11-28
                    //修改说明：屏蔽速度等级的bit8,bit7
                    SpeedGrade = Convert.ToByte(buf[7] & 0x3F);
                    if (SpeedGrade > 0 && SpeedGrade < SpeedOpt.Length)
                    {
                        AGVStatus[AgvNum - 1].agvSpeed = SpeedGrade;
                    }
                    //当前地标功能
                    if (buf[4] > 0 && buf[4] < MarkFuncOpt.Length)
                    {
                        AGVStatus[AgvNum - 1].markFunction = buf[4];
                    }
                    //当前AGV运行路线
                    AGVStatus[AgvNum - 1].lineNum = buf[6];
                    //AGV当前剩余电量
                    if (buf[8] > 0 && buf[8] <= 100)
                    {
                        AGVStatus[AgvNum - 1].agvPower = buf[8];
                    }

                }
                //当前AGV状态，由于接收到数据不是按照顺序，中间有间隔，在使用时才验证数据正确性 
                AGVStatus[AgvNum - 1].agvStatus = buf[5];
                //无线连接正常
                AGVStatus[AgvNum - 1].wlLink = AGVWLLINK_OK;
                return true;
            }
            else
            {
                if (AGVStatus[AgvNum - 1].wlLinkCount < 5)
                {
                    AGVStatus[AgvNum - 1].wlLinkCount++;
                }
                else
                {
                    //第一次断线时，连接失败，更新交通管制参数，如果在交通管制区则出交通管制区
                    if (AGVStatus[AgvNum - 1].wlLink == AGVWLLINK_OK)
                    {
                        AGVStatus[AgvNum - 1].wlLink = AGVWLLINK_ERROR;
                        AGVStatus[AgvNum - 1].markNum = 0;
                        AGVStatus[AgvNum - 1].dockNum = 0;
                        AGVStatus[AgvNum - 1].lineNum = 0;
                        if (agvTrafficList != null)
                        {
                            agvTrafficList.DrvWlConUpdateRunPar(ref AGVStatus[AgvNum - 1]);
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        #region 界面消息响应
        /// <summary>
        /// 系统打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OpenSystem_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpenSystem)
            {
                MessageBox.Show("系统已启动!");
                return;
            }
            try
            {
                //交通管制串口
                if (SPComControl == null)
                {
                    ControlCOMInit();
                }
                if (!SPComControl.IsOpen)
                {
                    SPComControl.portname = GlobalPara.Gcontrolcomname;
                    SPComControl.Open();
                    SPComControl.DiscardInBuffer();
                    SPComControl.Write(buf_trafficctl, 0, buf_trafficctl.Length);
                }

                //叫料系统串口
                if (SPComCall == null)
                {
                    CallCOMInit();
                }
                if (!SPComCall.IsOpen)
                {
                    SPComCall.portname = GlobalPara.Gcallcomname;
                    SPComCall.Open();
                    SPComCall.DiscardInBuffer();
                }
            }
            catch (System.Exception ex)
            {
                if (SPComControl != null)
                {
                    SPComControl.Close();
                }
                if (SPComCall != null)
                {
                    SPComCall.Close();
                }
                MessageBox.Show(ex.Message);
                return;
            }

            Array.Clear(buf_virtualtrafficctl, 0, buf_virtualtrafficctl.Length);
            if (!t.IsEnabled)
            {
                t.Start();//开始计时，开始循环
            }
            if (!DataTimer.IsEnabled)
            {
                SPComControl.DiscardInBuffer();
                DataTimer.Start();//数据发送Timers
            }
            if (SPComControl.IsOpen)
            {
                imgCOM.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Open_24.png"));
                lblcomstate.Content = "打开";
                lblcomstate.Foreground = Brushes.Green;
            }
            else
            {
                return;
            }
            IsOpenSystem = true;
            imgSystem.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Open_24.png"));
            lblsystemstate.Content = "打开";
            lblsystemstate.Foreground = Brushes.Green;
            btn_OpenSystem.IsEnabled = false;
            btn_CloseSystem.IsEnabled = true;
        }

        /// <summary>
        /// 系统关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CloseSystem_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOpenSystem)
            {
                MessageBox.Show("系统已关闭!");
                return;
            }
            SPComControl.Close();
            if (t.IsEnabled)
            {
                t.Stop();
            }
            if (DataTimer.IsEnabled)
            {
                DataTimer.Stop();
            }
            if (!SPComControl.IsOpen)
            {
                imgCOM.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Close_24.png"));
                lblcomstate.Content = "关闭";
                lblcomstate.Foreground = Brushes.Red;
            }
            IsOpenSystem = false;
            for (int i = 0; i < AGVNUM_MAX; i++)
            {
                if (AGVStatus[i].wlLink)
                {
                    AGVStatus[i].wlLink = false;
                }
            }
            //定时刷新界面的定时器已经关闭，需要手动刷新
            UpdateCurrentScreen();
            imgSystem.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Close_24.png"));
            lblsystemstate.Content = "关闭";
            lblsystemstate.Foreground = Brushes.Red;
            btn_OpenSystem.IsEnabled = true;
            btn_CloseSystem.IsEnabled = false;
        }

        /// <summary>
        /// 小车控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            int iAgvnum = this.cb_AgvNum.SelectedIndex;
            int iOperation = this.cb_Operation.SelectedIndex;
            int iSpeed = this.cb_Speed.SelectedIndex;
            AGVControlCommand(Convert.ToByte(iAgvnum + 1), Convert.ToByte(iOperation), Convert.ToByte(iSpeed), Convert.ToByte(this.cb_LineNum.Text));
        }

        /// <summary>
        /// AGV控制命令
        /// </summary>
        /// <param name="agvnum">AGV车辆编号</param>
        /// <param name="operation">AGV操作选项</param>
        /// <param name="speed">AGV速度等级</param>
        /// <param name="line">AGV线路</param>
        private void AGVControlCommand(byte agvnum, byte operation, byte speed, byte line)
        {
            byte Operation = 0;
            if (operation > 0 && operation < 8)
            {
                Operation = Convert.ToByte(0x01 << (operation-1));
            }
            else
            {
                Operation = 0x00;
            }
            byte[] temp = { 0x10, 0x71, agvnum, Operation, speed, line, 0x00, 0x00, 0x00, 0x00, 0x03 };
            temp[temp.Length -2] = COM.SerialPortWrapper.GetXORCheckCode(temp, temp.Length - 2);
            buf_runctl.AddRange(temp);
            ctrlWaitNum.Add(0);
        }

        private void cb_AgvNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //修改日期：2014-01-02
            cb_Speed.SelectedIndex = 0;
            cb_Operation.SelectedIndex = 0;
            cb_LineNum.SelectedIndex = 0;
        }
        #endregion

        #region 菜单栏响应消息

        /// <summary>
        /// 菜单栏获得焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            menu.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 菜单栏失去焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            menu.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 由于还能通过键盘组合键的方式来打开菜单，所以还要响应ContextMenuOpening事件，
        /// 不论Menu由于什么原因Opening了，菜单栏都需要为显示状态。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            menu.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 响应键盘Alt键，然后显示出Menu。这里需要用到WPF中的隧道事件(PreviewXXX)，
        /// 从根元素开始响应，这样不论焦点在哪个控件上，都能得到KeyDown事件。例子中是在Window根节点添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                if (menu.Visibility != Visibility.Visible) menu.Visibility = Visibility.Visible;
            }
        }

        private void AGVPara_Click(object sender, RoutedEventArgs e)
        {
            AGVParaSetting apsdialog = new AGVParaSetting();
            apsdialog.Show();
        }

        private void ControlCOM_Click(object sender, RoutedEventArgs e)
        {
            ControlCOMSetting ccsdialog = new ControlCOMSetting();
            ccsdialog.Show();
        }

        private void CallCOM_Click(object sender, RoutedEventArgs e)
        {
            CallCOMSetting csdialog = new CallCOMSetting();
            csdialog.Show();
        }

        private void PassWord_Click(object sender, RoutedEventArgs e)
        {
            PassWordSetting psdialog = new PassWordSetting();
            psdialog.Show();
        }

        private void UserManage_Click(object sender, RoutedEventArgs e)
        {
            UserManage umdialog = new UserManage();
            umdialog.Show();
        }

        private void Mark_Click(object sender, RoutedEventArgs e)
        {
            AGVManage amdialog = new AGVManage("Mark");
            amdialog.Show();
        }

        private void Route_Click(object sender, RoutedEventArgs e)
        {
            AGVManage amdialog = new AGVManage("Route");
            amdialog.Show();
        }

        private void Traffic_Click(object sender, RoutedEventArgs e)
        {
            AGVManage amdialog = new AGVManage("Traffic");
            amdialog.Show();
        }

        private void WorkLine_Click(object sender, RoutedEventArgs e)
        {
            WorkLineManage wmdialog = new WorkLineManage();
            wmdialog.Show();
        }

        private void DockArea_Click(object sender, RoutedEventArgs e)
        {
            DockManage dmdialog = new DockManage();
            dmdialog.Show();
        }

        private void ChargeArea_Click(object sender, RoutedEventArgs e)
        {
            ChargeManage cmdialog = new ChargeManage();
            cmdialog.Show();
        }

        private void Speed_Click(object sender, RoutedEventArgs e)
        {
            SpeedManage smdialog = new SpeedManage();
            smdialog.Show();
        }

        private void CallManage_Click(object sender, RoutedEventArgs e)
        {
            MaterialsSettings msdialog = new MaterialsSettings();
            msdialog.Show();
        }

        private void CallAddressSet_Click(object sender, RoutedEventArgs e)
        {
            CallAddressSet csdialog = new CallAddressSet();
            csdialog.Show();
        }

        private void Custom_Click(object sender, RoutedEventArgs e)
        {
            CustomManage cmdialog = new CustomManage();
            cmdialog.Show();
        }

        private void Exception_Click(object sender, RoutedEventArgs e)
        {
            ExceptionManage emdialog = new ExceptionManage();
            emdialog.Show();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help hdialog = new Help();
            hdialog.Show();
        }

        #endregion

        #region 在画布中的鼠标处理

        bool MouseLeftPress = false;
        Point lastpoint = new Point(0, 0);
        bool FullScreen = false;
        Point OffsetPoint = new Point(0, 0);
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseLeftPress = true;
            lastpoint.X = e.GetPosition(null).X + OffsetPoint.X;
            lastpoint.Y = e.GetPosition(null).Y + OffsetPoint.Y;
            this.Cursor = Cursors.Hand;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseLeftPress)
            {
                Point newpoint = e.GetPosition(null);
                canvas.Offset = new Point(lastpoint.X - newpoint.X, lastpoint.Y - newpoint.Y);
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseLeftPress = false;
            this.Cursor = Cursors.Arrow;
            OffsetPoint = canvas.Offset;
        }

        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeftPress = false;
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FullScreen = !FullScreen;
            if (FullScreen)
            {
                Panel.SetZIndex(canvas, 3);
            }
            else
            {
                Panel.SetZIndex(canvas, 0);
                canvas.Scale = 1;
                canvas.Offset = new Point(0, 0);
            }
        }

        private void mCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point zoomCenter = e.GetPosition(this.canvas);
            canvas.Scale += e.Delta / 1000.0;
            //最大放大十倍
            if (canvas.Scale > 10)
            {
                canvas.Scale = 10;
            }
            //缩小可以小到3倍
            if (canvas.Scale < 0.3)
            {
                canvas.Scale = 0.3;
            }
        }
        #endregion

        #region 加密狗事件插拨通知

        //加密狗变量定义，类中包含了加密狗的读写密码及其他信息
        SoftKey ytsoftkey = new SoftKey();

        /// <summary>
        /// 注册加密锁事件插拨通知
        /// </summary>
        void RegisterDeviceNotification()
        {
            Win32.DEV_BROADCAST_DEVICEINTERFACE dbi = new
                Win32.DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(dbi);
            dbi.dbcc_size = size;
            dbi.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
            dbi.dbcc_reserved = 0;
            dbi.dbcc_classguid = Win32.GUID_DEVINTERFACE_USB_DEVICE;
            dbi.dbcc_name = 0;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(dbi, buffer, true);
            WindowInteropHelper helper = new WindowInteropHelper(this);
            IntPtr ptr = helper.Handle;
            IntPtr r = Win32.RegisterDeviceNotification(ptr, buffer,
                Win32.DEVICE_NOTIFY_WINDOW_HANDLE);
            if (r == IntPtr.Zero)
                MessageBox.Show(Win32.GetLastError().ToString());
        }

        void win_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.win_SourceInitialized(this, e);
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_DEVICECHANGE:
                    OnDeviceChange(msg);
                    break;
            }
            return IntPtr.Zero;
        }

        void OnDeviceChange(int msg)
        {
            int wParam = 32772;
            int lParam = 170714116;
            if ((int)wParam == Win32.DBT_DEVICEARRIVAL)
            {
                int devType = Marshal.ReadInt32((int)lParam, 4);
                if (devType == Win32.DBT_DEVTYP_DEVICEINTERFACE)
                {
                    Win32.DEV_BROADCAST_DEVICEINTERFACE1 DeviceInfo = (Win32.DEV_BROADCAST_DEVICEINTERFACE1)Marshal.PtrToStructure((IntPtr)lParam, typeof(Win32.DEV_BROADCAST_DEVICEINTERFACE1));
                }
            }
            if ((int)wParam == Win32.DBT_DEVICEREMOVECOMPLETE)
            {
                if (ytsoftkey.CheckKeyByEncstring_New() != 0)
                {
                    this._loading.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion
    }
}
