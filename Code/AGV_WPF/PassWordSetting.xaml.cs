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
using DAL;
using AGV_WPF_Global;

namespace AGV_WPF
{
    /// <summary>
    /// PassWordSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PassWordSetting : Window
    {
        public PassWordSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            if (pwdboxNew.Password.ToLower().Trim() == string.Empty)
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            else if (pwdboxNew.Password.ToLower().Trim() != pwdboxNewagain.Password.ToLower().Trim())
            {
                MessageBox.Show("两次输入密码不一致");
                return;
            }
            DAL.ZSql sql = new DAL.ZSql();
            sql.Open("update T_UserInfo set Pwd='" + pwdboxNew.Password.ToLower().Trim() + "' where ID=" + GlobalPara.userid);
            MessageBox.Show("修改密码成功！");
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
