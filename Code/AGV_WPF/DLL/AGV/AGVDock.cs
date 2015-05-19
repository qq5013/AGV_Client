using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGV_WPF;

namespace AGV_WPF.DLL.AGV
{
    public class CarLine
    {
        public byte agvNum;
        public int lineNum;
        public int dockNum;
    }
    public class AGVDock
    {
        public int dockNum;
        public string dockName;
        public WorkMarkStr dockStartStop;   //停靠区起点
        public WorkMarkStr dockEndStop;     //停靠区终点 

        /// <summary>
        /// 停靠区总车位数
        /// </summary>
        public int dockCount;

        /// <summary>
        /// 停靠区已经停靠的车辆
        /// </summary>
        public int dockingCount;

        /// <summary>
        /// 停靠区停车位的停靠AGV队列
        /// </summary>
        byte[] dockQueue;

        /// <summary>
        /// 进入停靠区的路线
        /// </summary>
        int[] dockLine;

        /// <summary>
        /// 出停靠区的路线，在调用程序中需要判断自己的位置不为0时才可以启动车辆
        /// </summary>
        public int[] outDockLine;
        
        /// <summary>
        /// 停靠区车位满时，等待停车的车辆排队队列
        /// </summary>
        List<byte> waitingQueue;

        public AGVDock()
        {

        }

        ~AGVDock()
        {
            if (waitingQueue != null)
            {
                waitingQueue.Clear();
                waitingQueue = null;
            }
            dockQueue = null;
            dockLine = null;
            outDockLine = null;
        }

        public void Init()
        {
            //停靠区存在且读取起终点的卡号成功的话再读取线路
            if (DockInit(1))//停靠区的编号
            {
                //只有当线路读取成功时，才在其中赋值了dockCount停靠区停车位总数
                if(LineInit(1))
                {
                    dockQueue = new byte[dockCount];
                    dockingCount = 0;
                    waitingQueue = new List<byte>();
                }
            }
        }

        protected bool DockInit(int docknum)
        {
            bool flag = false;
            DAL.ZSql dockSql = new DAL.ZSql();
            if (docknum > 0)
            {
                dockSql.Open("Select * from T_DockArea where DockNum=" + docknum.ToString());
                if (dockSql.rowcount > 0)
                {
                    dockNum = docknum;
                    dockName = dockSql.Rows[0]["DockName"].ToString();
                    dockStartStop.Num = Convert.ToInt32(dockSql.Rows[0]["DockSSNum"]);
                    dockEndStop.Num = Convert.ToInt32(dockSql.Rows[0]["DockESNum"]);
                    dockStartStop.Line = Convert.ToInt32(dockSql.Rows[0]["DockSSLine"]);
                    dockEndStop.Line = Convert.ToInt32(dockSql.Rows[0]["DockESLine"]);
                    flag = true;
                }
                dockSql.Close();
                dockSql.Dispose();
            }
            return flag;
        }

        protected bool LineInit(int docknum)
        {
            bool flag = false;
            DAL.ZSql lineSql = new DAL.ZSql();
            if (docknum > 0)
            {
                lineSql.Open("Select RouteNum from T_DockSetting where DockNum=" + docknum + "Order by RouteNum");//升序
                if (lineSql.rowcount > 0)
                {
                    dockCount = lineSql.rowcount;//确定停车位总数
                    dockLine = new int[dockCount];//确定停靠区每个车位的线路
                    outDockLine = new int[dockCount];
                    for (int i = 0; i < lineSql.rowcount; i++)
                    {
                        if (!string.IsNullOrEmpty(lineSql.Rows[i]["RouteNum"].ToString()))
                        {
                            dockLine[i] = Convert.ToInt32(lineSql.Rows[i]["RouteNum"]);
                        }
                    }
                    flag = true;
                }
                lineSql.Close();
                lineSql.Dispose();
            }
            return flag;
        }

        public byte GetDockCarNum()
        {
            byte agvnum = 0;
            if (dockingCount > 0)
            {
                for (int i = 0; i < dockCount; i++)
                {
                    if (dockQueue[i] != 0)
                    {
                        agvnum = dockQueue[i];
                        break;
                    }
                }
            }
            return agvnum;
        }

