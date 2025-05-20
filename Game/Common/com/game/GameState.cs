using System;
using System.Collections.Generic;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.networking.Messages;
using Common.com.networking.Messages.ClientToServer;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.immovables.Buildings;
using Common.com.path;
using Common.com.rollbacks;

namespace Common.com.game
{
    public sealed class GameState
    {
        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored

        public GameMap Map { get; set; }
        public int TickNr { get; private set; }

        public bool IsPaused { get; private set; }
        public static bool RequestingResync{ get; set; }
        
        public int MaxPlayers { get; }
        public GameState(GameMap map,int tickNr, bool isPaused, int maxPlayers)
        {
            MaxPlayers = maxPlayers;
            IsPaused = isPaused;
            TickNr = tickNr;
            Map = map;
        }

        public GameState Clone()
        {
            Map.UpdateLasts();
            var gameMap = Map.Clone();
            gameMap.UseLogisticsSystem(Map);
            var res = new GameState(gameMap, TickNr, IsPaused,MaxPlayers)
                {
                    IsPaused = IsPaused
                };
            return res;
        }

        public void Revert(Message message)
        {
            if (message is PriorityMessage priorityMessage)
            {
                if (Map.GetObject(priorityMessage.BuildingId) is ResourceBuilding resourceBuilding)
                {
                    resourceBuilding.Priority = priorityMessage.OldPriority;
                }
            }
            if (message is NewBuildingPlacementMessage buildingMessage)
            {
                Map.RemoveObject(buildingMessage.NewBuilding,true,true);
            }
            if (message is TreeGrowthMessage treeGrowthMessage)
            {
                if (treeGrowthMessage.Remove)
                {
                    Map.AddObject(treeGrowthMessage.Tree,false);
                    var forestersLodge = (ForestersLodge)Map.GetObject(treeGrowthMessage.Tree.ForestersLodgeId);
                    
                    if (treeGrowthMessage.GivesResources)
                    {
                        GameMap.StatisticsManager.RemoveStatistic(Statistic.WoodCollected,forestersLodge.PlayerNumber);
                        forestersLodge.TakeResources(new Items(new []{Item.Wood},new []{1}));
                    }
                }
                else
                {
                    Map.RemoveObject(treeGrowthMessage.Tree);
                }
            }

            if (message is NewObjectMovingCreationMessage objectMovingMessage)
            {
                GameMap.StatisticsManager.RemoveStatistic(Statistic.UnitsProduced, objectMovingMessage.NewObjectMoving.PlayerNumber);
                if (objectMovingMessage.NewObjectMoving is Spy)
                {
                    GameMap.StatisticsManager.RemoveStatistic(Statistic.SpiesProduced, objectMovingMessage.NewObjectMoving.PlayerNumber);
                }
                var ob = objectMovingMessage.NewObjectMoving;
                var gameObject=(ResourceBuilding)Map.GetObject(objectMovingMessage.BuildingId);
                gameObject.GiveResources(ob.ResourceCost);
            }

            if (message is MultipleMessagesMessage multipleMessagesMessage)
            {
                foreach (var innerMessage in multipleMessagesMessage.MessageList)
                {
                    if (innerMessage is UnloadTowerMessage innerUnloadTowerMessage)
                    {
                        var ob = innerUnloadTowerMessage.UnloadObjectMoving;
                        var tower = (Tower)Map.GetObject(innerUnloadTowerMessage.BuildingId);
                        tower.AddUnit(ob);
                    }
                }
            }
            if (message is LeaveTowerMessage leaveTowerMessage)
            {
                var ob = leaveTowerMessage.LeaveObjectMoving;
                var tower = (Tower)Map.GetObject(leaveTowerMessage.BuildingId);
                tower.AddSpy(ob);
            }

            if (message is ForestersLodgeModeMessage forestersLodgeModeMessage)
            {
                var ob = (ForestersLodge)Map.GetObject(forestersLodgeModeMessage.Id);
                ob.Plants = !(forestersLodgeModeMessage.Mode);
            }

            if (message is NewRemoveBuildingMessage removeBuildingMessage)
            {
                var ob = removeBuildingMessage.RemoveBuilding;
                Map.AddObject(ob);
            }

            if (message is NewBridgeMessage newBridgeMessage)
            {
                foreach (var bridgeTile in newBridgeMessage.NewBridgeTiles)
                {
                    Map.RemoveObject(bridgeTile, true, true);
                }
            }

            if (message is NewRoadMessage newRoadMessage)
            {
                foreach (var roadTile in newRoadMessage.NewRoadTiles)
                {
                    Map.RemoveObject(roadTile, true, true);
                }
            }

        }
        public void Handle(Message message)
        {
            if (message is PriorityMessage priorityMessage)
            {
                if (Map.GetObject(priorityMessage.BuildingId) is ResourceBuilding resourceBuilding)
                {
                    priorityMessage.OldPriority = resourceBuilding.Priority;
                    resourceBuilding.Priority = priorityMessage.NewPriority;
                    
                }
            }

            if (message is MapMessage mapMessage)
            {
                Console.WriteLine("Synced");
                Map = GameMap.GetMap(mapMessage,true);
                
            }
            if (message is GameEndMessage endMessage)
            {
                AchievementManager.GetManager().AfterGameEnding(TickNr, endMessage.Winner);
                IsPaused = true;
            }
            if (message is LogisticIgnoreMessage logisticIgnoreMessage)
            {
                foreach (var worker in logisticIgnoreMessage.ToUnIgnore)
                {
                    Map.RedoTask(worker);
                }
            }
            if (message is PauseMessage pauseMessage)
            {
                IsPaused = pauseMessage.Pause;

            }

            if (message is NewObjectMovingCreationMessage objectMovingMessage)
            {
                GameMap.StatisticsManager.AddStatistic(Statistic.UnitsProduced, objectMovingMessage.NewObjectMoving.PlayerNumber);
                if (objectMovingMessage.NewObjectMoving is Spy)
                {
                    GameMap.StatisticsManager.AddStatistic(Statistic.SpiesProduced, objectMovingMessage.NewObjectMoving.PlayerNumber);
                }
                if (objectMovingMessage.NewObjectMoving is ScoutShip || objectMovingMessage.NewObjectMoving is TransportShip)
                {
                    GameMap.StatisticsManager.AddStatistic(Statistic.ShipsProduced, objectMovingMessage.NewObjectMoving.PlayerNumber);
                }
                var ob = objectMovingMessage.NewObjectMoving;
                var gameObject=(ResourceBuilding)Map.GetObject(objectMovingMessage.BuildingId);
                GameMap.mFilledCap[objectMovingMessage.NewObjectMoving.PlayerNumber-1] += objectMovingMessage.NewObjectMoving.RequiredCap;
                gameObject.TakeResources(ob.ResourceCost);
                Map.AddObject(ob);
            }

            if (message is LeaveTowerMessage leaveTowerMessage)
            {
                var ob = leaveTowerMessage.LeaveObjectMoving;
                var tower = (Tower)Map.GetObject(leaveTowerMessage.BuildingId);
                (ob.X, ob.Y) = Tower.GetLandSpawnLocation(tower, Map);
                if ((ob.X, ob.Y) == (-1, -1))
                {
                    return;
                }
                ob.X += NumberManager.ZeroPointFiveF;
                ob.Y += NumberManager.ZeroPointFiveF;
                ob.CurrentPath = new Path(new LinkedList<System.Drawing.Point>(new[] { new System.Drawing.Point((int)ob.X, (int)ob.Y) }), false);
                ob.CurrentAction.InfiltrateId = -1;
                Map.AddObject(ob);
                tower.RemoveSpy();
            }

            if (message is ForestersLodgeModeMessage forestersLodgeModeMessage)
            {
                var ob = (ForestersLodge)Map.GetObject(forestersLodgeModeMessage.Id);
                ob.Plants = forestersLodgeModeMessage.Mode;
            }

            if (message is NewBuildingPlacementMessage buildingMessage)
            {
                var ob = buildingMessage.NewBuilding;
                ob.IsBlueprint = true;
                if (ob is Scaffolding scaffolding)
                {
                    scaffolding.EmptyResources(true);
                }
                Map.AddObject(ob);
            }

            if (message is NewRemoveBuildingMessage removeBuildingMessage)
            {
                var ob = removeBuildingMessage.RemoveBuilding;
                Map.RemoveObject(ob);
            }

            if (message is TreeGrowthMessage treeGrowthMessage)
            {
                
                if (treeGrowthMessage.Remove)
                {
                    
                    var forestersLodge = (ForestersLodge)Map.GetObject(treeGrowthMessage.Tree.ForestersLodgeId);

                        Map.RemoveObject(treeGrowthMessage.Tree);
                    if (treeGrowthMessage.GivesResources)
                    {
                        forestersLodge.AddResource(Item.Wood,1);
                        GameMap.StatisticsManager.AddStatistic(Statistic.WoodCollected,forestersLodge.PlayerNumber);
                    }
                }
                else
                {
                    Map.AddObject(treeGrowthMessage.Tree);
                }
            }
            if (message is NewObjectActionMessage actionMessage)
            {
                Map.ChangeObjectAction((ObjectMoving)Map.GetObject(actionMessage.Id),actionMessage.Action);
            }

            if (message is MultipleMessagesMessage multipleMessagesMessage)
            {
                foreach (var innerMessage in multipleMessagesMessage.MessageList)
                {
                    if (innerMessage is NewObjectActionMessage newObjectActionMessage)
                    {
                        Map.ChangeObjectAction((ObjectMoving)Map.GetObject(newObjectActionMessage.Id), newObjectActionMessage.Action);
                    }
                    else if (innerMessage is UnloadTowerMessage innerUnloadTowerMessage)
                    {
                        var ob = innerUnloadTowerMessage.UnloadObjectMoving;
                        var tower = (Tower)Map.GetObject(innerUnloadTowerMessage.BuildingId);
                        (ob.X, ob.Y) = Tower.GetLandSpawnLocation(tower, Map);
                        if ((ob.X, ob.Y) == (-1, -1))
                        {
                            return;
                        }
                        ob.X += NumberManager.ZeroPointFiveF;
                        ob.Y += NumberManager.ZeroPointFiveF;
                        ob.CurrentPath = new Path(new LinkedList<System.Drawing.Point>(new[] { new System.Drawing.Point((int)ob.X, (int)ob.Y) }), false);
                        ob.CurrentAction.GoingIntoId = -1;
                        Map.AddObject(ob);
                        tower.RemoveUnit(ob.Id);
                    }
                    else if (innerMessage is UnloadTransportShipMessage innerUnloadTransportShipMessage)
                    {
                        var ob = innerUnloadTransportShipMessage.UnloadObjectMoving;
                        var transportShip = (TransportShip)Map.GetObject(innerUnloadTransportShipMessage.TransportShipId);
                        (ob.X, ob.Y) = TransportShip.GetLandSpawnLocation(transportShip, Map);
                        if ((ob.X, ob.Y) == (-1, -1))
                        {
                            return;
                        }
                        ob.CurrentPath = new Path(new LinkedList<System.Drawing.Point>(new[] { new System.Drawing.Point((int)ob.X, (int)ob.Y) }), false);
                        ob.CurrentAction.GoingIntoId = -1;
                        Map.AddObject(ob);
                        transportShip.RemoveUnit(ob.Id);
                    }
                }
            }

            if (message is NewPathsMessage newPathsMessage)
            {
                for (var i = 0; i < newPathsMessage.Ids.Count; i++)
                {
                    var id=newPathsMessage.Ids[i];
                    var p=newPathsMessage.Paths[i];
                    Map.ChangeObjectPath((ObjectMoving)Map.GetObject(id),p,false,newPathsMessage.ChangeAggressiveness,newPathsMessage.Aggressive);
                }
            }

            if (message is ItemOutMessage itemOutMessage)
            {
                foreach (var worker in itemOutMessage.Workers)
                {
                    var w = (Worker)Map.GetObject(worker.Id);
                    if (itemOutMessage.Action.ItemTransportIntent == Item.Nothing)
                    {
                        itemOutMessage.Action.ItemTransportIntent = w.HoldingItem;
                    }
                    Map.ChangeObjectAction(w,itemOutMessage.Action.Clone());
                    w.RedoPath(Map);
                    Map.OnNewPosition(w);
                }
            }

            if (message is ResyncMessage {DealtWith: false} resyncMessage)
            {
                resyncMessage.Resynced=!Map.Resync(resyncMessage);
            }
            if (message is NewRoadMessage newRoadMessage)
            {
                foreach (var newRoadTile in newRoadMessage.NewRoadTiles)
                {
                    Map.AddObject(newRoadTile);
                }
            }
            if (message is NewBridgeMessage newBridgeMessage)
            {
                foreach (var newBridgeTile in newBridgeMessage.NewBridgeTiles)
                {
                    Map.AddObject(newBridgeTile);
                }
            }
        }
        public void Tick()
        {
            TickNr++;
            if (!RollbackManager.HasEnded)
            {
                Map.Changes = new List<IChange>();
                Map.GetLogisticsSystem().TickNr = TickNr;
                Map.DoMovementTicks();
                if (TickNr % NumberManager.Eight == 0)
                {
                    Map.DoDamageTicks(TickNr);
                }

                if (TickNr % NumberManager.OneHundred == 0)
                {
                    Map.RedoLogisticSystem();
                }

                Map.DoProductionTicks(TickNr);
            }
        }
    }
}