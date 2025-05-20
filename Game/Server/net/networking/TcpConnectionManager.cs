using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common.com.networking;

namespace Server.net.networking
{
    public sealed class TcpConnectionManager
    {
        public List<Connection> Connections { get; } = new List<Connection>();

        private TcpListener mListener;
        private readonly IConnectionListener mConnectionListener;
        private readonly ushort mPort;
        private bool mRunning;
        public TcpConnectionManager(ushort port,IConnectionListener listener)
        {
            mPort = port;
            mConnectionListener = listener;
        }

        public void Start()
        {
            if (!mRunning)
            {
                var threadListening = new Thread(Listen);
                threadListening.Start();
                mRunning = true;
            }
        }
        public void Stop()
        {
            if (mRunning)
            {
                mRunning = false;
                if (mListener != null)
                {
                    try
                    {
                        mListener.Server.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException)
                    {
                        // empty 
                    }

                    mListener.Stop();
                }

                DisconnectAll();
                
            }
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new IOException("No network adapters with an IPv4 address in the system!");
        }

        private void DisconnectAll()
        {
            for (var i = Connections.Count - 1; i >= 0;i--)
            {
                Disconnect(Connections[i]);
            }
        }

        private void Disconnect(Connection connection)
        {
            Connections.Remove(connection);
            connection.Disconnect();
        }
        /// <summary>
        /// Makes the current Thread listen for incoming 
        /// </summary>
        /// <exception cref="FormatException"></exception>
        private void Listen()
        {
            if(!IPAddress.TryParse(GetLocalIpAddress(), out var ip))
            {
                throw new FormatException("Invalid local ip-address");
            }
            mListener = new TcpListener(ip,mPort);
            
            mListener.Start();
            while (mRunning)
            {
                try
                {
                    var client = mListener.AcceptTcpClient();
                    var ipAddress=((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                
                    var c = new Connection(ipAddress,mPort);
                    c.Connect(client);
                    mConnectionListener.OnConnection(c);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                    break;
                    
                }

                
            }
        }
    }
}