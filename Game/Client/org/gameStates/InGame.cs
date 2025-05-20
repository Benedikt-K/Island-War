using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.com.game;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.networking;
using Common.com.networking.Messages;
using Common.com.networking.Messages.ClientToServer;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;
using Common.com.rollbacks;
using Common.com.serialization;
using Game.org.graphic;
using Game.org.input;
using Game.org.main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Game.org.gameStates
{
    public sealed class InGame : IGameState, IPacketListener
    {
        public RollbackManager RollbackManager { get; }
        public HashSet<Menu> ActiveMenus { get; }
        private bool mHasEnded;
        public Connection Server { get; }
        private readonly GameInputManager mInput;
        private readonly IRendering mRenderer;
        public Camera Camera { get; set; }
        public int PlayerNumber { get; }

        private TimeManager TimeManager { get; set; }
        public MouseState LastState { get; private set; }
        private readonly ContentManager mContentManager;
        private static readonly TimeSpan sTimeOutTime = new TimeSpan(0, 0, NumberManager.Ten);
        private DateTime mLastPing;
        public Building BuildingSelected { get; private set; }
        private ResourceBuilding ResourceBuildingSelected { get; set; }
        private ObjectMoving ObjectMovingCreate { get; set; }
        public System.Drawing.Point SelectedStart { get; set; }
        public System.Drawing.Point SelectedStartRoad { get; set; }
        public bool IsPaused => RollbackManager.CurrentState.IsPaused;

        private long mTickAmount;
        private long mTickAmount2;
        
        public bool AttackButtonOn { get; private set; }

        public HashSet<int> SelectedGameObjectId { get; }

        private readonly List<Worker> mWorkers;
        public Dictionary<int, int> GameObjectIdAnimationFrame { get; }
        public List<Scaffolding> RoadBuildingList { get; set; }
        public List<Scaffolding> BridgeBuildingList { get; set; }
        public bool RoadSelected { get; private set; }

        public readonly GraphicsDeviceManager mGraphicsDeviceManager;

        public readonly MenuGenerator mMenuGenerator;
        public Vector2 mCameraStartPoint;
        private SoundManager SoundManager { get; }

        public InGame(GameState state,
            Connection server,
            int playerNum,
            GraphicsDeviceManager graphicsDevice,
            Vector2 cameraStart,
            ContentManager contentManager)
        {
            AchievementManager.PlayerNumber = playerNum;
            SoundManager = Game1.GetGame().SoundManager;
            ActiveMenus = new HashSet<Menu>();
            mMenuGenerator = new MenuGenerator(contentManager, graphicsDevice.GraphicsDevice);
            var gamingMenu = mMenuGenerator.GetIngameMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(gamingMenu);
            var fightingMenu = mMenuGenerator.GetFightingMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(fightingMenu);
            var buildingMenu = mMenuGenerator.GetBuildingsMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(buildingMenu);
            var pauseMenu = mMenuGenerator.GetPauseMenu(Game1.GetGame().ImageManager, this);
            if (state.IsPaused)
            {
                ActiveMenus.Add(pauseMenu);
            }

            SelectedStart = new System.Drawing.Point(NumberManager.MinusOneThousand, NumberManager.MinusOneThousand);
            mContentManager = contentManager;
            mGraphicsDeviceManager = graphicsDevice;

            mWorkers = new List<Worker>();
            PlayerNumber = playerNum;
            Server = server;
            RollbackManager = new RollbackManager(state, RollbackManager.DefaultSize);
            ActiveMenus.Add(mMenuGenerator.GetTimeMenu(RollbackManager));
            ActiveMenus.Add(mMenuGenerator.GetTimer(Game1.GetGame().ImageManager,this,RollbackManager));
            server.AddListener(this);
            mInput = new GameInputManager(this);
            mRenderer = new GameRendering(this);
            mCameraStartPoint = cameraStart;
            Camera = new Camera(graphicsDevice.GraphicsDevice.Viewport, mCameraStartPoint, state.Map.GetSize());
            SelectedGameObjectId = new HashSet<int>();
            GameObjectIdAnimationFrame = new Dictionary<int, int>();
            mLastPing = DateTime.Now;
            mTickAmount = 0;
            mTickAmount2 = 0;
            AttackButtonOn = false;
            SelectedStartRoad = new System.Drawing.Point(NumberManager.MinusOneThousandRoad, NumberManager.MinusOneThousandRoad);
            RoadBuildingList = null;
            BridgeBuildingList = null;
        }

        public IInputManager GetInputManager()
        {
            return mInput;
        }

        public void Pause(bool pause)
        {
            SelectedGameObjectId.Clear();
            new PauseMessage(RollbackManager.CurrentState.TickNr, pause).Send(Server, true);
        }

        private void UpdateSecond()
        {
            new PingMessage().Send(Server, true);
            if (DateTime.Now.Subtract(mLastPing) > sTimeOutTime)
            {
                Console.WriteLine("Disconnected");
                Stop();
            }
        }

        private void UpdateAnimationFrame()
        {
            foreach (var (id, animationFrame) in GameObjectIdAnimationFrame.ToList())
            {
                if (animationFrame > 0)
                {
                    GameObjectIdAnimationFrame[id] += 1;
                }
            }
        }

        public void Update(GameTime gameTime)
        {

            mTickAmount += gameTime.ElapsedGameTime.Ticks;
            if (mTickAmount >= TimeSpan.TicksPerSecond)
            {
                mTickAmount -= TimeSpan.TicksPerSecond;
                UpdateSecond();
            }

            mTickAmount2 += gameTime.ElapsedGameTime.Ticks;
            if (!IsPaused && mTickAmount2 >= TimeSpan.TicksPerSecond / NumberManager.Eighth)
            {
                mTickAmount2 %= TimeSpan.TicksPerSecond / NumberManager.Eighth;
                UpdateAnimationFrame();
            }

            LastState = Mouse.GetState();
            if (!RollbackManager.IsPaused())
            {
                TimeManager ??= new TimeManager();
                RollbackManager.CalculateNextTicks((uint) TimeManager.ElapseTime());
                if (GameState.RequestingResync)
                {
                    new ResyncRequestMessage().Send(Server,true);
                    GameState.RequestingResync = false;
                }

            }
            else
            {
                if (TimeManager != null)
                {
                    TimeManager.ElapseTime();
                }
            }

        }

        private string mTempName;

        private bool Equal(Menu menu)
        {
            return menu.SameStart(mTempName);
        }

        public void RemoveMenus(string name)
        {
            mTempName = name;
            ActiveMenus.RemoveWhere(Equal);
        }

        public void AddIfNewName(Menu menu)
        {
            foreach (var activeMenu in ActiveMenus)
            {
                if (activeMenu.SameName(menu))
                {
                    return;
                }
            }

            ActiveMenus.Add(menu);
        }
        
        public IRendering GetRenderer()
        {
            return mRenderer;
        }

        public GameState GetCurrentState()
        {
            return RollbackManager.CurrentState;
        }

        public void Stop()
        {
            Server.Disconnect();
        }

        public void HandlePacket(byte[] content, Connection c)
        {
            
            mLastPing = DateTime.Now;
            var jsonSerializable = JsonSerializable.Deserialize(content);
            if (jsonSerializable is Message message && !(message is PingMessage))
            {
                RollbackManager.RollbackState(message, message.Tick);
                var sound = SoundManager;
                if (message is NewBuildingPlacementMessage buildingPlacementMessage&&buildingPlacementMessage.NewBuilding.PlayerNumber==PlayerNumber)
                {
                    sound.PlaySfx(sound.Building1);
                }

                if (message is GameEndMessage gameEndMessage)
                {
                    
                    if (mHasEnded)
                    {
                        return;
                    }
                    mHasEnded = true;
                    RollbackManager.OnGameEnd();
                    ShowStatistics(gameEndMessage.Winner);
                    if (PlayerNumber == gameEndMessage.Winner)
                    {
                        sound.StopMusic();
                        sound.PlaySfx(sound.GameWon);
                    } else
                    {
                        sound.StopMusic();
                        sound.PlaySfx(sound.GameLost);
                    }
                }
                if (message is NewPathsMessage {Ids: { }} pathsMessage && pathsMessage.Ids.Any() && RollbackManager.CurrentState.Map.GetObject(pathsMessage.Ids.First()) is { } gameObject && gameObject.PlayerNumber==PlayerNumber)
                {
                    if (RollbackManager.CurrentState.Map.GetObject(pathsMessage.Ids[0]) is ScoutShip ||
                        RollbackManager.CurrentState.Map.GetObject(pathsMessage.Ids[0]) is TransportShip)
                    {
                        sound.PlaySfx(sound.Sailing);
                    }
                    else
                    {
                        sound.PlaySfx(sound.UnitStartedMoving);
                    }
                }

                if (message is PauseMessage {Pause: true})
                {
                    var pauseMenu = mMenuGenerator.GetPauseMenu(Game1.GetGame().ImageManager, this);
                    if (!(ActiveMenus.Contains(pauseMenu)))
                    {
                        ActiveMenus.Add(pauseMenu);
                    }
                }

                if (message is PauseMessage {Pause: false})
                {
                    RemoveMenus("In-game_Settings");
                    RemoveMenus("pause");
                }
            }
        }

        private void RemoveCostMenu()
        {
            if (BuildingSelected != null)
            {
                RemoveMenus(BuildingSelected.ClassNumber.ToString());
            }
        }

        private void AddCostMenu()
        {
            if (BuildingSelected != null)
            {
                mMenuGenerator.AddCost(BuildingSelected, this, Game1.GetGame().ImageManager);
            }
        }

        public void OnDisconnect(Connection c)
        {
            Game1.GetGame().SwitchGameState(new MainMenu(Game1.GetGame(), mGraphicsDeviceManager, mContentManager));
        }

        public void BackToMainMenuButton_Click(object sender, EventArgs e)
        {
            Game1.GetGame().SwitchGameState(new MainMenu(Game1.GetGame(), mGraphicsDeviceManager, mContentManager));
        }

        public void PauseButton_Click(object sender, EventArgs e)
        {
                Pause(!IsPaused);

        }

        public void SaveGameButton_Click(object sender, EventArgs e)
        {
            var saveMessage = new SaveGameMessage(RollbackManager.CurrentState.TickNr, "Standard");
            saveMessage.Send(Server, true);
        }

        public static void Icon_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("placeholder");
        }

        public void SettingsButton_Click(object sender, EventArgs e)
        {
            RemoveMenus("pause");
            RemoveMenus("In-game_Settings");
            var getSettingsMenu = mMenuGenerator.GetIngameSettingsMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(getSettingsMenu);
        }
        
        public void EffectsVolumeIncrease_Click(object sender, EventArgs e)
        {
            var currentVolume = SoundManager.GetSfxVolume();
            if (currentVolume + NumberManager.ZeroPointZeroFive <= 1)
            {
                currentVolume += NumberManager.ZeroPointZeroFive;
            }

            SoundManager.SetSfxMasterVolume(currentVolume);
            Game1.GetGame().mSettings.mEffectsVolume = currentVolume;
        }

        public void EffectsVolumeDecrease_Click(object sender, EventArgs e)
        {
            var currentVolume = SoundManager.GetSfxVolume();
            if (currentVolume - NumberManager.ZeroPointZeroFive >= 0)
            {
                currentVolume -= NumberManager.ZeroPointZeroFive;
            }

            SoundManager.SetSfxMasterVolume(currentVolume);
            Game1.GetGame().mSettings.mEffectsVolume = currentVolume;
        }
        
        public void MusicVolumeIncrease_Click(object sender, EventArgs e)
        {
            var currentVolume = SoundManager.GetMusicVolume();
            if (currentVolume + NumberManager.ZeroPointZeroFive <= 1)
            {
                currentVolume += NumberManager.ZeroPointZeroFive;
            }

            SoundManager.SetMusicVolume(currentVolume);
            Game1.GetGame().mSettings.mMusicVolume = currentVolume;
        }

        public void MusicVolumeDecrease_Click(object sender, EventArgs e)
        {
            var currentVolume = SoundManager.GetMusicVolume();
            Debug.WriteLine(currentVolume);
            if (currentVolume - NumberManager.ZeroPointZeroFive >= 0)
            {
                currentVolume -= NumberManager.ZeroPointZeroFive;
            }

            SoundManager.SetMusicVolume(currentVolume);
            Game1.GetGame().mSettings.mMusicVolume = currentVolume;
        }

        public void NumberBox_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("NumberBox_Click");
        }

        public void DisplayModeButton_Click(object sender, EventArgs e)
        {
            if (!mGraphicsDeviceManager.IsFullScreen)
            {
                mGraphicsDeviceManager.PreferredBackBufferWidth = Game1.mScreenW;
                mGraphicsDeviceManager.PreferredBackBufferHeight = Game1.mScreenH;
            }
            else
            {
                mGraphicsDeviceManager.PreferredBackBufferWidth =
                    (int) (Game1.mScreenW * NumberManager.ZeroPointSevenFiveD);
                mGraphicsDeviceManager.PreferredBackBufferHeight =
                    (int) (Game1.mScreenH * NumberManager.ZeroPointSevenFiveD);
            }
            mGraphicsDeviceManager.IsFullScreen = !mGraphicsDeviceManager.IsFullScreen;
            mGraphicsDeviceManager.ApplyChanges();
        }
        public void BackToPauseMenuButton_Click(object sender, EventArgs e)
        {
            Game1.GetGame().SaveSettings();
            RemoveMenus("In-game_Settings");
            var getPauseMenu = mMenuGenerator.GetPauseMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(getPauseMenu);

        }
        
        private void ShowStatistics(int winner)
        {
            ActiveMenus.Clear();
            RemoveRoadSelection();
            BuildingSelected = null;
            ActiveMenus.Add(mMenuGenerator.GetStatisticsMenu(Game1.GetGame().ImageManager,this,winner));
        }
        public void WorkerButton_Click(object sender, EventArgs e)
        {
            RemoveRoadSelection();
            BuildingSelected = null;
            RemoveMenus("BuildingSwitches");
            RemoveMenus("UnitAmounts");
            var amounts = RollbackManager.CurrentState.Map.AmountOfAllStoredResources(PlayerNumber);
            var getResourcesMenu = mMenuGenerator.GetResourcesMenu(Game1.GetGame().ImageManager, amounts);
            ActiveMenus.Add(getResourcesMenu);
        }

        public void BuildButton_Click(object sender, EventArgs e)
        {
            RemoveRoadSelection();
            BuildingSelected = null;
            RemoveMenus("UnitAmounts");
            RemoveMenus("ResourcesAmounts");
            var getBuildMenu = mMenuGenerator.GetBuildingsMenu(Game1.GetGame().ImageManager, this);
            ActiveMenus.Add(getBuildMenu);
        }

        public void UnitsButton_Click(object sender, EventArgs e)
        {
            RemoveRoadSelection();
            BuildingSelected = null;
            RemoveMenus("BuildingSwitches");
            RemoveMenus("ResourcesAmounts");
            var units = RollbackManager.CurrentState.Map.CountPeopleByProfession(PlayerNumber);
            var getUnitsMenu = mMenuGenerator.GetUnitsMenu(Game1.GetGame().ImageManager, this, units);
            ActiveMenus.Add(getUnitsMenu);
        }

        public void AttackButton_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton button)
            {
                
                AttackButtonOn = button.IsClick();
                
            }
        }

        public void HouseButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is House ? null : new House();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void BarracksButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is Barracks ? null : new Barracks();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void WorkerTrainingCenterButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is WorkerTrainingCenter ? null : new WorkerTrainingCenter();
            AddCostMenu();
            RemoveRoadSelection();
        }
        
        public void TowerButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is Tower ? null : new Tower();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void LodgeButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is ForestersLodge ? null : new ForestersLodge();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void IronForgeButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is IronForge ? null : new IronForge();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void IronMineButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is IronMine ? null : new IronMine();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void SawmillButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is Sawmill ? null : new Sawmill();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void StoneProcessingButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is StoneProcessing ? null : new StoneProcessing();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void StoneMine_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is StoneMine ? null : new StoneMine();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void WarehouseButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is Warehouse ? null : new Warehouse();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void ShipyardButton_Click(object sender, EventArgs e)
        {
            RemoveCostMenu();
            BuildingSelected = BuildingSelected is Shipyard ? null : new Shipyard();
            AddCostMenu();
            RemoveRoadSelection();
        }

        public void RoadButton_Click(object sender, EventArgs e)
        {

            if (BuildingSelected is Scaffolding)
            {
                RoadSelected = false;
                BuildingSelected = null;
                RemoveRoadSelection();
            }
            else
            {
                RemoveCostMenu();
                BuildingSelected = new Scaffolding();
                RoadSelected = true;
                var roadConfirmMenu = mMenuGenerator.GetRoadConfirmMenu(Game1.GetGame().ImageManager, this);
                ActiveMenus.Add(roadConfirmMenu);
            }
        }

        private void RemoveRoadSelection()
        {
            SelectedStartRoad = new System.Drawing.Point(-NumberManager.OneThousand, -NumberManager.OneThousand);
            RoadBuildingList = null;
            BridgeBuildingList = null;
            RoadSelected = false;
            RemoveMenus("RoadConfirmMenu");
        }

        public void Deposit_Click(object sender, EventArgs e)
        {
            var work = new List<Worker>();
            foreach (var id in SelectedGameObjectId)
            {
                var ob = RollbackManager.CurrentState.Map.GetObject(id);
                if (ob is Worker worker)
                {
                    work.Add(worker);
                }
                if (ob is ResourceBuilding resourceBuilding)
                {
                    ResourceBuildingSelected = resourceBuilding;
                }
            }

            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle &&
                RollbackManager.CurrentState.Map.GetObject(gameObjectRectangle.TargetId) is ResourceBuilding building)
            {
                new ItemOutMessage(RollbackManager.CurrentState.TickNr, new ObjectAction()
                {
                    TransportingFromId = building.Id,
                    TransportingToId = building.Id,
                    ItemTransportIntent = Item.Nothing,
                    UserMade = true

                }, work).Send(Server, true);

                ResourceBuildingSelected = null;
            }
        }

        public void RemoveUnneeded()
        {
            RemoveMenus("worker-menu");
            RemoveMenus("StoredItems");
            RemoveMenus("tower");
            RemoveMenus("ship");
        }
        public void Withdraw_Click(object sender, EventArgs e)
        {
            foreach (var id in SelectedGameObjectId)
            {
                var ob = RollbackManager.CurrentState.Map.GetObject(id);
                if (ob is Worker worker)
                {
                    mWorkers.Add(worker);
                }
                if (ob is ResourceBuilding resourceBuilding)
                {
                    ResourceBuildingSelected = resourceBuilding;
                }
            }

            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle&&RollbackManager.CurrentState.Map.GetObject(gameObjectRectangle.TargetId) is ResourceBuilding building)
            {
                AddIfNewName(mMenuGenerator.GetStoredItems(Game1.GetGame().ImageManager,this,mWorkers.Count,building));
                
            }
            RemoveMenus("worker-menu");

            
        }

        public void Man_Tower_Click(object sender, EventArgs e)
        {
            if (!(sender is MenuButton b) || !(b.Menu.Area is GameObjectRectangle gameObjectRectangle))
            {
                return;
            }
            var targetId = gameObjectRectangle.TargetId;
            var messageList = new List<Message>();
            foreach (var id in SelectedGameObjectId)
            {
                var ob = RollbackManager.CurrentState.Map.GetObject(id);
                if (!(ob is ObjectMoving) || (!(ob is Shieldman) && !(ob is Spearman) && !(ob is Swordsman)))
                {
                    continue;
                }
                var newObjectAction = new ObjectAction
                {
                    GoingIntoId = targetId
                };
                var objectActionMessage = new NewObjectActionMessage(RollbackManager.CurrentState.TickNr, id, newObjectAction);
                messageList.Add(objectActionMessage);
            }
            if (messageList.Count > 0)
            {
                var multipleMessagesMessage =
                    new MultipleMessagesMessage(RollbackManager.CurrentState.TickNr, messageList);
                multipleMessagesMessage.Send(Server, true);
            }
        }

        public void Unman_Tower_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var tower = (Tower)RollbackManager.CurrentState.Map.GetObject(targetId);
                var messageList = new List<Message>();
                for (var i = 0; i < NumberManager.Ten; i++)
                {
                    var message = (UnloadTowerMessage)tower.UnmanTower(i);
                    if (message is null)
                    {
                        break;
                    }
                    message.Tick = RollbackManager.CurrentState.TickNr;
                    messageList.Add(message);
                }
                if (messageList.Count > 0)
                {
                    var multipleMessagesMessage =
                        new MultipleMessagesMessage(RollbackManager.CurrentState.TickNr, messageList);
                    multipleMessagesMessage.Send(Server, true);
                }
            }
        }

        internal void Man_TransportShip_Click(object sender, EventArgs e)
        {
            if (!(sender is MenuButton b) || !(b.Menu.Area is GameObjectRectangle gameObjectRectangle))
            {
                return;
            }
            var targetId = gameObjectRectangle.TargetId;
            var messageList = new List<Message>();
            foreach (var id in SelectedGameObjectId)
            {
                var ob = RollbackManager.CurrentState.Map.GetObject(id);
                if (!(ob is ObjectMoving) || (ob is TransportShip || ob is ScoutShip))
                {
                    continue;
                }
                var newObjectAction = new ObjectAction
                {
                    GoingIntoId = targetId
                };
                var objectActionMessage = new NewObjectActionMessage(RollbackManager.CurrentState.TickNr, id, newObjectAction);
                messageList.Add(objectActionMessage);
            }
            if (messageList.Count > 0)
            {
                var multipleMessagesMessage =
                    new MultipleMessagesMessage(RollbackManager.CurrentState.TickNr, messageList);
                multipleMessagesMessage.Send(Server, true);
            }
        }

        public void Unload_TransportShip_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var messageList = new List<Message>();
                var transportShip = (TransportShip)RollbackManager.CurrentState.Map.GetObject(targetId);
                for (var i = 0; i < NumberManager.Ten; i++)
                {
                    var message = (UnloadTransportShipMessage)transportShip.UnloadTransportShip(i);
                    if (message is null)
                    {
                        break;
                    }
                    message.Tick = RollbackManager.CurrentState.TickNr;
                    messageList.Add(message);
                }
                if (messageList.Count > 0)
                {
                    var multipleMessagesMessage =
                        new MultipleMessagesMessage(RollbackManager.CurrentState.TickNr, messageList);
                    multipleMessagesMessage.Send(Server, true);
                }
            }
        }

        public void Infiltrate_Click(object sender, EventArgs e)
        {
            if (!(sender is MenuButton b) || !(b.Menu.Area is GameObjectRectangle gameObjectRectangle))
            {
                return;
            }
            var targetId = gameObjectRectangle.TargetId;
            foreach (var id in SelectedGameObjectId)
            {
                var ob = RollbackManager.CurrentState.Map.GetObject(id);
                if (!(ob is Spy))
                {
                    continue;
                }
                var newObjectAction = new ObjectAction
                {
                    InfiltrateId = targetId
                };
                var objectActionMessage =
                    new NewObjectActionMessage(RollbackManager.CurrentState.TickNr, id, newObjectAction);
                objectActionMessage.Send(Server, true);
            }
        }

        public void Leave_Tower_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var tower = (Tower)RollbackManager.CurrentState.Map.GetObject(targetId);

                var message = (LeaveTowerMessage)tower.LeaveTower();
                if (message is null)
                {
                    return;
                }
                message.Tick = RollbackManager.CurrentState.TickNr;
                message.Send(Server, true);
            }
        }

        public void Kill_Spy_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var tower = (Tower) RollbackManager.CurrentState.Map.GetObject(targetId);
                foreach (var id in SelectedGameObjectId)
                {
                    var ob = RollbackManager.CurrentState.Map.GetObject(id);
                    if (!(ob is Spy))
                    {
                        continue;
                    }
                    var message = (NewObjectActionMessage)tower.KillSpy();
                    if (message is null)
                    {
                        return;
                    }
                    message.Tick = RollbackManager.CurrentState.TickNr;
                    message.Id = ob.Id;
                    message.Action.GoingIntoId = tower.Id;
                    message.Send(Server, true);
                }
            }
        }

        public void Plant_Harvest_Mode_Button_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var message = new ForestersLodgeModeMessage(RollbackManager.CurrentState.TickNr, targetId, true);
                message.Send(Server, true);
            }
        }

        public void Harvest_Only_Button_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var message = new ForestersLodgeModeMessage(RollbackManager.CurrentState.TickNr, targetId, false);
                message.Send(Server, true);
            }
        }

        public void DestroyBuildingButton_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                var building = (Building) RollbackManager.CurrentState.Map.GetObject(targetId);
                var message = new NewRemoveBuildingMessage(RollbackManager.CurrentState.TickNr,building);
                message.Send(Server, true);
                RemoveMenus("selected" + targetId);
            }
        }

        private Vector2 GetLandSpawnLocation(ObjectImmovable resourceBuildingSelected)
        {
            var spawnPoint = new Vector2(-1, -1);
            for (var j = 0; j <= resourceBuildingSelected.TileSize.Height; j++)
            {
                
                for (var i = 0; i <= resourceBuildingSelected.TileSize.Width; i++) // 
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j >= 1)
                    {
                        xAdd = resourceBuildingSelected.TileSize.Width;
                    }

                    var convertedLocationX = (uint) resourceBuildingSelected.Location.X + xAdd;
                    var convertedLocationY = (uint) resourceBuildingSelected.Location.Y + yAdd;
                    if (RollbackManager.CurrentState.Map.GetObject(resourceBuildingSelected.Location.X + xAdd,
                            resourceBuildingSelected.Location.Y + yAdd) == null &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) != Terrain.Water &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) != Terrain.OutOfBounds)
                    {
                        spawnPoint.X = (resourceBuildingSelected.Location.X + xAdd);
                        spawnPoint.Y = (resourceBuildingSelected.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            for (var j = resourceBuildingSelected.TileSize.Height + 1; j >= 0; j--)
            {

                for (var i = resourceBuildingSelected.TileSize.Width; i >= -1; i--)
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j < resourceBuildingSelected.TileSize.Height + 1)
                    {
                        xAdd = -1;
                    }

                    var convertedLocationX = (uint) resourceBuildingSelected.Location.X + xAdd;
                    var convertedLocationY = (uint) resourceBuildingSelected.Location.Y + yAdd;
                    if (RollbackManager.CurrentState.Map.GetObject(resourceBuildingSelected.Location.X + xAdd,
                            resourceBuildingSelected.Location.Y + yAdd) == null &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) != Terrain.Water &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) != Terrain.OutOfBounds)
                    {
                        spawnPoint.X = (resourceBuildingSelected.Location.X + xAdd);
                        spawnPoint.Y = (resourceBuildingSelected.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            return spawnPoint;
        }

        private Vector2 GetWaterSpawnLocation(ObjectImmovable resourceBuildingSelected)
        {
            var spawnPoint = new Vector2(0, 0);
            for (var j = 0; j <= resourceBuildingSelected.TileSize.Height; j++)
            {

                for (var i = 0; i <= resourceBuildingSelected.TileSize.Width; i++) // 
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j >= 1)
                    {
                        xAdd = resourceBuildingSelected.TileSize.Width;
                    }

                    var convertedLocationX = (uint) resourceBuildingSelected.Location.X + xAdd;
                    var convertedLocationY = (uint) resourceBuildingSelected.Location.Y + yAdd;
                    if (RollbackManager.CurrentState.Map.GetObject(resourceBuildingSelected.Location.X + xAdd,
                            resourceBuildingSelected.Location.Y + yAdd) == null &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) == Terrain.Water)
                    {
                        spawnPoint.X = (resourceBuildingSelected.Location.X + xAdd);
                        spawnPoint.Y = (resourceBuildingSelected.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            for (var j = resourceBuildingSelected.TileSize.Height + 1; j >= 0; j--)
            {

                for (var i = resourceBuildingSelected.TileSize.Width; i >= -1; i--)
                {
                    var yAdd = j - 1;
                    var xAdd = i;
                    if (j < resourceBuildingSelected.TileSize.Height + 1)
                    {
                        xAdd = -1;
                    }

                    var convertedLocationX = (uint) resourceBuildingSelected.Location.X + xAdd;
                    var convertedLocationY = (uint) resourceBuildingSelected.Location.Y + yAdd;
                    if (RollbackManager.CurrentState.Map.GetObject(resourceBuildingSelected.Location.X + xAdd,
                            resourceBuildingSelected.Location.Y + yAdd) == null &&
                        RollbackManager.CurrentState.Map.GetTerrainAt((uint) convertedLocationX,
                            (uint) convertedLocationY) == Terrain.Water)
                    {
                        spawnPoint.X = (resourceBuildingSelected.Location.X + xAdd);
                        spawnPoint.Y = (resourceBuildingSelected.Location.Y + yAdd);
                        return spawnPoint;
                    }

                }
            }

            return spawnPoint;
        }

        private void ChangePriorityClick(object sender, int amount)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                var message =
                    new PriorityMessage(RollbackManager.CurrentState.TickNr,
                        ResourceBuildingSelected.Priority + amount,
                        targetId);
                if (NumberManager.Ten > ResourceBuildingSelected.Priority + amount && ResourceBuildingSelected.Priority + amount > 0)
                {
                    message.Send(Server, true);
                }
            }
        }

        public void AddPriorityClick(object sender, EventArgs e)
        {
            ChangePriorityClick(sender, 1);
        }

        public void RemovePriorityClick(object sender, EventArgs e)
        {
            ChangePriorityClick(sender, -1);
        }

        public void TrainShieldman_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);
                ObjectMovingCreate = new Shieldman();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetLandSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(-1, -1))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                        var sound = SoundManager;
                        sound.PlaySfx(sound.Building2);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainSpearman_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new Spearman();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetLandSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(-1, -1))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                        var sound = SoundManager;
                        sound.PlaySfx(sound.Building2);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainSwordsman_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new Swordsman();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetLandSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(-1, -1))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                        var sound = SoundManager;
                        sound.PlaySfx(sound.Building2);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainWorker_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new Worker();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetLandSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(-1, -1))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainTransportShip_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new TransportShip();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetWaterSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(0, 0))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainScoutShip_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new ScoutShip();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetWaterSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(0, 0))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void TrainSpy_Click(object sender, EventArgs e)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle)
            {
                var targetId = gameObjectRectangle.TargetId;
                ResourceBuildingSelected = (ResourceBuilding) RollbackManager.CurrentState.Map.GetObject(targetId);

                ObjectMovingCreate = new Spy();
                if (ObjectMovingCreate.Train(ResourceBuildingSelected))
                {
                    ObjectMovingCreate.PlayerNumber = PlayerNumber;
                    var spawnPoint = GetLandSpawnLocation(ResourceBuildingSelected);
                    if (spawnPoint != new Vector2(-1, -1))
                    {
                        ObjectMovingCreate.X = spawnPoint.X + (float)NumberManager.ZeroPointFive;
                        ObjectMovingCreate.Y = spawnPoint.Y + (float)NumberManager.ZeroPointFive;
                        var message =
                            new NewObjectMovingCreationMessage(RollbackManager.CurrentState.TickNr,
                                ObjectMovingCreate,
                                targetId);
                        message.Send(Server, true);
                        var sound = SoundManager;
                        sound.PlaySfx(sound.Building2);
                    }
                }
                else
                {
                    Debug.WriteLine("NOT ENOUGH RESOURCES");
                }
            }
        }

        public void RoadCheck_Click(object sender, EventArgs e)
        {
            if (RoadBuildingList != null)
            {
                if (RoadBuildingList.Count >= 1)
                {
                    var newRoadMessage =
                        new NewRoadMessage(RollbackManager.CurrentState.TickNr, RoadBuildingList);
                    newRoadMessage.Send(Server, true);
                }
            }

            if (BridgeBuildingList != null)
            {
                if (BridgeBuildingList.Count >= 1)
                {
                    var newBridgeMessage =
                        new NewBridgeMessage(RollbackManager.CurrentState.TickNr, BridgeBuildingList);
                    newBridgeMessage.Send(Server, true);
                }
            }

            BridgeBuildingList = null;
            RoadBuildingList = null;
            RoadSelected = false;
            SelectedStartRoad = new System.Drawing.Point(-NumberManager.OneThousand, -NumberManager.OneThousand);
            RemoveMenus("RoadConfirmMenu");
            ClickActiveSwitch("BuildingSwitches");
        }

        private void ClickActiveSwitch(string menuName)
        {
            foreach (var menu in ActiveMenus)
            {
                if (menu.SameName(menuName))
                {
                    menu.Switches(true)[0].OnClick();
                    return;
                }
            }
        }

        public void RoadCross_Click(object sender, EventArgs e)
        {
            RoadBuildingList = null;
            RoadSelected = false;
            BridgeBuildingList = null;
            SelectedStartRoad = new System.Drawing.Point(-NumberManager.OneThousand, -NumberManager.OneThousand);
            RemoveMenus("RoadConfirmMenu");
            ClickActiveSwitch("BuildingSwitches");
        }

        private void GetItemOut(object sender, Item item)
        {
            if (sender is MenuButton b && b.Menu.Area is GameObjectRectangle gameObjectRectangle &&
                RollbackManager.CurrentState.Map.GetObject(gameObjectRectangle.TargetId) is ResourceBuilding building)
            {
                new ItemOutMessage(RollbackManager.CurrentState.TickNr, new ObjectAction()
                {
                    TransportingFromId = building.Id,
                    TransportingToId = -1,
                    ItemTransportIntent = item,
                    IsOccupied = true,
                    UserMade = true
                }, mWorkers).Send(Server, true);
                mWorkers.Clear();
                RemoveMenus("StoredItems");
            }
        }
        public void GetItemOutPlanks(object sender, EventArgs e)
        {

            GetItemOut(sender, Item.Plank);

        }

        public void GetItemOutStone(object sender, EventArgs e)
        { 
            GetItemOut(sender, Item.Stone);
        }

        public void GetItemOutRawStone(object sender, EventArgs e)
        {

            GetItemOut(sender, Item.RawStone);

        }

        public void GetItemOutIron(object sender, EventArgs e)
        {

            GetItemOut(sender, Item.Iron);

        }

        public void GetItemOutWood(object sender, EventArgs e)
        {

            GetItemOut(sender, Item.Wood);

        }

        public void GetItemOutIronOre(object sender, EventArgs e)
        {

            GetItemOut(sender, Item.IronOre);

        }
    }
}
    