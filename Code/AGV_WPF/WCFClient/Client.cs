using AGV_WPF;
using System.Linq;
using System;
using System.Collections.Generic;
using WcfDuplexMessageService;
using AGV_WPF.DLL.AGV;
using System.Reflection;

namespace WcfDuplexMessageClient
{
    public class PropertyChangedMessageEventArgs : EventArgs
    {
        public PropertyChangedMessageEventArgs(PropertyChangedMessage propertyMessage)
        {
            this.PropertyMessage = propertyMessage;
        }

        public PropertyChangedMessage PropertyMessage { get; private set; }
    }

    public class CarMessageEventArgs : EventArgs
    {
        public CarMessageEventArgs(AGVCar_WCF carMessage)
        {
            this.CarMessage = carMessage;
        }

        public AGVCar_WCF CarMessage { get; private set; }
    }

    public class SystemMessageEventArgs : EventArgs
    {
        public SystemMessageEventArgs(string systemMessage)
        {
            this.SystemMessage = systemMessage;
        }

        public string SystemMessage { get; private set; }
    }

    public class Client : IClient
    {
        public event EventHandler<PropertyChangedMessageEventArgs> PropChangedMessageRecevied;
        public event EventHandler<CarMessageEventArgs> CarMessageRecevied;
        public event EventHandler<SystemMessageEventArgs> SystemMessageReceived;
        public void SendPropertyChangedMessage(PropertyChangedMessage message)
        {
            if (PropChangedMessageRecevied != null)
            {
                PropChangedMessageRecevied(this, new PropertyChangedMessageEventArgs(message));
            }
        }

        public void SendCarMessage(AGVCar_WCF carMessage)
        {
            if (CarMessageRecevied != null)
            {
                CarMessageRecevied(this, new CarMessageEventArgs(carMessage));
            }
        }

        public void SendSystemMessage(string systemMessage)
        {
            if (SystemMessageReceived != null)
            {
                SystemMessageReceived(this, new SystemMessageEventArgs(systemMessage));
            }
        }
    }
}

