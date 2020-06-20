using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;


namespace LahoreSocketAsync
{
    public class MyServer
    {


        public static int mPort = 23000;
        public static string ipAddr = "192.168.0.2";
        public static IPAddress mIp = IPAddress.Parse(ipAddr);
        public static TcpListener mTCPListener;
        public static List<TcpClient> mClients = new List<TcpClient>();
        public static bool KeepRunning;
        public static async void StartListeningForIncomingConnection()
        {
            System.Diagnostics.Debug.WriteLine(string.Format("IP Address: {0} - Port: {1}", MyServer.mIp.ToString(), MyServer.mPort));
            mTCPListener = new TcpListener(mIp, mPort);
            try
            {

                mTCPListener.Start();
                KeepRunning = true;
                while (KeepRunning)
                {
                    var returnedByAccept = await mTCPListener.AcceptTcpClientAsync();
                    mClients.Add(returnedByAccept);
                    System.Diagnostics.Debug.WriteLine(string.Format("Client connected success, count: {0} - {1}", mClients.Count, returnedByAccept.Client.RemoteEndPoint));
                    TakeCareOfTcpClient(returnedByAccept);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public static void StopServer()
        {
            try
            {
                if (mTCPListener != null)
                {
                    mTCPListener.Stop();
                }
                foreach (TcpClient c in mClients)
                {
                    c.Close();
                }
                System.Diagnostics.Debug.WriteLine("Server Closed");
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("stop error");
            }
        }

        public static async void TakeCareOfTcpClient(TcpClient x)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            try
            {
                stream = x.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[64];
                while (KeepRunning)
                {
                    int nRet = await reader.ReadAsync(buff, 0, buff.Length);
                    System.Diagnostics.Debug.WriteLine(nRet);
                    if (nRet == 0)
                    {
                        RemoveClient(x);
                        System.Diagnostics.Debug.WriteLine("Socket disconnected");
                        break;
                    }
                    string receivedText = new string(buff);
                    System.Diagnostics.Debug.WriteLine(receivedText);
                    Array.Clear(buff, 0, buff.Length);

                }
            }
            catch (Exception e)
            {
                RemoveClient(x);
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public static void RemoveClient(TcpClient x)
        {
            if (mClients.Contains(x))
            {
                mClients.Remove(x);
                System.Diagnostics.Debug.WriteLine(String.Format("Client Removed, count: {0}", mClients.Count));
            }
        }
        public static async void SendToAll(string m)
        {

            if (String.IsNullOrEmpty(m))
            {
                return;
            }
            try
            {
                byte[] buffMessage = Encoding.ASCII.GetBytes(m);
                foreach (TcpClient c in mClients)
                {
                    c.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
    }
}
