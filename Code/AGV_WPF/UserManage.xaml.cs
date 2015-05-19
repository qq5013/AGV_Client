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
using System.Data;

namespace AGV_WPF
{
    /// <summary>
    /// UserManage.xaml 的交互逻辑
    /// </summary>
    public partial class UserManage : Window
    {
        private int id = -1;
        public DAL.ZSql sql1 = new DAL.ZSql();
        public UserManage()
        {
            InitializeComponent();
            LoadDataGrid();
        }

        /// <summary>
        /// 加载表格数据
        /// </summary>
        private void LoadDataGrid()
        {
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("select * from T_UserInfo");
            dataGrid1.ItemsSource = sql2.m_table.DefaultView;
            sql2.Close();
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            string strusername = tbUserName.Text.ToString().Trim().ToLower();
            string strpwd = tbPwd.Text.ToString().Trim().ToLower();
            int bismanage = cbUserType.Text.Trim() == "管理员" ? 1 : 0;
            if (string.IsNullOrEmpty(strusername) || string.IsNullOrEmpty(strpwd))
            {
                MessageBox.Show("对不起，请同时用户名和密码！");
                return;
            }
            sql1.Open("select * from T_UserInfo where UserName='" + strusername + "'");
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此用户名不存在！");
            }
            else
            {
                sql1.Open("update T_UserInfo set Pwd='" + strpwd + "',IsManager=" + bismanage.ToString() + " where ID=" + id.ToString());
                MessageBox.Show("修改成功！");
            }
            sql1.Close();
            LoadDataGrid();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strusername = tbUserName.Text.ToString().Trim();
            string strpwd= tbPwd.Text.ToString().Trim();
            int bismanage = cbUserType.Text.Trim() == "管理员" ? 1 : 0;
            if (string.IsNullOrEmpty(strusername) || string.IsNullOrEmpty(strpwd))
            {
                MessageBox.Show("对不起，请同时用户名和密码！");
                return;
            }
            sql1.Open("select * from T_UserInfo where UserName='" + strusername + "'");
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("此用户名已经存在！");
            }
            else
            {
                sql1.Open("insert into T_UserInfo (UserName,Pwd,IsManager) Values ('" + strusername + "','" + strpwd + "'," + bismanage.ToString() + ")");
                MessageBox.Show("添加用户成功！");
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
            string strusername = tbUserName.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strusername))
            {
                MessageBox.Show("对不起，请选择删除用户！");
                return;
            }
            sql1.Open("select * from T_UserInfo where UserName='" + strusername + "'");
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此用户名不存在！");
            }
            else
            {
                sql1.Open("delete from T_UserInfo" + " where ID=" + id.ToString());
                MessageBox.Show("删除用户成功！");
            }
            sql1.Close();
            LoadDataGrid();
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
                id = Convert.ToInt32(selectItem["ID"].ToString().Trim());
                tbUserName.Text = selectItem["UserName"].ToString().Trim();
                tbPwd.Text = selectItem["Pwd"].ToString().Trim();
                cbUserType.Text = (selectItem["IsManager"].ToString()=="True"?"管理员":"用户");
            }
            else
            {
                id = -1;
                tbUserName.Text = "";
                tbPwd.Text = "";
                cbUserType.Text = "";
            }
        }
    }
}
