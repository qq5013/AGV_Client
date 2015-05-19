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
using AGV_WPF_Global;

namespace AGV_WPF
{
    /// <summary>
    /// COMSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ControlCOMSetting : Window
    {
        /// <summary>
        /// 串口设置页面构造函数
        /// </summary>
        public ControlCOMSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 确认修改消息响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool resetflag = false;
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (!cfa.AppSettings.Settings["ControlCOMBaudrate"].Value.Equals(cbControlcombaudrate.Text.Trim()))
                {
                    cfa.AppSettings.Settings["ControlCOMBaudrate"].Value = cbControlcombaudrate.Text.Trim();
                    resetflag = true;
                }
                if (!cfa.AppSettings.Settings["ControlCOMDataBits"].Value.Equals(cbControlcomdatabits.Text.Trim()))
                {
                    cfa.AppSettings.Settings["ControlCOMDataBits"].Value = cbControlcomdatabits.Text.Trim();
                    resetflag = true;
                }
                if (!cfa.AppSettings.Settings["ControlCOMStopBits"].Value.Equals(cbControlcomstopbits.Text.Trim()))
                {
                    cfa.AppSettings.Settings["ControlCOMStopBits"].Value = cbControlcomstopbits.Text.Trim();
                    resetflag = true;
                }
                if (!cfa.AppSettings.Settings["ControlCOMParity"].Value.Equals(cbControlcomparity.Text.Trim()))
                {
                    cfa.AppSettings.Settings["ControlCOMParity"].Value = cbControlcomparity.Text.Trim();
                    resetflag = true;
                }
                if (resetflag)
                {
                    MessageBox.Show("修改成功！重启软件生效。");
                    this.Close();
                }
                else
                {
                    if (!GlobalPara.Gcontrolcomname.Equals(cbControlcomname.Text.Trim()))
                    {
                        GlobalPara.Gcontrolcomname = cbControlcomname.Text.Trim();
                        cfa.AppSettings.Settings["ControlCOMName"].Value = cbControlcomname.Text.Trim();
                        MessageBox.Show("修改成功！");
                        this.Close();
                    }
                }
                cfa.Save(ConfigurationSaveMode.Minimal, false);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("修改失败！");
            }
        }


        #region 退出系统
        /// <summary>
        /// 退出系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] portList = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(portList);
            for (int i = 0; i < portList.Length; i++)
            {
                cbControlcomname.Items.Add(portList[i]);
            }
            cbControlcomname.Text = GlobalPara.Gcontrolcomname;
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cbControlcombaudrate.Text = cfa.AppSettings.Settings["ControlCOMBaudrate"].Value;
            cbControlcomdatabits.Text = cfa.AppSettings.Settings["ControlCOMDataBits"].Value;
            cbControlcomstopbits.Text = cfa.AppSettings.Settings["ControlCOMStopBits"].Value;
            cbControlcomparity.Text = cfa.AppSettings.Settings["ControlCOMParity"].Value;
        }

    }
}
