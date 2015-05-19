using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGV_WPF.DLL.AGV
{
   public struct AGVNumLine
    {
       public byte agvNum;
       public byte lineNum;
       public AGVNumLine(byte agvnum, byte linenum)
        {
            this.agvNum = agvnum;
            this.lineNum = linenum;
        }
    }
    class AGVCharge
    {
        /// <summary>
        /// 充电区编号
        /// </summary>
        public int dockNum;

        /// <summary>
        /// 充电区名称
        /// </summary>
        public string dockName;

        /// <summary>
        /// 充电区起点
        /// </summary>
        public WorkMarkStr dockStartStop;

        /// <summary>
        /// 充电区终点
        /// </summary>
        public WorkMarkStr dockEndStop;

        /// <summary>
        /// 充电区总车位数
        /// </summary>
        public int dockCount;

        /// <summary>
        /// 充电区已经停发的车辆
        /// </summary>
        public int dockingCount;

        /// <summary>
        /// 充电区停车位的停靠AGV队列
        /// </summary>
        byte[] dockQueue;

        /// <summary>
        /// 进入充电区的路线
        /// </summary>
        int[] dockLine;

        /// <summary>
        /// 进入充电区之前的路线
        /// </summary>
        public byte[] beforeEnterLine;

        /// <summary>
        /// 充电区车位满时，等待停车的车辆排队队列
        /// 队列记录agv的编号和路线
        /// </summary>
        List<AGVNumLine> waitingQueue;

        /// <summary>
        /// 充电线路，进出都是在一条线路上
        /// </summary>
        //public int chargeLine;

        public AGVCharge()
        {
        }

        ~AGVCharge()
        {
            if (waitingQueue != null)
            {
                waitingQueue.Clear();
                waitingQueue = null;
            }
            dockQueue = null;
            dockLine = null;
        }

        public void Init()
        {
            //停靠区存在且读取起终点的卡号成功的话再读取线路
            if (DockInit(1))//停靠区的编号
            {
                //只有当线路读取成功时，才在其中赋值了dockCount停靠区停车位总数
                if (LineInit(1))
                {
                    dockQueue = new byte[dockCount];
                    beforeEnterLine = new byte[dockCount];
                    dockingCount = 0;
                    waitingQueue = new List<AGVNumLine>();
                }
            }
        }

        protected bool DockInit(int docknum)
        {
            bool flag = false;
            DAL.ZSql dockSql = new DAL.ZSql();
            if (docknum > 0)
            {
                dockSql.Open("Select * from T_ChargeArea where ChargeNum=" + docknum.ToString());
                if (dockSql.rowcount > 0)
                {
                    dockNum = docknum;
                    dockName = dockSql.Rows[0]["ChargeName"].ToString();
                    //chargeLine = Convert.ToInt32(dockSql.Rows[0]["ChargeLine"]);
                    dockStartStop.Num = Convert.ToInt32(dockSql.Rows[0]["ChargeSSNum"]);
                    dockEndStop.Num = Convert.ToInt32(dockSql.Rows[0]["ChargeESNum"]);
                    dockStartStop.Line = Convert.ToInt32(dockSql.Rows[0]["ChargeSSLine"]);
                    dockEndStop.Line = Convert.ToInt32(dockSql.Rows[0]["ChargeESLine"]);
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
                lineSql.Open("Select RouteNum from T_ChargeSetting where ChargeNum=" + docknum + "Order by RouteNum");//升序
                if (lineSql.rowcount > 0)
                {
                    dockCount = lineSql.rowcount;//确定停车位总数
                    dockLine = new int[dockCount];//确定停靠区每个车位的线路
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

        /// <summary>
        /// 获取充电路线
        /// </summary>
        /// <param name="oldline">充电前的路线</param>
        /// <returns>充电路线</returns>
        public byte GetChargeLine(byte oldline)
        {
            byte retLine = 0;
            if (oldline > 0)
            {
                DAL.ZSql chargelineSql = new DAL.ZSql();
                chargelineSql.Open("Select ChargeLine from T_LineSet Where LineNum = " + oldline.ToString());
                if (chargelineSql.rowcount > 0)
                {
                    retLine = Convert.ToByte(chargelineSql.Rows[0]["ChargeLine"]);
                }
            }
            return retLine;
        }

        public CarLine Add(byte agvnum, byte line)
        {
            CarLine carline = null;
            if (dockingCount < dockCount)
            {
                for (int i = 0; i < dockCount; i++)
                {
                    if (dockQueue[i] != 0)
                    {
                        dockQueue[i] = agvnum;
                        beforeEnterLine[i] = line;
                        carline = new CarLine();
                        carline.agvNum = agvnum;
                        carline.dockNum = i + 1;
                        carline.lineNum = dockLine[carline.dockNum - 1];//可能返回0，在调用此函数返回后需要判断线路是否为0
                        dockingCount++;
                        break;
                    }
                }
            }
            else
            {
                waitingQueue.Add(new AGVNumLine(agvnum,line));
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
                    beforeEnterLine[i] = 0;
                    dockingCount--;
                    if (waitingQueue.Count > 0)
                    {
                        //此处不需要添加到停靠队列dockQueue中，当检测到进入停靠队列地标时，会在add中加入，与待装停靠不一样
                        carline = new CarLine();
                        carline.agvNum = waitingQueue[0].agvNum;
                        carline.dockNum = i + 1;
                        carline.lineNum = dockLine[i];
                        beforeEnterLine[i] = waitingQueue[0].lineNum;
                        waitingQueue.RemoveAt(0);
                    }
                    break;
                }
            }
            return carline;
        }
    }
}
