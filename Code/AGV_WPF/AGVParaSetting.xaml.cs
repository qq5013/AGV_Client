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
using System.Windows.Shapes;
using System.Configuration;
using System.Text.RegularExpressions;
using AGV_WPF.DLL;

namespace AGV_WPF
{
    /// <summary>
    /// AGVParaSetting.xaml 的交互逻辑
    /// </summary>
    public partial class AGVParaSetting : Window
    {
        /// <summary>
        /// 初始化时的参数设置字符串
        /// </summary>
        String InitParamString = "";

        /// <summary>
        /// 构造函数，读取配置文件中的系统参数
        /// </summary>
        public AGVParaSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载窗体，初始化textbox中内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbAGVnum.Text = ConfigurationManager.AppSettings["AGVNUM_MAX"];
            tbMapScale.Text = ConfigurationManager.AppSettings["MapScale"];
            cbTrafficFun.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["TRAFFICFUN"]);
            cbDockFun.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["DOCKFUN"]);
            cbChargeFun.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["CHARGEFUN"]);
            cbCallFun.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["CALLFUN"]);
            InitParamString = GetParamString();
            //tbTraffic.Text = ConfigurationManager.AppSettings["TRAFFIC_CONAREA_MAX"];
            //tbMark.Text = ConfigurationManager.AppSettings["TRAFFIC_CONAREA_MARKNUM_MAX"];
            //tbWaiting.Text = ConfigurationManager.AppSettings["TRAFFIC_CONAREA_WAITAGVNUM_MAX"];
            //DAL.ZSql sqlRequirePara = new DAL.ZSql();
            ////修改日期：2014-1-21
            ////总共交通管制数量，与生产区无关，无数据时也会有一行数据，数据为0
            //sqlRequirePara.Open("SELECT COUNT(DISTINCT TrafficNum) AS TrafficCount FROM T_Traffic");
            //if (sqlRequirePara.rowcount > 0)
            //{
            //    tbTraffic.Text = sqlRequirePara.Rows[0]["TrafficCount"].ToString();
            //}
            //else
            //{
            //    tbTraffic.Text = "0";
            //}

            ////修改日期：2014-1-21
            ////交通管制中设置了地标数量最大的一个，无数据时也会有一行数据，其中没有任何数据，也不为空
            //sqlRequirePara.Open("SELECT MAX(TrafficCount) AS MaxTrafficCount FROM (SELECT COUNT(MarkID) AS TrafficCount FROM T_Traffic GROUP BY TrafficNum) AS T_TrafficCount");
            //if (sqlRequirePara.rowcount > 0)
            //{
            //    string oMaxTrafficCount = sqlRequirePara.Rows[0]["MaxTrafficCount"].ToString().Trim();
            //    tbMark.Text = string.IsNullOrEmpty(oMaxTrafficCount) ? "0" : oMaxTrafficCount;
            //}
            //else
            //{
            //    tbMark.Text = "0";
            //}
            //sqlRequirePara.Close();
        }

        /// <summary>
        /// 获取界面参数字符串值
        /// </summary>
        /// <returns></returns>
        private String GetParamString()
        {
            return tbAGVnum.Text + tbMapScale.Text + cbTrafficFun.IsChecked.ToString() + cbDockFun.IsChecked.ToString() 
                + cbChargeFun.IsChecked.ToString() + cbCallFun.IsChecked.ToString();
        }


        #region 确认修改
        /// <summary>
        /// 修改配置参数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(tbAGVnum.Text.Trim()))// || string.IsNullOrEmpty(tbWaiting.Text.Trim()))
                {
                    MessageBox.Show("输入不能为空！");
                    return;
                }
                Regex regex = new Regex(@"^[0-9]*[1-9][0-9]*$");//匹配正整数 
                if (regex.IsMatch(tbAGVnum.Text.Trim()))// && regex.IsMatch(tbWaiting.Text.Trim()))
                {

                    Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    cfa.AppSettings.Settings["AGVNUM_MAX"].Value = tbAGVnum.Text.Trim();
                    cfa.AppSettings.Settings["MapScale"].Value = tbMapScale.Text.Trim();
                    cfa.AppSettings.Settings["TRAFFICFUN"].Value = cbTrafficFun.IsChecked.ToString();
                    cfa.AppSettings.Settings["DOCKFUN"].Value = cbDockFun.IsChecked.ToString();
                    cfa.AppSettings.Settings["CHARGEFUN"].Value = cbChargeFun.IsChecked.ToString();
                    cfa.AppSettings.Settings["CALLFUN"].Value = cbCallFun.IsChecked.ToString();
                    //cfa.AppSettings.Settings["TRAFFIC_CONAREA_MAX"].Value = tbTraffic.Text.Trim();
                    //cfa.AppSettings.Settings["TRAFFIC_CONAREA_MARKNUM_MAX"].Value = tbMark.Text.Trim();
                    //cfa.AppSettings.Settings["TRAFFIC_CONAREA_WAITAGVNUM_MAX"].Value = tbWaiting.Text.Trim();
                    cfa.Save(ConfigurationSaveMode.Modified);
                    if (MessageBox.Show("修改成功！重启软件生效。是否立即重启软件？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        AGVUtils.RestartSystem();
                    }
                }
                else
                {
                    MessageBox.Show("请输入正整数字！");
                }
            }
            catch
            {
                MessageBox.Show("修改失败！");
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(tbAGVnum.Text.Trim()))
            {
                MessageBox.Show("输入不能为空！");
                tbAGVnum.Focus();
                e.Cancel = true;
                return;
            }
            Regex regex = new Regex(@"^[0-9]*[1-9][0-9]*$");//匹配正整数 
            if (!regex.IsMatch(tbAGVnum.Text.Trim()))
            {
                MessageBox.Show("请输入正整数字！");
                tbAGVnum.Focus();
                e.Cancel = true;
                return;
            }
            try
            {
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cfa.AppSettings.Settings["AGVNUM_MAX"].Value = tbAGVnum.Text.Trim();
                cfa.AppSettings.Settings["MapScale"].Value = tbMapScale.Text.Trim();
                cfa.AppSettings.Settings["TRAFFICFUN"].Value = cbTrafficFun.IsChecked.ToString();
                cfa.AppSettings.Settings["DOCKFUN"].Value = cbDockFun.IsChecked.ToString();
                cfa.AppSettings.Settings["CHARGEFUN"].Value = cbChargeFun.IsChecked.ToString();
                cfa.AppSettings.Settings["CALLFUN"].Value = cbCallFun.IsChecked.ToString();
                cfa.Save(ConfigurationSaveMode.Modified);
                if (!GetParamString().Equals(InitParamString))
                {
                    if (MessageBox.Show("修改成功！重启软件生效。是否立即重启软件？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        AGVUtils.RestartSystem();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改失败！原因：" + ex.Message);
                e.Cancel = true;
            }
        }
    }
}
