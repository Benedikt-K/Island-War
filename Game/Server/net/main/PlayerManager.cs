using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.com.game;
using Common.com.game.settings;
using Common.com.networking;
using Microsoft.Xna.Framework;
using Server.net.networking;
using Point = System.Drawing.Point;

namespace Server.net.main
{
    public class PlayerManager:IConnectionListener
    {

        public TcpConnectionManager ConnectionManager { get; }
        private readonly SortedSet<int> mPlayersAvailable;
        private int MaximumPlayers { get;}
        private List<Connection> Connections => ConnectionManager.Connections;
        private readonly Game1 mGame1;
        private readonly MessageWrapper mMessageWrapper;
        private readonly Dictionary<int, Connection> mPlayerNumToConnection=new Dictionary<int, Connection>();
        private static readonly TimeSpan sTimeOutTime=new TimeSpan(0,0,10);
        public PlayerManager(int maxPlayers, ushort port,Game1 game1,MessageWrapper messageWrapper)
        {
            mPlayersAvailable = new SortedSet<int>();
            for (var i = 0; i < maxPlayers; i++)
            {
                mPlayersAvailable.Add(i);
            }
            MaximumPlayers = maxPlayers;
            mMessageWrapper = messageWrapper;
            ConnectionManager = new TcpConnectionManager(port,this);
            ConnectionManager.Start();
            mGame1 = game1;
        }

        public int ToPlayerId(Connection c)
        {
            foreach (var i in mPlayerNumToConnection.Keys)
            {
                var connection = mPlayerNumToConnection[i];
                if (connection==c)
                {
                    return i ;
                }
            }

            return -1;
        }
        private int CurrentPlayers()
        {
            return Connections.Count;
        }

        private bool IsFull()
        {
            return CurrentPlayers() >= MaximumPlayers;
        }
        public Connection ToPlayer(int uid)
        {
            return mPlayerNumToConnection[uid];
        }
        public void OnConnection(Connection c)
        {
            Debug.WriteLine(c.Ip+" "+CurrentPlayers());
            if (!IsFull())
            {
                Connections.Add(c);
                var playerNum = mPlayersAvailable.First()+1;
                mPlayersAvailable.Remove(playerNum-1);
                mMessageWrapper.MessageReceived(playerNum);
                mPlayerNumToConnection[playerNum] = c;
                if (IsFull())
                {
                    mMessageWrapper.Pause(false);
                }
                
                c.AddListener(mGame1);
                Thread.Sleep(NumberManager.TwoHundred);
                var mapMessage = mGame1.GetMapMessage(playerNum, true);
                
                if (Game1.CameraStart[0].X < 0)
                {
                    var p = GameMap.IslandMapper == null
                        ? new[] {new Point(), new Point()}
                        : new[] {GameMap.IslandMapper.BestSpawnBottom, GameMap.IslandMapper.BestSpawnTop};
                    mapMessage.CameraStart = new[] {new Vector2(p[0].X, p[0].Y), new Vector2(p[1].X, p[1].Y)};
                    Game1.CameraStart = mapMessage.CameraStart;
                }                mapMessage.Send(c,true);
                
                Debug.WriteLine("allowed connection");
            }
            else
            {
                Debug.WriteLine("refused connection");
                c.Disconnect();
            }
        }

        public void DisconnectInactive()
        {
            foreach (var i in mPlayerNumToConnection.Keys)
            {
                if (!mMessageWrapper.LastPing.ContainsKey(i))
                {
                    continue;
                }
                var lastPing=mMessageWrapper.LastPing[i];
                var res=DateTime.Now.Subtract(lastPing);
                if (res > sTimeOutTime)
                {
                    var c=ToPlayer(i);
                    c.Disconnect();
                }
            }
            
        }

        public void Stop()
        {
            
            ConnectionManager.Stop();
        }
        public void OnDisconnect(Connection c)
        {
            foreach (var i in mPlayerNumToConnection.Keys.ToArray())
            {
                if (mPlayerNumToConnection[i].Ip.Equals(c.Ip))
                {
                    mPlayersAvailable.Add(i-1);
                    mPlayerNumToConnection.Remove(i);
                }
            }
            Connections.Remove(c);
            mMessageWrapper.Pause(true);
        }
    }
}