using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace wcf_Parkingsingle
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IServiceParkingSingle" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IServerParkingSingleCallback))]
    public interface IServiceParkingSingle
    {
        [OperationContract]
        void Connect();

        [OperationContract]
        void Disconnect();
        [OperationContract]
        void SendCount();
    }
    public interface IServerParkingSingleCallback
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(int free, int all);
    }
}
