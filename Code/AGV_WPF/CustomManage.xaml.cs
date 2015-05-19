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
using System.Data.SqlClient;
using AGV_WPF.DLL;

namespace AGV_WPF
{
    /// <summary>
    /// CustomManage.xaml 的交互逻辑
    /// CustomType: 1-地标功能 2-运行状态(不超过0x40;因为0x40:运行 0x41:暂停 0x42:结束地标停止)
    /// </summary>
    public partial class CustomManage : Window
    {
        private bool IsModify = false;
        public DAL.ZSql sql1 = new DAL.ZSql();
        public int iCustomType = 1;

        /// <summary>
        /// 自定义页面构造函数，加载表格数据
        /// </summary>
        public CustomManage()
        {
            InitializeComponent();
            LoadDataGrid();
        }

        /// <summary>
        /// 加载表格设置
        /// </summary>
        private void LoadDataGrid()
        {
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("select * from T_Custom where CustomType=" + iCustomType.ToString() + " and CmdNum>0 order by CmdNum");
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
                tbCmdNum.Text = selectItem["CmdNum"].ToString().Trim();
                tbCmdFunction.Text = selectItem["CmdFunction"].ToString().Trim();
            }
            else
            {
                tbCmdNum.Text = "";
                tbCmdFunction.Text = "";
            }
        }

        /// <summary>
        /// 添加按钮消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strcmdnum = tbCmdNum.Text.ToString().Trim();
            string strcmdfunction = tbCmdFunction.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strcmdnum) || string.IsNullOrEmpty(strcmdfunction))
            {
                MessageBox.Show("对不起，请同时输入指令和指令功能！");
                return;
            }
            if (Convert.ToInt16(strcmdnum) <= 0 || Convert.ToInt16(strcmdnum) >= 64)
            {
                MessageBox.Show("对不起，请输入指令必须大于0小于64！");
                return;
            }
            sql1.Open("select * from T_Custom where CmdFunction='" + strcmdfunction + "' and CustomType=" + iCustomType.ToString());
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("此功能已经存在！");
                return;
            }
            else
            {
                sql1.Open("SELECT MAX(CmdNum) AS MaxNum FROM T_Custom where CustomType=" + iCustomType.ToString());
                if (sql1.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(sql1.Rows[0]["MaxNum"].ToString()))
                    {
                        if ((Convert.ToInt32(sql1.Rows[0]["MaxNum"]) + 1) < Convert.ToInt32(strcmdnum))
                        {
                            MessageBox.Show("对不起，请按顺序输入指令！");
                            return;
                        }
                    }
                }

                sql1.Open("update T_Custom set CmdNum=CmdNum+1 from T_Custom where CustomType=" + iCustomType.ToString() + " and CmdNum>=" + strcmdnum);
                sql1.Open("insert into T_Custom (CmdFunction,CmdNum,CustomType) Values ('" + strcmdfunction + "'," + strcmdnum + "," + iCustomType.ToString() + ")");
                MessageBox.Show("添加指令成功！");
            }
            sql1.Close();
            LoadDataGrid();
            tbCmdNum.Text = (Convert.ToInt32(strcmdnum) + 1).ToString();
            IsModify = true;
        }

        /// <summary>
        /// 修改按钮消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            string strcmdnum = tbCmdNum.Text.ToString().Trim();
            string strcmdfunction = tbCmdFunction.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strcmdnum) || string.IsNullOrEmpty(strcmdfunction))
            {
                MessageBox.Show("对不起，请同时输入指令和指令功能！");
                return;
            }
            sql1.Open("select * from T_Custom where CmdNum=" + strcmdnum + " and CustomType=" + iCustomType.ToString());
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此指令不存在！");
                return;
            }
            sql1.Open("select * from T_Custom where CmdFunction='" + strcmdfunction + "' and CustomType=" + iCustomType.ToString());
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("此功能已经存在！");
                return;
            }
            sql1.Open("update T_Custom set CmdFunction='" + strcmdfunction + "' where CmdNum=" + strcmdnum + " and CustomType=" + iCustomType.ToString());
            MessageBox.Show("修改指令成功！");
            sql1.Close();
            LoadDataGrid();
            IsModify = true;
        }

        /// <summary>
        /// 删除按钮消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strcmdnum = tbCmdNum.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strcmdnum))
            {
                MessageBox.Show("对不起，请选择删除指令！");
                return;
            }
            sql1.Open("select * from T_Custom where CmdNum=" + strcmdnum + " and CustomType=" + iCustomType.ToString());
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此指令不存在！");
                return;
            }
            else
            {
                sql1.Open("delete from T_Custom" + " where CmdNum=" + strcmdnum + " and CustomType=" + iCustomType.ToString());
                sql1.Open("update T_Custom set CmdNum=CmdNum-1  from T_Custom where CustomType=" + iCustomType.ToString() + " and CmdNum>" + strcmdnum);
                MessageBox.Show("删除指令成功！");
            }
            sql1.Close();
            LoadDataGrid();
            IsModify = true;
        }

        /// <summary>
        /// 自定义类型检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Custom_Checked(object sender, RoutedEventArgs e)
        {
            if (MarkRadioBtn.IsChecked.HasValue)//是否为null
            {
                if ((bool)MarkRadioBtn.IsChecked)
                {
                    iCustomType = 1;//地标功能
                    LoadDataGrid();
                }
                else
                {
                    iCustomType = 2;//运行状态
                    LoadDataGrid();
                }
            }
            else
            {
                iCustomType = 0;
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (IsModify)
            {
                if (MessageBox.Show("修改成功！重启软件生效。是否立即重启软件？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    AGVUtils.RestartSystem();
                }
            }
        }
    }
}
