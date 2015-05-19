using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL;
using AGV_WPF.DLL.AGV;

namespace AGV_WPF.DLL.AGV
{
    /// <summary>
    /// 管制区地标结构体
    /// </summary>
    struct TrafficMark
    {
        /// <summary>
        /// AGV生产区号
        /// </summary>
        public int workLineNum;

        /// <summary>
        /// 对应生产区号的地标号
        /// </summary>
        public int markNum;
    };

    /// <summary>
    /// 管制区类
    /// </summary>
    class AGVTraffic
    {
        /// <summary>
        /// 管制区编号
        /// </summary>
        public int trafficNum;

        /// <summary>
        /// 管制区名称
        /// </summary>
        public string trafficName;

        /// <summary>
        /// 管制区排队AGV，Index=0处的车辆为占用路口车辆
        /// </summary>
        public List<byte> waitAGVList;

        /// <summary>
        /// 管制区内地标总数
        /// </summary>
        public int trafficMarkCount;

        /// <summary>
        /// 管制区内的地标：生产区号+地标号
        /// </summary>
        public TrafficMark[] trafficMarkArray;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AGVTraffic()
        {
            trafficNum = 0;
            trafficName = "管制路口0";
            waitAGVList = new List<byte>();
            trafficMarkCount = 0;
            //trafficMarkArray = new List<TrafficMark>();
        }
    }

    /// <summary>
    /// AGV管制区总类
    /// </summary>
    class AGVTrafficList
    {
        //AGV运行控制
        public const bool AGVMODERUN = true;
        public const bool AGVMODESTOP = false;

        /// <summary>
        /// 管制区总数
        /// </summary>
        public int trafficCount;

        /// <summary>
        /// AGV管制区列表类
        /// </summary>
        public List<AGVTraffic> agvTrafficList;

        /// <summary>
        /// AGV管制区列表类构造函数
        /// </summary>
        public AGVTrafficList()
        {
            agvTrafficList = new List<AGVTraffic>();
        }

        /// <summary>
        /// 路口初始化,读取数据表T_Traffic
        /// </summary>
        /// <returns>初始化true成功orfalse失败(没有设置管制区)</returns>
        public bool Init()
        {
            DAL.ZSql trafficSql = new DAL.ZSql();
            bool flag = false;
            try
            {
                trafficSql.Open("SELECT MAX(TrafficNum) AS MaxTrafficNum FROM T_Traffic");
                if (trafficSql.rowcount < 1 || string.IsNullOrEmpty(trafficSql.Rows[0]["MaxTrafficNum"].ToString()))
                {
                    trafficSql.Close();
                    return false;
                }
                trafficCount = Convert.ToInt32(trafficSql.Rows[0]["MaxTrafficNum"]);
                if (trafficCount > 0)
                {
                    DAL.ZSql SqlLineNum = new DAL.ZSql();
                    for (int i = 0; i < trafficCount; i++)
                    {
                        agvTrafficList.Add(new AGVTraffic());
                        trafficSql.Open("SELECT COUNT(TrafficNum) AS MarkSum FROM T_Traffic WHERE TrafficNum =" + (i + 1).ToString());//一定会有一行数据，不存在的管制区为iMarkSum=0                    
                        if (string.IsNullOrEmpty(trafficSql.Rows[0]["MarkSum"].ToString()))//存在一行数据，但是字段里面是空的，什么都没有
                        {
                            continue;
                        }
                        agvTrafficList[i].trafficMarkCount = Convert.ToInt32(trafficSql.Rows[0]["MarkSum"]);
                        if (agvTrafficList[i].trafficMarkCount == 0)
                        {
                            continue;
                        }
                        else
                        {
                            agvTrafficList[i].trafficNum = i + 1;
                            agvTrafficList[i].trafficName = "管制路口" + (i + 1).ToString();
                            agvTrafficList[i].trafficMarkArray = new TrafficMark[agvTrafficList[i].trafficMarkCount];
                            SqlLineNum.Open("Select T_Mark.WorkLine,T_Mark.Mark,T_Traffic.MarkID from T_Traffic Left Join T_Mark ON T_Traffic.MarkID = T_Mark.ID Where TrafficNum=" + Convert.ToString(i + 1));
                            for (int j = 0; j < SqlLineNum.Rows.Count; j++)
                            {
                                if (!string.IsNullOrEmpty(SqlLineNum.Rows[j]["WorkLine"].ToString().Trim()) && !string.IsNullOrEmpty(SqlLineNum.Rows[j]["Mark"].ToString().Trim()))
                                {
                                    agvTrafficList[i].trafficMarkArray[j].workLineNum = Convert.ToInt32(SqlLineNum.Rows[j]["WorkLine"]);
                                    agvTrafficList[i].trafficMarkArray[j].markNum = Convert.ToInt32(SqlLineNum.Rows[j]["Mark"]);
                                }
                            }
                        }
                    }
                    flag = true;
                }
            }
            catch (System.Exception ex)
            {
                flag = false;
            }
            trafficSql.Close();
            return flag;
        }

