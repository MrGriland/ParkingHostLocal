using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wcf_Parking
{
    public class OrderInfo
    {
        public int OrderInfo_ID;
        public int OrderInfo_Transport;
        public string OrderInfo_TransportMark;
        public string OrderInfo_TransportModel;
        public string OrderInfo_Number;
        public int OrderInfo_Creator;
        public string OrderInfo_CreatorLogin;
        public string OrderInfo_CreationDate;
        public string OrderInfo_EndingDate;
        public double OrderInfo_Sum;
        public bool OrderInfo_IsConfirmed;
        public string OrderInfo_Notification;
    }
}
