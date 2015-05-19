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
using System.Windows.Media.Animation;
using ColorFont;
using AGV_WPF_Global;

namespace AGV_WPF
{
    /// <summary>
    /// MarkManage.xaml 的交互逻辑
    /// </summary>
    public partial class RouteManage : Page
    {
        public DAL.ZSql sql = new DAL.ZSql();
        public int linenum = -1;
        public int id = -1;
        public bool IsIDModel = true;
        public bool IsbtnCopy = true;
        public SolidColorBrush IsVirtualMark = Brushes.Gray;
        public SolidColorBrush NotVirtualMark = Brushes.Yellow;
        public SolidColorBrush RouteColor = Brushes.Tomato;
        public double MarkDiameter = 4;
        Path path;
        PathGeometry animationPath;
        PathFigure pFigure;
        PolyLineSegment route;
        string strrouteworkline = null;
        public RouteManage()
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
            MarkDiameter = Properties.Settings.Default.MarkDiameter;
            RouteColor = new SolidColorBrush(Properties.Settings.Default.RouteColor);
            EVirtualMark.Fill = IsVirtualMark;
            ENotVirtualMark.Fill = NotVirtualMark;
            RecRoute.Fill = RouteColor;
            // Create the animation path.
            path = new Path();
            path.Stroke = RouteColor;
            path.StrokeThickness = 3;
            animationPath = new PathGeometry();
            pFigure = new PathFigure();
            route = new PolyLineSegment();
            path.Data = animationPath;
            pFigure.Segments.Add(route);
            animationPath.Figures.Add(pFigure);
            MapInit();
            //修改日期：2013-12-1
            //修改日期：2013-12-30
            BindWorkLineCombox();
            BindLineCombox(cbRoute_WorkLine.Text.Trim());
            LoadAllMark();
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
        /// 加载地标
        /// </summary>
        private void LoadAllMark()
        {
            DAL.ZSql sql1 = new DAL.ZSql();
            sql1.Open("select * from T_Mark");
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
                Canvas.SetLeft(markellipse, x - MarkDiameter / 2);
                Canvas.SetTop(markellipse, y - MarkDiameter / 2);
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
        /// 加载数据表格
        /// </summary>
        /// <param name="paraworkline"></param>
        /// <param name="paralinenum"></param>
        private void LoadDataGrid(string paraworkline, int paralinenum)
        {
            if (paralinenum > 0)
            {
                DAL.ZSql sql1 = new DAL.ZSql();
                sql1.Open("select l.*,m.Mark,m.WorkLine,m.XPos,m.YPos  from T_Line as l LEFT OUTER JOIN T_Mark as m ON l.MarkID = m.ID where l.MarkID IS NOT NULL and l.MarkOrder IS NOT NULL and l.LineNum=" + Convert.ToString(paralinenum) + " and WorkLine=" + paraworkline + " order by l.MarkOrder");
                canvas.Children.Remove(path);
                if (sql1.Rows.Count > 1)
                {
                    route.Points.Clear();
                    for (int i = 0; i < sql1.Rows.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(sql1.Rows[i]["XPos"].ToString()) && !string.IsNullOrEmpty(sql1.Rows[i]["YPos"].ToString()))
                        {
                            route.Points.Add(new Point(Convert.ToDouble(sql1.Rows[i]["XPos"]), Convert.ToDouble(sql1.Rows[i]["YPos"])));
                        }
                    }
                    if (route.Points.Count >= 2)
                    {
                        pFigure.StartPoint = route.Points[0];
                        canvas.Children.Add(path);
                    }
                }
                dataGrid1.ItemsSource = sql1.m_table.DefaultView;
                sql1.Close();
                linenum = paralinenum;
            }
            else
            {
                dataGrid1.ItemsSource = null;
            }
        }

