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
using System.Data.SqlClient;
using System.Configuration;
using ColorFont;
using AGV_WPF_Global;

namespace AGV_WPF
{
    /// <summary>
    /// TrafficManage.xaml 的交互逻辑
    /// </summary>
    public partial class TrafficManage : Page
    {
        public DAL.ZSql sql1 = new DAL.ZSql();
        public SolidColorBrush IsVirtualMark = Brushes.Gray;
        public SolidColorBrush NotVirtualMark = Brushes.Yellow;
        public SolidColorBrush TrafficColor = Brushes.Red;
        public int trafficnum = -1;
        public int id = -1;
        public double MarkDiameter = 4;
        public bool IsIDModel = true;
        //可以在此类中设置限制管制区和管制区地标数
        public int TRAFFIC_CONAREA_MAX = Convert.ToInt32(ConfigurationManager.AppSettings["TRAFFIC_CONAREA_MAX"]);//可以设置的最大管制区数量
        public byte TRAFFIC_CONAREA_MARKNUM_MAX = Convert.ToByte(ConfigurationManager.AppSettings["TRAFFIC_CONAREA_MARKNUM_MAX"]);//管制区最大地标数量
        public TrafficManage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IsVirtualMark = new SolidColorBrush(Properties.Settings.Default.MarkVirtualColor);
            NotVirtualMark = new SolidColorBrush(Properties.Settings.Default.MarkNotColor);
            TrafficColor = new SolidColorBrush(Properties.Settings.Default.TrafficColor);
            MarkDiameter = Properties.Settings.Default.MarkDiameter;
            MapInit();
            EVirtualMark.Fill = IsVirtualMark;
            ENotVirtualMark.Fill = NotVirtualMark;
            ETrafficMark.Fill = TrafficColor;
            this.dataGrid1.LoadingRow += new EventHandler<DataGridRowEventArgs>(this.DataGridSoftware_LoadingRow);
            BindLineCombox();
            BindWorkLineCombox();
            LoadAllMark();
        }

