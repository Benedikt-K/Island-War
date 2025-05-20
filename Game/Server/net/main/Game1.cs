using System;
using System.Collections.Generic;
using System.IO;
using Common.com.game;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.networking;
using Common.com.networking.Messages;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.rollbacks;
using Common.com.serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Server.net.map;
using Server.net.networking;

namespace Server.net.main
{
    public sealed class Game1 : Game, IPacketListener
    {
        private readonly GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;
        private PlayerManager mPlayerManager;
        public MessageWrapper MessageWrapper { get; private set; }
        public RollbackManager RollbackManager { get; private set; }
        private TimeManager mTimeManager;
        private ServerMenu mServerMenu;
        private readonly ServerMenuRendering mServerMenuRendering;
        private long mTickAmount;
        public ServerMenuGenerator MenuGenerator { get; private set; }
        public ServerMenuInputManager Input { get; private set; }
        private TreeSystem TreeSystem { get; set; }
        public bool Start { get; set; }
        private static Game1 sGame1;
        public static Game1 Game1Get => GetGame1();

        public static Vector2[] CameraStart { get; set; }
        public MouseState LastMouseState { get; set; }
        public HashSet<Menu> ActiveMenus { get; }
        
        public Game1()
        {
            CameraStart = new Vector2[NumberManager.Two];
            CameraStart[0] = new Vector2(-1,-1);
            CameraStart[1] = new Vector2(-1,-1);
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mServerMenuRendering = new ServerMenuRendering(this);
            ActiveMenus = new HashSet<Menu>();
            sGame1 = this;

        }

        private static Game1 GetGame1()
        {
            return sGame1;
        }
        public void Stop()
        {
            mPlayerManager.Stop();
        }
        public MapMessage GetMapMessage(int uid,bool cloneTiles)
        {
            MapMessage res= RollbackManager.CurrentState.Map.ToMapMessage(RollbackManager.CurrentState.TickNr,uid,new Vector2[2],RollbackManager.IsPaused(),RollbackManager.CurrentState.MaxPlayers,cloneTiles);
            if (res.StatisticsManager != null)
            {
                Console.WriteLine(res.StatisticsManager.GetStatistic(Statistic.BuildingsBuilt,1));
            }
            res.CurrentId = IdGenerator.CurrentId;
            if (CameraStart[0].X>=0)
            {
                res.CameraStart = CameraStart;
            }

            return res;
        }
        protected override void Initialize()
        {
            Start = false;
            Window.AllowUserResizing = true;
            base.Initialize();
        }
        public void LoadTch()
        {
            GameMap.OnStart();
            var maxPlayers = NumberManager.Two;
            var map=MapGeneration.TestMap;
            var gameState = new GameState(map,0,true,maxPlayers);
            RollbackManager = new RollbackManager(gameState,RollbackManager.DefaultSize);
            MessageWrapper = new MessageWrapper(RollbackManager,null);
            mPlayerManager = new PlayerManager(maxPlayers,ServerMenuGenerator.Port,this,MessageWrapper);
            MessageWrapper.SetConnectionManager(mPlayerManager.ConnectionManager);
            TreeSystem = new TreeSystem(MessageWrapper,RollbackManager.CurrentState.TickNr,map);
            RollbackManager.AddTickListener(TreeSystem);
            RollbackManager.AddTickListener(new Synchronicity(MessageWrapper));
            
        }
        public void LoadTest()
        {
            GameMap.OnStart();
            var maxPlayers = NumberManager.Two;
            var map=MapGeneration.GenerateMap();
            var gameState = new GameState(map,0,true,maxPlayers);
            RollbackManager = new RollbackManager(gameState,RollbackManager.DefaultSize);
            MessageWrapper = new MessageWrapper(RollbackManager,null);
            mPlayerManager = new PlayerManager(maxPlayers,ServerMenuGenerator.Port,this,MessageWrapper);
            MessageWrapper.SetConnectionManager(mPlayerManager.ConnectionManager);
            TreeSystem = new TreeSystem(MessageWrapper,RollbackManager.CurrentState.TickNr,map);
            RollbackManager.AddTickListener(TreeSystem);
            RollbackManager.AddTickListener(new Synchronicity(MessageWrapper));

        }

