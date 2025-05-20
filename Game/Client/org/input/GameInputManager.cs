using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.com.game.settings;
using Common.com.networking.Messages;
using Common.com.networking.Messages.CommonMessages;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;
using Common.com.path;
using Game.org.gameStates;
using Game.org.graphic;
using Game.org.main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Point = System.Drawing.Point;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Game.org.input
{
    public sealed class GameInputManager : IInputManager
    {
        private bool mLastForward;
        private readonly InGame mGame;
        private static readonly Dictionary<KeyboardInputOption, Keys> sInputs=new Dictionary<KeyboardInputOption, Keys>();
        private MouseState mPreviousMouse;
        private KeyboardState mPreviousKeyboard;



        static GameInputManager()
        {
            InitializeKeymap();
        }
        public GameInputManager(InGame game)
        {
            InitializeKeymap();
            mGame = game;
        }

        public static Keys GetKeyOf(KeyboardInputOption keyboardInputOption)
        {
            return sInputs[keyboardInputOption];
        }
        private static void InitializeKeymap()
        {
            sInputs[KeyboardInputOption.Esc] = Keys.Escape;
            sInputs[KeyboardInputOption.Down] = Keys.S;
            sInputs[KeyboardInputOption.Left] = Keys.A;
            sInputs[KeyboardInputOption.Right] = Keys.D;
            sInputs[KeyboardInputOption.Up] = Keys.W;
            sInputs[KeyboardInputOption.TimeBack] = Keys.Left; 
            sInputs[KeyboardInputOption.TimeForward] = Keys.Right;
            sInputs[KeyboardInputOption.LogisticIgnore] = Keys.E;
            sInputs[KeyboardInputOption.Freeze] = Keys.F;
            sInputs[KeyboardInputOption.LogisticIgnoreAll] = Keys.LeftShift;
            sInputs[KeyboardInputOption.Jump] = Keys.Space;
        }

        private NewPathsMessage NewPathClickRectangle(int x, int y)
        {
            if (mGame.SelectedGameObjectId.Count == 0)
            {
                return null;
            }
            var res = new NewPathsMessage(mGame.RollbackManager.CurrentState.TickNr,true,mGame.AttackButtonOn);
            var rectangleRadius = (int)((Math.Sqrt(mGame.SelectedGameObjectId.Count) + 0.999) / 2);
            var bounds = new System.Drawing.Rectangle(x - rectangleRadius, y - rectangleRadius, rectangleRadius + 1,
                rectangleRadius + 1);
            var possibleEnds = new HashSet<Point>();
            for (var x2 = bounds.Left; x2 < bounds.Right; x2++)
            {
                for (var y2 = bounds.Top; y2 < bounds.Bottom; y2++)
                {
                    possibleEnds.Add(new Point(x2, y2));
                }
            }
            foreach (var id in mGame.SelectedGameObjectId)
            {
                var gameObject = mGame.RollbackManager.CurrentState.Map.GetObject(id);
                if (gameObject is ObjectMoving objectMoving&&(objectMoving.CurrentAction.FightingWithId==-1||mGame.RollbackManager.CurrentState.Map.GetObject(objectMoving.CurrentAction.FightingWithId) is Building))
                {

                    var path = objectMoving.PathToRectangle(mGame.RollbackManager.CurrentState.Map,
                            bounds, possibleEnds, !objectMoving.OnLand, null,
                            mGame.SelectedGameObjectId);


                    if (path != null)
                    {
                        res.AddPath(objectMoving.Id, path);
                        possibleEnds.Remove(path.End);
                    }
                }
            }

            return res;
        }
        private void OnClick(int x, int y, bool left, bool shift)
        {
            var map = mGame.RollbackManager.CurrentState.Map;
            var ob = map.GetObject(x, y);
            if (!shift && left)
            {
                mGame.SelectedGameObjectId.Clear();
            }

            if (left)
            {
                if (ob is ObjectMoving || ob is Building)
                {
                    if (ob.PlayerNumber == mGame.PlayerNumber)
                    {
                        mGame.SelectedGameObjectId.Add(ob.Id);
                        if (ob is Building)
                        {
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetSelectedBuildingsMenu(Game1.GetGame().ImageManager,
                                mGame, ob.Id, ob));
                        }
                        if (ob is TransportShip)
                        {
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetTransportShipMenu(Game1.GetGame().ImageManager,
                                mGame, ob.Id, ob));
                        }
                    }
                }
            }
            else
            {
                if (ob == null && map.InBounds(x, y))
                {
                    var message = NewPathClickRectangle(x, y);

                    if (mGame.AttackButtonOn)
                    {
                        message.Aggressive = true;
                    }
                    if (message != null && message.Paths.Count != 0)
                    {
                        message.Send(mGame.Server, true);
                    }
                }
                else if ((ob is ObjectMoving || ob is Building) && ob.PlayerNumber != mGame.PlayerNumber)
                {
                    MultipleMessagesMessage messageMult = new MultipleMessagesMessage(mGame.RollbackManager.CurrentState.TickNr,new List<Message>());
                    foreach (var id in mGame.SelectedGameObjectId)
                    {
                        if (map.GetObject(id) is ObjectMoving && (map.GetObject(id) is Shieldman
                                                                               || map.GetObject(id) is Spearman
                                                                               || map.GetObject(id) is Swordsman) && !(ob is Spy))
                        {
                            var message = new NewObjectActionMessage
                            {
                                Id = id,
                                Tick = mGame.RollbackManager.CurrentState.TickNr,
                                Action = new ObjectAction
                                {
                                    GoingToFightWithId = ob.Id,
                                    IsOccupied = true,
                                    IsAggressive = mGame.AttackButtonOn
                                }
                            };
                            messageMult.MessageList.Add(message);
                        }

                        if (map.GetObject(id) is Spy && ob is Tower tower && tower.SpyInside.Count == 0)
                        {
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetSpyTowerMenu(Game1.GetGame().ImageManager, mGame, ob.Id));
                        }

                        if (map.GetObject(id) is Spy && ob is Spy)
                        {
                            var message = new NewObjectActionMessage
                            {
                                Id = id,
                                Tick = mGame.RollbackManager.CurrentState.TickNr,
                                Action = new ObjectAction
                                {
                                    GoingToFightWithId = ob.Id,
                                    IsOccupied = true,
                                    IsAggressive = true,
                                    GoingToMurderId = ob.Id
                                }
                            };
                            messageMult.MessageList.Add(message);
                        }
                    }
                    messageMult.Send(mGame.Server,true);
                }
                else if (ob is ResourceBuilding)
                {
                    foreach (var id in mGame.SelectedGameObjectId)
                    {
                        if (mGame.RollbackManager.CurrentState.Map.GetObject(id) is Worker)
                        {
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetWorkerBuildingMenu(Game1.GetGame().ImageManager, mGame, ob.Id));
                        }
                    }
                }
                else if (ob is Tower tower)
                {
                    foreach (var id in mGame.SelectedGameObjectId)
                    {
                        if (map.GetObject(id) is ObjectMoving && (map.GetObject(id) is Shieldman
                                                                               || map.GetObject(id) is Spearman
                                                                               || map.GetObject(id) is Swordsman))
                        {
                            mGame.RemoveMenus("tower" + tower.Id);
                            mGame.AddIfNewName(mGame.mMenuGenerator.ShowTowerInside(Game1.GetGame().ImageManager, mGame, ob, true));
                        }
                        else if (map.GetObject(id) is ObjectMoving && map.GetObject(id) is Spy &&
                                 tower.SpyInside.Count == 1)
                        {
                            mGame.RemoveMenus("tower" + tower.Id);
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetOwnInfiltratedTowerMenu(Game1.GetGame().ImageManager, mGame, ob.Id));
                        } 
                    }
                    
                }
                else if (ob is TransportShip transportShip)
                {
                    mGame.RemoveMenus("ship" + transportShip.Id);
                    mGame.AddIfNewName(mGame.mMenuGenerator.ShowShipInside(Game1.GetGame().ImageManager, mGame, ob, true));
                }
                else if (ob is INonCollisional)
                {
                    var message = NewPathClickRectangle(x, y);
                    if (message != null && message.Paths.Count != 0)
                    {
                        message.Send(mGame.Server, true);
                    }
                }
            }
        }
        
        private void OnClickMultiple(int x, int y, bool left, bool shift)
        {
            var map = mGame.RollbackManager.CurrentState.Map;
            var ob = map.GetSecondaryObject(x, y);
            if (!shift&&left)
            {
                mGame.SelectedGameObjectId.Clear();
            }
            if (left)
            {
                if (ob is ObjectMoving)
                {
                    if (ob.PlayerNumber == mGame.PlayerNumber)
                    {
                        mGame.SelectedGameObjectId.Add(ob.Id);
                        if (ob is TransportShip)
                        {
                            mGame.AddIfNewName(mGame.mMenuGenerator.GetTransportShipMenu(Game1.GetGame().ImageManager,
                                mGame, ob.Id, ob));
                        }
                    }
                }
            }
        }

        // Clears the currently selected GameObjects. Then adds all GameObjects in the rectangle to the current selection. Resets the SelectionRectangle to Empty when finished.
        private void ReleaseRectangle(int xPos,int yPos,bool shift)
        {
            var heightSelected = mGame.SelectedStart.X-mGame.SelectedStart.Y;
            var heightPos = xPos - yPos;
            var widthSelected = mGame.SelectedStart.X + mGame.SelectedStart.Y;
            var widthPos = xPos + yPos;
            var res = new Rectangle(Math.Min(widthSelected,widthPos)+1,Math.Min(heightSelected,heightPos),
                Math.Max(widthSelected,widthPos)-Math.Min(widthSelected,widthPos),
                Math.Max(heightSelected,heightPos)-Math.Min(heightSelected,heightPos));
            
            if (!shift)
            {
                mGame.RemoveMenus("selected");
                mGame.RemoveMenus("workermenu");
                mGame.RemoveMenus("unitmenu");
                mGame.RemoveMenus("towermenu");
                mGame.RemoveMenus("transportship");
                mGame.RemoveMenus("spytower");
                mGame.RemoveMenus("tower");
                mGame.RemoveMenus("ship");
                mGame.RemoveMenus("infiltrate");
                mGame.RemoveMenus("StoredItems");
                mGame.SelectedGameObjectId.Clear();
            }

            if (res.Left == res.Right && res.Top == res.Bottom)
            {
                OnClick((res.Left + res.Top) / NumberManager.Two, (res.Left - res.Top) / NumberManager.Two, true, true);
            }
            for (var x = res.Left; x <= res.Right; x++)
            {
                for (var y = res.Top; y <= res.Bottom; y++)
                {
                    OnClickMultiple((x+y)/ NumberManager.Two,(x-y)/ NumberManager.Two,true,true);
                }
            }
        }

        private void OnLogisticPress(bool all)
        {
            var workers = new List<Worker>();
            var logisticIgnoreMessage =
                new LogisticIgnoreMessage(mGame.RollbackManager.CurrentState.TickNr, workers);
            if (all)
            {
                workers.AddRange(mGame.RollbackManager.CurrentState.Map.GetAllWorkers().Where(worker => worker.PlayerNumber == mGame.PlayerNumber));
                logisticIgnoreMessage =
                    new LogisticIgnoreMessage(mGame.RollbackManager.CurrentState.TickNr, workers);
            }
            else
            {
                foreach (var gameObject in mGame.SelectedGameObjectId.Select(id => mGame.RollbackManager.CurrentState.Map.GetObject(id)))
                {
                    if (gameObject is Worker worker)
                    {
                        logisticIgnoreMessage.ToUnIgnore.Add(worker);
                    }
                }
            }
            if (logisticIgnoreMessage.ToUnIgnore.Count > 0)
            {
                logisticIgnoreMessage.Send(mGame.Server, true);
            }
        }
        
        public void HandleInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if(keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.Jump)) &&
               !mPreviousKeyboard.IsKeyDown(GetKeyOf(KeyboardInputOption.Jump)))
            {
                mGame.Camera = new Camera(mGame.mGraphicsDeviceManager.GraphicsDevice.Viewport,
                    mGame.mCameraStartPoint,
                   mGame.RollbackManager.CurrentState.Map.GetSize(), mGame.Camera.Zoom - NumberManager.ZeroPointZeroFive);
            }
            if (keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.Esc)) &&
                !mPreviousKeyboard.IsKeyDown(GetKeyOf(KeyboardInputOption.Esc)))
            {
                mGame.Pause(!mGame.IsPaused);
            }
            if (keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.LogisticIgnore))&&
                !mPreviousKeyboard.IsKeyDown(GetKeyOf(KeyboardInputOption.LogisticIgnore)))
            {
                OnLogisticPress(keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.LogisticIgnoreAll)));

            }
            mGame.Camera.UpdateCamera(mouseState,keyboardState);
            if (keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.TimeBack))&&
                !mPreviousKeyboard.IsKeyDown(GetKeyOf(KeyboardInputOption.TimeBack)))
            {
                new RevertTimeMessage(mGame.RollbackManager.CurrentState.TickNr- NumberManager.OneHundred).Send(mGame.Server,true);
            }

            if (keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.Freeze)) &&
                !mPreviousKeyboard.IsKeyDown(GetKeyOf(KeyboardInputOption.Freeze)))
            {
                Thread.Sleep(NumberManager.TwoThousand);
            }


            if (keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.TimeForward))&&!mLastForward)
            {
                new RevertTimeMessage(NumberManager.Forty,false).Send(mGame.Server,true);
            }
            mLastForward = keyboardState.IsKeyDown(GetKeyOf(KeyboardInputOption.TimeForward));
            var left = (mouseState.LeftButton == ButtonState.Released &&
                        mPreviousMouse.LeftButton == ButtonState.Pressed);
            var right = mouseState.RightButton == ButtonState.Released &&
                        mPreviousMouse.RightButton == ButtonState.Pressed;
            var shift = keyboardState.IsKeyDown(Keys.LeftShift);
            var leftDown = (mouseState.LeftButton == ButtonState.Pressed &&
                            mPreviousMouse.LeftButton == ButtonState.Released);
            var inverseTransformMatrix = Matrix.Invert(mGame.Camera.Transform);
            var mousePosition = Vector2.Transform(new Vector2(mouseState.Position.X, mouseState.Position.Y), inverseTransformMatrix);
            int x = (int)mousePosition.X, y=(int)mousePosition.Y;
            
            foreach (var menu in mGame.ActiveMenus.ToArray())
            {
                if (menu.HoveringOverThis(mouseState))
                {
                    if (left)
                    {
                        var click = Game1.GetGame().SoundManager.MouseClick;
                        Game1.GetGame().SoundManager.PlaySfx(click);
                        menu.OnClick(mouseState);
                    }
                    mPreviousMouse = mouseState;
                    return;
                }
            }

            if (left)
            {
                mGame.RemoveUnneeded();
            }

            if (leftDown&&mGame.BuildingSelected == null)
            {
                mGame.SelectedStart=new Point(x,y);
            }
            if (leftDown && mGame.BuildingSelected == null && !mGame.RoadSelected)
            {
                mGame.SelectedStart = new Point(x, y);
            }

            if (right)
            {
                OnClick(x, y, false, shift);
            }

            if (left)
            {
                if (mGame.BuildingSelected == null)
                {
                    if (mGame.SelectedStart.X == -NumberManager.OneThousand)
                    {
                        //should never happen
                        OnClick(x, y, true, shift);
                    }
                    else
                    {
                        ReleaseRectangle(x, y, shift);
                        mGame.SelectedStart = new Point(-NumberManager.OneThousand, -NumberManager.OneThousand);
                    }
                }
                else if (mGame.BuildingSelected is Scaffolding)
                {
                    if (mGame.SelectedStartRoad != new Point(-NumberManager.OneThousand, -NumberManager.OneThousand))
                    {
                        if (mGame.RoadBuildingList == null)
                        {
                            mGame.RoadBuildingList = new List<Scaffolding>();
                        }

                        if (mGame.BridgeBuildingList == null)
                        {
                            mGame.BridgeBuildingList = new List<Scaffolding>();
                        }
                        var bridgeTilesScaffolding = new List<Scaffolding>();
                        var roadTilesScaffolding = new List<Scaffolding>();
                        var roads = new List<Point>();
                        if (mGame.RollbackManager.CurrentState.Map.GetTerrainAt((uint)mGame.SelectedStartRoad.X,
                            (uint)mGame.SelectedStartRoad.Y) != Terrain.Water)
                        {
                            var newPath = new PathAbstract(mGame.SelectedStartRoad, new Point(x, y), false);
                            var foundPath = newPath.FindPath(mGame.RollbackManager.CurrentState.Map, new Path[] { },null,null,true);
                            if (foundPath != null)
                            {
                                foreach (var tile in foundPath.Points)
                                {
                                    roads.Add(new Point(tile.X, tile.Y));
                                }
                            }
                        }
                        else
                        {
                            var newPath = new PathAbstract(mGame.SelectedStartRoad, new Point(x, y), true);
                            var foundPath = newPath.FindPath(mGame.RollbackManager.CurrentState.Map, new Path[] { },null,null,true);
                            if (foundPath != null)
                            {
                                foreach (var tile in foundPath.Points)
                                {
                                    roads.Add(new Point(tile.X, tile.Y));
                                }
                            }
                        }
                        if (true)
                        {

                            foreach (var roadTile in roads)
                            {
                                if (mGame.RollbackManager.CurrentState.Map.GetTerrainAt((uint) roadTile.X,
                                        (uint) roadTile.Y) != Terrain.Water &&
                                    mGame.RollbackManager.CurrentState.Map.GetTerrainAt((uint) roadTile.X,
                                        (uint) roadTile.Y) != Terrain.Road)
                                {
                                    var newRoad = new Scaffolding();
                                    newRoad.Location = roadTile;
                                    newRoad.IsRoad = true;
                                    newRoad.IsBridge = false;
                                    newRoad.TurnsInto = new Barracks();
                                    newRoad.PlayerNumber = mGame.PlayerNumber;
                                    roadTilesScaffolding.Add(newRoad);
                                }
                                else if (mGame.RollbackManager.CurrentState.Map.GetTerrainAt((uint) roadTile.X,
                                             (uint) roadTile.Y) == Terrain.Water)
                                {
                                    var newBridge = new Scaffolding();
                                    newBridge.Location = roadTile;
                                    newBridge.IsRoad = false;
                                    newBridge.IsBridge = true;
                                    newBridge.TurnsInto = new Shipyard();
                                    newBridge.PlayerNumber = mGame.PlayerNumber;
                                    bridgeTilesScaffolding.Add(newBridge);
                                }

                            }

                            if (roadTilesScaffolding.Any())
                            {
                                mGame.RoadBuildingList.AddRange(roadTilesScaffolding);
                            }
                            if (bridgeTilesScaffolding.Any())
                            {
                                mGame.BridgeBuildingList.AddRange(bridgeTilesScaffolding);
                            }

                        }
                        mGame.SelectedStartRoad = new Point(-NumberManager.OneThousand,-NumberManager.OneThousand);
                    }
                    else
                    {
                        mGame.SelectedStartRoad = new Point(x, y);
                    }

                }
                else
                {

                    if (mGame.BuildingSelected != null )
                    {
                        var newBuilding = mGame.BuildingSelected;
                        newBuilding.IsBlueprint = true;
                        newBuilding.PlayerNumber = mGame.PlayerNumber;
                        newBuilding.Location = new Point(x, y);
                        if (mGame.RollbackManager.CurrentState.Map.CanBePut(mGame.BuildingSelected,mGame.PlayerNumber) &&
                            !(mGame.BuildingSelected is Scaffolding))
                        {
                            var message =
                                new NewBuildingPlacementMessage(mGame.RollbackManager.CurrentState.TickNr, newBuilding);
                            message.Send(mGame.Server, true);
                        }
                    }
                }

            }

            mPreviousKeyboard = keyboardState;
            mPreviousMouse = mouseState;
        }
    }
}