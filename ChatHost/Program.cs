using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using wcf_Parking;

namespace ChatHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //Microsoft.Win32.RegistryKey Key =
            //Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            //"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
            ////добавляем первый параметр - название ключа  
            //// Второй параметр - это путь к   
            //// исполняемому файлу нашей программы.  
            //Key.SetValue("ChatHost", @"C:\Users\grish\OneDrive\Рабочий стол\wcf_chat\ChatHost\bin\Debug\ChatHost.exe");
            //Key.Close();

            ////удаляем  
            //Microsoft.Win32.RegistryKey key =
            //Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            //    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //key.DeleteValue("ChatHost", false);
            //key.Close();
            //using (var hostSingle = new ServiceHost(typeof(wcf_Parkingsingle.ServiceParkingSingle)))
            //{
                using (var host = new ServiceHost(typeof(wcf_Parking.ServiceParking)))
                {
                    host.Open();
                    Console.WriteLine("Хост стартовал!");
                    while (true)
                    {
                        string str = Console.ReadLine();
                        if (str == "shutdown")
                            break;
                    }
                }
            //}
        }
    }
}
