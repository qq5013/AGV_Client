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
using DAL;
using System.Data;
using System.Data.SqlClient;
using ColorFont;
using AGV_WPF_Global;

namespace AGV_WPF
{
    /// <summary>
    /// MarkManage.xaml 的交互逻辑
    /// </summary>
    public partial class MarkManage : Page
    {
        public DAL.ZSql sql = new DAL.ZSql();
        public SolidColorBrush IsVirtualMark = Brushes.OrangeRed;
        public SolidColorBrush NotVirtualMark = Brushes.Yellow;
        public int id = -1;
        public bool IsQuickAdd = false;
        public bool IsQuickGet = false;
        public double MarkDiameter = 4;
        public Ellipse SelectedMark = new Ellipse();
        public MarkManage()
        {
            InitializeComponent();
            SelectedMark.Height = MarkDiameter * 2;
            SelectedMark.Width = MarkDiameter * 2;
            SelectedMark.Fill = Brushes.Brown;
            canvas.Children.Add(SelectedMark);
        }

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MapInit();
            IsVirtualMark = new SolidColorBrush(Properties.Settings.Default.MarkVirtualColor);
            NotVirtualMark = new SolidColorBrush(Properties.Settings.Default.MarkNotColor);
            MarkDiameter = Properties.Settings.Default.MarkDiameter;
            EVirtualMark.Fill = IsVirtualMark;
            ENotVirtualMark.Fill = NotVirtualMark;
            LoadData();
        }

