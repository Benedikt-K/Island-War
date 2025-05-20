using System.Drawing;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.Menu.Alignment;
using Common.com.rollbacks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;

namespace Server.net.main
{
    public class ServerMenuGenerator
    {
        public ServerMenuGenerator(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            mFont = contentManager.Load<SpriteFont>("ServerMenuButton/Font");
            mButtonTexture2D = contentManager.Load<Texture2D>("ServerMenuButton/Button");
            mGraphicsDevice = graphicsDevice;
        }
        private const ushort StandardPort=61991;
        public static MenuButton PauseButton { get; private set; }
        private readonly SpriteFont mFont;
        private readonly Texture2D mButtonTexture2D;
        private readonly GraphicsDevice mGraphicsDevice;
        private static readonly Size sMenuButtonSize = new Size(160, 40);
        private static readonly int sButtonDist = 10;
        private static MenuTextField sField;
        public static ushort Port => sField != null && ushort.TryParse(sField.Text,out var n)?n:StandardPort;
        
        public Menu GetMain(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.NewGameButton_Click, res, "New Game"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.LoadButton_Click, res, "Load Game"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.TechdemoButton_Click, res, "Start Techdemo"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.QuitButton_Click, res, "Quit Menu"), sMenuButtonSize);
            var button=new MenuTextField(mButtonTexture2D,serverMenu.ActiveTextField_Click,res)
            {
                Text = ""+StandardPort
            };
            sField = button;
            res.AddMenuButton(new NumberBox(mButtonTexture2D,null,res,INumber.GetConst("Port:")),new Size(sMenuButtonSize.Width/ NumberManager.Two -sButtonDist,sMenuButtonSize.Height));
            res.AddMenuButton(sField, new Size(sMenuButtonSize.Width/ NumberManager.Two,sMenuButtonSize.Height));
            return res;
        }

        private sealed class TickNr:INumber
        {
            private readonly RollbackManager mRollbackManager;
            public TickNr(RollbackManager rollbackManager)
            {
                mRollbackManager = rollbackManager;
            }

            public string Number => ""+(mRollbackManager.CurrentState?.TickNr ?? 0);
        }
        public Menu GetTimeMenu(RollbackManager rollbackManager)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.TopRight, sMenuButtonSize),
                sButtonDist,
                new Point());
            res.AddMenuButton(new NumberBox(null,null,res,new TickNr(rollbackManager)),sMenuButtonSize);
            return res;
        }
        public Menu GetBackMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.BottomLeft, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.BackButton_Click, res, "Return To Menu"), sMenuButtonSize);
            return res;
        }

        public Menu GetBackIngameMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.BottomLeft, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.InGameBackButton_Click, res, "Return To Menu"), sMenuButtonSize);
            return res;
        }

        public Menu GetNewGameMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuTextField(mButtonTexture2D, serverMenu.ActiveTextField_Click, res), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.ConfirmNewGameButton_Click, res, "Create New Game"), sMenuButtonSize);
            return res;
        }

        public Menu GetSaveGameMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuTextField(mButtonTexture2D, serverMenu.ActiveTextField_Click, res), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.ConfirmSaveButton_Click, res, "Save Game"), sMenuButtonSize);
            return res;
        }

        public Menu GetLoadGameMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuTextField(mButtonTexture2D, serverMenu.ActiveTextField_Click, res), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.ConfirmLoadButton_Click, res, "Load Game"), sMenuButtonSize);
            return res;
        }

        public Menu GetStartMenu(ServerMenu serverMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            PauseButton = new MenuButton(mButtonTexture2D, serverMenu.StartGameButton_Click, res, "Start Game");
            res.AddMenuButton(PauseButton, sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.SaveButton_Click, res, "Save Game"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(mButtonTexture2D, serverMenu.QuitGameButton_Click, res, "Quit Game"), sMenuButtonSize);
            return res;
        }
    }
}