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
using Grpc.Core;
using Helloworld;
using Google.Protobuf;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            const int port = 8080;
            const int socketPort = 8888;
            string address = "109.120.151.33";
            int roomNumber = 0;
            TcpClient socketClient = null;
            NetworkStream stream = null;
            Channel channel = new Channel($"{address}:{port}", ChannelCredentials.Insecure);
            bool playerFlag = false;

            var client = new Greeter.GreeterClient(channel);
            var ServerList = client.GetServerList(new Empty()).ServerList;
            var listSL = ServerList.Split('/');

            int serverCount = 1;
            foreach(var item in listSL)
            {
                if(item != "")
                    Console.WriteLine($"{serverCount}){item}");

            }
            var ans = Console.ReadLine();
            if(ans == "test")
            {
                address = "127.0.0.1";
            }
            else
            {
                address = Convert.ToString(listSL.GetValue(Convert.ToInt32(ans)-1));
            }

            //String user = "you";

            string flag = "";
            Console.WriteLine("1.Create room");
            Console.WriteLine("2.Connect to room");
            Console.WriteLine("3.Quit");
            //Console.Write(userName + ": ");
            flag = Console.ReadLine();
            switch (flag)
            {
                case "1":
                    var t = client.CreateRoom(new Empty());
                    Console.WriteLine($"Your room number is {t.RoomNumber}");
                    roomNumber = Convert.ToInt32(t.RoomNumber);


                    try
                    {
                        socketClient = new TcpClient(address, socketPort);
                        byte[] byt = new byte[1];
                        var roomNumberByte = Convert.ToByte(t.RoomNumber);
                        byt[0] = roomNumberByte;
                        socketClient.Client.Send(byt);
                        stream = socketClient.GetStream();
                        Console.WriteLine("We get a stream!");

                        playerFlag = true;

                        //string message = "Hosted";
                        //byte[] data = Encoding.Unicode.GetBytes(message);
                        //stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "2":
                    try
                    {
                        Console.WriteLine("Enter room number");
                        int roomN = Convert.ToInt32(Console.ReadLine());
                        roomNumber = roomN;
                        socketClient = new TcpClient(address, socketPort);
                        byte[] byt = new byte[1];
                        byt[0] = Convert.ToByte(roomN);
                        socketClient.Client.Send(byt);
                        stream = socketClient.GetStream();
                        Console.WriteLine("We get a stream!");

                        playerFlag = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    break;
                case "3":
                    Environment.Exit(1);
                    break;
            }

            Console.WriteLine("E E E");
            Console.WriteLine("E E E");
            Console.WriteLine("E E E");

            string stringState = "0000000000";

            bool turn = playerFlag;
            while (true)
            {



                byte[] data = new byte[64]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                string message = "";
                if (!turn)
                {
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        message = builder.ToString();
                        turn = true;
                        continue;
                    }
                    while (stream.DataAvailable);
                }
                else
                {
                    string playerMove = "";
                    while (true)
                    {
                        playerMove = Console.ReadLine();
                        if (stringState[Convert.ToInt32(playerMove) -1] != '0')
                        {
                            Console.WriteLine("That field is engaged");
                        }
                        else
                            break;
                    }
                    message = $"{roomNumber}:{playerMove}:{playerFlag}";

                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        message = builder.ToString();
                    }

                    while (stream.DataAvailable);


                    if (message == "This is not your turn")
                    {
                        Console.WriteLine(message);
                        continue;
                    }
                    if (message == "")
                    {
                        Console.WriteLine("This is not your turn");
                        continue;
                    }
                    turn = false;
                }
                int msgRoomNum = Convert.ToInt32(message.Split(":")[0]);
                stringState = message.Split(":")[1];
                
                if (msgRoomNum == roomNumber)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Console.WriteLine();
                        for (int i = 0; i < 3; i++)
                        {
                            if (stringState[i + k*3] == '0')
                            {
                                Console.Write("E ");
                            }
                            else
                            {
                                if (stringState[i + k*3] == '1')
                                {
                                    Console.Write("X ");
                                }
                                else
                                {
                                    if (stringState[i + k*3] == '2')
                                    {
                                        Console.Write("0 ");
                                    }
                                }
                            }
                        }
                    }
                    //Console.WriteLine("Сервер: {0}", message);
                }
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
