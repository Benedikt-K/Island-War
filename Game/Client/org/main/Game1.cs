using System;
using System.IO;
using Common.com.game;
using Common.com.game.settings;
using Common.com.networking;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.serialization;
using Game.org.gameStates;
using Game.org.graphic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.org.main
{
    public sealed class Game1 : Microsoft.Xna.Framework.Game
    {
        private static Game1 sInstance;
        private readonly GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;
        private IRendering mRenderer;  
        public IGameState mGameState;
        public ImageManager ImageManager { get; private set; }
        public SoundManager SoundManager { get; private set; }
        public Settings mSettings;

        public static int mScreenW;
        public static int mScreenH;

        private Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mScreenW = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; 
            mScreenH = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
    }

        public static Game1 GetGame()
        {
            return sInstance ??= new Game1();
        }
        public void SwitchGameState(IGameState newState)
        {
            if (mGameState != null) 
            { 
               mGameState.Stop(); 
            }
            
            var s = SoundManager;
            mGameState = newState; 
            mRenderer=mGameState.GetRenderer();
    
            if(mGameState is MainMenu)
            {
                LoadSettings();
                s.StopMusic();
                s.PlayMusic(s.MusicMainMenu);
            } else if(mGameState is InGame)
            {
                LoadSettings();
                s.StopMusic();
                s.PlayRandomMusic();
            }
        }
        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            mGameState.Stop();
        }
        protected override void Initialize()
        {

            Window.AllowUserResizing = true;

            mGraphics.PreferredBackBufferWidth =
                (int) (Game1.mScreenW * NumberManager.ZeroPointSevenFiveD);
            mGraphics.PreferredBackBufferHeight =
                (int) (Game1.mScreenH * NumberManager.ZeroPointSevenFiveD);
            mGraphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);
            ImageManager = new ImageManager(Content);
            SoundManager = new SoundManager(Content);
            ImageManager.LoadContent();
            SoundManager.LoadContent();
            LoadSettings();
            SwitchGameState(new MainMenu(this, mGraphics, Content));
        }

        protected override void Update(GameTime gameTime)
        { 
            mGameState.Update(gameTime);
            mGameState.GetInputManager().HandleInput(Keyboard.GetState(),Mouse.GetState());
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.CornflowerBlue);
            mSpriteBatch.Begin();
            mRenderer.Render(mSpriteBatch);
            mSpriteBatch.End();
            base.Draw(gameTime);
        }

        public void SwitchWith(byte[] content, Connection connection)
        {
            GameMap.OnStart();
            var res = JsonSerializable.Deserialize(content);
            if (res is MapMessage mapMessage)
            {
                GameMap.IslandMapper = null;
                SwitchGameState(new InGame(
                    new GameState(GameMap.GetMap(mapMessage,true),
                        mapMessage.Tick,
                        mapMessage.IsPaused,
                        mapMessage.MaxPlayers),
                    connection,
                    mapMessage.PlayerNumber,
                    mGraphics,
                    mapMessage.CameraStart[mapMessage.PlayerNumber-1],
                    Content));
            }

            if (res is PauseMessage pauseMessage && mGameState is InGame inGame)
            {
                inGame.RollbackManager.RollbackState(pauseMessage, pauseMessage.Tick);
            }
        }

        public void SaveSettings()
        {
            var dir = Directory.GetParent(Content.RootDirectory);
            var settingsFileNameDir = dir + "/Settings.json";
            File.WriteAllBytes(settingsFileNameDir, mSettings.Serialize());

        }

        public void LoadSettings()
        {
            var dir = Directory.GetParent(Content.RootDirectory);
            var settingsFileNameDir = dir + "/Settings.json";
            if (File.Exists(settingsFileNameDir))
            {
                mSettings = (Settings)JsonSerializable.Deserialize(File.ReadAllBytes(settingsFileNameDir));
            }
            else
            {
                mSettings = new Settings(NumberManager.ZeroPointFiveF, NumberManager.ZeroPointFiveF, NumberManager.PortNumber);
            }
            GetGame().SoundManager.SetMusicVolume(mSettings.mMusicVolume);
            GetGame().SoundManager.SetSfxMasterVolume(mSettings.mEffectsVolume);
            
        }

    }
}