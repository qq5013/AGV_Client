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
    public partial class ChargeManage : Window
    {
        //实际的AGV数量
        public byte AGVNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["AGVNUM_MAX"]);
        public DAL.ZSql sql = new DAL.ZSql();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChargeManage()
        {
            InitializeComponent();
            InitComboBox();
            LoadDataGrid(1);
        }

        /// <summary>
        /// 绑定下拉列表
        /// </summary>
        private void InitComboBox()
        {
            BindWorkLineComboBox();
            BindRouteComboBox();
            BindParkingNumComboBox(1);
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
            cbLineSetRouteNum.ItemsSource = sql1.m_table.DefaultView;
            cbLineSetRouteNum.DisplayMemberPath = "LineNum";
            cbLineSetRouteNum.SelectedValuePath = "LineNum";
            cbChargeLine.ItemsSource = sql1.m_table.DefaultView;
            cbChargeLine.DisplayMemberPath = "LineNum";
            cbChargeLine.SelectedValuePath = "LineNum";
            sql1.Close();
        }

        /// <summary>
        /// 停车位下拉列表绑定
        /// </summary>
        private void BindParkingNumComboBox(int chargenum)
        {
            //停车位编号加载初始化
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT ParkingNum from T_ChargeSetting Where ChargeNum=@chargenum order by ParkingNum",
                new SqlParameter[] { new SqlParameter("@chargenum", chargenum) });
            cbParkingNum.ItemsSource = sql1.m_table.DefaultView;
            cbParkingNum.DisplayMemberPath = "ParkingNum";
            cbParkingNum.SelectedValuePath = "ParkingNum";
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
                sql1.Open("select * from T_ChargeSetting where ChargeNum=" + Convert.ToString(paradocknum) + " order by ParkingNum");
                dataGrid1.ItemsSource = sql1.m_table.DefaultView;
                sql1.Close();
                DAL.ZSql sql2 = new DAL.ZSql();
                sql2.Open("select * from T_ChargeArea where ChargeNum=" + Convert.ToString(paradocknum));
                dataGrid2.ItemsSource = sql2.m_table.DefaultView;
                sql2.Close();
                DAL.ZSql sql3 = new DAL.ZSql();
                sql3.Open("select * from T_LineSet Order by LineNum");
                dataGrid3.ItemsSource = sql3.m_table.DefaultView;
                sql3.Close();
            }
            else
            {
                dataGrid1.ItemsSource = null; 
                dataGrid2.ItemsSource = null;
                dataGrid3.ItemsSource = null;
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
                cbParkingNum.Text = selectItem["ChargeNum"].ToString().Trim();
                cbRouteNum.Text = selectItem["RouteNum"].ToString().Trim();
            }
            else
            {
                cbParkingNum.Text = "";
                cbRouteNum.Text = "";
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
                cbSWorkLine.Text = selectItem["ChargeSSLine"].ToString().Trim();
                cbSMark.Text = selectItem["ChargeSSNum"].ToString().Trim();
                cbEWorkLine.Text = selectItem["ChargeESLine"].ToString().Trim();
                cbEMark.Text = selectItem["ChargeESNum"].ToString().Trim();
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
            try
            {
                //验证是否已存在相同的停靠区
                sql.Open("Select * from T_ChargeArea Where ChargeNum=1");
                if (sql.rowcount > 0)
                {
                    MessageBox.Show("充电区已存在！");
                    return;
                }
                sql.Open("insert into T_ChargeArea (ChargeNum,ChargeSSNum,ChargeSSLine,ChargeESNum,ChargeESLine) Values (1,@chargessnum,@chargessline,@chargeesnum,@chargeesline)",
                new SqlParameter[]{
                    new SqlParameter("chargessnum",cbSMark.Text.Trim()),
                    new SqlParameter("chargessline",cbSWorkLine.Text.Trim()),
                    new SqlParameter("chargeesnum",cbEMark.Text.Trim()),
                    new SqlParameter("chargeesline",cbEWorkLine.Text.Trim())
                });
                sql.Close();
                MessageBox.Show("添加成功！");
                BindParkingNumComboBox(1);
                LoadDataGrid(1);
            }
            catch (Exception ex)
            {    
                MessageBox.Show(ex.Message);
            }
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
            sql.Open("select * from T_ChargeArea where ChargeNum=1");
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("输入的充电区不存在！");
                return;
            }
            sql.Open("update T_ChargeArea set ChargeSSNum=@dockssnum,ChargeSSLine=@dockssline,ChargeESNum=@dockesnum,ChargeESLine=@dockesline where ChargeNum=1",
            new SqlParameter[]{
                new SqlParameter("@dockssnum",cbSMark.Text.Trim()),
                new SqlParameter("@dockssline",cbSWorkLine.Text.Trim()),
                new SqlParameter("@dockesnum",cbEMark.Text.Trim()),
                new SqlParameter("@dockesline",cbEWorkLine.Text.Trim())
            });
            sql.Close();
            MessageBox.Show("修改成功！");
            LoadDataGrid(1);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDockDelete_Click(object sender, RoutedEventArgs e)
        {
            sql.Open("Select * from T_ChargeArea Where ChargeNum=1");
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("记录不存在！");
                return;
            }
            sql.Open("delete from T_ChargeSetting where ChargeNum=1");
            sql.Open("delete from T_ChargeArea where ChargeNum=1");
            sql.Close();
            MessageBox.Show("删除成功！");
            BindParkingNumComboBox(1);
            LoadDataGrid(1);
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbParkingNum.Text.Trim()) || string.IsNullOrEmpty(cbRouteNum.Text.Trim()))
            {
                MessageBox.Show("请输入完整信息！");
                return;
            }
            sql.Open("select * from T_ChargeSetting where ChargeNum=1 and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount > 0)
            {
                MessageBox.Show("添加失败！您输入的AGV车辆线路设置已存在！");
                return;
            }
            sql.Open("insert into T_ChargeSetting (ChargeNum,ParkingNum,RouteNum) Values (1,@agvnum,@routenum)",
            new SqlParameter[]{
                                                    new SqlParameter("@agvnum",cbParkingNum.Text.Trim()),
                                                    new SqlParameter("@routenum",cbRouteNum.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("添加成功！");
            BindParkingNumComboBox(1);
            LoadDataGrid(1);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingModify_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbParkingNum.Text.Trim()) || string.IsNullOrEmpty(cbRouteNum.Text.Trim()))
            {
                MessageBox.Show("请输入完整信息！");
                return;
            }
            sql.Open("select * from T_ChargeSetting where ChargeNum=1 and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("修改失败！您输入的AGV车辆线路设置不存在！");
                return;
            }
            sql.Open("update T_ChargeSetting set RouteNum=@routenum where ChargeNum=1 and ParkingNum=@agvnum",
            new SqlParameter[]{
                                                    new SqlParameter("@agvnum",cbParkingNum.Text.Trim()),
                                                    new SqlParameter("@routenum",cbRouteNum.Text.Trim())
                                });
            sql.Close();
            MessageBox.Show("修改成功！");
            LoadDataGrid(1);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettingDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbParkingNum.Text.Trim()))
            {
                MessageBox.Show("请输入AGV编号！");
                return;
            }
            sql.Open("select * from T_ChargeSetting where ChargeNum=1 and ParkingNum=" + cbParkingNum.Text.Trim());
            if (sql.rowcount <= 0)
            {
                MessageBox.Show("删除失败！您输入的AGV车辆线路设置不存在！");
                return;
            }
            sql.Open("delete from T_ChargeSetting where ChargeNum=1 and ParkingNum=" + cbParkingNum.Text.Trim());
            sql.Close();
            MessageBox.Show("删除成功！");
            BindParkingNumComboBox(1);
            LoadDataGrid(1);
        }

        /// <summary>
        /// 验证输入
        /// </summary>
        /// <returns>是否满足要求</returns>
        private bool Vertify_Input()
        {
            if (string.IsNullOrEmpty(cbSWorkLine.Text.Trim()) ||
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

        private void btnChargeLineModif_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cbLineSetRouteNum.Text.Trim()) || string.IsNullOrEmpty(cbChargeLine.Text.Trim()))
            {
                MessageBox.Show("请选择路线和对应的充电路线！");
                return;
            }
            sql.Open("update T_LineSet set ChargeLine=@chargeline where LineNum=@linenum",
                new SqlParameter[]{
                                    new SqlParameter("@chargeline",cbChargeLine.Text.Trim()),
                                    new SqlParameter("@linenum",cbLineSetRouteNum.Text.Trim())
                });
            sql.Close();
            MessageBox.Show("修改成功！");
            LoadDataGrid(1);
        }

        private void dataGrid3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView selectItem = dataGrid3.SelectedItem as DataRowView;
            if (selectItem != null)
            {
                cbLineSetRouteNum.Text = selectItem["LineNum"].ToString().Trim();
                cbChargeLine.Text = selectItem["ChargeLine"].ToString().Trim();
            }
            else
            {
                cbLineSetRouteNum.Text = "";
                cbChargeLine.Text = "";
            }
        }
    }
}
