using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GreeterSocketServer
{
    public class Program
    {
        static int[][] state = new int[64][];
        public static bool[] playerTurnState = new bool[64];
        public static bool[] stateChanged = new bool[64];
        public static ClientObject[] firstPlayers = new ClientObject[64];
        public static ClientObject[] secondPlayers = new ClientObject[64];


        static void Main(string[] args)
        {
            //Socket start
            const int socketPort = 8888;
            TcpListener listener = null;
            bool playerTurn = false;
            //List<int[]> state = new List<int[]>();

            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), socketPort);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    byte[] t = new byte[1];
                    client.Client.Receive(t);

                    var curState = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                    
                    bool firstOrSecondFlag = false;
                    if (t[0] != 0)
                    {
                        int rNum = Convert.ToInt32(t[0]);
                        if (state[rNum] == null)
                        {
                            curState = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            state[rNum] = curState;
                            playerTurn = true;
                            playerTurnState[rNum] = true;
                            firstOrSecondFlag = true;

                        }
                        else
                        {
                            firstOrSecondFlag = false;
                            curState = state[rNum];
                        }
                    }
                    else
                    {

                        //curState = state[]
                    }
                    ClientObject clientObject = new ClientObject(client, curState, Convert.ToInt32(t[0]), playerTurn, firstOrSecondFlag);

                    if (t[0] != 0)
                    {
                        int rNum = Convert.ToInt32(t[0]);
                        if (firstPlayers[Convert.ToInt32(t[0])] == null)
                        {
                            firstPlayers[Convert.ToInt32(t[0])] = clientObject;

                        }
                        else
                        {
                            secondPlayers[Convert.ToInt32(t[0])] = clientObject;
                        }
                    }

                    //clientObject.state = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    clientObject.roomNum = t[0];
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
            //Socket end
        }
    }
}
