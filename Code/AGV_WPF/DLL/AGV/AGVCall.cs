using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGV_CALL_Info;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace AGV_WPF.DLL.AGV
{
    public class AGVCall
    {
        public ObservableCollection<AGV_CALLMember> memberData = new ObservableCollection<AGV_CALLMember>();
        public List<int> lineNum = new List<int>();
        //private delegate void AddCallMemberEvent(AGV_CALLMember si);
        //private delegate void DeleteCallMemberEvent(AGV_CALLMember si);

        public bool Add(int stationnum, int materialnum)
        {
            bool flag = false;
            if (stationnum > 0 && materialnum > 0)
            {
                DAL.ZSql callSql = new DAL.ZSql();
                bool bFind = memberData.Any<AGV_CALLMember>(p => p.iStationNum == stationnum && p.iMaterialNum == materialnum);
                if (!bFind)
                {
                    AGV_CALLMember DisMember = new AGV_CALLMember();
                    DisMember.iNO = memberData.Count + 1;
                    DisMember.dtTime = new DateTime();
                    DisMember.iStationNum = stationnum;
                    DisMember.iMaterialNum = materialnum;
                    callSql.Open("Select * from T_CallSetting where StationNum=" + stationnum.ToString() + " and MaterialNum=" + materialnum);
                    if (callSql.rowcount > 0)
                    {
                        DisMember.sMaterialName = callSql.Rows[0]["MaterialName"].ToString();
                        DisMember.iLineNum = Convert.ToInt32(callSql.Rows[0]["LineNum"]);
                        lineNum.Add(DisMember.iLineNum);
                        flag = true;
                    }
                    callSql.Close();
                    /*********************************
                     http://blog.csdn.net/luminji/article/details/5353644
                    典型应用场景：WPF页面程序中，ListView的ItemsSource是一个ObservableCollection<StudentInfo>；
                    操作：另起一个线程，为ListView动态更新数据，也就是给ObservableCollection<StudentInfo>添加记录。
                    这类操作，就是跨线程访问线程安全的数据，如果不使用Dispatcher，就会导致出错“该类型的CollectionView
                    不支持从调度程序线程以外的线程对其SourceCollection”。
                    **********************************/
                    //Dispatcher.Invoke(new AddCallMemberEvent(this.AddCallMember), DisMember);
                    AddCallMember(DisMember);
                }
            }
            return flag;
        }

        /// <summary>
        /// 删除
        /// </summary>
        public bool Delete(int stationnum, int materialnum)
        {
            bool flag = false;
            if (stationnum > 0 && materialnum > 0)
            {
                bool bFind = memberData.Any<AGV_CALLMember>(p => p.iStationNum == stationnum && p.iMaterialNum == materialnum);
                if (bFind)
                {
                    IEnumerable<AGV_CALLMember> deletemember1 = memberData.Where<AGV_CALLMember>(p => p.iStationNum == stationnum && p.iMaterialNum == materialnum);
                    AGV_CALLMember member1 = deletemember1.First<AGV_CALLMember>();
                    //Dispatcher.Invoke(new DeleteCallMemberEvent(this.DeleteCallMember), member1);
                    DeleteCallMember(member1);
                    lineNum.Remove(member1.iLineNum);
                    flag = true;
                }
            }
            return flag;
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="si">成员数据</param>
        private void AddCallMember(AGV_CALLMember si)
        {
            memberData.Add(si);
        }

        /// <summary>
        /// 删除成员
        /// </summary>
        /// <param name="si">成员数据</param>
        private void DeleteCallMember(AGV_CALLMember si)
        {
            memberData.Remove(si);
        } 

    }
}
