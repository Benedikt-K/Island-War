using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.com.game;
using Common.com.game.settings;
using Common.com.networking;
using Common.com.networking.Messages;
using Common.com.networking.Messages.ClientToServer;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects;
using Common.com.objects.immovables.Buildings;
using Common.com.rollbacks;
using Server.net.map;
using Server.net.networking;

namespace Server.net.main
{
    public sealed class MessageWrapper:IGameEndListener
    {
        private readonly RollbackManager mRollbackManager;
        private TcpConnectionManager mTcpConnectionManager;
        private bool mEnded;
        private int mPauseId;
        private int mLastSync=-1;
        public Dictionary<int, DateTime> LastPing { get; }
        private readonly Game1 mGame1;
        private readonly ServerMenu mServerMenu;
        public MessageWrapper(RollbackManager rollbackManager, TcpConnectionManager tcpConnectionManager)
        {
            GameMap.mGameEndListeners.Add(this);
            LastPing=new Dictionary<int, DateTime>();
            mRollbackManager = rollbackManager;
            mTcpConnectionManager = tcpConnectionManager;
            mPauseId = 0;
            mGame1 = Game1.Game1Get;
            mServerMenu = ServerMenu.ServerMenuGet;
        }

        public void SetConnectionManager(TcpConnectionManager tcpConnectionManager)
        {
            mTcpConnectionManager = tcpConnectionManager;
        }
        

        public void MessageReceived(int uid)
        {
            LastPing[uid] = DateTime.Now;
        }

        private void ExtraHandling(Message message, int senderId)
        {
            SpreadMessage(message, true);
            
            if (message is PauseMessage pauseMessage)
            {
                
                mPauseId = senderId;
                ServerMenuGenerator.PauseButton.Text = pauseMessage.Pause ? "UnPause" : "Pause";
                
            }

            if (message is SaveGameMessage saveGame)
            {
                if (saveGame.SaveName.Equals("Standard") && mServerMenu.NewGameName != null)
                {
                    mGame1.SaveGame(mServerMenu.NewGameName);
                }
                else
                {
                    mGame1.SaveGame(saveGame.SaveName);
                }
            }
        }
        public void Handle(Message message, int senderId, Connection senderConnection)
        {
            MessageReceived(senderId);
            if (message.Tick<=mLastSync)
            {
                return;
            }
            if (message is ResyncRequestMessage)
            {
                GameState current = mGame1.RollbackManager.CurrentState;
                current.Map.ToMapMessage(current.TickNr,senderId,null,current.IsPaused,2,false,true).Send(senderConnection,true);
                mLastSync = message.Tick;
                return;
            }
            if (IsPermitted(message, senderId)&&!(message is PingMessage))
            {
                if (message is NewBuildingPlacementMessage bp)
                {
                    Console.Write("test");
                    Scaffolding res;
                    if (bp.NewBuilding is Scaffolding scaffolding)
                    {
                        res = scaffolding;
                    }
                    else
                    {
                        res = new Scaffolding
                        {
                            TurnsInto = bp.NewBuilding,
                            IsRoad = false,
                            Location = bp.NewBuilding.Location,
                            PlayerNumber = senderId
                        };
                        res.EmptyResources();
                    }
                    res.Id = IdGenerator.NewId;
                    res.IsBlueprint = true;
                    res.PlayerNumber = senderId;
                    res.TurnsInto.Id = IdGenerator.NewId;
                    bp.NewBuilding = res;
                    
                }
                SetRoadOrBridge(message);
                if (message is NewObjectMovingCreationMessage omc)
                {
                    if ((GameMap.GetCap(senderId) - GameMap.mFilledCap[senderId-1]) >= omc.NewObjectMoving.RequiredCap)
                    {
                        omc.NewObjectMoving.Id = IdGenerator.NewId;
                    }
                    else
                    {
                        return;
                    }
                }
                ExtraHandling(message, senderId);
                if (!mRollbackManager.RollbackState(message, message.Tick)&&senderConnection!=null) {
                    KickPlayer(senderId,senderConnection,"Packet too old");
                }
                    
                
            }
            
        }