        public void LoadTechDemo()
        {
            GameMap.OnStart();
            var maxPlayers = NumberManager.Two;
            var map = MapGeneration.TechDemo;
            var gameState = new GameState(map, 0, true, maxPlayers);
            RollbackManager = new RollbackManager(gameState, RollbackManager.DefaultSize);
            MessageWrapper = new MessageWrapper(RollbackManager, null);
            mPlayerManager = new PlayerManager(maxPlayers, ServerMenuGenerator.Port, this, MessageWrapper);
            MessageWrapper.SetConnectionManager(mPlayerManager.ConnectionManager);
            TreeSystem = new TreeSystem(MessageWrapper, RollbackManager.CurrentState.TickNr, map);
            RollbackManager.AddTickListener(TreeSystem);
            CameraStart[0] = new Vector2(NumberManager.Thirty, NumberManager.Thirty);
            CameraStart[1] = new Vector2(NumberManager.Seventy, NumberManager.Seventy);
            RollbackManager.AddTickListener(new Synchronicity(MessageWrapper));

        }
        public void SaveGame(string name)
        {
            if (!(Directory.Exists(Content.RootDirectory + "/saves")))
            {
                Directory.CreateDirectory(Content.RootDirectory + "/saves");
            }
            File.WriteAllBytes(Content.RootDirectory + "/saves/" + name+".json", GetMapMessage(0,true).Serialize());
        }

        private void LoadGame(MapMessage message)
        {
            IdGenerator.CurrentId=message.CurrentId;
            GameMap.OnStart();
            var map = GameMap.GetMap(message,true);
            CameraStart = message.CameraStart;
            var gameState = new GameState(map,message.Tick,true,message.MaxPlayers);
            RollbackManager = new RollbackManager(gameState,RollbackManager.DefaultSize);
            MessageWrapper = new MessageWrapper(RollbackManager, null);
            mPlayerManager = new PlayerManager(message.MaxPlayers,ServerMenuGenerator.Port,this,MessageWrapper);
            MessageWrapper.SetConnectionManager(mPlayerManager.ConnectionManager);
            TreeSystem = new TreeSystem(MessageWrapper,RollbackManager.CurrentState.TickNr,map);
            RollbackManager.AddTickListener(TreeSystem);
            RollbackManager.AddTickListener(new Synchronicity(MessageWrapper));
        }
        public void LoadGame(string name)
        {
            if (File.Exists(Content.RootDirectory + "/saves/" + name + ".json"))
            {
                LoadGame((MapMessage) JsonSerializable.Deserialize(
                    File.ReadAllBytes(Content.RootDirectory + "/saves/" + name + ".json")));
            }

        }
        protected override void LoadContent()
        {
            
            mSpriteBatch = new SpriteBatch(GraphicsDevice);
            MenuGenerator = new ServerMenuGenerator(Content, mGraphics.GraphicsDevice);
            mServerMenu = new ServerMenu(this);
            ActiveMenus.Add(MenuGenerator.GetMain(mServerMenu));
            Input = new ServerMenuInputManager(mServerMenu, this);
            mServerMenu.Start();
            
            

        }

        private void UpdateSecond()
        {
            mPlayerManager.DisconnectInactive();
            MessageWrapper.SpreadMessage(new PingMessage(),true);
        }
        protected override void Update(GameTime gameTime)
        {
            if (Start)
            {
                mTickAmount += gameTime.ElapsedGameTime.Ticks;
                if (mTickAmount >= TimeSpan.TicksPerSecond)
                {
                    mTickAmount -= TimeSpan.TicksPerSecond;
                    UpdateSecond();
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                    Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }

                if (!RollbackManager.IsPaused())
                {
                    mTimeManager ??= new TimeManager();
                    var toCalc = mTimeManager.ElapseTime();
                    
                    
                    for (var i = toCalc-1; i >= 0; i--)
                    {
                        RollbackManager.CalculateNextTicks(1);
                    }
                }
                else
                {
                    if (mTimeManager != null)
                    {
                        mTimeManager.ElapseTime();
                    }
                }
            }

            mServerMenu.Update();
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            mServerMenuRendering.Render(mSpriteBatch);
            mSpriteBatch.End();
            base.Draw(gameTime);
        }


        public void HandlePacket(byte[] content, Connection connection)
        {
            var playerId = mPlayerManager.ToPlayerId(connection);
            if (playerId==-1)
            {
                return;
            }
            var sender=mPlayerManager.ToPlayer(playerId);
            var serializable = JsonSerializable.Deserialize(content);
            if (serializable is Message message)
            {
                MessageWrapper.Handle(message,playerId,sender);
            }
        }
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            if (mPlayerManager != null)
            {
                mPlayerManager.Stop();
            }
        }
        public void OnDisconnect(Connection c)
        {
            mPlayerManager.OnDisconnect(c);
        }

    }
}