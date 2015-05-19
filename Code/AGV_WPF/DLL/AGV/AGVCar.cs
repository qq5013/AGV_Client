using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WcfDuplexMessageService;

namespace AGV_WPF.DLL.AGV
{
    /// <summary>
    /// AGV车辆类
    /// </summary>
    public class AGVCar : INotifyPropertyChanged
    {
        #region 成员变量

        /// <summary>
        /// AGV标号，即ID
        /// </summary>
        private byte agvNum;

        public byte AGVNum
        {
            get { return this.agvNum; }
            set
            {
                if (value != this.agvNum)
                {
                    this.agvNum = value;
                    NotifyPropertyChanged("AGVNum");
                }
            }
        }

        /// <summary>
        /// AGV今日行驶总距离，以软件启动开始计算
        /// </summary>
        private float totalDistance;

        public float TotalDistance
        {
            get { return this.totalDistance; }
            set
            {
                if (value != this.totalDistance)
                {
                    this.totalDistance = value;
                    NotifyPropertyChanged("TotalDistance");
                }
            }
        }

        /// <summary>
        /// 开机次数,以无线连接失败1次无关机一次
        /// </summary>
        private int bootCount;

        public int BootCount
        {
            get { return this.bootCount; }
            set
            {
                if (value != this.bootCount)
                {
                    this.bootCount = value;
                    NotifyPropertyChanged("BootCount");
                }
            }
        }

        /// <summary>
        /// AGV当前的生产区编号，即ID
        /// </summary>
        private byte worklineNum;

        public byte WorklineNum
        {
            get { return this.worklineNum; }
            set
            {
                if (value != this.worklineNum)
                {
                    this.worklineNum = value;
                    NotifyPropertyChanged("WorklineNum");
                }
            }
        }

        /// <summary>
        /// AGV当前的线路编号，即ID
        /// </summary>
        private Int16 lineNum;

        public Int16 LineNum
        {
            get { return this.lineNum; }
            set
            {
                if (value != this.lineNum)
                {
                    this.lineNum = value;
                    NotifyPropertyChanged("LineNum");
                }
            }
        }

        /// <summary>
        /// AGV当前地标编号，即ID
        /// </summary>
        private int markNum;

        public int MarkNum
        {
            get { return this.markNum; }
            set
            {
                if (value != this.markNum)
                {
                    this.markNum = value;
                    NotifyPropertyChanged("MarkNum");
                }
            }
        }

        /// <summary>
        /// AGV当前地标功能命令
        /// </summary>
        private byte markFunction;

        public byte MarkFunction
        {
            get { return this.markFunction; }
            set
            {
                if (value != this.markFunction)
                {
                    this.markFunction = value;
                    NotifyPropertyChanged("MarkFunction");
                }
            }
        }

        /// <summary>
        /// 无线连接状态
        /// </summary>
        private bool wlLink;

        public bool WlLink
        {
            get { return this.wlLink; }
            set
            {
                if (value != this.wlLink)
                {
                    this.wlLink = value;
                    NotifyPropertyChanged("WlLink");
                }
            }
        }


        /// <summary>
        /// 无线连接中断次数
        /// </summary>
        private byte wlLinkCount;

        public byte WlLinkCount
        {
            get { return this.wlLinkCount; }
            set
            {
                if (value != this.wlLinkCount)
                {
                    this.wlLinkCount = value;
                    NotifyPropertyChanged("WlLinkCount");
                }
            }
        }

        /// <summary>
        /// AGV当前运行状态
        /// </summary>
        private byte agvStatus;

        public byte AGVStatus
        {
            get { return this.agvStatus; }
            set
            {
                if (value != this.agvStatus)
                {
                    this.agvStatus = value;
                    NotifyPropertyChanged("AGVStatus");
                }
            }
        }

        /// <summary>
        /// AGV当前速度
        /// </summary>
        private byte agvSpeed;

        public byte AGVSpeed
        {
            get { return this.agvSpeed; }
            set
            {
                if (value != this.agvSpeed)
                {
                    this.agvSpeed = value;
                    NotifyPropertyChanged("AGVSpeed");
                }
            }
        }

        /// <summary>
        /// AGV当前剩余电量
        /// </summary>
        private byte agvPower;

        public byte AGVPower
        {
            get { return this.agvPower; }
            set
            {
                if (value != this.agvPower)
                {
                    this.agvPower = value;
                    NotifyPropertyChanged("AGVPower");
                }
            }
        }

        /// <summary>
        /// AGV充电信号标志位
        /// 0：无请求
        /// 1：发出充电请求
        /// 2：正在去充电路线上
        /// 3：正在充电
        /// 4：正在充电返回线路上
        /// </summary>
        private byte agvCharge;

        public byte AGVCharge
        {
            get { return this.agvCharge; }
            set
            {
                if (value != this.agvCharge)
                {
                    this.agvCharge = value;
                    NotifyPropertyChanged("AGVCharge");
                }
            }
        }

        /// <summary>
        /// AGV当前的管制区号
        /// </summary>
        private int trafficNum;

        public int TrafficNum
        {
            get { return this.trafficNum; }
            set
            {
                if (value != this.trafficNum)
                {
                    this.trafficNum = value;
                    NotifyPropertyChanged("TrafficNum");
                }
            }
        }

        /// <summary>
        /// AGV当前的管制状态
        /// </summary>
        private bool trafficStatus;

        public bool TrafficStatus
        {
            get { return this.trafficStatus; }
            set
            {
                if (value != this.trafficStatus)
                {
                    this.trafficStatus = value;
                    NotifyPropertyChanged("TrafficStatus");
                }
            }
        }

        /// <summary>
        /// AGV当前的停靠区号
        /// </summary>
        private int dockNum;

        public int DockNum
        {
            get { return this.dockNum; }
            set
            {
                if (value != this.dockNum)
                {
                    this.dockNum = value;
                    NotifyPropertyChanged("DockNum");
                }
            }
        }

        /// <summary>
        /// AGV的任务状态
        /// 0：空闲等待
        /// 1：正在执行任务
        /// </summary>
        private int agvTask;

        public int AGVTask
        {
            get { return this.agvTask; }
            set
            {
                if (value != this.agvTask)
                {
                    this.agvTask = value;
                    NotifyPropertyChanged("AGVTask");
                }
            }
        }

        /// <summary>
        /// AGV动画
        /// </summary>
        public AGVAnimation agvAnimation;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region 成员函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public AGVCar()
        {
            agvNum = 0;
            totalDistance = 0;
            bootCount = 0;
            worklineNum = 0;
            lineNum = 0;
            markNum = 0;
            markFunction = 0;
            wlLink = false;
            wlLinkCount = 0;
            agvStatus = 0;
            agvSpeed = 0;
            trafficNum = 0;
            trafficStatus = false;
            dockNum = 0;
            agvPower = 0;
            agvCharge = 0;
            agvTask = 0;
            agvAnimation = new AGVAnimation();
        }

        ~AGVCar()
        {
            if (agvAnimation != null)
            {
                agvAnimation = null;
            }
        }

        public AGVCar_WCF Convert2WCF()
        {
            AGVCar_WCF wcf = new AGVCar_WCF(agvNum, totalDistance, bootCount, worklineNum, lineNum, markNum,
                markFunction, wlLink, agvStatus, agvSpeed, agvPower, agvCharge, trafficNum,trafficStatus, dockNum, agvTask);
            return wcf;
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}