        /// <summary>
        /// 加载电子地图背景图片
        /// </summary>
        /// <param name="struri">电子地图图片位置</param>
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
        /// 加载表格中数据
        /// </summary>
        private void LoadData()
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
                markellipse.ToolTip = "ID:" + sql1.Rows[i]["ID"].ToString() + "\r\nWorkLine:" + sql1.Rows[i]["WorkLine"].ToString() + "  Mark:" + sql1.Rows[i]["Mark"];
                Canvas.SetLeft(markellipse, x - MarkDiameter/2);
                Canvas.SetTop(markellipse, y - MarkDiameter/2);
                //设置虚拟点颜色
                if (Convert.ToBoolean(sql1.Rows[i]["VirtualMark"]))
                {
                    markellipse.Fill = IsVirtualMark;
                }
                else
                {
                   //非虚拟点加入ID显示
                    markellipse.Fill = NotVirtualMark;
                    Label marklable = new Label();
                    marklable.FontSize = 10;
                    marklable.Foreground = Brushes.Black;
                    marklable.Content = sql1.Rows[i]["ID"].ToString();
                    Canvas.SetLeft(marklable, x - 14);
                    Canvas.SetTop(marklable, y - 20);
                    canvas.Children.Add(marklable);
                }
                //地标都显示出来
                canvas.Children.Add(markellipse);
            }
            //加载表格
            dataGrid1.ItemsSource=sql1.m_table.DefaultView;
            sql1.Close();
        }

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string txtworkline = tbWorkLine.Text.Trim();
            string txtmark = tbMark.Text.Trim();
            string txtxpos = tbXPos.Text.Trim();
            string txtypos = tbYPos.Text.Trim();
            if (string.IsNullOrEmpty(txtworkline) || string.IsNullOrEmpty(txtmark) ||
                string.IsNullOrEmpty(txtxpos) || string.IsNullOrEmpty(txtypos))
            {
                MessageBox.Show("对不起，您输入信息不全！");
                return;
            }
            sql.Open("select * from T_Mark where WorkLine=" + txtworkline + " and Mark=" + txtmark);
            if (sql.Rows.Count > 0)
            {
                MessageBox.Show("已存在此记录！");
                return;
            }
            int i = sql.Open("insert into T_Mark (WorkLine,Mark,XPos,YPos,VirtualMark) Values (@workline,@mark,@xpos,@ypos,@virtualmark)",
                new SqlParameter[]{
                    new SqlParameter("workline",txtworkline),
                    new SqlParameter("mark",txtmark),
                    new SqlParameter("xpos",txtxpos),
                    new SqlParameter("ypos",txtypos),
                    new SqlParameter("virtualmark", Convert.ToBoolean(rbIsVirtual.IsChecked)?"1":"0")
                });
            if (i >= 0)
            {
                MessageBox.Show("添加记录成功！");
                LoadData();
            }
        }

        /// <summary>
        /// 修改记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyRecord_Click(object sender, RoutedEventArgs e)
        {
            //修改日期：2014-04-04
            //不能修改地标
            //             if (id <= 0)
            //             {
            //                 MessageBox.Show("对不起，请选择修改记录！");
            //                 return;
            //             }
            string txtworkline = tbWorkLine.Text.Trim();
            string txtmark = tbMark.Text.Trim();
            string txtxpos = tbXPos.Text.Trim();
            string txtypos = tbYPos.Text.Trim();
            if (string.IsNullOrEmpty(txtworkline) || string.IsNullOrEmpty(txtmark) ||
                string.IsNullOrEmpty(txtxpos) || string.IsNullOrEmpty(txtypos))
            {
                MessageBox.Show("对不起，不能输入为空信息！");
                return;
            }
            //修改日期：2014-01-02
            //修改日期：2014-04-04
            //不能修改地标
            sql.Open("select * from T_Mark where WorkLine=" + txtworkline + " and Mark=" + txtmark);
            if (sql.Rows.Count < 1)
            {
                MessageBox.Show("您需要修改的地标不存在，请重新输入！");
                return;
            }
            //修改日期：2014-04-04
            //不能修改地标
            //             int i = sql.Open("update T_Mark set WorkLine=@workline,Mark=@mark,XPos=@xpos,YPos=@ypos,VirtualMark=@virtualmark where ID=@id", new SqlParameter[]{
            //                 new SqlParameter("id",id.ToString()),    
            //                 new SqlParameter("workline",txtworkline),
            //                 new SqlParameter("mark",txtmark),
            //                 new SqlParameter("xpos",txtxpos),
            //                 new SqlParameter("ypos",txtypos),
            //                 new SqlParameter("virtualmark", Convert.ToBoolean(rbIsVirtual.IsChecked)?"1":"0")
            //                 });
            //             if (i >= 0)
            //             {
            //                 MessageBox.Show("修改记录成功！");
            //                 LoadData();
            //             }
            int i = sql.Open("update T_Mark set XPos=@xpos,YPos=@ypos,VirtualMark=@virtualmark where WorkLine=@workline and Mark=@mark", new SqlParameter[]{ 
                new SqlParameter("workline",txtworkline),
                new SqlParameter("mark",txtmark),
                new SqlParameter("xpos",txtxpos),
                new SqlParameter("ypos",txtypos),
                new SqlParameter("virtualmark", Convert.ToBoolean(rbIsVirtual.IsChecked)?"1":"0")
                });
            if (i >= 0)
            {
                MessageBox.Show("修改记录成功！");
                LoadData();
            }
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (id > 0)
            {
                int i = sql.Open("delete from T_Mark where ID=" + id.ToString());
                if (i >= 0)
                {
                    MessageBox.Show("删除记录成功！");
                    LoadData();
                }
                sql.Close();
            }
        }

        /// <summary>
        /// 删除全部记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认要删除此全部地标？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //int i = sql.Open("DELETE FROM T_Mark");
                int i = sql.Open("Truncate Table T_Mark");
                if (i >= 0)
                {
                    sql.Open("DELETE FROM T_Line Where LineNum > 0");
                    //sql.Open("DELETE FROM T_Traffic");
                    //sql.Open("Truncate Table T_Line");
                    sql.Open("Truncate Table T_Traffic");
                    MessageBox.Show("删除记录成功！");
                    LoadData();
                }
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
                id = Convert.ToInt16(selectItem["ID"].ToString().Trim());
                tbWorkLine.Text = selectItem["WorkLine"].ToString().Trim();
                tbMark.Text = selectItem["Mark"].ToString().Trim();
                tbXPos.Text = selectItem["XPos"].ToString().Trim();
                tbYPos.Text = selectItem["YPos"].ToString().Trim();
                if (Convert.ToBoolean(selectItem["VirtualMark"]))
                {
                    rbIsVirtual.IsChecked = true;
                } 
                else
                {
                    rbNotVirtual.IsChecked = true;
                }
                canvas.Children.Remove(SelectedMark);
                Canvas.SetLeft(SelectedMark, Convert.ToInt16(selectItem["XPos"]) - SelectedMark.Width / 2);
                Canvas.SetTop(SelectedMark, Convert.ToInt16(selectItem["YPos"]) - SelectedMark.Width / 2);
                canvas.Children.Add(SelectedMark);
            }
            else
            {
                tbWorkLine.Text = "";
                tbMark.Text = "";
                tbXPos.Text = "";
                tbYPos.Text = "";
            }
        }

        /// <summary>
        /// 快速添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuickAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbWorkLine.Text.Trim()))
            {
                MessageBox.Show("请输入要添加的生产区号。");
                return;
            }
            IsQuickAdd = true;
            tbMark.IsEnabled = false;
            tbXPos.IsEnabled = false;
            tbYPos.IsEnabled = false;
            rbNotVirtual.IsChecked = true;
            sql.Open("Select Max(Mark) As MaxValue from T_Mark where WorkLine=" + tbWorkLine.Text.Trim());
            if (sql.Rows.Count <= 0)
            {
                MessageBox.Show("不存在此生产区号，请重新输入。");
                return;
            }
            if (string.IsNullOrEmpty(sql.Rows[0]["MaxValue"].ToString()))
            {
                tbMark.Text = "1";
            }
            else
            {
                tbMark.Text = (Convert.ToUInt32(sql.Rows[0]["MaxValue"]) + 1).ToString();
            }
        }

        bool MapZoomInOut = false;
        Point lastpoint = new Point(0, 0);
        Point OffsetPoint = new Point(0, 0);

        /// <summary>
        /// 画布中鼠标移动消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsQuickAdd || IsQuickGet)
            {
                Point point=Mouse.GetPosition(canvas);
                tbXPos.Text = point.X.ToString("F2");
                tbYPos.Text = point.Y.ToString("F2");
                return;
            }
            if (MapZoomInOut)
            {
                Point newpoint = e.GetPosition(null);
                canvas.Offset = new Point(lastpoint.X - newpoint.X, lastpoint.Y - newpoint.Y);
            }
        }

        /// <summary>
        /// 画布中鼠标右键消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            IsQuickAdd = false;
            IsQuickGet = false;
            tbMark.IsEnabled = true;
            tbXPos.IsEnabled = true;
            tbYPos.IsEnabled = true;
        }

        /// <summary>
        /// 快速添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuickGet_Click(object sender, RoutedEventArgs e)
        {
            IsQuickGet = true;
        }

        /// <summary>
        /// 画布中鼠标左键消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsQuickGet)
            {
                IsQuickGet = false;
                this.Cursor = Cursors.Arrow;
            }
            else if (IsQuickAdd)
            {
                string strworkline = tbWorkLine.Text.Trim();
                if (string.IsNullOrEmpty(tbMark.Text.Trim()) || string.IsNullOrEmpty(strworkline))
                {
                    MessageBox.Show("请输入生产区和地标号！");
                    return;
                }
                uint i = Convert.ToUInt32(tbMark.Text.Trim());
                sql.Open("insert into T_Mark (WorkLine,Mark,XPos,YPos,VirtualMark) Values (@workline,@mark,@xpos,@ypos,@virtualmark)",
                new SqlParameter[]{
                    new SqlParameter("workline",tbWorkLine.Text.Trim()),
                    new SqlParameter("mark",i.ToString()),
                    new SqlParameter("xpos",tbXPos.Text.Trim()),
                    new SqlParameter("ypos",tbYPos.Text.Trim()),
                    new SqlParameter("virtualmark", Convert.ToBoolean(rbIsVirtual.IsChecked)?"1":"0")
                });
                LoadData();
                tbMark.Text = (i + 1).ToString();
                tbWorkLine.Text = strworkline;
            }
            else
            {
                MapZoomInOut = true;
                lastpoint.X =  e.GetPosition(null).X + OffsetPoint.X;
                lastpoint.Y = e.GetPosition(null).Y + OffsetPoint.Y;
                this.Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// 鼠标移动到画布消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsQuickAdd || IsQuickGet)
            {
                this.Cursor = Cursors.Cross;
            }
        }

        /// <summary>
        /// 鼠标移出画布消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsQuickAdd || IsQuickGet)
            {
                this.Cursor = Cursors.Arrow;
            }
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
            if (!(IsQuickAdd || IsQuickGet))
            {
                this.Cursor = Cursors.Arrow;
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
        /// 非虚拟点颜色设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ENotVirtualMark_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorFont.ColorDialog fntDialog = new ColorFont.ColorDialog(NotVirtualMark, MarkDiameter);
            if (fntDialog.ShowDialog() == true)
            {
                NotVirtualMark = fntDialog.selectedColor;
                MarkDiameter = fntDialog.selectedSize;
                ENotVirtualMark.Fill = fntDialog.selectedColor;
                LoadData();
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
                LoadData();
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
            Properties.Settings.Default.Save();
        }
    }
}
