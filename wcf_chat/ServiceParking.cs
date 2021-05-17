using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;

namespace wcf_Parking
{
  
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ServiceParking : IServiceParking
    {
        string mainconnstring = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=localhost";
        List<OrderInfo> orderInfos = new List<OrderInfo>();
        List<UserInfo> userInfos = new List<UserInfo>();
        List<string> marks = new List<string>();
        List<string> models = new List<string>();
        ServerUser user;
        public void Connect()
        {
            user = new ServerUser(){ operationContext = OperationContext.Current };
            Console.WriteLine("Connected");
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnected");
            user = null;
        }

        public string SendDB(string login)
        {
            if (IsAdmin(login))
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    orderInfos.Clear();
                    connection.Open();
                    string select = "select o.OrderInfo_ID, o.OrderInfo_Transport, o.OrderInfo_Number, o.OrderInfo_Creator, o.OrderInfo_CreationDate, o.OrderInfo_EndingDate, t.TransportInfo_Mark, t.TransportInfo_Model, o.OrderInfo_IsConfirmed, u.UserInfo_Login from OrderInfo o join TransportInfo t on o.OrderInfo_Transport = t.TransportInfo_ID join UserInfo u on o.OrderInfo_Creator = u.UserInfo_ID";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        OrderInfo orderInfo = new OrderInfo();
                        orderInfo.OrderInfo_ID = Convert.ToInt32(dr[0]);
                        orderInfo.OrderInfo_Transport = Convert.ToInt32(dr[1]);
                        orderInfo.OrderInfo_Number = dr[2].ToString();
                        orderInfo.OrderInfo_Creator = Convert.ToInt32(dr[3]);
                        orderInfo.OrderInfo_CreationDate = dr[4].ToString();
                        orderInfo.OrderInfo_EndingDate = dr[5].ToString();
                        orderInfo.OrderInfo_TransportMark = dr[6].ToString();
                        orderInfo.OrderInfo_TransportModel = dr[7].ToString();
                        orderInfo.OrderInfo_Sum = (Convert.ToDateTime(dr[5]) - Convert.ToDateTime(dr[4])).TotalMinutes * 0.01;
                        orderInfo.OrderInfo_IsConfirmed = Convert.ToBoolean(dr[8]);
                        orderInfo.OrderInfo_CreatorLogin = dr[9].ToString();
                        orderInfos.Add(orderInfo);
                        //string json = JsonConvert.SerializeObject(orderInfos);
                        //user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    return JsonConvert.SerializeObject(orderInfos);
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    orderInfos.Clear();
                    connection.Open();
                    string select = "select t.TransportInfo_Mark, t.TransportInfo_Model, o.OrderInfo_Number, o.OrderInfo_CreationDate, o.OrderInfo_EndingDate, o.OrderInfo_IsConfirmed, o.OrderInfo_ID from OrderInfo o join TransportInfo t on o.OrderInfo_Transport = t.TransportInfo_ID join UserInfo u on o.OrderInfo_Creator=u.UserInfo_ID where u.UserInfo_Login = @login";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter loginParam = new SqlParameter("@login", login);
                    cmd.Parameters.Add(loginParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        OrderInfo orderInfo = new OrderInfo();
                        orderInfo.OrderInfo_TransportMark = dr[0].ToString();
                        orderInfo.OrderInfo_TransportModel = dr[1].ToString();
                        orderInfo.OrderInfo_Number = dr[2].ToString();
                        orderInfo.OrderInfo_CreationDate = dr[3].ToString();
                        orderInfo.OrderInfo_EndingDate = dr[4].ToString();
                        orderInfo.OrderInfo_Sum = (Convert.ToDateTime(dr[4])-Convert.ToDateTime(dr[3])).TotalMinutes*0.01;
                        orderInfo.OrderInfo_IsConfirmed = Convert.ToBoolean(dr[5]);
                        orderInfo.OrderInfo_ID = Convert.ToInt32(dr[6]);
                        orderInfos.Add(orderInfo);
                        //string json = JsonConvert.SerializeObject(orderInfos);
                        //user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    return JsonConvert.SerializeObject(orderInfos);
                }
            }
        }
        public bool IsAdmin(string login)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select UserInfo_IsAdmin from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read())
                {
                    if (dr[0].ToString() == "True")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        public bool TryLogin(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select * from UserInfo where UserInfo_Login = @login and UserInfo_Password = @password";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                loginParam = new SqlParameter("@password", password);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                if (dr.HasRows && dr[1].ToString() == login)
                {
                    if(IsAdmin(login))
                        Console.WriteLine("Logged as admin");
                    else
                        Console.WriteLine("Logged as user");
                    return true;
                }
                else
                {
                    Console.WriteLine("Login failed");
                    return false;
                }
            }
        }

        public bool TryRegister(string login, string password, string name, string surname)
        {
            if (!SearchLogin(login))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(mainconnstring))
                    {
                        connection.Open();
                        string insert = "insert into UserInfo values (@login,@password,@name,@surname,'false')";
                        SqlCommand cmd = new SqlCommand(insert, connection);
                        SqlParameter loginParam = new SqlParameter("@login", login);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@password", password);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@name", name);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@surname", surname);
                        cmd.Parameters.Add(loginParam);
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (SearchLogin(login))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        public bool SearchLogin(string login)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select * from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string SendMarks()
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                marks.Clear();
                connection.Open();
                string select = "select TransportInfo_Mark from TransportInfo group by TransportInfo_Mark order by TransportInfo_Mark";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    marks.Add(dr[0].ToString());
                    //string json = JsonConvert.SerializeObject(marks);
                    //user.operationContext.GetCallbackChannel<IServerChatCallback>().MarksCallback(json);
                }
                return JsonConvert.SerializeObject(marks);
            }
        }
        public string SendModels(string mark)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                models.Clear();
                connection.Open();
                string select = "select TransportInfo_Model from TransportInfo where TransportInfo_Mark = @mark";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter Param = new SqlParameter("@mark", mark);
                cmd.Parameters.Add(Param);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    models.Add(dr[0].ToString());
                    //string json = JsonConvert.SerializeObject(models);
                    //user.operationContext.GetCallbackChannel<IServerChatCallback>().ModelsCallback(json);
                }
                return JsonConvert.SerializeObject(models);
            }
        }

        public bool TryOrder(int transport, string number, int creator, string creationdate, string endingdate)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                        connection.Open();
                        string insert = "insert into OrderInfo values(@transport,@number,@creator,convert(datetime,@creationdate),convert(datetime,@endingdate),'false',NULL)";
                        SqlCommand cmd = new SqlCommand(insert, connection);
                        SqlParameter loginParam = new SqlParameter("@transport", transport);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@number", number);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@creator", creator);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@creationdate", creationdate);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@endingdate", endingdate);
                        cmd.Parameters.Add(loginParam);
                        SqlDataReader dr = cmd.ExecuteReader();
                        return true;
                }
            }
            catch
            {
                return false;
            }

        }

        public int GetTransport(string mark, string model)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select TransportInfo_ID from TransportInfo where TransportInfo_Mark = @mark and TransportInfo_Model = @model";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter Param = new SqlParameter("@mark", mark);
                cmd.Parameters.Add(Param);
                Param = new SqlParameter("@model", model);
                cmd.Parameters.Add(Param);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                return Convert.ToInt32(dr[0]);
            }
        }
        public int GetUserID(string login)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select UserInfo_ID from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                return Convert.ToInt32(dr[0]);
            }
        }
        public int GetCount()
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                connection.Open();
                string select = "select * from ParkingInfo";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                return Convert.ToInt32(dr[1]) - Convert.ToInt32(dr[0]);
            }
        }
        public string SendNotifications(string login)
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                orderInfos.Clear();
                connection.Open();
                string select = "select o.OrderInfo_ID, t.TransportInfo_Mark, t.TransportInfo_Model, o.OrderInfo_Number, o.OrderInfo_Notification from OrderInfo o join TransportInfo t on o.OrderInfo_Transport = t.TransportInfo_ID join UserInfo u on o.OrderInfo_Creator=u.UserInfo_ID where u.UserInfo_Login = @login and o.OrderInfo_Notification is not null";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    OrderInfo orderInfo = new OrderInfo();
                    orderInfo.OrderInfo_ID = Convert.ToInt32(dr[0]);
                    orderInfo.OrderInfo_TransportMark = dr[1].ToString();
                    orderInfo.OrderInfo_TransportModel = dr[2].ToString();
                    orderInfo.OrderInfo_Number = dr[3].ToString();
                    orderInfo.OrderInfo_Notification = dr[4].ToString();
                    orderInfos.Add(orderInfo);
                    //string json = JsonConvert.SerializeObject(orderInfos);
                    //user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                }
                return JsonConvert.SerializeObject(orderInfos);
            }
        }
        public bool ClearNotification(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    orderInfos.Clear();
                    connection.Open();
                    string select = "update OrderInfo set OrderInfo_Notification = null where OrderInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
            }
            catch { return false; }
        }
        public bool ChangeConfirmed(int transport, string number, string creationdate, string endingdate, bool isConfirmed, int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    connection.Open();
                    string select = "update OrderInfo set OrderInfo_Transport = @transport, OrderInfo_Number = @number, OrderInfo_CreationDate = convert(datetime,@creationdate), OrderInfo_EndingDate = convert(datetime,@endingdate), OrderInfo_IsConfirmed = @isConfirmed where OrderInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter transportParam = new SqlParameter("@transport", transport);
                    cmd.Parameters.Add(transportParam);
                    SqlParameter numberParam = new SqlParameter("@number", number);
                    cmd.Parameters.Add(numberParam);
                    SqlParameter creationdateParam = new SqlParameter("@creationdate", creationdate);
                    cmd.Parameters.Add(creationdateParam);
                    SqlParameter endingdateParam = new SqlParameter("@endingdate", endingdate);
                    cmd.Parameters.Add(endingdateParam);
                    SqlParameter icParam = new SqlParameter("@isConfirmed", isConfirmed);
                    cmd.Parameters.Add(icParam);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
        }
            catch
            {
                return false;
            }
        }

        public bool DeleteUnconfirmed(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    connection.Open();
                    string select = "delete from OrderInfo where OrderInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public string SendUsers()
        {
            using (SqlConnection connection = new SqlConnection(mainconnstring))
            {
                userInfos.Clear();
                connection.Open();
                string select = "select u.UserInfo_ID, u.UserInfo_Login, u.UserInfo_Name, u.UserInfo_Surname, u.UserInfo_IsAdmin, (select count(*) from OrderInfo where OrderInfo_Creator = UserInfo_ID) from UserInfo u";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserInfo_ID = Convert.ToInt32(dr[0]);
                    userInfo.UserInfo_Login = dr[1].ToString();
                    userInfo.UserInfo_Name = dr[2].ToString();
                    userInfo.UserInfo_Surname = dr[3].ToString();
                    if(Convert.ToBoolean(dr[4]))
                        userInfo.UserInfo_IsAdmin = "Администратор";
                    else
                        userInfo.UserInfo_IsAdmin = "Пользователь";
                    userInfo.UserInfo_Count = Convert.ToInt32(dr[5]);
                    userInfos.Add(userInfo);
                }
                return JsonConvert.SerializeObject(userInfos);
            }
        }
        public bool TryToConfirm(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    connection.Open();
                    string select = "update OrderInfo set OrderInfo_IsConfirmed = 'true' where OrderInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool TryToAdmin(bool isadmin,int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    connection.Open();
                    string select = "update UserInfo set UserInfo_IsAdmin = @isadmin where UserInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter isParam = new SqlParameter("@isadmin", isadmin);
                    cmd.Parameters.Add(isParam);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool TryToNotify(string message,int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(mainconnstring))
                {
                    connection.Open();
                    string select = "update OrderInfo set OrderInfo_Notification = @message where OrderInfo_ID = @id";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter mParam = new SqlParameter("@message", message);
                    cmd.Parameters.Add(mParam);
                    SqlParameter idParam = new SqlParameter("@id", id);
                    cmd.Parameters.Add(idParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
