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
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Data;

namespace AGV_WPF
{
    /// <summary>
    /// ExceptionManage.xaml 的交互逻辑
    /// </summary>
    public partial class ExceptionManage : Window
    {
        public int carid = -1;
        public string strdate;
        public ExceptionManage()
        {
            InitializeComponent();
            this.dataGrid1.LoadingRow += new EventHandler<DataGridRowEventArgs>(this.DataGridSoftware_LoadingRow);
            BindCombox();
        }

        /// <summary>
        /// 表格增加行号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridSoftware_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        /// <summary>
        /// 下拉框绑定
        /// </summary>
        private void BindCombox()
        {
            //绑定ＡＧＶ小车编号
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT CarID from T_Ex order by CarID");
            cbCarid.ItemsSource = sql1.m_table.DefaultView;
            cbCarid.DisplayMemberPath = "CarID";
            cbCarid.SelectedValuePath = "CarID";
            sql1.Close();
            //日期绑定到今天的
            cbYear.Text = DateTime.Now.Year.ToString();        //获取年份
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认要删除此全部记录？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DAL.ZSql sql1 = new DAL.ZSql();
                sql1.Open("delete from T_Ex where CarID=" + carid.ToString() + " and ExTimer like '" + strdate + "%'");
                sql1.Close();
                LoadDataGrid(carid, strdate);
            }
        }

        /// <summary>
        /// AGV编号下拉框选择响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCarid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCarid.SelectedValue != null)
            {
                carid = Convert.ToUInt16(cbCarid.SelectedValue.ToString().Trim());
                LoadDataGrid(carid, strdate);
            }
        }

        /// <summary>
        /// 加载数据表格
        /// </summary>
        /// <param name="paracarid"></param>
        /// <param name="strdate"></param>
        private void LoadDataGrid(int paracarid, string strdate)
        {
            if (paracarid > 0)
            {
                DAL.ZSql sql2 = new DAL.ZSql();
                sql2.Open("select * from T_Ex where CarID=" + paracarid.ToString() + " and ExTimer like '" + strdate + "%' order by ExTimer");
                dataGrid1.ItemsSource = sql2.m_table.DefaultView;
                sql2.Close();
            }
        }

        /// <summary>
        /// 日期下拉框选择响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbYear.SelectedValue != null && cbMonth.SelectedValue != null && cbDay.SelectedValue != null)
            {
                ComboBoxItem cbi = (ComboBoxItem)cbYear.SelectedItem;
                string stryear = cbi.Content.ToString();
                cbi = (ComboBoxItem)cbMonth.SelectedItem;
                string strmonth = cbi.Content.ToString();
                cbi = (ComboBoxItem)cbDay.SelectedItem;
                string strday = cbi.Content.ToString();
                if (strmonth == "-选择月份-")
                {
                    strmonth = "%";
                }

                if (strday == "-选择日期-")
                {
                    strday = "";
                }
                strdate = stryear + "-" + strmonth + "-" + strday;
                LoadDataGrid(carid, strdate);
            }
        }

        /// <summary>
        /// 导出到excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            MyExportDataGrid(dataGrid1);
        }

        #region DataGrid数据导出Excel
        private void MyExportDataGrid(DataGrid dg)
        {
            if (dg.HasItems)//判断datagrid中是否有数据
            {
                try
                {
                    string strPath = Environment.CurrentDirectory;
                    Microsoft.Win32.SaveFileDialog dialogOpenFile = new Microsoft.Win32.SaveFileDialog();
                    dialogOpenFile.DefaultExt = "csv";//默认扩展名
                    dialogOpenFile.AddExtension = true;//是否自动添加扩展名
                    dialogOpenFile.Filter = "*.csv|.csv";
                    dialogOpenFile.OverwritePrompt = true;//文件已存在是否提示覆盖
                    dialogOpenFile.FileName = "文件名";//默认文件名
                    dialogOpenFile.CheckPathExists = true;//提示输入的文件名无效
                    dialogOpenFile.Title = "对话框标题";
                    //显示对话框
                    bool? b = dialogOpenFile.ShowDialog();
                    if (b == true)//点击保存
                    {
                        using (Stream stream = dialogOpenFile.OpenFile())
                        {
                            string c = "";
                            //DataTable dt = (DataTable)dg.DataContext;
                            DataTable dt = ((DataView)dg.ItemsSource).Table;
                            for (int k = 0; k < dt.Columns.Count; k++)
                            {
                                string strcolumns;
                                switch (dt.Columns[k].Caption)
                                {
                                    case "CarID":
                                        strcolumns = "小车编号";
                                        break;
                                    case "ExTimer":
                                        strcolumns = "报警时间";
                                        break;
                                    case "ExType":
                                        strcolumns = "报警类别";
                                        break;
                                    case "ExWorkLine":
                                        strcolumns = "报警生产区";
                                        break;
                                    case "ExRouteNum":
                                        strcolumns = "报警路线";
                                        break;
                                    case "ExMarkNum":
                                        strcolumns = "报警地标";
                                        break;
                                    default:
                                        strcolumns = dt.Columns[k].Caption;
                                        break;
                                }
                                c += strcolumns + ",";
                            }
                            c = c.Substring(0, c.Length - 1) + "\r\n";
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    c += dt.Rows[i][j].ToString() + ",";
                                }
                                c = c.Substring(0, c.Length - 1) + "\r\n";
                            }
                            Byte[] fileContent = System.Text.Encoding.GetEncoding("gb2312").GetBytes(c);
                            stream.Write(fileContent, 0, fileContent.Length);
                            stream.Close();
                        }
                    }
                    //恢复系统路径-涉及不到的可以去掉
                    Environment.CurrentDirectory = strPath;
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.ToString());
                }
                finally
                { MessageBox.Show("导出成功!", "系统提示"); }
            }
            else
            {
                MessageBox.Show("没有可以输出的数据!", "系统提示");
            }
        }
        #endregion
    }
}
