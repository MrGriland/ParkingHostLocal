using System;
using System.Collections.Generic;
using System.ServiceModel;
using wcf_Parking;

namespace wcf_Parkingsingle
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceParkingSingle : IServiceParkingSingle
    {
        List<ServerUser> users = new List<ServerUser>();
        ServerUser user;
        public void Connect()
        {
            user = new ServerUser()
            {
                operationContext = OperationContext.Current
            };
            users.Add(user);
            Console.WriteLine("Connected");
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnected");
            user = null;
        }

        public void SendCount()
        {
            foreach (var user in users)
                user.operationContext.GetCallbackChannel<IServerParkingSingleCallback>().MsgCallback(70, 200);
        }
    }
}
