using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace GreeterSocketServer
{
    public class ClientObject
    {
        public TcpClient client;
        public int[][] state = new int[64][];
        public bool isHost = false;
        public int roomNum;
        public bool playerTurn = true;
        NetworkStream _stream;
        public ClientObject(TcpClient tcpClient, int[] _state, int roomN, bool turnFlag, bool _isHost)
        {
            client = tcpClient;
            state[roomN] = _state;
            roomNum = roomN;
            playerTurn = turnFlag;
            isHost = _isHost;
        }

        public void SendRoomState()
        {
            if(Program.stateChanged[roomNum] == true)
            {
                string stringState = "";
                for (int i = 0; i < 9; i++)
                {
                    stringState += state[roomNum][i];
                }
                string message = roomNum + ":" + stringState;
                byte[] data = Encoding.Unicode.GetBytes(message);
                _stream.Write(data, 0, data.Length);
                Program.stateChanged[roomNum] = false;
            }
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                _stream = stream;
                byte[] data = new byte[64]; // буфер для получаемых данных
                while (true)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    while (isHost != Program.playerTurnState[roomNum])
                    {
                        if (Program.stateChanged[roomNum])
                        {
                            SendRoomState();
                        }
                    }
                    do
                    {
                        SendRoomState();
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    int msgRoomNum = Convert.ToInt32(message.Split(":")[0]);
                    int hod = Convert.ToInt32(message.Split(":")[1]);
                    bool playerFlag = Convert.ToBoolean(message.Split(":")[2]);

                    Program.stateChanged[roomNum] = true;

                    if (playerFlag == Program.playerTurnState[msgRoomNum])
                    {
                        if (isHost)
                        {
                            state[msgRoomNum][hod - 1] = 1;
                        }
                        else
                        {
                            state[msgRoomNum][hod - 1] = 2;
                        }
                        string stringState = "";
                        for (int i = 0; i < 9; i++)
                        {
                            stringState += state[msgRoomNum][i];
                        }
                        Console.WriteLine(message);
                        // отправляем обратно сообщение в верхнем регистре
                        //message = message.Substring(message.IndexOf(':') + 1).Trim().ToUpper();



                        message = msgRoomNum + ":" + stringState;
                        playerTurn = !playerTurn;
                        Program.playerTurnState[msgRoomNum] = !Program.playerTurnState[msgRoomNum];

                        if (playerFlag)
                        {
                            Program.secondPlayers[msgRoomNum].SendRoomState();
                        }
                        else
                        {
                            Program.firstPlayers[msgRoomNum].SendRoomState();
                        }
                    }
                    else
                    {
                        message = "This is not your turn";
                    }
                    

                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
}