        /// <summary>
        /// 获取所在的管制区号,为0时则不在管制区内
        /// </summary>
        /// <param name="trafficmark">地标结构体</param>
        /// <returns>管制区号</returns>
        public int GetTrafficNum(TrafficMark trafficmark)
        {
            int traffic_mark = 0;
            int agv_mark = (trafficmark.workLineNum << 12) | trafficmark.markNum; //得到AGV地标与流水线对应的地标总和
            for (int i = 0; i < trafficCount; i++)   //检测到最大路口数
            {
                for (int j = 0; j < agvTrafficList[i].trafficMarkCount; j++)
                {
                    traffic_mark = (agvTrafficList[i].trafficMarkArray[j].workLineNum << 12) | agvTrafficList[i].trafficMarkArray[j].markNum;   //得到路口地标与流水线对应的地标总和
                    if (traffic_mark == agv_mark) //如果AGV当前地标和路口的地标相同
                    {
                        return agvTrafficList[i].trafficNum;  //返回路口值
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取所在的管制区号,为0时则不在管制区内
        /// </summary>
        /// <param name="workLineNum">生产区号</param>
        /// <param name="markNum">地标号</param>
        /// <returns>管制区号</returns>
        public int GetTrafficNum(int workLineNum, int markNum)
        {
            int traffic_mark = 0;
            int agv_mark = (workLineNum << 12) | markNum; //得到AGV地标与流水线对应的地标总和
            for (int i = 0; i < trafficCount; i++)   //检测到最大路口数
            {
                for (int j = 0; j < agvTrafficList[i].trafficMarkCount; j++)
                {
                    traffic_mark = (agvTrafficList[i].trafficMarkArray[j].workLineNum << 12) | agvTrafficList[i].trafficMarkArray[j].markNum;   //得到路口地标与流水线对应的地标总和
                    if (traffic_mark == agv_mark) //如果AGV当前地标和路口的地标相同
                    {
                        return agvTrafficList[i].trafficNum;  //返回路口值
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 单台AGV更新运行参数，使用了管制区号TrafficNum和无线状态WlLink，需要更新管制区号
        /// |  0:正在占用路口的AGV编号 | 1:等待排队AGV的编号，优先级-1 | 2:等待排队AGV的编号，优先级-2 |.......
        /// </summary>
        /// <param name="AGVStatus">AGV运行状态结构体，指针类型</param>
        /// <returns>返回AGV运行or停止</returns>
        public bool DrvWlConUpdateRunPar(ref AGV_WPF.DLL.AGV.AGVCar AGVStatus)
        {
            int trafficnum = 0;
            int lasttrafficnum = AGVStatus.trafficNum;
            byte agvnum = AGVStatus.agvNum;
            if (AGVStatus.wlLink)//当前AGV在线
            {
                //trafficnum为当前AGV所在交通管制区编号
                trafficnum = GetTrafficNum(AGVStatus.worklineNum, AGVStatus.markNum);
            }
            else//当前AGV不在线，直接返回不在管制区，假如上一次在管制区的话，直接清除上次的记录
            {
                //如果无线连接断开，清除该小车所在管制区的数据
                trafficnum = 0;
            }
            if (trafficnum == 0)  //不是在管制区
            {
                if (lasttrafficnum == 0) //上一次地标不在管制区
                {
                    return AGVMODERUN;
                }
                else  //上一次小车是在管制区中，现在是清除管制状态，清除该路口该车辆排队信息
                {
                    //移除排队队列中
                    agvTrafficList[lasttrafficnum - 1].waitAGVList.Remove(agvnum);
                    AGVStatus.trafficNum = 0;
                    return AGVMODERUN;
                }
            }
            else  //是在管制区
            {
                if ((lasttrafficnum != 0) && (trafficnum != lasttrafficnum)) //与上一次的管制区不同，更新上一路口参数
                {
                    //移除在上一管制区中的排队信息
                    agvTrafficList[lasttrafficnum - 1].waitAGVList.Remove(agvnum);
                }
                if (lasttrafficnum != trafficnum)//上次的管制区号与这次的管制区号不同
                {
                    //加入排队队列中
                    agvTrafficList[trafficnum - 1].waitAGVList.Add(agvnum);
                }
                AGVStatus.trafficNum = trafficnum;//更新AGV状态中现在所在管制区号
                if (agvTrafficList[trafficnum - 1].waitAGVList[0] == agvnum)//在第一的排队位置
                {
                    return AGVMODERUN; //启动
                }
                else//非第一的排队位置，即等待队列
                {
                    return AGVMODESTOP;  //停止，排队等待
                }
            }
        }


    }
}
