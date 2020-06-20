using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LahoreSocketAsync
{
    public class LahoreSocketClient
    {
        IPAddress mServerIPAddress;
        int mServerPort;
        TcpClient mClient;
        public LahoreSocketClient() {
            mServerPort = MyServer.mPort;
            mClient = null;
            mServerIPAddress = MyServer.mIp;
        }
        public IPAddress ServerIPAddress { get { return mServerIPAddress; } }
        public int ServerPort { get { return mServerPort; } }
        public bool SetServerIPAddress(string x) {
            IPAddress ipaddr = null;
            if (!IPAddress.TryParse(x, out ipaddr))
            {
                Console.WriteLine("Invalid server IP supplied");
                return false;
            }
            this.mServerIPAddress = ipaddr;
            return true;
        }
        public bool SetPortNumber(string x) {
            int portNumber = 0;
            if (!int.TryParse(x.Trim(), out portNumber)) {
                Console.WriteLine("Invalid port number supplied");
                return false;
            }
            if (portNumber <1 || portNumber > 65535) {
                Console.WriteLine("Port Number must be between 1 and 65535");
                return false;
            }
            this.mServerPort = portNumber;
            return true;
        }

        public void CloseAndDisconnect()
        {
            if (mClient != null && mClient.Connected)
            {
                mClient.Close();
            }
        }

        public async Task SendToServer(string strInputUser)
        {
            if (string.IsNullOrEmpty(strInputUser.Trim())) {
                Console.WriteLine("Empty string supplied. Try again");
                return;
            }
            if (mClient != null && mClient.Connected) {
                StreamWriter clientStreamWriter = new StreamWriter(mClient.GetStream());
                clientStreamWriter.AutoFlush = true;
                await clientStreamWriter.WriteAsync(strInputUser);
                Console.WriteLine("Data sent...");
            }
        }

        public async Task ConnectToServer() {
            if (this.mClient == null) {
                mClient = new TcpClient();
            }
            try {
                await mClient.ConnectAsync(mServerIPAddress, mServerPort);
                Console.WriteLine(string.Format("Connected to server IP/Port: {0} / {1}", mServerIPAddress, mServerPort));
                ReadDataAsync(mClient);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task ReadDataAsync(TcpClient mClient)
        {
            try {
                StreamReader clientStreamReader = new StreamReader(mClient.GetStream());
                char[] buff = new char[64];
                int readByteCount = 0;
                while (true) {
                    readByteCount=await clientStreamReader.ReadAsync(buff, 0, buff.Length);
                    if (readByteCount <= 0) {
                        Console.WriteLine("Disconnected from server");
                        mClient.Close();
                        break;
                    }
                    Console.WriteLine(string.Format("Received bytes: {0} - Message: {1}", readByteCount, new string(buff)));
                    Array.Clear(buff, 0, buff.Length);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
