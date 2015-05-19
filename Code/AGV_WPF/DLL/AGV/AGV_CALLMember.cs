using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AGV_CALL_Info
{
    public class AGV_CALLMember : INotifyPropertyChanged
    {
        public int iNO { get; set; }
        public DateTime dtTime { get; set; }
        public int iStationNum { get; set; }
        public int iMaterialNum { get; set; }
        public string sMaterialName { get; set; }
        public int iLineNum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AGV_CALLMember()
        {
            iNO = 0;
            dtTime = new DateTime();
            iStationNum = 0;
            iMaterialNum = 0;
            sMaterialName = "";
        }

        public int iNOValue
        {
            get { return this.iNO; }
            set
            {
                if (value != this.iNO)
                {
                    this.iNO = value;
                    NotifyPropertyChanged("iNO");
                }
            }
        }

        public DateTime dtTimeValue
        {
            get { return this.dtTime; }
            set
            {
                if (value != this.dtTime)
                {
                    this.dtTime = value;
                    NotifyPropertyChanged("dtTime");
                }
            }
        }

        public int iStationNumValue
        {
            get { return this.iStationNum; }
            set
            {
                if (value != this.iStationNum)
                {
                    this.iStationNum = value;
                    NotifyPropertyChanged("iStationNum");
                }
            }
        }

        public int iMaterialNumValue
        {
            get { return this.iMaterialNum; }
            set
            {
                if (value != this.iMaterialNum)
                {
                    this.iMaterialNum = value;
                    NotifyPropertyChanged("iMaterialNum");
                }
            }
        }

        public string sMaterialNameValue
        {
            get { return this.sMaterialName; }
            set
            {
                if (value != this.sMaterialName)
                {
                    this.sMaterialName = value;
                    NotifyPropertyChanged("sMaterialName");
                }
            }
        }

        public int iLineNumValue
        {
            get { return this.iLineNum; }
            set
            {
                if (value != this.iLineNum)
                {
                    this.iLineNum = value;
                    NotifyPropertyChanged("iLineNum");
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
    }
}