        /// <summary>
        /// 加载电子地图背景图片
        /// </summary>
        public void MapInit()
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = GlobalPara.mapImage;
            imageBrush.Stretch = Stretch.Uniform;
            canvas.Height = GlobalPara.MapHeight;
            canvas.Width = GlobalPara.MapWidth;
            imageBrush.AlignmentX = AlignmentX.Left;
            imageBrush.AlignmentY = AlignmentY.Top;
            canvas.Background = imageBrush;
        }

        /// <summary>
        /// 为表格加入行号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridSoftware_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        /// <summary>
        /// 加载地标
        /// </summary>
        private void LoadAllMark()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select * from T_Mark Order by WorkLine,Mark");
            //加载地图中地标
            canvas.Children.Clear();
            double x = 0, y = 0;
            for (int i = 0; i < sql1.Rows.Count; i++)
            {
                x = Convert.ToDouble(sql1.Rows[i]["XPos"]);
                y = Convert.ToDouble(sql1.Rows[i]["YPos"]);
                Ellipse markellipse = new Ellipse();
                markellipse.Height = MarkDiameter;
                markellipse.Width = MarkDiameter;
                markellipse.ToolTip = sql1.Rows[i]["ID"].ToString() + "\r\nWorkLine:" + sql1.Rows[i]["WorkLine"].ToString() + "  Mark:" + sql1.Rows[i]["Mark"];
                Canvas.SetLeft(markellipse, x - MarkDiameter/2);
                Canvas.SetTop(markellipse, y - MarkDiameter/2);
                //设置虚拟点颜色
                if (Convert.ToBoolean(sql1.Rows[i]["VirtualMark"]))
                {
                    markellipse.Fill = IsVirtualMark;
                }
                else
                {
                    markellipse.Fill = NotVirtualMark;
                    Label marklable = new Label();
                    marklable.FontSize = 10;
                    marklable.Foreground = Brushes.Black;
                    marklable.Content = sql1.Rows[i]["ID"].ToString();
                    Canvas.SetLeft(marklable, x - 14);
                    Canvas.SetTop(marklable, y - 20);
                    canvas.Children.Add(marklable);
                }
                canvas.Children.Add(markellipse);
            }
             sql1.Close();
        }

        /// <summary>
        /// 交通管制区号绑定
        /// </summary>
        private void BindLineCombox()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select DISTINCT TrafficNum from T_Traffic order by TrafficNum");
            cbTraffic.ItemsSource = sql1.m_table.DefaultView;
            cbTraffic.DisplayMemberPath = "TrafficNum";
            cbTraffic.SelectedValuePath = "TrafficNum";
            sql1.Close();
        }

        
        //修改日期：2013-12-1
        /// <summary>
        /// 生产区下拉框绑定
        /// </summary>
        private void BindWorkLineCombox()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("Select DISTINCT WorkLine from T_Mark");
            cbWorkLine.ItemsSource = sql1.m_table.DefaultView;
            cbWorkLine.DisplayMemberPath = "WorkLine";
            cbWorkLine.SelectedValuePath = "WorkLine";
            sql1.Close();
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
                id = Convert.ToInt16(selectItem["ID"].ToString().Trim());
                tbMarkID.Text = selectItem["MarkID"].ToString().Trim();
                cbWorkLine.Text = selectItem["WorkLine"].ToString().Trim();
                tbMark.Text = selectItem["Mark"].ToString().Trim();
            }
            else
            {
                tbMarkID.Text = "";
                tbMark.Text = "";
            }
        }

        /// <summary>
        /// 删除交通管制区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteTraffic_Click(object sender, RoutedEventArgs e)
        {
            string trafficnum = cbTraffic.Text.ToString().Trim();
            if (string.IsNullOrEmpty(trafficnum))
            {
                MessageBox.Show("请输要删除的交通管制区号！");
                cbTraffic.Focus();
                return;
            }
            sql1.Open("select * from T_Traffic where TrafficNum=" + trafficnum);
            if (sql1.Rows.Count <= 0)
            {
                MessageBox.Show("不存在此交通管制区号！");
                return;
            }
            if (MessageBox.Show("确认要删除此管制区？","警告",MessageBoxButton.YesNo,MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                sql1.Open("delete T_Traffic where TrafficNum=" + trafficnum);
                MessageBox.Show("删除交通管制区号成功！");
                BindLineCombox();
            }
            sql1.Close();
        }

        /// <summary>
        /// 添加交通管制区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strtraffic = cbTraffic.Text.ToString().Trim();
            string strmarkid = tbMarkID.Text.ToString().Trim();
            string strmark = tbMark.Text.ToString().Trim();
            string strworkline = cbWorkLine.Text.ToString().Trim();
            if (IsIDModel)
            {
                if (string.IsNullOrEmpty(strmarkid) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入交通管制区和地标ID！");
                    return;
                }
                sql1.Open("select * from T_Mark where ID=" + strmarkid);
                if (sql1.Rows.Count <= 0)
                {
                    MessageBox.Show("输入的地标ID不存在！");
                    return;
                }
            }

            else
            {
                if (string.IsNullOrEmpty(strmark) || string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入交通管制区、生产区、地标！");
                    return;
                }
                sql1.Open("select ID from T_Mark where WorkLine=" + strworkline + " and Mark=" + strmark);
                if (sql1.rowcount < 1)
                {
                    MessageBox.Show("输入的地标不存在！");
                    return;
                }
                strmarkid = sql1.Rows[0]["ID"].ToString();
            }
            //修改日期：2013-12-1
            sql1.Open("select * from T_Traffic where MarkID=@markid",
                new SqlParameter[]{
                        new SqlParameter("markid",strmarkid)
                    });
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("对不起，该地标已存在管制区中！");
                return;
            }
            sql1.Open("insert into T_Traffic (TrafficNum,MarkID) Values (@trafficnum,@markid)",
                new SqlParameter[]{
                        new SqlParameter("trafficnum",strtraffic),
                        new SqlParameter("markid",strmarkid)
                    });
            sql1.Close();
            trafficnum = Convert.ToInt32(strtraffic);
            LoadDataGrid(trafficnum.ToString());
            BindLineCombox();
            MessageBox.Show("添加成功！");
            cbTraffic.SelectedValue = strtraffic;
        }

        /// <summary>
        /// 修改交通管制区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (id <= 0)
            {
                MessageBox.Show("请在表格中选择要修改的行！");
                return;
            }
            string strtraffic = cbTraffic.Text.ToString().Trim();
            string strmarkid = tbMarkID.Text.ToString().Trim();
            string strmark = tbMark.Text.ToString().Trim();
            string strworkline = cbWorkLine.Text.ToString().Trim();
            if (IsIDModel)
            {
                if (string.IsNullOrEmpty(strmarkid) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入管制区和地标ID！");
                    return;
                }
                sql1.Open("select * from T_Mark where ID=" + strmarkid);
                if (sql1.Rows.Count <= 0)
                {
                    MessageBox.Show("输入的地标ID不存在！");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strmark) || string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入管制区、生产区和地标！");
                    return;
                }
                sql1.Open("select ID from T_Mark where WorkLine=" + strworkline + " and Mark=" + strmark);
                if (sql1.rowcount < 1)
                {
                    MessageBox.Show("输入的地标不存在！");
                    return;
                }
                strmarkid = sql1.Rows[0]["ID"].ToString();
            }
            //修改日期：2013-12-1
            sql1.Open("select * from T_Traffic where MarkID=@markid",
                new SqlParameter[]{
                        new SqlParameter("markid",strmarkid)
                        });
            if (sql1.Rows.Count > 0)
            {
                MessageBox.Show("对不起，该地标已存在管制区中！");
                return;
            }

            sql1.Open("update T_Traffic set MarkID=@markid where ID=@id",
                new SqlParameter[]{
                            new SqlParameter("ID",id.ToString()),
                        new SqlParameter("markid",strmarkid)
                    });
            sql1.Close();
            LoadDataGrid(trafficnum.ToString());
            MessageBox.Show("修改成功！");
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strtraffic = cbTraffic.Text.ToString().Trim();
            string strmarkid = tbMarkID.Text.ToString().Trim();
            string strmark = tbMark.Text.ToString().Trim();
            string strworkline = cbWorkLine.Text.ToString().Trim();
            if (IsIDModel)
            {
                if (string.IsNullOrEmpty(strmarkid) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入管制区和地标ID！");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strmark) || string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strtraffic))
                {
                    MessageBox.Show("对不起，请输入管制区、生产区和地标！");
                    return;
                }
                sql1.Open("select ID from T_Mark where WorkLine=" + strworkline + " and Mark=" + strmark);
                if (sql1.rowcount < 1)
                {
                    MessageBox.Show("输入的地标不存在！");
                    return;
                }
                strmarkid = sql1.Rows[0]["ID"].ToString();
            }
            sql1.Open("select * from T_Traffic where TrafficNum=" + strtraffic + " and MarkID=" + strmarkid);
            if (sql1.Rows.Count <= 0)
            {
                MessageBox.Show("管制区中不存在此地标ID！");
                return;
            }
            sql1.Open("delete from T_Traffic where TrafficNum=@trafficnum and MarkID=@markid",
                    new SqlParameter[]{
                    new SqlParameter("trafficnum",strtraffic),
                    new SqlParameter("markid",strmarkid)
                });
            LoadDataGrid(strtraffic);
            sql1.Close();
            MessageBox.Show("删除成功！");
        }

        /// <summary>
        /// 交通管制区号下拉框选择消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTraffic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTraffic.SelectedValue != null)
            {
                    trafficnum = Convert.ToUInt16(cbTraffic.SelectedValue.ToString().Trim());
                    LoadDataGrid(trafficnum.ToString());
            }
        }

        /// <summary>
        /// 加载数据表格
        /// </summary>
        /// <param name="paratrafficnum">交通管制编号</param>
        private void LoadDataGrid(string paratrafficnum)
        {
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("select T_Traffic.ID,T_Traffic.TrafficNum,T_Traffic.MarkID,T_Mark.XPos,T_Mark.YPos,T_Mark.ID,T_Mark.WorkLine,T_Mark.Mark from T_Traffic left join T_Mark on T_Mark.ID=T_Traffic.MarkID where TrafficNum=" + paratrafficnum);
            canvas.Children.Clear();
            LoadAllMark();
            double x, y;
            for (int i = 0; i < sql2.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(sql2.Rows[i]["MarkID"].ToString()) && !string.IsNullOrEmpty(sql2.Rows[i]["XPos"].ToString()) && !string.IsNullOrEmpty(sql2.Rows[i]["YPos"].ToString()))
                {
                    x = Convert.ToDouble(sql2.Rows[i]["XPos"]);
                    y = Convert.ToDouble(sql2.Rows[i]["YPos"]);
                    Ellipse markellipse = new Ellipse();
                    markellipse.Height = MarkDiameter + 2;
                    markellipse.Width = MarkDiameter + 2;
                    markellipse.Fill = TrafficColor;
                    markellipse.ToolTip = "MarkID:" + sql2.Rows[i]["MarkID"].ToString() + "\r\nWorkLine:" + sql2.Rows[i]["WorkLine"].ToString() + "  Mark:" + sql2.Rows[i]["Mark"];
                    Canvas.SetLeft(markellipse, x - MarkDiameter/2);
                    Canvas.SetTop(markellipse, y - MarkDiameter/2);

                    Label marklable = new Label();
                    marklable.FontSize = 10;
                    marklable.Foreground = TrafficColor;
                    marklable.Content = sql2.Rows[i]["MarkID"].ToString();
                    Canvas.SetLeft(marklable, x - 14);
                    Canvas.SetTop(marklable, y - 20);

                    canvas.Children.Add(markellipse);
                    canvas.Children.Add(marklable);
                }
            }
            dataGrid1.ItemsSource = sql2.m_table.DefaultView;
            sql2.Close();
        }

        bool MapZoomInOut = false;
        Point lastpoint = new Point(0, 0);
        Point OffsetPoint = new Point(0, 0);

        /// <summary>
        /// 画布中鼠标左键消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MapZoomInOut = true;
            lastpoint.X = e.GetPosition(null).X + OffsetPoint.X;
            lastpoint.Y = e.GetPosition(null).Y + OffsetPoint.Y;
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 鼠标移出画布消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            MapZoomInOut = false;
        }

        /// <summary>
        /// 鼠标左键弹起消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MapZoomInOut)
            {
                MapZoomInOut = false;
                OffsetPoint = canvas.Offset;
            }
        }

        /// <summary>
        /// 鼠标滚轮消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            canvas.Scale += e.Delta / 1000.0;
            //最大放大十倍
            if (canvas.Scale > 10)
            {
                canvas.Scale = 10;
            }
            //缩小可以小到3倍
            if (canvas.Scale < 0.3)
            {
                canvas.Scale = 0.3;
            }
        }

        /// <summary>
        /// 画布中鼠标移动消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MapZoomInOut)
            {
                Point newpoint = e.GetPosition(null);
                canvas.Offset = new Point(lastpoint.X - newpoint.X, lastpoint.Y - newpoint.Y);
            }
        }

        //修改日期：2013-12-1
        //修改内容：添加了输入地标时的切换功能，地标ID与地标、生产区可以切换输入
        private void btn_Shift_Click(object sender, RoutedEventArgs e)
        {
            IsIDModel = !IsIDModel;
            tbMarkID.Visibility = IsIDModel ? Visibility.Visible : Visibility.Hidden;
            labelMarkID.Visibility = IsIDModel ? Visibility.Visible : Visibility.Hidden;
            tbMark.Visibility = !IsIDModel ? Visibility.Visible : Visibility.Hidden;
            labelMark.Visibility = !IsIDModel ? Visibility.Visible : Visibility.Hidden;
            cbWorkLine.Visibility = !IsIDModel ? Visibility.Visible : Visibility.Hidden;
            labelWorkLine.Visibility = !IsIDModel ? Visibility.Visible : Visibility.Hidden;
        }
        

        private void ENotVirtualMark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorFont.ColorDialog fntDialog = new ColorFont.ColorDialog(NotVirtualMark, MarkDiameter);
            if (fntDialog.ShowDialog() == true)
            {
                NotVirtualMark = fntDialog.selectedColor;
                MarkDiameter = fntDialog.selectedSize;
                ENotVirtualMark.Fill = fntDialog.selectedColor;
                LoadAllMark();
            }
        }

        /// <summary>
        /// 虚拟点颜色设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EVirtualMark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorFont.ColorDialog fntDialog = new ColorFont.ColorDialog(IsVirtualMark, MarkDiameter);
            if (fntDialog.ShowDialog() == true)
            {
                IsVirtualMark = fntDialog.selectedColor;
                MarkDiameter = fntDialog.selectedSize;
                EVirtualMark.Fill = fntDialog.selectedColor;
                LoadAllMark();
            }
        }

        /// <summary>
        /// 非虚拟点颜色设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ETrafficMark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorFont.ColorDialog fntDialog = new ColorFont.ColorDialog(TrafficColor, MarkDiameter);
            if (fntDialog.ShowDialog() == true)
            {
                TrafficColor = fntDialog.selectedColor;
                ETrafficMark.Fill = fntDialog.selectedColor;
                if (cbTraffic.SelectedValue != null)
                {
                        trafficnum = Convert.ToUInt16(cbTraffic.SelectedValue.ToString().Trim());
                        LoadDataGrid(trafficnum.ToString());
                }
            }
        }

        /// <summary>
        /// 页面卸载消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.MarkVirtualColor = IsVirtualMark.Color;
            Properties.Settings.Default.MarkNotColor = NotVirtualMark.Color;
            Properties.Settings.Default.MarkDiameter = MarkDiameter;
            Properties.Settings.Default.TrafficColor = TrafficColor.Color;
            Properties.Settings.Default.Save();
        }
    }
}
