using System;
using Game.org.graphic;
using Game.org.input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using Common.com.game.settings;
using Common.com.Menu;
using Game.org.main;
using Microsoft.Xna.Framework.Content;
using Common.com.networking;
using Common.com.game.achievments;

namespace Game.org.gameStates
{
    public sealed class MainMenu : IGameState, IPacketListener
    {
        private readonly Menu mBackToMain;
        public HashSet<Menu> ActiveMenus { get; }
        public MouseState LastMouseState { get; set; }
        public MenuTextField ActiveField { get; private set; }
        private readonly MenuRendering mRenderer;
        private readonly MenuInputManager mInput;
        private readonly Game1 mGame;
        private Connection mConnection;
        private readonly MenuGenerator mMenuGenerator;
        private readonly SoundManager mSoundManager;
        private readonly GraphicsDeviceManager mGraphicsDeviceManager;
        private readonly AchievementManager mAchievementManager;
        public string mLastIpAddress;

        public MainMenu(Game1 game, GraphicsDeviceManager graphicsDeviceManager, ContentManager content)
        {
            ActiveMenus = new HashSet<Menu>();
            var menuGenerator = new MenuGenerator(content, graphicsDeviceManager.GraphicsDevice);



            mMenuGenerator = menuGenerator;
            mSoundManager = game.SoundManager;
            mGraphicsDeviceManager = graphicsDeviceManager;

            mRenderer = new MenuRendering(this);
            mInput = new MenuInputManager(this);
            mGame = game;
            ActiveMenus.Add(menuGenerator.GetMain(mGame.ImageManager, this));
            mGame.LoadSettings();
            mBackToMain = menuGenerator.GetBackMenu(mGame.ImageManager, this);
            mLastIpAddress = mGame.mSettings.mLastIpAddress;

            mAchievementManager = AchievementManager.GetManager();
        }

        public IInputManager GetInputManager()
        {
            return mInput;
        }

        public void Update(GameTime gameTime)
        {

        }

        public IRendering GetRenderer()
        {
            return mRenderer;
        }

        public void OnClick(MouseState mouseState)
        {
            foreach (var menu in ActiveMenus)
            {
                if (menu.OnClick(mouseState))
                {
                    break;
                }
            }
        }

        public void Stop()
        {
            if (mConnection != null)
            {
                mConnection.RemoveListener(this);
            }
        }

        public void DemoGameButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("New Game");
            if (mConnection != null)
            {
                mConnection.Disconnect();
            }

            try
            {

                mConnection = Connection.SelfConnect(Game1.GetGame().mSettings.mPort);
                mConnection.AddListener(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void NewGameButton_Click(object sender, EventArgs e)
        {
            mGame.LoadSettings();
            mLastIpAddress = mGame.mSettings.mLastIpAddress;
            ActiveMenus.Clear();
            var inputMenu = mMenuGenerator.GetInputMenu(mGame.ImageManager, this);
            ActiveMenus.Add(inputMenu);
            ActiveMenus.Add(mBackToMain);
        }


        public void QuitGameButton_Click(object sender, EventArgs e)
        {
            mGame.Exit();
        }

        public void AchievementsButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Im just here to fix a Issue from SonarCube :)");
            ActiveMenus.Clear();
            ActiveMenus.Add(mMenuGenerator.GetAchievementsMenu(mGame.ImageManager, mAchievementManager));
            ActiveMenus.Add(mBackToMain);
        }

        public void SettingButton_Click(object sender, EventArgs e)
        {
            ActiveMenus.Clear();
            ActiveMenus.Add(mBackToMain);
            ActiveMenus.Add(mMenuGenerator.GetMainMenuSettingsMenu(mGame.ImageManager, this));
        }

        public void BackButton_Click(object sender, EventArgs e)
        {
            Game1.GetGame().mSettings.mPort = mMenuGenerator.Port;
            ActiveField = null;
            Game1.GetGame().SaveSettings();
            ActiveMenus.Clear();
            ActiveMenus.Add(mMenuGenerator.GetMain(mGame.ImageManager, this));
        }

        public void HandlePacket(byte[] content, Connection c)
        {
            Game1.GetGame().SwitchWith(content, mConnection);
        }

        public void OnDisconnect(Connection c)
        {
            mConnection = null;
        }

        public void ActiveTextField_Click(object sender, EventArgs e)
        {
            ActiveField = (MenuTextField) sender;
        }

        public void ConnectButton_Click(object sender, EventArgs e)
        {
            Connect(mMenuGenerator.mConnectField.Text);
        }

        private void Connect(string ip)
        {
            mLastIpAddress = ip;
            mGame.mSettings.mLastIpAddress = ip;
            mGame.SaveSettings();
            mConnection = new Connection(ip,Game1.GetGame().mSettings.mPort);
            mConnection.Connect(null);
            mConnection.AddListener(this);
        }

        public void EffectsVolumeIncrease_Click(object sender, EventArgs e)
        {
            var currentVolume = mSoundManager.GetSfxVolume();
            if (currentVolume + NumberManager.ZeroPointZeroFive <= 1)
            {
                currentVolume += NumberManager.ZeroPointZeroFive;
            }
            mSoundManager.SetSfxMasterVolume(currentVolume);
            Game1.GetGame().mSettings.mEffectsVolume = currentVolume;
        }

        public void EffectsVolumeDecrease_Click(object sender, EventArgs e)
        {
            var currentVolume = mSoundManager.GetSfxVolume();
            if (currentVolume < NumberManager.ZeroPointOne)
            {
                currentVolume = 0.0f;
            }
            if (currentVolume - NumberManager.ZeroPointZeroFive >= 0)
            {
                currentVolume -= NumberManager.ZeroPointZeroFive;
            }

            mSoundManager.SetSfxMasterVolume(currentVolume);
            Game1.GetGame().mSettings.mEffectsVolume = currentVolume;
        }
        
        public void MusicVolumeIncrease_Click(object sender, EventArgs e)
        {
            var currentVolume = mSoundManager.GetMusicVolume();
            if (currentVolume + NumberManager.ZeroPointZeroFive <= 1)
            {
                currentVolume += NumberManager.ZeroPointZeroFive;
            }
            mSoundManager.SetMusicVolume(currentVolume);
            Game1.GetGame().mSettings.mMusicVolume = currentVolume;
        }

        public void MusicVolumeDecrease_Click(object sender, EventArgs e)
        {
            var currentVolume = mSoundManager.GetMusicVolume();
            if (currentVolume < NumberManager.ZeroPointOne)
            {
                currentVolume = 0.0f;
            }
            if (currentVolume - NumberManager.ZeroPointZeroFive >= 0)
            {
                currentVolume -= NumberManager.ZeroPointZeroFive;
            }
            mSoundManager.SetMusicVolume(currentVolume);
            Game1.GetGame().mSettings.mMusicVolume = currentVolume;
        }

        public void NumberBox_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("NumberBox_Click");
        }

        public void DisplaymodeButton_Click(object sender, EventArgs e)
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
            mGraphicsDeviceManager.ToggleFullScreen();
            mGraphicsDeviceManager.ApplyChanges();
        }
    }
}
