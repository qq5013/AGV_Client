/* 
 * 创建时间：2010-04-27
 * 创建原因：为了在使用中不受到SerialPort的关闭困扰(有数据在传输并处理时关闭串口导致严重到死机的现象)
 * 实现：增加正在接收数据标志和试图关闭串口标志，
 *  增加对串口DataReceived事件的包装。
 * 1.在执行DataReceived委托前先判断是否要关闭串口，如果是则不再进行下面的一系列处理
 * 2.把正在接收数据标志设置为true
 * 3.执行委托
 * 4.把正在接收数据标志设置为false
 * 
 * 原理：在DataReceived事件中如果产生了对窗体或其上控件的调用的话，必须使用Invoke来操作。
 *  这实际上隐式的产生了线程，线程间的操作并发性导致在平时关闭正在处理DataReceived
 *  的串口和窗体间的线程产生了死锁。解决办法就是增加状态控制，主动释放线程资源。
 *  注意事项：1.在网站中时，添加System.Windows.Forms.dll
 *               步骤：网站->启动选项->引用->添加->.net->System.Windows.Forms
 *                     或者直接：网站->引用->添加->.net->System.Windows.Forms
 */

/*****************************函数、变量详细列表***********************************
public string PortName
public int BaudRate  
public int DataBits
public bool IsOpen
public string NewLine
public int BytesToRead
public int ReceivedBytesThreshold
public SerialPortWrapper(): this("COM1", 9600, 8)
public SerialPortWrapper(string portName): this(portName, 9600, 8)
public SerialPortWrapper(string portName, int baudRate, int dataBits)
public SerialPortWrapper(string portName, int baudRate, int parity, int dataBits)
public void Open()
public void Close()
public int Read(byte[] buffer, int offset, int count)
public string ReadLine()
public string ReadExisting()
public void Write(string text)
public void Write(byte[] buffer, int offset, int count)
public void Write(char[] buffer, int offset, int count)
public void WriteLine(string text)
public void DiscardInBuffer()
public void Dispose()
private void Dispose(bool disposing)
*****************************************END*************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Linq;
using System.Web;

namespace COM
{
    /// <summary>
    /// 串口简单包装类
    /// </summary>
    public class SerialPortWrapper : IDisposable
    {
        /// <summary>
        /// 串口对象
        /// </summary>
        private SerialPort serialPort;

        /// <summary>
        /// 是否已释放
        /// </summary>
        private bool disposed;

        /// <summary>
        /// 是否正在接收数据
        /// </summary>
        private bool isReceivingData;

        /// <summary>
        /// 是否正在试图关闭串口
        /// </summary>
        private bool isTryToClose;

        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event SerialDataReceivedEventHandler OnDataReceived;


        /// <summary>
        /// 获取或设置串口的端口
        /// </summary>
        public string portname
        {
            get { return serialPort.PortName; }
            set
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置串口的波特率
        /// </summary>
        public int baudrate
        {
            get { return serialPort.BaudRate; }
            set { serialPort.BaudRate = value; }
        }

        /// <summary>
        /// 获取或设置串口的数据位
        /// </summary>
        public int databits
        {
            get { return serialPort.DataBits; }
            set { serialPort.DataBits = value; }
        }

        /// <summary>
        /// 获取或设置串口的停止位
        /// </summary>
        public StopBits stopbits
        {
            get { return serialPort.StopBits; }
            set { serialPort.StopBits = value; }
        }

        /// <summary>
        /// 获取或设置串口的检验位
        /// </summary>
        public Parity parity
        {
            get { return serialPort.Parity; }
            set { serialPort.Parity = value; }
        }

        /// <summary>
        /// 串口是否已经打开
        /// </summary>
        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        /// <summary>
        /// 设置串口通讯中结束符的值
        /// </summary>
        public string NewLine
        {
            get { return serialPort.NewLine; }
            set { serialPort.NewLine = value; }
        }

        /// <summary>
        /// 获取接收到的字节的长度
        /// </summary>
        public int BytesToRead
        {
            get { return serialPort.BytesToRead; }
        }

        /// <summary>
        /// 获取或设置触发Read事件前要求的缓存中的可用字节数
        /// </summary>
        public int ReceivedBytesThreshold
        {
            get { return serialPort.ReceivedBytesThreshold; }
            set { serialPort.ReceivedBytesThreshold = value; }
        }


        /// <summary>
        /// 构造串口简单包装类
        /// </summary>
        public SerialPortWrapper()
            : this("COM1", 9600, 8)
        {
        }

        /// <summary>
        /// 构造串口简单包装类
        /// </summary>
        /// <param name="portName">端口(如COM1)</param>
        public SerialPortWrapper(string portName)
            : this(portName, 9600, 8)
        {
        }

        /// <summary>
        /// 构造串口简单包装类
        /// </summary>
        /// <param name="portName">端口(如COM1)</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        public SerialPortWrapper(string portName, int baudRate, int dataBits)
        {
            serialPort = new SerialPort(portName, baudRate, Parity.None, dataBits);
            serialPort.DataReceived += delegate(object sender, SerialDataReceivedEventArgs e)
            {
                if (isTryToClose) return;

                isReceivingData = true;

                if (OnDataReceived != null)
                {
                    OnDataReceived(sender, e);
                }

                isReceivingData = false;
            };
        }

        /// <summary>
        /// 构造串口简单包装类
        /// </summary>
        /// <param name="portName">端口(如COM1)</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">检验位</param>
        /// <param name="dataBits">数据位</param>
        public SerialPortWrapper(string portName, int baudRate, Parity parity, int dataBits,StopBits stopbits)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopbits);
            serialPort.DataReceived += delegate(object sender, SerialDataReceivedEventArgs e)
            {
                if (isTryToClose) return;

                isReceivingData = true;

                if (OnDataReceived != null)
                {
                    OnDataReceived(sender, e);
                }

                isReceivingData = false;
            };
        }

        /// <summary>
        /// 构造串口简单包装类
        /// </summary>
        /// <param name="portName">端口(如COM1)</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">检验位</param>
        /// <param name="dataBits">数据位</param>
        public SerialPortWrapper(string portName, int baudRate, string sparity, int dataBits, string sstopbits)
        {
            Parity parity;
            StopBits stopbits;
            switch (sparity)
            {
                case "None":
                    parity = Parity.None;
                    break;
                case "Space":
                    parity = Parity.Space;
                    break;
                case "Mark":
                    parity = Parity.Mark;
                    break;
                case "Odd":
                    parity = Parity.Odd;
                    break;
                case "Even":
                    parity = Parity.Even;
                    break;
                    default:
                    return;
            }
            switch (sstopbits)
            {
                case "0":
                    stopbits = StopBits.None;
                    break;
                case "1":
                    stopbits = StopBits.One;
                    break;
                case "1.5":
                    stopbits = StopBits.OnePointFive;
                    break;
                case "2":
                    stopbits = StopBits.Two;
                    break;
                default:
                    return;
            }
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopbits);
            serialPort.DataReceived += delegate(object sender, SerialDataReceivedEventArgs e)
            {
                if (isTryToClose) return;

                isReceivingData = true;

                if (OnDataReceived != null)
                {
                    OnDataReceived(sender, e);
                }

                isReceivingData = false;
            };
        }

        ~SerialPortWrapper()
        {
            Dispose(false);
        }


        /// <summary>
        /// 打开串口
        /// </summary>
        public void Open()
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            isReceivingData = false;

            if (!serialPort.IsOpen)
            {
                isTryToClose = false; // 将关闭标志重置
                serialPort.Open();
                //try
                //{
                //    serialPort.Open();
                //}
                //catch {
                //    Exception ex = new Exception("Open COM Failed!");
                //    throw ex;
                //}
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            // 打开关闭标志，这样在Invoke前检测到关闭标志就不会进行Invoke调用
            isTryToClose = true;

            // 在数据处理中时，等待Invoke的调用结束
            while (isReceivingData)
            {
                Application.DoEvents();
            }
            serialPort.Close();
        }

        /// <summary>
        /// 读取串口接收缓冲区中的字节数据
        /// </summary>
        /// <returns>读取到的字节的长度</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            int length = 0;
            if (serialPort.IsOpen && !isTryToClose)
            {
                length = serialPort.Read(buffer, offset, count);
            }
            return length;
        }

        /// <summary>
        /// 读取串口接收缓冲区中的一行数据
        /// </summary>
        /// <returns>以SerialPort的NewLine定义为结束符的一行字符</returns>
        public string ReadLine()
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            string line = null;
            if (serialPort.IsOpen && !isTryToClose)
            {
                line = serialPort.ReadLine();
            }
            return line;
        }

        /// <summary>
        /// 读取接收缓冲区中所有可用的字符
        /// </summary>
        /// <returns>缓冲区中的字符串</returns>
        public string ReadExisting()
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            string existing = null;
            if (serialPort.IsOpen && !isTryToClose)
            {
                existing = serialPort.ReadExisting();
            }
            return existing;
        }

        /// <summary>
        /// 将指定的文本写入串行端口
        /// </summary>
        /// <param name="text">文本</param>
        public void Write(string text)
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            if (serialPort.IsOpen && !isTryToClose)
            {
                serialPort.Write(text);
            }
        }

        /// <summary>
        /// 将制定的字节数组写入串行端口
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="offset">起始索引</param>
        /// <param name="count">写入长度</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            if (serialPort.IsOpen && !isTryToClose)
            {
                serialPort.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// 将制定的字符数组写入串行端口
        /// </summary>
        /// <param name="buffer">字符数组</param>
        /// <param name="offset">起始索引</param>
        /// <param name="count">写入长度</param>
        public void Write(char[] buffer, int offset, int count)
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            if (serialPort.IsOpen && !isTryToClose)
            {
                serialPort.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// 将指定的文本和默认的换行符写入串行端口
        /// </summary>
        /// <param name="text">文本</param>
        public void WriteLine(string text)
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            if (serialPort.IsOpen && !isTryToClose)
            {
                serialPort.WriteLine(text);
            }
        }

        /// <summary>
        /// 丢弃串口缓冲区的数据
        /// </summary>
        public void DiscardInBuffer()
        {
            if (disposed)
            {
                throw new Exception("此串口对象的实例已释放，无法调用!");
            }

            if (serialPort.IsOpen)
            {
                serialPort.DiscardInBuffer();
            }
        }


        #region IDisposable 成员

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">显示释放(手动指定释放资源)</param>
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.Close();
                    serialPort = null;
                }
                disposed = true;
            }
        }

        #endregion
    }
}