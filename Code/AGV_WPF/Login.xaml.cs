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
using System.Configuration;
using System.Windows.Navigation;
using DAL;
using System.Data.SqlClient;
using AGV_WPF;
using AGV_WPF_Global;


namespace AGV_WPF
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>

    public partial class Login : Window
    {

        #region 构造函数
        public Login()
        {
            InitializeComponent();
        }
        #endregion

        #region 退出系统
        private void Exitbutton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region 设置快捷键
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Loginbutton_Click(sender, null);
            }
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }
        #endregion

        #region 登录系统
        private void Loginbutton_Click(object sender, RoutedEventArgs e)
        {
            string UserName = this.UserNametextBox.Text.ToLower().Trim();
            string PassWord = this.passwordBox.Password.ToLower().Trim();
            DAL.ZSql sql = new DAL.ZSql();
            int i = sql.Open("select * from T_UserInfo where UserName=@username and Pwd=@password", new SqlParameter[] { new SqlParameter("username", UserName), new SqlParameter("password", PassWord) });
            if (i < 0)
            {
                sql.Close();
                MessageBox.Show("数据库连接失败，请检查是否开启数据库服务或正确配置系统连接字符串", "登录错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (sql.Rows.Count > 0)
            {
                if (sql.Rows[0]["IsManager"].ToString() == "True")
                {
                    GlobalPara.IsManager = true;
                }
                else
                {
                    GlobalPara.IsManager = false;
                }
                GlobalPara.strName = UserName;
                GlobalPara.userid = sql.Rows[0]["ID"].ToString();
                sql.Close();
                try
                {
                    MainWindow mn = new MainWindow();
                    mn.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "系统发生严重错误！！！即将退出系统！！！", "系统异常", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                MessageBox.Show("用户名或密码错误，请检查", "警告");
                this.UserNametextBox.Clear();
                this.passwordBox.Clear();
                this.UserNametextBox.Focus();
            }
            sql.Close();
        }
        #endregion

        #region 窗体加载与退出
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.UserNametextBox.Text = Properties.Settings.Default.UserName;
            this.passwordBox.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.UserName = this.UserNametextBox.Text.ToLower().Trim();
            Properties.Settings.Default.Save();//使用Save方法保存更改
        }
        #endregion

    }
}

