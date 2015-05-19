using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace AGV_WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            DirectoryInfo info = new DirectoryInfo("Exception");
            if (!info.Exists)
            {
                Directory.CreateDirectory("Exception");
            }
            DirectoryInfo info1 = new DirectoryInfo("Link");
            if (!info1.Exists)
            {
                Directory.CreateDirectory("Link");
            }
            int temp;
            if (int.TryParse(ConfigurationManager.AppSettings["SAVEPIC"], out temp))
            {
                IsSavePic = temp == 0 ? false : true;
            }
            else
                IsSavePic = false;
        }
        private bool IsSavePic = false;
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DateTime time = DateTime.Now;
            //MessageBox.Show("线程异常:"+e.ExceptionObject);
            FileStream stream = new FileStream(@"Exception\程序异常.txt", FileMode.Append, FileAccess.Write);
            byte[] buffer = System.Text.Encoding.Default.GetBytes("[" + time.ToShortDateString() + " " + time.ToLongTimeString() + "]线程:" + e.ExceptionObject + "\r\n");
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();

            if (IsSavePic)
            {
                try
                {
                    string fileName = string.Format("{0}{1:d2}{2:d2}{3:d2}{4:d2}{5:d2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                    Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
                    Bitmap bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(bmp);
                    g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                    bmp.Save(@"Exception\" + fileName + ".png");
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            DateTime time = DateTime.Now;

            FileStream stream = new FileStream(@"Exception\程序异常.txt", FileMode.Append, FileAccess.Write);

            byte[] buffer = System.Text.Encoding.Default.GetBytes("[" + time.ToShortDateString() + " " + time.ToLongTimeString() + "]UI线程:" + e.Exception.Message + "\r\n");
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();
            //MessageBox.Show("UI线程异常:"+e.Exception.Message);//这里通常需要给用户一些较为友好的提示，并且后续可能的操作

            e.Handled = true;//使用这一行代码告诉运行时，该异常被处理了，不再作为UnhandledException抛出了。
            if (IsSavePic)
            {
                try
                {
                    string fileName = string.Format("{0}{1:d2}{2:d2}{3:d2}{4:d2}{5:d2}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                    Rectangle rect = System.Windows.Forms.SystemInformation.VirtualScreen;
                    Bitmap bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(bmp);
                    g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                    bmp.Save(@"Exception\" + fileName + ".png");

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

    }
}
