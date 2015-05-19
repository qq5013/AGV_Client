using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AGV_WPF.DLL.AGV
{
    /// <summary>
    /// AGV主界面动画类
    /// </summary>
    public class AGVAnimation
    {

        #region 成员变量
        /// <summary>
        /// 起终点标示的半径
        /// </summary>
        private int EllipseRadius =  Convert.ToInt32(ConfigurationManager.AppSettings["ELLIPSERADIUS"]);
        private int NavZoomMultis = Convert.ToInt32(ConfigurationManager.AppSettings["NAVZOOMMULTIS"]);
        private int CarLabFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["CARLABFONTSIZE"]);
        private int DottedLineThick = Convert.ToInt32(ConfigurationManager.AppSettings["DOTTEDLINETHICK"]);
        /// <summary>
        /// AGV运行区段的虚线连接路径
        /// </summary>
        public Path dottedLine { get; private set; }

        /// <summary>
        /// AGV运行时动画的签名状态标签
        /// </summary>
        public Label carLabel { get; private set; }

        /// <summary>
        /// AGV运行区段的起点地标标示
        /// </summary>
        public Ellipse startEllipse { get; private set; }

        /// <summary>
        /// AGV运行区段的终点地标标示
        /// </summary>
        public Ellipse endEllipse { get; private set; }

        /// <summary>
        /// AGV车辆动画的X属性动画路径
        /// </summary>
        public DoubleAnimationUsingPath animationX { get; private set; }

        /// <summary>
        /// AGV车辆动画的Y属性动画路径
        /// </summary>
        public DoubleAnimationUsingPath animationY { get; private set; }

        /// <summary>
        /// AGV车辆动画的Angle(角度)属性动画路径
        /// </summary>
        public DoubleAnimationUsingPath animationAngle { get; private set; }

        /// <summary>
        /// AGV车辆动画的显示外观图片
        /// </summary>
        public Image animatedImage { get; private set; }

        /// <summary>
        /// AGV车辆动画的故事板
        /// </summary>
        public Storyboard carStoryboard { get; private set; }

        /// <summary>
        /// AGV签名状态标签报警闪烁动画
        /// </summary>
        private DoubleAnimationUsingKeyFrames CarWarnningAnimation;

        /// <summary>
        /// 动画总量，用户在命名空间注册动画
        /// </summary>
        public static int AnimationNum = 0;

        /// <summary>
        /// WPF主界面框架
        /// </summary>
        public static FrameworkElement CarFElement = null;

        /// <summary>
        /// 动画画布
        /// </summary>
        public static ZoomableCanvas CarCanvas = null;
        #endregion

        #region 成员方法

        /// <summary>
        /// AGV主界面动画类构造函数
        /// </summary>
        public AGVAnimation()
        {
            if (CarFElement != null && CarCanvas != null)
            {
                CarCanvas.Dispatcher.Invoke(new Action(delegate
                {
                    DottedLineInit();
                    CarLabelInit();
                    StartEllipseInit();
                    EndEllipseInit();
                    AnimatedImageInit();
                    CarStoryboardInit();
                    CarAnimationInit();
                    CarWarnningAnimationInit();
                }));
            }
        }

        /// <summary>
        /// AGV主界面动画类析构函数
        /// </summary>
        ~AGVAnimation()
        {

        }

        /// <summary>
        /// 虚线初始化
        /// </summary>
        public void DottedLineInit()
        {
            dottedLine = new Path();
            dottedLine.StrokeThickness = DottedLineThick;
            dottedLine.Stroke = Brushes.GreenYellow;
            DoubleCollection dc = new DoubleCollection();
            dc.Add(5);
            dottedLine.StrokeDashArray = dc;
        }

        /// <summary>
        /// 区段起点地标标示初始化
        /// </summary>
        public void StartEllipseInit()
        {
            startEllipse = new Ellipse();
            startEllipse.Height = EllipseRadius * 2;
            startEllipse.Width = EllipseRadius * 2;
        }

        /// <summary>
        /// 区段终点地标标示初始化
        /// </summary>
        public void EndEllipseInit()
        {
            endEllipse = new Ellipse();
            endEllipse.Height = EllipseRadius * 2;
            endEllipse.Width = EllipseRadius * 2;
        }

        /// <summary>
        /// 车辆标签标示初始化
        /// </summary>
        public void CarLabelInit()
        {
            carLabel = new Label();
            carLabel.Content = "AGV0";
            //carLabel.Height = 40;
            //carLabel.Width = 160;
            carLabel.FontSize = CarLabFontSize;
            carLabel.Foreground = Brushes.PaleVioletRed;
            carLabel.FontWeight = FontWeights.UltraBold;
        }

        /// <summary>
        /// AGV标示图片初始化
        /// </summary>
        public void AnimatedImageInit()
        {
            animatedImage = new Image();
            animatedImage.Source = AGVUtils.GetImageFromFile(@"Image\navigation_24.png");//new BitmapImage( new Uri(@"Image\navigation_24.png", UriKind.RelativeOrAbsolute));
            animatedImage.Width = animatedImage.Source.Width * NavZoomMultis;
            animatedImage.Height = animatedImage.Source.Height * NavZoomMultis;
            CarCanvas.Children.Add(animatedImage);
        }

        /// <summary>
        /// AGV动画初始化
        /// </summary>
        public void CarAnimationInit()
        {
            TranslateTransform translate = new TranslateTransform();
            RotateTransform rotate = new RotateTransform();

            //AGV标签动画设置
            TransformGroup lablegroup = new TransformGroup();
            animatedImage.RenderTransformOrigin = new Point(0.5, 0.5);
            lablegroup.Children.Add(translate);//平移
            carLabel.RenderTransform = lablegroup;

            //AGV图标动画设置
            TransformGroup imagegroup = new TransformGroup();
            carLabel.RenderTransformOrigin = new Point(0.5, 0.5);
            imagegroup.Children.Add(rotate);//先旋转
            imagegroup.Children.Add(translate);//再平移
            animatedImage.RenderTransform = imagegroup;

            //设置XYAngle关联的动画

            animationX = new DoubleAnimationUsingPath();
            animationX.Source = PathAnimationSource.X;

            animationY = new DoubleAnimationUsingPath();
            animationY.Source = PathAnimationSource.Y;

            animationAngle = new DoubleAnimationUsingPath();
            animationAngle.Source = PathAnimationSource.Angle;

            CarFElement.RegisterName("translate" + AnimationNum.ToString(), translate);
            CarFElement.RegisterName("rotate" + AnimationNum.ToString(), rotate);
            Storyboard.SetTargetName(animationX, "translate" + AnimationNum.ToString());
            Storyboard.SetTargetProperty(animationX, new PropertyPath(TranslateTransform.XProperty));
            Storyboard.SetTargetName(animationY, "translate" + AnimationNum.ToString());
            Storyboard.SetTargetProperty(animationY, new PropertyPath(TranslateTransform.YProperty));
            Storyboard.SetTargetName(animationAngle, "rotate" + (AnimationNum++).ToString());
            Storyboard.SetTargetProperty(animationAngle, new PropertyPath(RotateTransform.AngleProperty));
        }

        /// <summary>
        /// 报警闪烁关键帧动画初始化
        /// </summary>
        public void CarWarnningAnimationInit()
        {
            CarWarnningAnimation = new DoubleAnimationUsingKeyFrames();
            CarWarnningAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            CarWarnningAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            CarWarnningAnimation.RepeatBehavior = RepeatBehavior.Forever;
        }

        /// <summary>
        /// 故事板初始化
        /// </summary>
        public void CarStoryboardInit()
        {
            if (carStoryboard == null)
            {
                carStoryboard = new Storyboard();
            }
        }

        /// <summary>
        /// 设置标签内容
        /// </summary>
        /// <param name="content"></param>
        public void SetCarLabelContent(string content)
        {
            carLabel.Content = content;
        }

        /// <summary>
        /// 设置Object相对于Canvas的位置
        /// </summary>
        /// <param name="uiobject">UIElement</param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        private void SetObjectPosition(UIElement uiobject, double x, double y)
        {
            Canvas.SetTop(uiobject, y);
            Canvas.SetLeft(uiobject, x);
        }

        /// <summary>
        /// 设置StartEllipse相对于Canvas的位置
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        public void SetStartEllipsePosition(double x, double y)
        {
            SetObjectPosition(startEllipse, x, y);
        }

        /// <summary>
        /// 设置EndEllipse相对于Canvas的位置
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        public void SetEndEllipsePosition(double x, double y)
        {
            SetObjectPosition(endEllipse, x, y);
        }

        /// <summary>
        /// 设置CarLabel相对于Canvas的位置
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        public void SetCarLabelPosition(double x, double y)
        {
            SetObjectPosition(carLabel, x, y);
        }

        /// <summary>
        /// 设置AnimatedImage相对于Canvas的位置
        /// </summary>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        public void SetAnimatedImagePosition(double x, double y)
        {
            SetObjectPosition(animatedImage, x, y);
        }

        /// <summary>
        /// 更新AGV编号和线路
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        /// <param name="agvnum"></param>
        /// <param name="animationtime"></param>
        public void DrawCarLine(List<Point> points, Brush color, double animationtime)
        {
            CarCanvas.Dispatcher.Invoke(new Action(delegate
            {
                Point startpoint = points[0];
                Point endpoint = points[points.Count - 1];
                carStoryboard.Children.Clear();
                SetStartEllipsePosition(startpoint.X - EllipseRadius, startpoint.Y - EllipseRadius);
                SetEndEllipsePosition(endpoint.X - EllipseRadius, endpoint.Y - EllipseRadius);

                endEllipse.Fill = color;
                startEllipse.Fill = color;

                CarCanvas.Children.Remove(startEllipse);
                CarCanvas.Children.Remove(endEllipse);
                CarCanvas.Children.Remove(carLabel);
                CarCanvas.Children.Remove(dottedLine);
                CarCanvas.Children.Add(startEllipse);
                CarCanvas.Children.Add(endEllipse);
                CarCanvas.Children.Add(carLabel);

                //SetCarLabelPosition(-carLabel.ActualHeight, carLabel.ActualHeight);
                SetAnimatedImagePosition(-animatedImage.Source.Height * NavZoomMultis / 2, -animatedImage.Source.Width * NavZoomMultis / 2);

                // Create the animation path.
                PathGeometry animationPath = new PathGeometry();
                PathFigure pFigure = new PathFigure();
                pFigure.StartPoint = points[0];
                PolyLineSegment pLineSegment = new PolyLineSegment();
                for (int i = 1; i < points.Count; i++)
                {
                    pLineSegment.Points.Add(points[i]);
                }
                pFigure.Segments.Add(pLineSegment);
                animationPath.Figures.Add(pFigure);

                dottedLine.Data = animationPath;
                CarCanvas.Children.Add(dottedLine);

                animationX.PathGeometry = animationPath;
                animationX.Duration = new Duration(TimeSpan.FromMilliseconds(animationtime * 1000));

                animationY.PathGeometry = animationPath;
                animationY.Duration = animationX.Duration;

                animationAngle.PathGeometry = animationPath;
                animationAngle.Duration = animationX.Duration;

                carStoryboard.Children.Add(animationX);
                carStoryboard.Children.Add(animationY);
                carStoryboard.Children.Add(animationAngle);
                carStoryboard.Begin(CarFElement, true);
            }));
        }

        /// <summary>
        /// AGV状态更改时的动画
        /// </summary>
        /// <param name="status">AGV状态</param>
        /// <param name="agvnum">AGV编号</param>
        /// <param name="docknum">AGV停靠区号</param>
        public void StatusChangeAnimation(int status, byte agvnum, int docknum)
        {
            CarCanvas.Dispatcher.Invoke(new Action(delegate
            {
                carLabel.BeginAnimation(UIElement.OpacityProperty, null);
                carLabel.Opacity = 1;
                switch (status)
                {
                    case 0x40:
                        carStoryboard.Resume(CarFElement);
                        carLabel.ToolTip = "正常";
                        carLabel.Content = "AGV" + agvnum;
                        carLabel.Foreground = Brushes.Black;
                        break;
                    case 0x41:
                        carLabel.ToolTip = "暂停";
                        if (docknum > 0)
                        {
                            carLabel.Content = agvnum;
                        }
                        else
                        {
                            carLabel.Content = agvnum + "暂停";
                        }
                        carLabel.Foreground = Brushes.OrangeRed;
                        carStoryboard.Pause(CarFElement);
                        carLabel.BeginAnimation(UIElement.OpacityProperty, CarWarnningAnimation);
                        break;
                    case 0x42:
                        carLabel.ToolTip = "结束地标停止";
                        carLabel.Foreground = Brushes.GreenYellow;
                        carLabel.Content = agvnum;
                        carStoryboard.Pause(CarFElement);
                        carLabel.BeginAnimation(UIElement.OpacityProperty, CarWarnningAnimation);
                        break;
                    default:
                        {
                            carLabel.Content = agvnum + MainWindow.StatusOpt[status];
                            carLabel.Foreground = Brushes.Yellow;
                            carLabel.ToolTip = MainWindow.StatusOpt[status];
                            carStoryboard.Pause(CarFElement);
                            carLabel.BeginAnimation(UIElement.OpacityProperty, CarWarnningAnimation);
                            break;
                        }
                }
            }));
        }

        /// <summary>
        /// AGV无线连接状态更改时动画
        /// </summary>
        /// <param name="wl">AGV无线连接状态</param>
        public void WLChangeAnimation(bool wl)
        {
            CarCanvas.Dispatcher.Invoke(new Action(delegate
            {
                if (wl)
                {
                    carStoryboard.Resume(CarFElement);
                }
                else
                {
                    CarCanvas.Children.Remove(startEllipse);
                    CarCanvas.Children.Remove(endEllipse);
                    CarCanvas.Children.Remove(dottedLine);
                    CarCanvas.Children.Remove(carLabel);
                    CarCanvas.Children.Remove(animatedImage);
                    carStoryboard.Pause(CarFElement);
                }
            }));
        }

        public void ClearAllElements()
        {
            CarCanvas.Dispatcher.Invoke(new Action(delegate
            {
                CarCanvas.Children.Remove(startEllipse);
                CarCanvas.Children.Remove(endEllipse);
                CarCanvas.Children.Remove(dottedLine);
                CarCanvas.Children.Remove(carLabel);
                CarCanvas.Children.Remove(animatedImage);
                animationX = null;
                animationY = null;
                animationAngle = null;
                animatedImage = null;
                carStoryboard = null;
                CarWarnningAnimation = null;
            }));
        }

        #endregion
    }
}
