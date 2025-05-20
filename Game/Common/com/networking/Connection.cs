using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common.com.game.settings;
using ICSharpCode.SharpZipLib.GZip;

namespace Common.com.networking
{
    public sealed class Connection
    {
        private readonly string mIp;
        private readonly ushort mPort;
        private TcpClient mTcpClient;
        private NetworkStream mTcpStream;
        private readonly IPAddress mIpAddress;
        private List<IPacketListener> mListeners;
        private Thread mTcpListener;
        private bool mIsOpen;
        public string Ip => mIp;
        private bool IsOpen => mIsOpen;
        private const int BufferSize = 524288;
        /// <summary>
        ///Creates a new connection to the Server.
        /// It should be noted that no networking actually happens in this constructor
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Connection(string ip, ushort port)
        {
            mIp = ip;
            mPort = port;
            mIpAddress = GetIpAddress();
            mListeners = new List<IPacketListener>();
        }
        /// <summary>
        /// Opens the Tcp and the Udp connections.
        /// The tcp connection takes place on port
        /// If isClientSide is true port+1 is the udp receiving port and port+2 is the sending port
        /// else its the other way around
        /// </summary>
        /// <param name="client">The TcpClient to use. Leave null if none given</param>
        public void Connect(TcpClient client)
        {
          
            
            mTcpClient = client;
            if (client == null)
            {
                mTcpClient = new TcpClient(mIpAddress.ToString(), mPort);
            }

            mTcpClient.ReceiveBufferSize = BufferSize * NumberManager.Ten;
            mTcpClient.SendBufferSize = BufferSize * NumberManager.Ten;
            if (!mTcpClient.Connected)
            {
                mTcpClient.Connect(mIpAddress.ToString(), mPort);
            }

            mTcpStream = mTcpClient.GetStream();
            /*
            mUdpClientReceive = new UdpClient();
            mUdpClientReceive.Client    //This makes it possible to run both server and client on the same machine
                .SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            mUdpClientReceive.Connect(mEndpoint);
            mUdpClientSend = new UdpClient();
            mUdpClientSend.Client.      //This makes it possible to run both server and client on the same machine
                SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);
            mUdpClientSend.Connect(mIpAddress.ToString(),mPort+1+(isClientSide?1:0));*/
            mIsOpen = true;
        }
        /// <summary>
        ///Closes all connections and removes all Listeners
        /// </summary>
        public void Disconnect()
        {
            if (IsOpen)
            {
                mIsOpen = false;
                foreach (var listener in mListeners)
                {
                    listener.OnDisconnect(this);
                }
                mListeners = new List<IPacketListener>();
                if (mTcpListener != null)
                {
                    //mUdpListener.Interrupt();
                    mTcpListener.Interrupt();
                    //mUdpListener = 
                        mTcpListener = null;
                }

                try
                {
                    mTcpClient.Close();
                    /*
                    try
                    {
                        mUdpClientReceive.Close();
                    }
                    catch (ThreadInterruptedException)
                    {
                        // empty
                    }*/
                }
                catch (ThreadInterruptedException)
                {
                    // empty
                }
                //mUdpClientSend.Close();
                Console.WriteLine("closing tcp");
                Debug.WriteLine("closing tcp");
                mTcpStream.Close();
                
            }

        }
        /// <summary>
        /// Returns a connection to the localhost and starts it
        /// </summary>
        /// <returns>A Connection to the localhost</returns>
        public static Connection SelfConnect(ushort port)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var c = new Connection(ip.ToString(),port);
                    c.Connect(null);
                    return c;
                }
            }

            return null;
        }
        /// <summary>
        /// adds the listener to the listeners, which will then be provided with packet updates.
        /// </summary>
        /// <param name="listener">the listener to be added</param>
        public void AddListener(IPacketListener listener)
        {
            mListeners.Add(listener);
            if (mTcpListener == null)
            {
                //mUdpListener = new Thread(GetUdpPackages);
                mTcpListener = new Thread(GetTcpPackages);
                //mUdpListener.Start();
                mTcpListener.Start();
            }
            
        }
        /// <summary>
        /// removes the listener from the listeners
        /// </summary>
        /// <param name="listener">the listener to be removed</param>
        public void RemoveListener(IPacketListener listener)
        {
            mListeners.Remove(listener);
        }
        /*
        /// <summary>
        /// Waits for new UDP packets which are then used by all IPacketListeners in mListeners
        /// </summary>
        private void GetUdpPackages()
        {
            while (mIsOpen)
            {
                byte[] str;
                try
                {
                    str = GetUdp();
                }
                catch (IOException)
                {
                    Disconnect();
                    return;
                }
                catch (SocketException)
                {
                    Disconnect();
                    return;
                }

                if (str.Length != 0)
                {
                    for(var i=mListeners.Count-1;i>=0;i--)
                    {
                        
                        mListeners[i].HandlePacket(str, this);
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }*/
        /// <summary>
        /// Waits for new TCP packets which are then used by all IPacketListeners in mListeners
        /// </summary>
        private void GetTcpPackages()
        {
            
            while (mIsOpen)
            {
                try
                {
                    byte[] str;
                    try
                    {
                        str = GetTcp();
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Disconnect();
                        return;
                    }
                    catch (IOException)
                    {
                        Disconnect();
                        return;
                    }
                    catch (SocketException)
                    {

                        Disconnect();
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        Disconnect();
                        return;
                    }
                    if (str.Length != 0)
                    {
                        for (var i = mListeners.Count - 1; i >= 0; i--)
                        {
                            mListeners[i].HandlePacket(str, this);
                        }
                    }
                    else
                    {
                        Disconnect();
                    }
                }
                catch (ArgumentException)//happens when Disconnecting
                {
                }
            }
        }

        ///  <summary>
        /// Sends the Raw byte array using UDP
        ///  </summary>
        ///  <param name="message">The message to be sent</param>
        private void SendRawUdp(IEnumerable message)
        {
            //mUdpClientSend.Send(message, message.Length);
            Console.WriteLine(message);
        }/*
        /// <summary>
        /// converts the ip to an IPEndPoint
        /// </summary>
        /// <returns>The IPEndPoint</returns>
        /// <exception cref="FormatException">When the Ip is invalid</exception>
        private IPEndPoint CreateIpEndPoint(int port)
        {
            return new IPEndPoint(mIpAddress, port);
        }*/

        private IPAddress GetIpAddress()
        {
            if(!IPAddress.TryParse(mIp, out var ip))
            {
                throw new FormatException("Invalid ip-address");
            }

            return ip;
        }
        /// <summary>
        /// Waits for a TCP package and then returns it
        /// </summary>
        /// <returns>The received packet</returns>
        private byte[] GetDecompressedTcp()
        {
            var lengthBytes = new byte[NumberManager.Four];
            mTcpStream.Read(lengthBytes, 0, NumberManager.Four);
            
            var res = new byte[BitConverter.ToInt32(lengthBytes, 0)];
            int toRead = res.Length;
            while (toRead>0)
            {
                int read = Math.Min(toRead, 1);
                mTcpStream.Read(res, res.Length-toRead, read);
                toRead -= read;
            }
            return Decompress(res);
        }
        /*
        /// <summary>
        /// Waits for a UDP package and then returns it
        /// </summary>
        /// <returns>The received packet</returns>
        private byte[] GetRawUdp()
        {
            return null;
            //return mUdpClientReceive.Receive(ref mEndpoint);
        }*/
        /// <summary>
        ///Waits for the next TCP Packet and translates it to a byte array
        /// </summary>
        /// <returns>The received byte array</returns>
        private byte[] GetTcp()
        {
            return GetDecompressedTcp();
        }/*
        /// <summary>
        ///Waits for the next UDP Packet and translates it to a byte array
        /// </summary>
        /// <returns>The received byte array</returns>
        private byte[] GetUdp()
        {
            var res=GetRawUdp();
            
            return GetContentOf(res);
        }*/
        /// <summary>
        ///Sends the Raw byte array using TCP
        /// </summary>
        /// <param name="message">The message to be sent</param>
        private void SendRawTcp(byte[] message)
        {
            byte[] res = new byte[NumberManager.Four+message.Length];
            BitConverter.GetBytes(message.Length).CopyTo(res,0);
            message.CopyTo(res,NumberManager.Four);
            mTcpStream.Write(res,0,res.Length);

        }
        /// <summary>
        /// Sends the message using TCP
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public void SendTcp(byte[] message)
        {
            
            SendRawTcp(
                Compress
                    (message));
        }
        /// <summary>
        /// Sends the message using UDP
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public void SendUdp(byte[] message)
        {
            SendRawUdp(
                Compress
                (message));
        }/*
        /// <summary>
        ///Decompresses the message using gzip and returns it as a byte array.
        /// </summary>
        /// <param name="message">The message to be translated</param>
        /// <returns>The translation</returns>
        private byte[] GetContentOf(byte[] message)
        {
            return 
                Decompress
                (message);
        }*/
        /// <summary>
        /// Decompresses the input using gzip.
        /// </summary>
        /// <param name="input">he bytes to be decompressed</param>
        /// <returns>the decompressed message</returns>
        private byte[] Decompress(byte[] input)
        {
            using var source = new MemoryStream(input);
            using var res = new MemoryStream();
            GZip.Decompress(source,res,false);
            var result = res.ToArray();
            return result;
        }

        /// <summary>
        /// Compresses the input using gzip
        /// </summary>
        /// <param name="input">the bytes to be compressed</param>
        /// <returns>the compressed bytes</returns>
        private byte[] Compress(byte[] input)
        {
            byte[] o;
            using (var result = new MemoryStream()){
                
                using (var compressionStream = new GZipOutputStream(result))
                {
                    compressionStream.Write(input);
                    
                    compressionStream.Finish();
                    compressionStream.Flush();
                }
            
                o=result.ToArray();
            }
            
            return o;
        }

    }
}