        /// <summary>
        /// 线路号绑定
        /// </summary>
        /// <param name="worklinenum">生产区</param>
        private void BindLineCombox(string worklinenum)
        {
            if (!string.IsNullOrEmpty(worklinenum))
            {
                DAL.ZSql sql1 = new DAL.ZSql();
                sql1.Open("select DISTINCT LineNum from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum > 0 AND WorkLine=" + worklinenum + " order by LineNum");
                cbRoute.ItemsSource = sql1.m_table.DefaultView;
                cbRoute.DisplayMemberPath = "LineNum";
                cbRoute.SelectedValuePath = "LineNum";
                sql1.Close();
            }
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

            //修改日期：2013-12-30
            DAL.ZSql sql2 = new DAL.ZSql();
            sql2.Open("Select DISTINCT WorkLine from T_Mark");
            cbRoute_WorkLine.ItemsSource = sql2.m_table.DefaultView;
            cbRoute_WorkLine.DisplayMemberPath = "WorkLine";
            cbRoute_WorkLine.SelectedValuePath = "WorkLine";
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
                id = Convert.ToInt16(selectItem["ID"].ToString().Trim());
                tbOrder.Text = selectItem["MarkOrder"].ToString().Trim();
                tbMarkID.Text = selectItem["MarkID"].ToString().Trim();
                tbDistance.Text = selectItem["Distance"].ToString().Trim();
                cbWorkLine.Text = selectItem["WorkLine"].ToString().Trim();
                tbMark.Text = selectItem["Mark"].ToString().Trim();
            }
            else
            {
                tbOrder.Text = "";
                tbMarkID.Text = "";
                tbDistance.Text = "";
                tbMark.Text = "";
            }
        }

