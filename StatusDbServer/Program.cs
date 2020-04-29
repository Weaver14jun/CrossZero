using Npgsql;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace StatusDbServer
{
    class Program
    {
        public static List<ServerModel> GetServerList(string cs)
        {
            List<ServerModel> ServerList = new List<ServerModel>();
            using (NpgsqlConnection con = new NpgsqlConnection(cs))//("Server = 127.0.0.1; User Id = postgres; " + "Password=a54g5x; Database=sldb;"))
            {
                //var serverListTemp = new List<ServerModel>();
                con.Open();
                string stm = "SELECT * FROM serverlisttable";
                using (var cmd = new NpgsqlCommand(stm, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ServerList.Add(new ServerModel
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                Status = bool.Parse(reader["status"].ToString()),
                                IpAddress = reader["ipaddress"].ToString()
                            });
                        }
                        reader.Close();
                    }
                }
                con.Close();
                return ServerList;
            }
        }

        public static void UpdateServerStatus(List<ServerModel> serverModelList)
        {
            using (var ping = new Ping())
            {
                PingReply pingReply;
                foreach (var item in serverModelList)
                {
                    pingReply = ping.Send(item.IpAddress);
                    if (pingReply.Status == IPStatus.Success)
                    {
                        item.Status = true;
                    }
                    else
                    {
                        item.Status = false;
                    }
                    var t = pingReply.Status;
                }
            }
        }

        static void Main(string[] args)
        {
            var serverList = GetServerList("Server = 127.0.0.1; Port = 5433; User Id = postgres; " + "Password=a54g5x; Database=sldb;");
            UpdateServerStatus(serverList);
        }
    }
}
