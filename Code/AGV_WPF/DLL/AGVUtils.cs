
using System.Diagnostics;
using System.Windows.Media.Imaging;
namespace AGV_WPF.DLL
{
    class AGVUtils
    {
        /// <summary>
        /// 重启系统
        /// </summary>
        public static void RestartSystem()
        {
            string path = Process.GetCurrentProcess().MainModule.FileName;
            Process p = new Process();
            p.StartInfo.FileName = path;
            p.Start();
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 字符串是否是数字组成
        /// </summary>
        /// <param name="_string"></param>
        /// <returns></returns>
        public static bool IsNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 从应用程序Exe文件夹下加载图片
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static BitmapImage GetImageFromFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return null;
            System.IO.FileStream stream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new System.IO.MemoryStream(buffer);
            bmp.EndInit();
            return bmp;
        }
    }
}