        /// <summary>
        /// 删除路线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            strrouteworkline = cbRoute_WorkLine.Text.ToString().Trim();
            string strRoute = cbRoute.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strRoute))
            {
                MessageBox.Show("请输入要删除路线！");
                cbRoute.Focus();
                return;
            }
            sql.Open("select * from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strRoute + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count <= 0)
            {
                MessageBox.Show("不存在此路线！");
                return;
            }
            if (MessageBox.Show("确认要删除此路线？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                sql.Open("delete T_Line from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strRoute + " and WorkLine=" + strrouteworkline);
                MessageBox.Show("删除线路成功！");
                LoadAllMark();
                cbRoute.Text = "";
                tbOrder.Text = "";
                tbMarkID.Text = "";
                tbDistance.Text = "";
                BindLineCombox(cbRoute_WorkLine.Text.Trim());
            }
            sql.Close();
        }

        /// <summary>
        /// 复制线路
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyRoute_Click(object sender, RoutedEventArgs e)
        {
            this.tbOrder.Visibility = Visibility.Hidden;
            this.tbMarkID.Visibility = Visibility.Hidden;
            this.tbMark.Visibility = Visibility.Hidden;
            this.labelMark.Visibility = Visibility.Hidden;
            this.labelWorkLine.Visibility = Visibility.Hidden;
            this.cbWorkLine.Visibility = Visibility.Hidden;
            this.tbDistance.Visibility = Visibility.Hidden;
            this.labelOrder.Visibility = Visibility.Hidden;
            this.labelDistance.Visibility = Visibility.Hidden;
            this.labelMarkID.Visibility = Visibility.Hidden;
            this.btn_Shift.Visibility = Visibility.Hidden;
            this.labelNewRoute.Visibility = Visibility.Visible;
            this.tbNewRoute.Visibility = Visibility.Visible;
            this.CancelCopy.Visibility = Visibility.Visible;
            string strlinenum = cbRoute.Text.ToString().Trim();
            string strnewroute = tbNewRoute.Text.ToString().Trim();
            strrouteworkline = cbRoute_WorkLine.Text.ToString().Trim();
            btnAdd.IsEnabled = false;
            ModifyRecord.IsEnabled = false;
            DeleteRecord.IsEnabled = false;
            if (string.IsNullOrEmpty(strlinenum))
            {
                MessageBox.Show("对不起，请选择要复制的路线！");
                this.cbRoute.Focus();
                return;
            }
            if (string.IsNullOrEmpty(strnewroute))
            {
                MessageBox.Show("对不起，请输入复制后新路线的编号！");
                this.tbNewRoute.Focus();
                return;
            }
            sql.Open("select * from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strlinenum + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count < 1)
            {
                MessageBox.Show("您输入的复制线路不存在，请重新输入！");
                this.cbRoute.Focus();
                return;
            }
            sql.Open("select * from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strnewroute + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count > 0)
            {
                MessageBox.Show("您输入的新线路编号已存在，请重新输入！");
                this.tbNewRoute.Focus();
                return;
            }
            sql.Open("insert into T_Line (LineNum,MarkID,MarkOrder,Distance) select " + strnewroute +
                ",MarkID,MarkOrder,Distance from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strlinenum + " and WorkLine=" + strrouteworkline);
            MessageBox.Show("添加成功！");
            this.tbOrder.Visibility = Visibility.Visible;
            this.tbMarkID.Visibility = Visibility.Visible;
            this.tbDistance.Visibility = Visibility.Visible;
            this.labelOrder.Visibility = Visibility.Visible;
            this.labelDistance.Visibility = Visibility.Visible;
            this.labelMarkID.Visibility = Visibility.Visible;
            this.btn_Shift.Visibility = Visibility.Visible;
            this.labelNewRoute.Visibility = Visibility.Hidden;
            this.tbNewRoute.Visibility = Visibility.Hidden;
            this.CancelCopy.Visibility = Visibility.Hidden;
            IsIDModel = true;
            btnAdd.IsEnabled = true;
            ModifyRecord.IsEnabled = true;
            DeleteRecord.IsEnabled = true;
            tbNewRoute.Text = "";
            BindLineCombox(cbRoute_WorkLine.Text.Trim());
        }

        /// <summary>
        /// 添加记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string strlinenum = cbRoute.Text.ToString().Trim();
            string strorder = tbOrder.Text.ToString().Trim();
            string strmarkid = tbMarkID.Text.ToString().Trim();
            string strdistance = tbDistance.Text.ToString().Trim();
            string strmark = tbMark.Text.ToString().Trim();
            strrouteworkline = cbRoute_WorkLine.Text.ToString().Trim();
            string strworkline = cbWorkLine.Text.ToString().Trim();
            if (IsIDModel)
            {
                if (string.IsNullOrEmpty(strorder) || string.IsNullOrEmpty(strmarkid) || string.IsNullOrEmpty(strdistance)
                    || string.IsNullOrEmpty(strrouteworkline) || string.IsNullOrEmpty(strlinenum))
                {
                    MessageBox.Show("对不起，请同时输入生产区、线路号、序号、地标ID号和距离！");
                    return;
                }
                sql.Open("select * from T_Mark where ID=" + strmarkid);
                if (sql.Rows.Count <= 0)
                {
                    MessageBox.Show("输入的地标不存在！");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strorder) || string.IsNullOrEmpty(strmark) || string.IsNullOrEmpty(strworkline)
                    || string.IsNullOrEmpty(strdistance) || string.IsNullOrEmpty(strrouteworkline) || string.IsNullOrEmpty(strlinenum))
                {
                    MessageBox.Show("对不起，请同时输入生产区、线路号、序号、生产区、地标和距离！");
                    return;
                }
                sql.Open("select ID from T_Mark where WorkLine=" + strworkline + " and Mark=" + strmark);
                if (sql.rowcount < 1)
                {
                    MessageBox.Show("输入的地标不存在！");
                    return;
                }
                strmarkid = sql.Rows[0]["ID"].ToString();
            }

            sql.Open("select * from T_Mark where ID=" + strmarkid + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count == 0)
            {
                MessageBox.Show("路线中地标不可以跨生产区输入！");
                return;
            }

            if (Convert.ToUInt32(strorder) > 1)
            {
                sql.Open("select * from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strlinenum + " and MarkID=" + strmarkid + " and WorkLine=" + strrouteworkline + "and MarkOrder=" + (Convert.ToUInt32(strorder) - 1).ToString());
                if (sql.Rows.Count > 0)
                {
                    MessageBox.Show("路线相邻地标不可重复地标！");
                    return;
                }
            }

            sql.Open("SELECT MAX(MarkOrder) AS MaxOrder FROM T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strlinenum + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(sql.Rows[0]["MaxOrder"].ToString()))
                {
                    if ((Convert.ToInt32(sql.Rows[0]["MaxOrder"]) + 1) < Convert.ToInt32(strorder))
                    {
                        MessageBox.Show("对不起，请按序号的顺序输入线路！");
                        return;
                    }
                }
            }

            sql.Open("update T_Line set MarkOrder=MarkOrder+1 from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum=" + strlinenum + " and MarkOrder>=" + strorder + " and WorkLine=" + strrouteworkline);
            sql.Open("insert into T_Line (LineNum,MarkID,MarkOrder,Distance) Values (@linenum,@markid,@markorder,@distance)",
                        new SqlParameter[]{
                                                    new SqlParameter("linenum",strlinenum),
                                                    new SqlParameter("markid",strmarkid),
                                                    new SqlParameter("markorder",strorder),
                                                    new SqlParameter("distance",strdistance)
                                });
            sql.Close();
            linenum = Convert.ToInt32(strlinenum);
            LoadDataGrid(cbRoute_WorkLine.Text.Trim(), linenum);
            BindLineCombox(cbRoute_WorkLine.Text.Trim());
            MessageBox.Show("添加成功！");
            this.cbRoute.SelectedValue = strlinenum;
            this.tbOrder.Text = (Convert.ToInt32(strorder) + 1).ToString();
            if (IsIDModel)
            {
                this.tbMarkID.Text = (Convert.ToInt32(strmarkid) + 1).ToString();
            }
            else
            {
                this.tbMark.Text = (Convert.ToInt32(strmark) + 1).ToString();
            }
        }

        /// <summary>
        /// 修改记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyRecord_Click(object sender, RoutedEventArgs e)
        {
            if (id <= 0)
            {
                MessageBox.Show("请在表格中选择要修改的行！");
                return;
            }
            string strlinenum = cbRoute.Text.ToString().Trim();
            string strorder = tbOrder.Text.ToString().Trim();
            string strmarkid = tbMarkID.Text.ToString().Trim();
            string strdistance = tbDistance.Text.ToString().Trim();
            string strmark = tbMark.Text.ToString().Trim();
            string strworkline = cbWorkLine.Text.ToString().Trim();
            strrouteworkline = cbRoute_WorkLine.Text.ToString().Trim();
            if (IsIDModel)
            {
                if (string.IsNullOrEmpty(strlinenum) || string.IsNullOrEmpty(strorder) || string.IsNullOrEmpty(strmark)
                    || string.IsNullOrEmpty(strworkline) || string.IsNullOrEmpty(strdistance) || string.IsNullOrEmpty(strrouteworkline))
                {
                    MessageBox.Show("对不起，请同时输入线路、序号、地标和距离！");
                    return;
                }
                sql.Open("select * from T_Mark where ID=" + strmarkid);
                if (sql.Rows.Count <= 0)
                {
                    MessageBox.Show("输入的地标ID不存在！");
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strlinenum) || string.IsNullOrEmpty(strorder)
                    || string.IsNullOrEmpty(strmarkid) || string.IsNullOrEmpty(strdistance) || string.IsNullOrEmpty(strrouteworkline))
                {
                    MessageBox.Show("对不起，请同时输入线路、序号、生产区、地标和距离！");
                    return;
                }

                sql.Open("select ID from T_Mark where WorkLine=" + strworkline + " and Mark=" + strmark);
                if (sql.Rows.Count <= 0)
                {
                    MessageBox.Show("输入的地标ID不存在！");
                    return;
                }
                strmarkid = sql.Rows[0]["ID"].ToString();
            }

            sql.Open("select * from T_Line where LineNum=" + linenum.ToString() + " and MarkOrder=" + strorder);
            if (sql.Rows.Count == 0)
            {
                MessageBox.Show("您修改的序号记录不存在！");
                return;
            }

            sql.Open("select * from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum="
                + linenum.ToString() + " and MarkID=" + strmarkid + " and Distance=" + strdistance + " and WorkLine=" + strrouteworkline);
            if (sql.Rows.Count > 0)
            {
                MessageBox.Show("路线中已存在此地标，不可重复输入相同地标！");
                return;
            }
            sql.Open("update T_Line set MarkID=@markid,Distance=@distance where ID=@id",
                new SqlParameter[]{
                            new SqlParameter("id",id.ToString()),
                            new SqlParameter("markid",strmarkid),
                            new SqlParameter("distance",strdistance)
                    });
            sql.Close();
            LoadDataGrid(cbRoute_WorkLine.Text.Trim(), linenum);
            MessageBox.Show("修改成功！");
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            string strlinenum = cbRoute.Text.ToString().Trim();
            string strorder = tbOrder.Text.ToString().Trim();
            strrouteworkline = cbRoute_WorkLine.Text.ToString().Trim();
            if (string.IsNullOrEmpty(strorder) || string.IsNullOrEmpty(strlinenum) || string.IsNullOrEmpty(strrouteworkline))
            {
                MessageBox.Show("对不起，请同时输入路线和序号！");
                return;
            }
            if (id > 0)
            {
                sql.Open("select * from T_Line where ID=" + id.ToString());
                if (sql.rowcount <= 0)
                {
                    MessageBox.Show("对不起，请从表格中选择要删除的行！");
                    return;
                }
                sql.Open("delete from T_Line where ID=" + id.ToString());
                sql.Open("update T_Line set MarkOrder=MarkOrder-1  from T_Line LEFT OUTER JOIN T_Mark ON T_Line.MarkID = T_Mark.ID where LineNum="
                    + strlinenum + " and MarkOrder>" + strorder + " and WorkLine=" + strrouteworkline);
                MessageBox.Show("删除成功！");
                LoadDataGrid(cbRoute_WorkLine.Text.Trim(), linenum);
                sql.Close();
            }
        }

        /// <summary>
        /// 线路下拉框消息触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRoute.SelectedValue != null)
            {
                linenum = Convert.ToUInt16(cbRoute.SelectedValue.ToString().Trim());
                LoadDataGrid(cbRoute_WorkLine.Text.Trim(), linenum);
            }
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

        //修改日期：2013-12-23
        //修改内容：复制线路时，添加了返回功能和禁用按钮功能
        private void CancelCopy_Click(object sender, RoutedEventArgs e)
        {
            this.tbOrder.Visibility = Visibility.Visible;
            this.tbMarkID.Visibility = Visibility.Visible;
            this.tbDistance.Visibility = Visibility.Visible;
            this.labelOrder.Visibility = Visibility.Visible;
            this.labelDistance.Visibility = Visibility.Visible;
            this.labelMarkID.Visibility = Visibility.Visible;
            this.btn_Shift.Visibility = Visibility.Visible;
            this.labelNewRoute.Visibility = Visibility.Hidden;
            this.tbNewRoute.Visibility = Visibility.Hidden;
            this.CancelCopy.Visibility = Visibility.Hidden;
            btnAdd.IsEnabled = true;
            ModifyRecord.IsEnabled = true;
            DeleteRecord.IsEnabled = true;
            tbNewRoute.Text = "";
        }

        //修改日期:2013-12-30
        //添加内容:线路号按照生产区划分
        private void cbRoute_WorkLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRoute_WorkLine.SelectedValue != null)
            {
                BindLineCombox(cbRoute_WorkLine.SelectedValue.ToString().Trim());
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
            Properties.Settings.Default.RouteColor = RouteColor.Color;
            Properties.Settings.Default.Save();
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
        /// 线路颜色设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecRoute_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorFont.ColorDialog fntDialog = new ColorFont.ColorDialog(RouteColor, MarkDiameter);
            if (fntDialog.ShowDialog() == true)
            {
                RouteColor = fntDialog.selectedColor;
                RecRoute.Fill = fntDialog.selectedColor;
                path.Stroke = RouteColor;
                LoadDataGrid(cbRoute_WorkLine.Text.Trim(), linenum);
            }
        }

    }
}
