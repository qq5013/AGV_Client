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

namespace AGV_WPF
{
    /// <summary>
    /// MaterialsSettings.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialsSettings : Window
    {
        private String strstation;
        private String strmaterialnum;
        private String strroute;
        private String streachnum;
        private String strinfo;
        public DAL.ZSql sql1 = new DAL.ZSql();
        public MaterialsSettings()
        {
            InitializeComponent();
            LoadDataGrid();
        }

        private bool VertifyInput(String[] strList)
        {
            bool flag = true;
            foreach (string item in strList)
            {
                if (String.IsNullOrEmpty(item.Trim()))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        private void GetAllInfo()
        {
            strstation = cbStation.Text.ToString().Trim();
            strmaterialnum = cbMaterialNum.Text.ToString().Trim();
            strroute = cbRoute.Text.ToString().Trim();
            streachnum = cbEachNum.Text.ToString().Trim();
            strinfo = tbMaterialName.Text.ToString().Trim();
        }


        private void LoadDataGrid()
        {
            //表格初始化
            DAL.ZSql sqlGrid = new DAL.ZSql();
            sqlGrid.Open("SELECT * FROM T_CallSetting ORDER BY StationNum,MaterialNum");
            dataGrid1.ItemsSource = sqlGrid.m_table.DefaultView;
            sqlGrid.Close();
            //工位号下列框初始化
            DAL.ZSql sqlSN = new DAL.ZSql();
            sqlSN.Open("SELECT DISTINCT StationNum FROM T_CallSetting ORDER BY StationNum");
            cbStation.ItemsSource = sqlSN.m_table.DefaultView;
            cbStation.DisplayMemberPath = "StationNum";
            cbStation.SelectedValuePath = "StationNum";
            cbStation.SelectedIndex = 0;
            sqlSN.Close();
            //物料编号下列框初始化
            DAL.ZSql sqlMN = new DAL.ZSql();
            sqlMN.Open("SELECT DISTINCT MaterialNum FROM T_CallSetting ORDER BY MaterialNum");
            cbMaterialNum.ItemsSource = sqlMN.m_table.DefaultView;
            cbMaterialNum.DisplayMemberPath = "MaterialNum";
            cbMaterialNum.SelectedValuePath = "MaterialNum";
            cbMaterialNum.SelectedIndex = 0;
            sqlMN.Close();
            //线路加载初始化
            DAL.ZSql sqlRoute = new DAL.ZSql();
            sqlRoute.Open("SELECT DISTINCT LineNum FROM T_Line ORDER BY LineNum");
            cbRoute.ItemsSource = sqlRoute.m_table.DefaultView;
            cbRoute.DisplayMemberPath = "LineNum";
            cbRoute.SelectedValuePath = "LineNum";
            cbRoute.SelectedIndex = 0;
            sqlRoute.Close();
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView selectItem = dataGrid1.SelectedItem as DataRowView;
            if (selectItem != null)
            {
                cbStation.Text = selectItem["StationNum"].ToString().Trim();
                cbEachNum.Text = selectItem["EachNum"].ToString().Trim();
                cbRoute.Text = selectItem["LineNum"].ToString().Trim();
                cbMaterialNum.Text = selectItem["MaterialNum"].ToString().Trim();
                tbMaterialName.Text = selectItem["MaterialName"].ToString().Trim();
            }
            else
            {
                cbStation.Text = "";
                cbEachNum.Text = "";
                cbRoute.Text = "";
                cbMaterialNum.Text = "";
                tbMaterialName.Text = "";
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GetAllInfo();
            if (!VertifyInput(new String[] { strstation, strmaterialnum, strroute, streachnum, strinfo }))
            {
                MessageBox.Show("对不起，您输入的信息不全，添加失败！");
                return;
            }
            sql1.Open("select * from T_CallSetting where StationNum=@stationnum and MaterialNum=@materialnum", new SqlParameter[]{
            new SqlParameter("@stationnum",strstation),
            new SqlParameter("@materialnum",strmaterialnum)
            });
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("该工位物料编号已经存在！请重新输入。");
            }
            else
            {
                try
                {
                    sql1.Open("insert into T_CallSetting (StationNum, MaterialNum, LineNum, EachNum, MaterialName) Values (@stationnum,@materialnum,@linenum,@eachnum,@materialname)",
                        new SqlParameter[]{
                        new SqlParameter("@stationnum",strstation),
                        new SqlParameter("@materialnum",strmaterialnum),
                        new SqlParameter("@linenum",strroute),
                        new SqlParameter("@eachnum",streachnum),
                        new SqlParameter("@materialname",strinfo)
                    });
                    MessageBox.Show("添加记录成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            sql1.Close();
            LoadDataGrid();
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            GetAllInfo();
            if (!VertifyInput(new String[] { strstation, strmaterialnum, strroute, streachnum, strinfo }))
            {
                MessageBox.Show("对不起，您输入的信息不全，修改失败！");
                return;
            }
            sql1.Open("select * from T_CallSetting where StationNum=@stationnum and MaterialNum=@materialnum",
                new SqlParameter[]{
                        new SqlParameter("@stationnum",strstation),
                        new SqlParameter("@materialnum",strmaterialnum)
                });
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("此工位号对应的物料编号不存在！请重新输入。");
            }
            else
            {
                try
                {
                    sql1.Open("update T_CallSetting set LineNum=@linenum,EachNum=@eachnum,MaterialName=@materialname  where StationNum=@stationnum and MaterialNum=@materialnum",
                                new SqlParameter[]{
                                                new SqlParameter("@stationnum",strstation),
                                                new SqlParameter("@materialnum",strmaterialnum),
                                                new SqlParameter("@linenum",strroute),
                                                new SqlParameter("@eachnum",streachnum),
                                                new SqlParameter("@materialname",strinfo)
                                            });
                    MessageBox.Show("修改记录成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            sql1.Close();
            LoadDataGrid();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            GetAllInfo();
            if (!VertifyInput(new String[] { strstation, strmaterialnum}))
            {
                MessageBox.Show("对不起，请您同时输入工位号和物料编号！");
                return;
            }
            sql1.Open("select * from T_CallSetting where StationNum=@stationnum and MaterialNum=@materialnum",
                        new SqlParameter[]{
                            new SqlParameter("@stationnum",strstation),
                            new SqlParameter("@materialnum",strmaterialnum)
                });
            if (sql1.Rows.Count == 0)
            {
                MessageBox.Show("对不起，请您输入的工位号和物料编号不存在！");
            }
            else
            {
                try
                {
                    sql1.Open("delete from T_CallSetting where StationNum=@stationnum and MaterialNum=@materialnum",
                            new SqlParameter[]{
                                                new SqlParameter("@stationnum",strstation),
                                                new SqlParameter("@materialnum",strmaterialnum)
                            });
                    MessageBox.Show("删除成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            sql1.Close();
            LoadDataGrid();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //strstation = cbStation.Text.ToString().Trim();
            //strmaterialnum = cbMaterialNum.Text.ToString().Trim();
            //try
            //{
            //    ComboBox cb = sender as ComboBox;
            //    object obj = (object)e.AddedItems;//获取下拉框中选择的值
            //    string str = Convert.ToString(((System.Data.DataRowView)(((object[])(obj))[0])).Row.ItemArray[0]);//获取下拉框中选择的值
            //    if (cb.Name.Equals("cbStation"))
            //    {
            //        strstation = str;
            //    }
            //    else if (cb.Name.Equals("cbMaterialNum"))
            //    {
            //        strmaterialnum = str;
            //    }
            //    DAL.ZSql sqlGrid = new DAL.ZSql();
            //    sqlGrid.Open("SELECT * FROM T_CallSetting WHERE StationNum=@stationnum and MaterialNum=@materialnum ORDER BY StationNum,MaterialNum",
            //            new SqlParameter[]{
            //                new SqlParameter("@stationnum",strstation),
            //                new SqlParameter("@materialnum",strmaterialnum)
            //    });
            //    dataGrid1.ItemsSource = sqlGrid.m_table.DefaultView;
            //    sqlGrid.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

    }
}
