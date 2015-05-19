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
using System.Data;
using System.Data.SqlClient;

namespace AGV_WPF
{
    /// <summary>
    /// DockManage.xaml 的交互逻辑
    /// </summary>
    public partial class DockManage : Window
    {
        //实际的AGV数量
        public byte AGVNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["AGVNUM_MAX"]);
        public int docknum = 0;
        public DAL.ZSql sql = new DAL.ZSql();

        /// <summary>
        /// 构造函数
        /// </summary>
        public DockManage()
        {
            InitializeComponent();
            InitComboBox();
            InitDockNum();
        }

        /// <summary>
        /// 绑定下拉列表
        /// </summary>
        private void InitComboBox()
        {
            BindWorkLineComboBox();
            BindRouteComboBox();
            BindParkingNumComboBox(docknum);
        }

        /// <summary>
        /// 生产区下拉列表绑定
        /// </summary>
        private void BindWorkLineComboBox()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("Select DISTINCT WorkLine from T_Mark");
            cbSWorkLine.ItemsSource = sql1.m_table.DefaultView;
            cbSWorkLine.DisplayMemberPath = "WorkLine";
            cbSWorkLine.SelectedValuePath = "WorkLine";
            cbEWorkLine.ItemsSource = sql1.m_table.DefaultView;
            cbEWorkLine.DisplayMemberPath = "WorkLine";
            cbEWorkLine.SelectedValuePath = "WorkLine";
            sql1.Close();
        }

        /// <summary>
        /// 路线下拉列表绑定
        /// </summary>
        private void BindRouteComboBox()
        {
            //线路加载初始化
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT LineNum from T_Line order by LineNum");
            cbRouteNum.ItemsSource = sql1.m_table.DefaultView;
            cbRouteNum.DisplayMemberPath = "LineNum";
            cbRouteNum.SelectedValuePath = "LineNum";
            sql1.Close();
        }

        /// <summary>
        /// 停车位下拉列表绑定
        /// </summary>
        private void BindParkingNumComboBox(int docknum)
        {
            //停车位编号加载初始化
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT ParkingNum from T_DockSetting Where DockNum=@docknum order by ParkingNum",
                new SqlParameter[] { new SqlParameter("@docknum", docknum) });
            cbParkingNum.ItemsSource = sql1.m_table.DefaultView;
            cbParkingNum.DisplayMemberPath = "ParkingNum";
            cbParkingNum.SelectedValuePath = "ParkingNum";
            sql1.Close();
        }

        /// <summary>
        /// 停靠区编号绑定
        /// </summary>
        private void InitDockNum()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT DockNum from T_DockArea order by DockNum");
            cbDockNum.ItemsSource = sql1.m_table.DefaultView;
            cbDockNum.DisplayMemberPath = "DockNum";
            cbDockNum.SelectedValuePath = "DockNum";
            sql1.Close();
        }

        /// <summary>
        /// 加载表格
        /// </summary>
        /// <param name="paradocknum">停靠区编号</param>
        private void LoadDataGrid(int paradocknum)
        {
            if (paradocknum > 0)
            {
                DAL.ZSql sql1 = new DAL.ZSql();
                sql1.Open("select * from T_DockSetting where DockNum=" + Convert.ToString(paradocknum) + " order by ParkingNum");
                dataGrid1.ItemsSource = sql1.m_table.DefaultView;
                sql1.Close();
                DAL.ZSql sql2 = new DAL.ZSql();
                sql2.Open("select * from T_DockArea where DockNum=" + Convert.ToString(paradocknum));
                dataGrid2.ItemsSource = sql2.m_table.DefaultView;
                sql2.Close();
                docknum = paradocknum;
            }
            else
            {
                dataGrid1.ItemsSource = null; 
                dataGrid2.ItemsSource = null;
            }
        }

        /// <summary>
        /// 表格选择不同行消息触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView selectItem = dataGrid1.SelectedItem as DataRowView;
            if (selectItem != null)
            {
                cbParkingNum.Text = selectItem["ParkingNum"].ToString().Trim();
                cbRouteNum.Text = selectItem["RouteNum"].ToString().Trim();
            }
            else
            {
                cbParkingNum.Text = "";
                cbRouteNum.Text = "";
            }
        }

        /// <summary>
        /// 停靠区编号选择消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDockNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDockNum.SelectedValue != null)
            {
                docknum = Convert.ToUInt16(cbDockNum.SelectedValue.ToString().Trim());
                LoadDataGrid(docknum);
                BindParkingNumComboBox(docknum);
            }
        }

        /// <summary>
        /// 表格选择不同行消息触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView selectItem = dataGrid2.SelectedItem as DataRowView;
            if (selectItem != null)
            {
                cbSWorkLine.Text = selectItem["DockSSLine"].ToString().Trim();
                cbSMark.Text = selectItem["DockSSNum"].ToString().Trim();
                cbEWorkLine.Text = selectItem["DockESLine"].ToString().Trim();
                cbEMark.Text = selectItem["DockESNum"].ToString().Trim();
            }
            else
            {
                cbSWorkLine.Text = "";
                cbSMark.Text = "";
                cbEWorkLine.Text = "";
                cbEMark.Text = "";
            }
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDockAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!Vertify_Input())
            {
                return;
            }
            //验证是否已存在相同的停靠区
            sql.Open("select * from T_DockArea where DockNum=" + cbDockNum.Text.Trim());
            if (sql.rowcount > 0)
            {
                MessageBox.Show("输入的停靠区编号已存在！");
                cbDockNum.Focus();
                return;
            }
            sql.Open("insert into T_DockArea (DockNum,DockSSNum,DockSSLine,DockESNum,DockESLine) Values (@docknum,@dockssnum,@dockssline,@dockesnum,@dockesline)",
            new SqlParameter[]{
                                                    new SqlParameter("docknum",cbDockNum.Text.Trim()),
                                                    new SqlParameter("dockssnum",cbSMark.Text.Trim()),
                                                    new SqlParameter("dockssline",cbSWorkLine.Text.Trim()),
                                                    new SqlParameter("dockesnum",cbEMark.Text.Trim()),
                                                    new SqlParameter("dockesline",cbEWorkLine.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("添加成功！");
            docknum = Convert.ToInt32(cbDockNum.Text.Trim());
            InitDockNum();
            BindParkingNumComboBox(docknum);
            LoadDataGrid(docknum);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDockModify_Click(object sender, RoutedEventArgs e)
        {
            if (!Vertify_Input())
            {
                return;
            }
            //验证是否已存在相同的停靠区
            sql.Open("select * from T_DockArea where DockNum=" + cbDockNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("输入的停靠区不存在！");
                cbDockNum.Focus();
                return;
            }
            sql.Open("update T_DockArea set DockSSNum=@dockssnum,DockSSLine=@dockssline,DockESNum=@dockesnum,DockESLine=@dockesline where DockNum=@docknum",
            new SqlParameter[]{
                                                    new SqlParameter("@docknum",cbDockNum.Text.Trim()),
                                                    new SqlParameter("@dockssnum",cbSMark.Text.Trim()),
                                                    new SqlParameter("@dockssline",cbSWorkLine.Text.Trim()),
                                                    new SqlParameter("@dockesnum",cbEMark.Text.Trim()),
                                                    new SqlParameter("@dockesline",cbEWorkLine.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("修改成功！");
            docknum = Convert.ToInt32(cbDockNum.Text.Trim());
            LoadDataGrid(docknum);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDockDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbDockNum.Text.Trim()))
            {
                MessageBox.Show("请输入停靠区编号！");
                return;
            }
            sql.Open("select * from T_DockArea where DockNum=" + cbDockNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("您输入的停靠区编号不存在！");
                return;
            }
            sql.Open("delete from T_DockArea where DockNum=" + cbDockNum.Text.Trim());
            sql.Open("delete from T_DockSetting where DockNum=" + cbDockNum.Text.Trim());
            sql.Close();
            MessageBox.Show("删除成功！");
            InitDockNum();
            cbDockNum.SelectedIndex = 0;
            docknum = Convert.ToInt32(cbDockNum.Text.Trim().Equals("") ? "0" : cbDockNum.Text.Trim());
            BindParkingNumComboBox(docknum);
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbDockNum.Text.Trim()) || string.IsNullOrEmpty(cbParkingNum.Text.Trim()) || string.IsNullOrEmpty(cbRouteNum.Text.Trim()))
            {
                MessageBox.Show("请输入完整信息！");
                return;
            }
            sql.Open("select * from T_DockSetting where DockNum=" + cbDockNum.Text.Trim() + " and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount > 0)
            {
                MessageBox.Show("添加失败！您输入的AGV车辆线路设置已存在！");
                return;
            }
            sql.Open("insert into T_DockSetting (DockNum,ParkingNum,RouteNum) Values (@docknum,@agvnum,@routenum)",
            new SqlParameter[]{
                                                    new SqlParameter("@docknum",cbDockNum.Text.Trim()),
                                                    new SqlParameter("@agvnum",cbParkingNum.Text.Trim()),
                                                    new SqlParameter("@routenum",cbRouteNum.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("添加成功！");
            docknum = Convert.ToInt32(cbDockNum.Text.Trim());
            BindParkingNumComboBox(docknum);
            LoadDataGrid(docknum);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingModify_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbDockNum.Text.Trim()) || string.IsNullOrEmpty(cbParkingNum.Text.Trim()) || string.IsNullOrEmpty(cbRouteNum.Text.Trim()))
            {
                MessageBox.Show("请输入完整信息！");
                return;
            }
            sql.Open("select * from T_DockSetting where DockNum=" + cbDockNum.Text.Trim() + " and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("修改失败！您输入的AGV车辆线路设置不存在！");
                return;
            }
            sql.Open("update T_DockSetting set RouteNum=@routenum where DockNum=@docknum and ParkingNum=@agvnum",
            new SqlParameter[]{
                                                    new SqlParameter("@docknum",cbDockNum.Text.Trim()),
                                                    new SqlParameter("@agvnum",cbParkingNum.Text.Trim()),
                                                    new SqlParameter("@routenum",cbRouteNum.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("修改成功！");
            docknum = Convert.ToInt32(cbDockNum.Text.Trim());
            LoadDataGrid(docknum);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbDockNum.Text.Trim()) || string.IsNullOrEmpty(cbParkingNum.Text.Trim()))
            {
                MessageBox.Show("请输入停靠区编号和AGV编号！");
                return;
            }
            sql.Open("select * from T_DockSetting where DockNum=" + cbDockNum.Text.Trim() + " and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("删除失败！您输入的AGV车辆线路设置不存在！");
                return;
            }
            sql.Open("delete from T_DockSetting where DockNum=" + cbDockNum.Text.Trim() + " and ParkingNum=" + cbParkingNum.Text.Trim());
            sql.Close();
            MessageBox.Show("删除成功！");
            docknum = Convert.ToInt32(cbDockNum.Text.Trim());
            BindParkingNumComboBox(docknum);
            LoadDataGrid(docknum);
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        /// <returns>是否满足要求</returns>
        private bool Vertify_Input()
        {
            if (string.IsNullOrEmpty(cbDockNum.Text.Trim()) || string.IsNullOrEmpty(cbSWorkLine.Text.Trim()) ||
                string.IsNullOrEmpty(cbSMark.Text.Trim()) || string.IsNullOrEmpty(cbEWorkLine.Text.Trim()) || string.IsNullOrEmpty(cbEMark.Text.Trim()))
            {
                MessageBox.Show("请输入完整信息！");
                return false;
            }
            sql.Open("select * from T_Mark where WorkLine=" + cbSWorkLine.Text.Trim() + " and Mark=" + cbSMark.Text.Trim() + " and VirtualMark=0");
            if (sql.rowcount < 1)
            {
                MessageBox.Show("输入的起点地标不存在或为虚拟地标！");
                cbSMark.Focus();
                return false;
            }
            sql.Open("select * from T_Mark where WorkLine=" + cbEWorkLine.Text.Trim() + " and Mark=" + cbEMark.Text.Trim() + " and VirtualMark=0");
            if (sql.rowcount < 1)
            {
                MessageBox.Show("输入的终点地标不存在或为虚拟地标！");
                cbEMark.Focus();
                return false;
            }
            return true;
        }
    }
}
