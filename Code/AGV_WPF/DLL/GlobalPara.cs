using AGV_WPF.DLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace AGV_WPF_Global
{
    class GlobalPara
    {
        public static string strName;
        public static string userid;
        public static bool IsManager = false;
        public static string Gcontrolcomname = "COM1";
        public static string Gcallcomname = "COM2";
        public static bool IsTrafficFun = false;
        public static bool IsDockFun = false;
        public static bool IsChargeFun = false;
        public static bool IsCallFun = false;
        public static int PageShiftInterval = 5;
        public static int MapWidth = 1600;
        public static int MapHeight = 564;
        public static BitmapImage mapImage = AGVUtils.GetImageFromFile(@"Image\background.png");
    }
}
