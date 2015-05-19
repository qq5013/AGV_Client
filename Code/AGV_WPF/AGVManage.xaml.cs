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

namespace AGV_WPF
{
    /// <summary>
    /// AGVManage.xaml 的交互逻辑
    /// </summary>
    public partial class AGVManage : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AGVManage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 主页
        /// </summary>
        /// <param name="str">子页参数</param>
        public AGVManage(string str)
        {
            InitializeComponent();
            switch (str)
            {
                case "Mark":
                    MarkManage mmpage = new MarkManage();
                    frameNav.Navigate(mmpage);
                    break;
                case "Route":
                    RouteManage rmpage = new RouteManage();
                    frameNav.Navigate(rmpage);
                    break;
                case "Traffic":
                    TrafficManage tmpage = new TrafficManage();
                    frameNav.Navigate(tmpage);
                    break;
                    default:
                    break;
            }
        }
    }
}