        /// <summary>
        /// 是否进入停靠区
        /// </summary>
        /// <returns>true：进入停靠区；false：未进入停靠区</returns>
        public bool IsIntoDockArea(WorkMarkStr wm)
        {
            return dockStartStop.Equals(wm) ? true : false;
        }

        /// <summary>
        /// 是否进入停靠区
        /// </summary>
        /// <param name="line">生产区号</param>
        /// <param name="num">地标号</param>
        /// <returns>true：进入停靠区；false：未进入停靠区</returns>
        public bool IsIntoDockArea(int line, int num)
        {
            return dockStartStop.Equals(new WorkMarkStr(line, num)) ? true : false;
        }

        /// <summary>
        /// 是否驶出停靠区
        /// </summary>
        /// <param name="line">生产区号</param>
        /// <param name="num">地标号</param>
        /// <returns>true：进入停靠区；false：为进入停靠区</returns>
        public bool IsOutDockArea(int line, int num)
        {
            return dockEndStop.Equals(new WorkMarkStr(line, num)) ? true : false;
        }

        /// <summary>
        /// 是否驶出停靠区
        /// </summary>
        /// <returns>true：进入停靠区；false：为进入停靠区</returns>
        public bool IsOutDockArea(WorkMarkStr wm)
        {
            return dockEndStop.Equals(wm) ? true : false;
        }

        /// <summary>
        /// 更新停靠区号
        /// </summary>
        /// <param name="AGVStatus">AGV车辆类，指针类型</param>
        public void UpdateDock(ref AGV_WPF.DLL.AGV.AGVCar AGVStatus)
        {
            if (!AGVStatus.wlLink) { AGVStatus.dockNum = 0; } //当前AGV不在线
            for (int i = 0; i < dockCount; i++)
            {
                //不在停靠区时判断是否进入停靠区
                if (AGVStatus.dockNum == 0)
                {
                    if (IsIntoDockArea(AGVStatus.worklineNum, AGVStatus.markNum))
                    {
                        AGVStatus.dockNum = dockNum;
                        break;
                    }
                }
                //在停靠区时判断是否出停靠区
                else
                {
                    if (IsOutDockArea(AGVStatus.worklineNum, AGVStatus.markNum))
                    {
                        AGVStatus.dockNum = 0;
                        break;
                    }
                }
            }
        }

        public CarLine Add(byte agvnum)
        {
            CarLine carline = null;
            if(dockingCount < dockCount)
            {
                for (int i = 0; i < dockCount; i++)
                {
                    if (dockQueue[i]==0)
                    {
                        dockQueue[i] = agvnum;
                        carline = new CarLine();
                        carline.agvNum = agvnum;
                        carline.dockNum = i + 1;
                        carline.lineNum = dockLine[i];//可能返回0，在调用此函数返回后需要判断线路是否为0
                        dockingCount++;
                        break;
                    }
                }
            }
            else
            {
                waitingQueue.Add(agvnum);
            }
            return carline;
        }

        public CarLine Delete(byte agvnum)
        {
            CarLine carline = null;
            for (int i = 0; i < dockCount; i++)
            {
                if (dockQueue[i] == agvnum)
                {
                    dockQueue[i] = 0;
                    //outDockLine[i] = 0;此时还没用
                    dockingCount--;
                    if (waitingQueue.Count>0)
                    {
                        //此处需要添加到停靠队列dockQueue中，当检测到已经进入到停靠队列地标时,与充电停靠不一样
                        carline = new CarLine();
                        carline.agvNum = waitingQueue[0];
                        carline.dockNum = i + 1;
                        carline.lineNum = dockLine[i];//可能返回0，在调用此函数返回后需要判断线路是否为0
                        dockQueue[i] = carline.agvNum;
                        waitingQueue.RemoveAt(0);
                        dockingCount++;
                    }
                    break;
                }
            }
            return carline;
        }
    }
}
