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
using System.Configuration;

namespace AGV_WPF
{
    /// <summary>
    /// WorkLineManage.xaml 的交互逻辑
    /// </summary>
    public partial class WorkLineManage : Window
    {
        public DAL.ZSql sql1 = new DAL.ZSql();
        //修改日期：2013-12-1
        //实际的AGV数量
        public byte AGVNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["AGVNUM_MAX"]);
        public WorkLineManage()
        {
            InitializeComponent();
            LoadDataGrid();
            //修改日期：2013-12-1
            BindComboBox();
        }

        /// <summary>
        /// 加载数据表格
        /// </summary>
        private void LoadDataGrid()
        {
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("select * from T_WorkLine");
            dataGrid1.ItemsSource = sql2.m_table.DefaultView;
            sql2.Close();
        }

        private void BindComboBox()
        {
            //修改日期：2013-12-1        
            for (int i = 0; i < AGVNUM_MAX; i++)
            {
                tbCarID.Items.Add((i+1).ToString());
            }
            //修改日期：2013-12-1
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("Select DISTINCT WorkLine from T_Mark");
            tbWorkline.ItemsSource = sql2.m_table.DefaultView;
            tbWorkline.DisplayMemberPath = "WorkLine";
            tbWorkline.SelectedValuePath = "WorkLine";
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
                tbWorkline.Text = selectItem["WorkLine"].ToString().Trim();
                tbCarID.Text = selectItem["CarID"].ToString().Trim();
            }
            else
            {
                tbWorkline.Text = "";
                tbCarID.Text = "";
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strcarid= tbCarID.Text.ToString().Trim();
            string  strworkline= tbWorkline.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strcarid))
            {
                MessageBox.Show("对不起，请同时输入生产区和小车编号！");
                return;
            }
            sql1.Open("select * from T_WorkLine where CarID=" + strcarid);
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("此AGV已经存在！");
                return;
            }
            else
            {
                sql1.Open("insert into T_WorkLine (CarID,WorkLine) Values (" + strcarid + "," + strworkline + ")");
                MessageBox.Show("添加AGV生产区成功！");
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
            string strcarid = tbCarID.Text.ToString().Trim();
            string strworkline = tbWorkline.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strcarid))
            {
                MessageBox.Show("对不起，请同时输入生产区和小车编号！");
                return;
            }
            sql1.Open("select * from T_WorkLine where CarID=" + strcarid);
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此AGV不存在！");
                return;
            }
            else
            {
                sql1.Open("update T_WorkLine set WorkLine=" + strworkline + " where CarID=" + strcarid );
                MessageBox.Show("修改AGV生产区成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bttnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strcarid = tbCarID.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strcarid))
            {
                MessageBox.Show("对不起，请选择删除AGV编号！");
                return;
            }
            sql1.Open("select * from T_WorkLine where CarID=" + strcarid);
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此AGV不存在！");
                return;
            }
            else
            {
                sql1.Open("delete from T_WorkLine" + " where CarID=" + strcarid);
                MessageBox.Show("删除AGV生产区成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }
    }
}
