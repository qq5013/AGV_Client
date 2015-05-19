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
using System.Data;

namespace AGV_WPF
{
    /// <summary>
    /// SpeedManage.xaml 的交互逻辑
    /// </summary>
    public partial class SpeedManage : Window
    {
        public DAL.ZSql sql1 = new DAL.ZSql();
        public SpeedManage()
        {
            InitializeComponent();
            LoadDataGrid();
        }

        /// <summary>
        /// 加载数据表格
        /// </summary>
        private void LoadDataGrid()
        {
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("select * from T_Speed where CmdNum > 0");
            dataGrid1.ItemsSource = sql2.m_table.DefaultView;
            sql2.Close();
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
                tbSpeedgrade.Text = selectItem["SpeedGrade"].ToString().Trim();
                tbSpeed.Text = selectItem["Speed"].ToString().Trim();
            }
            else
            {
                tbSpeedgrade.Text = "";
                tbSpeed.Text = "";
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strspeedgrade = tbSpeedgrade.Text.ToString().Trim();
            string strspeed = tbSpeed.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strspeedgrade) || string.IsNullOrEmpty(strspeed))
            {
                MessageBox.Show("对不起，请同时输入速度等级和速度！");
                return;
            }
            sql1.Open("select * from T_Speed where SpeedGrade='" + strspeedgrade +"'");
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("此速度等级已经存在！");
                return;
            }
            else
            {
                sql1.Open("insert into T_Speed (SpeedGrade,Speed) Values ('" + strspeedgrade + "'," + strspeed + ")");
                MessageBox.Show("添加速度等级成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            string strspeedgrade = tbSpeedgrade.Text.ToString().Trim();
            string strspeed = tbSpeed.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strspeedgrade) || string.IsNullOrEmpty(strspeed))
            {
                MessageBox.Show("对不起，请同时输入速度等级和速度！");
                return;
            }
            sql1.Open("select * from T_Speed where SpeedGrade='" + strspeedgrade + "'");
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此速度等级不存在！");
                return;
            }
            else
            {
                sql1.Open("update T_Speed set Speed=" + strspeed + " where SpeedGrade='" + strspeedgrade + "'");
                MessageBox.Show("修改速度等级成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strspeedgrade = tbSpeedgrade.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strspeedgrade))
            {
                MessageBox.Show("对不起，请选择删除速度等级！");
                return;
            }
            sql1.Open("select * from T_Speed where SpeedGrade='" + strspeedgrade + "'");
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此速度等级不存在！");
                return;
            }
            else
            {
                sql1.Open("delete from T_Speed" + " where SpeedGrade='" + strspeedgrade + "'");
                MessageBox.Show("删除速度等级成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }
    }
}
