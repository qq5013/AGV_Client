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
using AGV_WPF.DLL.AGV;
using AGV_WPF.DLL;
using System.IO;
using System.ServiceModel;
using WcfDuplexMessageClient;
using WcfDuplexMessageService;
using System.Reflection;

namespace AGV_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    #region AGV结构体的定义

    public struct SpeedStr
    {
        public int CmdNum;
        public double Speed;
        public string SpeedGrade;
        public SpeedStr(int num,double speed,string grade)
        {
            this.CmdNum = num;
            this.Speed = speed;
            this.SpeedGrade = grade;
        }
    }


    #endregion

    public partial class MainWindow : Window
    {
        #region 类变量

        #region AGV信息定义
        public static string[] MarkFuncOpt;
        public static string[] StatusOpt;
        public static SortedList<int, SpeedStr> SpeedOpt = null;
        public Brush[] ColorOpt = { Brushes.Yellow, Brushes.Tomato, Brushes.Purple, Brushes.Pink, Brushes.SkyBlue, Brushes.LightBlue, Brushes.GreenYellow, Brushes.Goldenrod };

        #endregion

        #region 动画定义
        //定时器定时刷新界面
        DispatcherTimer pageShift = new DispatcherTimer();
        public ObservableCollection<AGVCar> AGVStatus = new ObservableCollection<AGVCar>();
        #endregion

        #region 系统定义
        private Client client = null;
        private IMessageService svc = null;
        private bool IsOpenSystem = false;
        #endregion
        #endregion

        #region 窗体创建与销毁

        public MainWindow()
        {
            NameScope.SetNameScope(this, new NameScope());
            InitializeComponent();
            try
            {
                ReadConfigFile();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReadConfigFile()
        {
            try
            {
                //GlobalPara.Gcontrolcomname = ConfigurationManager.AppSettings["ControlCOMName"];
                //GlobalPara.Gcallcomname = ConfigurationManager.AppSettings["CallCOMName"];
                GlobalPara.IsTrafficFun = Convert.ToBoolean(ConfigurationManager.AppSettings["TRAFFICFUN"]);
                GlobalPara.IsDockFun = Convert.ToBoolean(ConfigurationManager.AppSettings["DOCKFUN"]);
                //GlobalPara.IsChargeFun = Convert.ToBoolean(ConfigurationManager.AppSettings["CHARGEFUN"]);
                //GlobalPara.IsCallFun = Convert.ToBoolean(ConfigurationManager.AppSettings["CALLFUN"]);
                GlobalPara.PageShiftInterval = Convert.ToByte(ConfigurationManager.AppSettings["PAGESHIFTINTERVAL"]);
                GlobalPara.MapWidth = Convert.ToInt32(ConfigurationManager.AppSettings["MAPWIDTH"]);
                GlobalPara.MapHeight = Convert.ToInt32(ConfigurationManager.AppSettings["MAPHEIGHT"]);
            }
            catch
            {
                throw new Exception("读取配置文件失败！！！");
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuBarInit();//菜单栏初始化
                StatusBarInit();//状态栏初始化
                MapInit();//地图初始化
                AGVGridInit();//界面表格初始化
                CustomInit();//自定义参数获取初始化
                TimerInit();//定时器初始化
                ClientInit();//客户端初始化
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "请配置好再启动系统！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClientInit()
        {
            client = new Client();
            client.PropChangedMessageRecevied += client_PropChangedMessageRecevied;
            client.CarMessageRecevied += client_CarMessageRecevied;
            client.SystemMessageReceived += client_SystemMessageReceived;
        }

        void client_SystemMessageReceived(object sender, SystemMessageEventArgs e)
        {
            btn_CloseSystem_Click(null,null);
            MessageBox.Show("系统自动关闭！原因：" + e.SystemMessage,"警告",MessageBoxButton.OK,MessageBoxImage.Warning);
        }

        void client_CarMessageRecevied(object sender, CarMessageEventArgs e)
        {
            IEnumerable<AGVCar> addmember = AGVStatus.Where<AGVCar>(p => p.AGVNum == e.CarMessage.AGVNum);
            if (addmember == null || addmember.Count() == 0)
            {
                AGVStatus.Add(new AGVCar());
                int index = AGVStatus.Count - 1;
                AGVStatus[index].WorklineNum = e.CarMessage.WorklineNum;
                AGVStatus[index].DockNum = e.CarMessage.DockNum;
                AGVStatus[index].AGVNum = e.CarMessage.AGVNum;
                AGVStatus[index].TrafficNum = e.CarMessage.TrafficNum;
                AGVStatus[index].TrafficStatus = e.CarMessage.TrafficStatus;
                AGVStatus[index].LineNum = e.CarMessage.LineNum;
                AGVStatus[index].MarkFunction = e.CarMessage.MarkFunction;
                AGVStatus[index].AGVSpeed = e.CarMessage.AGVSpeed;
                AGVStatus[index].MarkNum = e.CarMessage.MarkNum;
                AGVStatus[index].AGVPower = e.CarMessage.AGVPower;
                AGVStatus[index].AGVCharge = e.CarMessage.AGVCharge;
                AGVStatus[index].WlLink = e.CarMessage.WlLink;
                AGVStatus[index].AGVStatus = e.CarMessage.AGVStatus;

                AGVStatus[index].TotalDistance = e.CarMessage.TotalDistance;
                AGVStatus[index].BootCount = e.CarMessage.BootCount;
                AGVStatus[index].AGVTask = e.CarMessage.AGVTask;
            }
        }

        void client_PropChangedMessageRecevied(object sender, PropertyChangedMessageEventArgs e)
        {
            IEnumerable<AGVCar> modifymember = AGVStatus.Where<AGVCar>(p => p.AGVNum == e.PropertyMessage.AGVID);
            if (modifymember == null || modifymember.Count() == 0)
            {
                return;
            }
            else
            {
                AGVCar member1 = modifymember.First<AGVCar>();
                int index = AGVStatus.IndexOf(member1);
                PropertyInfo info = typeof(AGVCar).GetProperty(e.PropertyMessage.PropertyID);
                if (info != null)
                {
                    info.SetValue(AGVStatus[index], e.PropertyMessage.PropertyValue, null);
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Event arguments</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (null != this.pageShift)
            {
                this.pageShift.Stop();
                this.pageShift.Tick -= this.PageShift_Tick;
            }
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["PAGESHIFTINTERVAL"].Value = GlobalPara.PageShiftInterval.ToString();
            cfa.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            if (svc!=null)
            {
                try
                {
                    svc.UnregisterClient();
                }
                catch (Exception)
                {

                }
            }
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
                MenuSystemManage.Visibility = Visibility.Collapsed;
            }
            if (!GlobalPara.IsDockFun)
            {
                DockArea.Visibility = Visibility.Collapsed;
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
            pageShift.Interval = TimeSpan.FromSeconds(GlobalPara.PageShiftInterval);
            pageShift.Tick += new EventHandler(PageShift_Tick);
        }

        /// <summary>
        /// 自定义初始化
        /// </summary>
        private void CustomInit()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            try
            {
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
                //速度定义
                sql1.Open("select SpeedGrade,Speed,CmdNum from T_Speed order by CmdNum");
                if (sql1.rowcount > 0)
                {
                    SpeedOpt = new SortedList<int, SpeedStr>();
                    for (int i = 0; i < sql1.rowcount; i++)
                    {
                        string speedgrade = sql1.Rows[i]["SpeedGrade"].ToString();
                        int cmdnum;
                        Int32.TryParse(sql1.Rows[i]["CmdNum"].ToString(),out cmdnum);
                        double speed;
                        Double.TryParse(sql1.Rows[i]["Speed"].ToString(),out speed);
                        SpeedOpt.Add(cmdnum,new SpeedStr(cmdnum,speed,speedgrade));
                    }
                }
                else
                {
                    MessageBox.Show("请在“速度设置”中设置AGV速度信息！","错误",MessageBoxButton.OK,MessageBoxImage.Error);
                }

            }
            catch (Exception ex)
            {              
                throw new Exception("初始化定义读取数据库失败！"+ ex.Message);
            }
            finally 
            {
                sql1.Close();
                sql1 = null;
            }
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
        /// 加载电子地图背景图片
        /// </summary>
        public void MapInit()
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = GlobalPara.mapImage;
            imageBrush.Stretch = Stretch.Uniform;
            imageBrush.AlignmentX = AlignmentX.Left;
            imageBrush.AlignmentY = AlignmentY.Top;
            canvas.Background = imageBrush;
            canvas.Height = GlobalPara.MapHeight;
            canvas.Width = GlobalPara.MapWidth;
            background.Source = AGVUtils.GetImageFromFile(@"Image\mapbackground.png");
            Canvas.SetLeft(marquee, canvas.ActualWidth / 2 - marquee.ActualWidth / 2);
        }



        /// <summary>
        /// agv表格绑定数据初始化
        /// </summary>
        /// <param name="agvnum"></param>
        public void AGVGridInit()
        {
            AGVAnimation.CarFElement = this;
            AGVAnimation.CarCanvas = canvas;
            AGVStatus.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(memberData_CollectionChanged);
            dataGrid.DataContext = AGVStatus;
            tbpageinterval.Text = GlobalPara.PageShiftInterval.ToString();
        }

        void memberData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (AGVCar obj in e.NewItems)
                {
                    obj.PropertyChanged += Dis_PropertyChange;                    
                }
            }
        }
        

        #endregion

        #region 事件委托触发更新

        public void PageShift_Tick(object sender, EventArgs e)
        {
            if (Panel.GetZIndex(canvas) > 0)
            {
                Panel.SetZIndex(canvas, 0);
                Panel.SetZIndex(dataGrid, 3);
            }
            else
            {
                Panel.SetZIndex(canvas, 3);
                Panel.SetZIndex(dataGrid, 0);
            }

        }

        /// <summary>
        /// 界面显示类AGV_DisMember属性更改委托函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Dis_PropertyChange(object sender, PropertyChangedEventArgs e)
        {
            AGVCar agvmember = (AGVCar)sender;
            if (e.PropertyName.Equals("MarkNum"))
            {
                MarkChangeAction(agvmember);
            }
            else if (e.PropertyName.Equals("AGVStatus"))
            {
                StatusChangeAction(agvmember);
            }
            else if (e.PropertyName.Equals("WlLink"))
            {
                WLChangeAction(agvmember);
            }
        }

                /// <summary>
        /// 地标属性更改时触发的事件
        /// 移除上次动画，显示下一动画
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void MarkChangeAction(AGVCar temp1)
        {
            IEnumerable<AGVCar> member = AGVStatus.Where<AGVCar>(p => p.AGVNum == temp1.AGVNum);
            AGVCar member1 = member.First<AGVCar>();
            int listindex = AGVStatus.IndexOf(member1);
            Point pStart = new Point(), pVirtual = new Point();
            DAL.ZSql TrafficPara = new DAL.ZSql();
            try
            {
                TrafficPara.Open("select T_Line.MarkOrder, T_Mark.XPos, T_Mark.YPos FROM T_Line LEFT OUTER JOIN T_Mark ON T_Mark.ID = T_Line.MarkID where WorkLine=" + temp1.WorklineNum.ToString() + " and Mark=" + temp1.MarkNum.ToString() + " and LineNum=" + temp1.LineNum.ToString());
                if (TrafficPara.Rows.Count > 0)
                {
                    pStart.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                    pStart.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                    List<Point> pointcollection1 = new List<Point>();
                    pointcollection1.Add(pStart);
                    int currentOrder = Convert.ToInt16(TrafficPara.Rows[0]["MarkOrder"]) + 1;
                    bool isvirtualpoint = true;
                    double dMarkdistance = 0;
                    do
                    {
                        TrafficPara.Open("select XPos,YPos,T_Line.Distance,VirtualMark from T_Mark Left join T_Line on T_Mark.ID = T_Line.MarkID where T_Line.MarkOrder=" + currentOrder.ToString() +
                        "and LineNum=" + temp1.LineNum.ToString());
                        if (TrafficPara.Rows.Count > 0)
                        {
                            pVirtual.X = Convert.ToDouble(TrafficPara.Rows[0]["XPos"]);
                            pVirtual.Y = Convert.ToDouble(TrafficPara.Rows[0]["YPos"]);
                            pointcollection1.Add(pVirtual);
                            dMarkdistance += Convert.ToDouble(TrafficPara.Rows[0]["Distance"]);
                            isvirtualpoint = Convert.ToBoolean(TrafficPara.Rows[0]["VirtualMark"]);
                            currentOrder++;
                        }
                        else
                        {
                            isvirtualpoint = false;
                            break;
                        }
                    }
                    while (isvirtualpoint);
                    if (pointcollection1.Count >= 2)
                    {
                        double dAgvspeed = SpeedOpt[temp1.AGVSpeed].Speed / 60.0;
                        if (dAgvspeed == 0)
                        {
                            dAgvspeed = dMarkdistance;
                        }
                        App.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            AGVStatus[listindex].agvAnimation.DrawCarLine(pointcollection1, ColorOpt[temp1.AGVNum % ColorOpt.Length], dMarkdistance / dAgvspeed);
                        }));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                TrafficPara.Close();
            }
        }

        /// <summary>
        /// 运行状态属性更改时触发的事件
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void StatusChangeAction(AGVCar temp1)
        {
            IEnumerable<AGVCar> member = AGVStatus.Where<AGVCar>(p => p.AGVNum == temp1.AGVNum);
            AGVCar member1 = member.First<AGVCar>();
            int listindex = AGVStatus.IndexOf(member1);
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                AGVStatus[listindex].agvAnimation.StatusChangeAnimation(temp1.AGVStatus, temp1.AGVNum, temp1.DockNum);
            }));
        }

        /// <summary>
        /// 无线连接属性更改时触发的事件
        /// </summary>
        /// <param name="temp1">AGV_DisMember类</param>
        private void WLChangeAction(AGVCar temp1)
        {
            IEnumerable<AGVCar> member = AGVStatus.Where<AGVCar>(p => p.AGVNum == temp1.AGVNum);
            AGVCar member1 = member.First<AGVCar>();
            int listindex = AGVStatus.IndexOf(member1);
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                AGVStatus[listindex].agvAnimation.WLChangeAnimation(temp1.WlLink);
            }));
            if (temp1.WlLink)
            {
                MarkChangeAction(temp1);
            }

            else
            {
                AGVStatus[listindex].agvAnimation.ClearAllElements();
                AGVStatus[listindex].agvAnimation = null;
                AGVStatus.Remove(temp1);//20140309从表格中移除无线连接失败的agv
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
                svc = DuplexChannelFactory<IMessageService>.CreateChannel(client, "NetTcpBinding_IMessageService");
                svc.RegisterClient();
                if (!pageShift.IsEnabled && cbautoshift.IsChecked.Value)
                {
                    pageShift.Start();//开始计时，开始循环
                }
                IsOpenSystem = true;
                btn_OpenSystem.IsEnabled = false;
                btn_CloseSystem.IsEnabled = true;
                lblservicestate.Content = "打开";
                lblservicestate.Foreground = Brushes.Green;
                imgSystem.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Open_24.png"));
                lblsystemstate.Content = "打开";
                lblsystemstate.Foreground = Brushes.Green;
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show("启动系统失败！原因：注册的服务找不到或无法连接，请确认后再连接！","错误",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册服务失败！原因：" + ex.Message,"错误",MessageBoxButton.OK,MessageBoxImage.Error);
            }
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

            try
            {
                if (pageShift.IsEnabled)
                {
                    pageShift.Stop();
                }
                //删除所有动态数据集中AGV车辆
                while (AGVStatus.Count > 0)
                {
                    AGVStatus[0].WlLink = false;
                }
                IsOpenSystem = false;
                btn_OpenSystem.IsEnabled = true;
                btn_CloseSystem.IsEnabled = false;
                imgSystem.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Light_Close_24.png"));
                lblsystemstate.Content = "关闭";
                lblsystemstate.Foreground = Brushes.Red;
                lblservicestate.Content = "关闭";
                lblservicestate.Foreground = Brushes.Red;
                svc.UnregisterClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("关闭系统出现问题！原因：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        }

        /// <summary>
        /// AGV编号ComboBox选择更改触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_AgvNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //修改日期：2014-01-02
            cb_Speed.SelectedIndex = 0;
            cb_Operation.SelectedIndex = 0;
            cb_LineNum.SelectedIndex = 0;
        }

        /// <summary>
        /// 界面自动切换CheckBox勾选触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbautoshift_Checked(object sender, RoutedEventArgs e)
        {
            if (tbpageinterval != null)
            {
                tbpageinterval.IsEnabled = true;
                if (IsOpenSystem)
                {
                    pageShift.Start();
                }
            }
        }

        /// <summary>
        /// 界面自动切换CheckBox不选触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbautoshift_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tbpageinterval != null)
            {
                tbpageinterval.IsEnabled = false;
                pageShift.Stop();
            }
        }

        /// <summary>
        /// 界面切换时间间隔文本框粘帖触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbpageinterval_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!AGVUtils.IsNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }

        /// <summary>
        /// 界面切换时间间隔文本框按键触发事件
        /// 在文本框里按下一个键后会先确发PreviewKeyDown再确发KeyPress事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbpageinterval_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        /// <summary>
        /// 界面切换时间间隔文本框输入触发事件
        /// 以与设备无关的方式侦听文本输入，如果输入的是英文字符，那么执行e.Handled = true之后，TextBox中确实没有字符出现。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbpageinterval_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!AGVUtils.IsNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        /// <summary>
        /// 界面切换时间间隔文本框失去焦点时触发事件；
        /// 失去焦点后文本框内容作为修改的时间间隔，并立即执行。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbpageinterval_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                int temp;
                temp = Int32.Parse(tbpageinterval.Text);
                if (GlobalPara.PageShiftInterval != temp)
                {
                    GlobalPara.PageShiftInterval = temp;
                    pageShift.Interval = TimeSpan.FromSeconds(GlobalPara.PageShiftInterval);
                    if (IsOpenSystem)
                    {
                        pageShift.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void expanderMain_MouseEnter(object sender, MouseEventArgs e)
        {
            expanderMain.IsExpanded = true;
        }

        private void expanderMain_MouseLeave(object sender, MouseEventArgs e)
        {
            expanderMain.IsExpanded = false;
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


        private void Exception_Click(object sender, RoutedEventArgs e)
        {
            ExceptionManage emdialog = new ExceptionManage();
            emdialog.Show();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            string filename = System.Environment.CurrentDirectory + "\\ReadMe.pdf";
            if (File.Exists(filename))
            {
                System.Diagnostics.Process.Start(filename);
            }
            else
            {
                MessageBox.Show("请联系管理员！","提示",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
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
                //Panel.SetZIndex(canvas, 0);
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
    }
}
