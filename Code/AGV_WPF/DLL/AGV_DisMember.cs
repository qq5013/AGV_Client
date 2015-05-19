using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AGV_WPF_DisMember
{
    public class AGV_DisMember : INotifyPropertyChanged
    {
        public string txtAGVNum { get; set; }
        public string txtWL { get; set; }
        public string txtTrafficNum { get; set; }
        public string txtTrafficState { get; set; }
        public string txtStatus { get; set; }
        public string txtWorkLine { get; set; }
        public string txtLineNum { get; set; }
        public string txtMarkNum { get; set; }
        public string txtMarkFunction { get; set; }
        public string txtSpeed { get; set; }
        public string txtDockNum { get; set; }
        public byte txtPower { get; set; }
        public int txtagvCharge { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AGV_DisMember()
        {
            txtAGVNum = "AGV?";
            txtWL = "失败";
            txtTrafficNum = "0";
            txtTrafficState = "初始化...";
            txtStatus = "初始化...";
            txtWorkLine = "0";
            txtLineNum = "0";
            txtMarkNum = "0";
            txtMarkFunction = "初始化...";
            txtSpeed = "初始化...";
            txtDockNum = "0";
            txtPower = 0;
            txtagvCharge = 0;
        }

        public string txtAGVNumValue
        {
            get { return this.txtAGVNum; }
            set
            {
                if (value != this.txtAGVNum)
                {
                    this.txtAGVNum = value;
                    NotifyPropertyChanged("txtAGVNum");
                }
            }
        }

        public string txtWLValue
        {
            get { return this.txtWL; }
            set
            {
                if (value != this.txtWL)
                {
                    this.txtWL = value;
                    NotifyPropertyChanged("txtWL");
                }
            }
        }

        public string txtTrafficNumValue
        {
            get { return this.txtTrafficNum; }
            set
            {
                if (value != this.txtTrafficNum)
                {
                    this.txtTrafficNum = value;
                    NotifyPropertyChanged("txtTrafficNum");
                }
            }
        }

        public string txtTrafficStateValue
        {
            get { return this.txtTrafficState; }
            set
            {
                if (value != this.txtTrafficState)
                {
                    this.txtTrafficState = value;
                    NotifyPropertyChanged("txtTrafficState");
                }
            }
        }

        public string txtStatusValue
        {
            get { return this.txtStatus; }
            set
            {
                if (value != this.txtStatus)
                {
                    this.txtStatus = value;
                    NotifyPropertyChanged("txtStatus");
                }
            }
        }

        public string txtWorkLineValue
        {
            get { return this.txtWorkLine; }
            set
            {
                if (value != this.txtWorkLine)
                {
                    this.txtWorkLine = value;
                    NotifyPropertyChanged("txtWorkLine");
                }
            }
        }

        public string txtLineNumValue
        {
            get { return this.txtLineNum; }
            set
            {
                if (value != this.txtLineNum)
                {
                    this.txtLineNum = value;
                    NotifyPropertyChanged("txtLineNum");
                }
            }
        }

        public string txtMarkNumValue
        {
            get { return this.txtMarkNum; }
            set
            {
                if (value != this.txtMarkNum)
                {
                    this.txtMarkNum = value;
                    NotifyPropertyChanged("txtMarkNum");
                }
            }
        }

        public string txtMarkFunctionValue
        {
            get { return this.txtMarkFunction; }
            set
            {
                if (value != this.txtMarkFunction)
                {
                    this.txtMarkFunction = value;
                    NotifyPropertyChanged("txtMarkFunction");
                }
            }
        }

        public string txtSpeedValue
        {
            get { return this.txtSpeed; }
            set
            {
                if (value != this.txtSpeed)
                {
                    this.txtSpeed = value;
                    NotifyPropertyChanged("txtSpeed");
                }
            }
        }

        public string txtDockNumValue
        {
            get { return this.txtDockNum; }
            set
            {
                if (value != this.txtDockNum)
                {
                    this.txtDockNum = value;
                    NotifyPropertyChanged("txtDockNum");
                }
            }
        }

        public byte txtPowerValue
        {
            get { return this.txtPower; }
            set
            {
                if (value != this.txtPower)
                {
                    this.txtPower = value;
                    NotifyPropertyChanged("txtPower");
                }
            }
        }

        public int txtagvChargeValue
        {
            get { return this.txtagvCharge; }
            set
            {
                if (value != this.txtagvCharge)
                {
                    this.txtagvCharge = value;
                    NotifyPropertyChanged("txtagvCharge");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }
    }
}
