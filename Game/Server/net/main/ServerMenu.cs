using System;
using Common.com.game;
using Common.com.Menu;
using Microsoft.Xna.Framework.Input;

namespace Server.net.main
{
    public sealed class ServerMenu
    {
        public MenuTextField ActiveField { get; private set; }
        public string NewGameName { get; private set; }

        private ServerMenuInputManager mInput;
        private readonly Game1 mGame;
        private readonly ServerMenuGenerator mMenuGenerator;
        private static ServerMenu sServerMenu;
        public static ServerMenu ServerMenuGet => GetSeverMenu();

        public ServerMenu(Game1 game)
        {
            mGame = game;
            mMenuGenerator = mGame.MenuGenerator;
            SetMenu(this);
        }

        private static void SetMenu(ServerMenu menu)
        {
            sServerMenu = menu;
        }
        public void Start()
        {
            mInput = mGame.Input;
        }
        public void Update()
        {
            mInput.HandleInput(Keyboard.GetState(), Mouse.GetState());
        }
        
        public void OnClick(MouseState mouseState)
        {
            foreach (var menu in mGame.ActiveMenus)
            {
                if (menu.OnClick(mouseState))
                {
                    break;
                }
            }
        }
        public void ActiveTextField_Click(object sender, EventArgs e)
        {
            if (ActiveField == null)
            {
                ActiveField = (MenuTextField)sender;
            }
            else
            {
                ActiveField = null;
            }
        }
        public void BackButton_Click(object sender, EventArgs e)
        {
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetMain(this));
            ActiveField = null;
        }

        public void InGameBackButton_Click(object sender, EventArgs e)
        {
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
            ServerMenuGenerator.PauseButton.Text = mGame.RollbackManager.IsPaused() ? "UnPause" : "Pause";
            mGame.ActiveMenus.Add(mMenuGenerator.GetTimeMenu(mGame.RollbackManager));
            ActiveField = null;
        }

        public void QuitButton_Click(object sender, EventArgs e)
        {
            mGame.Exit();
        }

        public void NewGameButton_Click(object sender, EventArgs e)
        {
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetNewGameMenu(this));
            mGame.ActiveMenus.Add(mMenuGenerator.GetBackMenu(this));
            ActiveField = null;
        }

        public void SaveButton_Click(object sender, EventArgs e)
        {
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetSaveGameMenu(this));
            mGame.ActiveMenus.Add(mMenuGenerator.GetBackIngameMenu(this));
            ActiveField = null;
        }
        public void LoadButton_Click(object sender, EventArgs e)
        {
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetLoadGameMenu(this));
            mGame.ActiveMenus.Add(mMenuGenerator.GetBackMenu(this));
            ActiveField = null;
        }
        public void ConfirmNewGameButton_Click(object sender, EventArgs e)
        {
            mGame.LoadTest();
            mGame.Start = true;
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
            ActiveField = null;
        }
        
        public void ConfirmSaveButton_Click(object sender, EventArgs e)
        {
            if (ActiveField != null)
            {
                if (ActiveField.Text != null)
                {
                    NewGameName = ActiveField.Text;
                    mGame.SaveGame(ActiveField.Text);
                    mGame.ActiveMenus.Clear();
                    mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
                    mGame.ActiveMenus.Add(mMenuGenerator.GetTimeMenu(mGame.RollbackManager));
                }
                else
                {
                    mGame.ActiveMenus.Clear();
                    mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
                    mGame.ActiveMenus.Add(mMenuGenerator.GetTimeMenu(mGame.RollbackManager));
                }
            }
            else
            {
                mGame.ActiveMenus.Clear();
                mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
                mGame.ActiveMenus.Add(mMenuGenerator.GetTimeMenu(mGame.RollbackManager));
            }
            ActiveField = null;
        }

        public void ConfirmLoadButton_Click(object sender, EventArgs e)
        {
            if (ActiveField != null)
            {
                if (ActiveField.Text == "TEST")
                {
                    mGame.LoadTch();
                    mGame.Start = true;
                    mGame.ActiveMenus.Clear();
                    mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
                }
                else if (ActiveField.Text != null)
                {
                    mGame.LoadGame(ActiveField.Text);
                    mGame.Start = true;
                    mGame.ActiveMenus.Clear();
                    mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
                }
                ActiveField = null;
            }
        }

        public void StartGameButton_Click(object sender, EventArgs e)
        {
            if (mGame.RollbackManager.IsPaused())
            {
                mGame.MessageWrapper.Pause(false);
                ServerMenuGenerator.PauseButton.Text = "Pause";
            }
            else
            {
                mGame.MessageWrapper.Pause(true);
                ServerMenuGenerator.PauseButton.Text = "UnPause";
            }
            mGame.ActiveMenus.Add(mMenuGenerator.GetTimeMenu(mGame.RollbackManager));
        }

        //automatically saves Game with newGameName on Exit
        public void QuitGameButton_Click(object sender, EventArgs e)
        {
            mGame.SaveGame(NewGameName);
            NewGameName = null;
            GameMap.FogOfWarListener = null;
            mGame.ActiveMenus.Clear();
            mGame.Stop();
            GameMap.IslandMapper = null;
            mGame.ActiveMenus.Add(mMenuGenerator.GetMain(this));
            ActiveField = null;
        }

        public void TechdemoButton_Click(object sender, EventArgs e)
        {
            mGame.LoadTechDemo();
            mGame.Start = true;
            mGame.ActiveMenus.Clear();
            mGame.ActiveMenus.Add(mMenuGenerator.GetStartMenu(this));
            ActiveField = null;
        }

        private static ServerMenu GetSeverMenu()
        {
            return sServerMenu;
        }
    }

}