        private void SetRoadOrBridge(Message message)
        {
            switch (message)
            {
                case NewRoadMessage roadMessage:
                {
                    foreach (var newRoad in roadMessage.NewRoadTiles)
                    {
                        newRoad.TurnsInto = new Barracks();
                        newRoad.IsBlueprint = true;
                        newRoad.Id = IdGenerator.NewId;
                    }

                    break;
                }
                case NewBridgeMessage bridgeMessage:
                {
                    foreach (var newBridge in bridgeMessage.NewBridgeTiles)
                    {
                        newBridge.TurnsInto = new Shipyard();
                        newBridge.IsBlueprint = true;
                        newBridge.Id = IdGenerator.NewId;
                    }

                    break;
                }
            }
        }
        private void KickPlayer(int senderId, Connection connection,string reason)
        {
            Console.WriteLine("kicked player "+senderId+" because "+reason);
            connection.Disconnect();
        }

        private bool IsPermitted(Message message, int senderId)
        {
            
            if (message is PauseMessage pauseMessage)
            {
                return CheckPauseMessage(pauseMessage, senderId);
            }
            if (mRollbackManager.IsPaused())
            {
                return false;
            }
            if (senderId == 0) //sender Id 0 is the server
            {
                return true;
            }

            if (message is NewObjectActionMessage)
            {
                return true;
            }

            if (message is NewBuildingPlacementMessage newBuildingMessage)
            {
                return CheckBuildingMessage(newBuildingMessage, senderId);
            }

            if (message is NewObjectMovingCreationMessage newObjectMovingCreationMessage)
            {
                return CheckCreationMessage(newObjectMovingCreationMessage, senderId);
            }

            if (message is SaveGameMessage saveGame)
            {
                Debug.WriteLine(saveGame.ToString());
                return true;
            }
            if (message is NewRoadMessage roadMessage)
            {
                return roadMessage.NewRoadTiles != null;
            }

            if (message is NewBridgeMessage bridgeMessage)
            {
                return bridgeMessage.NewBridgeTiles != null;
            }
            return true;
        }

        private bool CheckCreationMessage(NewObjectMovingCreationMessage newObjectMovingCreationMessage, int senderId)
        {

            return newObjectMovingCreationMessage.NewObjectMoving != null && senderId == newObjectMovingCreationMessage.NewObjectMoving.PlayerNumber;
        }
        private bool CheckPauseMessage(PauseMessage pauseMessage,int senderId)
        {
            var correctPause = pauseMessage.Pause && !mRollbackManager.IsPaused();
            var correctUnpause = !pauseMessage.Pause && (senderId == mPauseId || senderId == 0) && mRollbackManager.IsPaused();
            return correctPause || correctUnpause;
        }
        private bool CheckBuildingMessage(NewBuildingPlacementMessage newBuildingMessage, int senderId)
        {
            ObjectImmovable tower = null;
            if (newBuildingMessage.NewBuilding is Tower t)
            {
                tower = t;
            }
            else if (newBuildingMessage.NewBuilding is MainBuilding m)
            {
                tower = m;
            }
            else if (newBuildingMessage.NewBuilding is Scaffolding { TurnsInto: Tower t2 })
            {
                tower = t2;
            }
            else if (newBuildingMessage.NewBuilding is Scaffolding { TurnsInto: MainBuilding t3 })
            {
                tower = t3;
            }

            if (tower != null && !GameMap.IslandMapper.CanAddTower(mRollbackManager.CurrentState.Map, tower))
            {
                return false;
            }
            return newBuildingMessage.NewBuilding != null && senderId == newBuildingMessage.NewBuilding.PlayerNumber;
        }
        public void SpreadMessage(Message message, bool tcp)
        {
            foreach (var c in mTcpConnectionManager.Connections)
            {
                message.Send(c,tcp);
            }
        }
        public void Pause(bool pause)
        {
            Handle(new PauseMessage(mRollbackManager.CurrentState.TickNr,pause),0,null);
        }

        public void OnGameEnd(int playerIdWon)
        {
            if (mEnded)
            {
                return;
            }
            mEnded = true;
            Handle(new GameEndMessage(mRollbackManager.CurrentState.TickNr-NumberManager.Twenty, playerIdWon), 0, null);
        }
    }
}