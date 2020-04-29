// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Npgsql;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        //public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        //{
        //    return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        //}

        //public override Task<Empty> SendPCList(Msg msg, ServerCallContext context)
        //{
        //    //return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        //    List<Normalizer.PCModel> mPCList = new List<Normalizer.PCModel>();
        //    foreach(var item in msg.PCList)
        //    {
        //        mPCList.Add(new Normalizer.PCModel
        //        {
        //            Id = item.Id,
        //            GraphicsCard = item.GraphicsCard,
        //            Motherboard = item.Motherboard,
        //            Os = item.Os,
        //            Processor = item.Processor,
        //            Ram = item.Ram
        //        });
        //    }
        //    Normalizer.PCTableNormalizer.SetPCList(mPCList, "Server = 127.0.0.1; User Id = postgres; " + "Password=a54g5x; Database=PC_DB;");
        //    return Task.FromResult(new Empty());
        //}
        int roomNmbr = 1;
        public override Task<RoomNumberReply> CreateRoom(Empty empty, ServerCallContext context)
        {
            return Task.FromResult(new RoomNumberReply() { RoomNumber = Convert.ToString(roomNmbr++)});
        }

        public override Task<Msg> GetServerList(Empty empty, ServerCallContext context)
        {
            string cs = "Server = 127.0.0.1; Port = 5433; User Id = postgres; " + "Password=a54g5x; Database=sldb;";
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
                                IpAddress = reader["ipaddress"].ToString(),
                                Status = bool.Parse(reader["status"].ToString())
                            });
                        }
                        reader.Close();
                    }
                }
                con.Close();
                var resServerList = UpdateServerStatus(ServerList);
                //Google.Protobuf.Collections.RepeatedField<Helloworld.ServerModel> protoServerModels = new Google.Protobuf.Collections.RepeatedField<Helloworld.ServerModel>();
                string res = "";
                foreach(var item in resServerList)
                {
                    //protoServerModels.Add(new Helloworld.ServerModel { Id = item.Id, IpAddress = item.IpAddress, Status = item.Status });
                    if (item.Status)
                    {
                        res += item.IpAddress + "/";
                    }
                }
                //return ServerList;
                return Task.FromResult(new Msg() { ServerList = res });
            }
        }

        public static List<ServerModel> UpdateServerStatus(List<ServerModel> serverModelList)
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
                }
            }
            return serverModelList;
        }

    }

    class Program
    {
        const int Port = 8080;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            //Console.WriteLine("Press any key to stop the server...");
            //Console.ReadKey();

            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
