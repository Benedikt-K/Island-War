using Microsoft.Xna.Framework.Input;

namespace Server.net.main
{
    public sealed class ServerMenuInputManager
    {
        private readonly ServerMenu mMenu;
        private bool mLastLeft;
        private readonly Game1 mGame;
        public ServerMenuInputManager(ServerMenu menu, Game1 game)
        {
            mMenu = menu;
            mLastLeft = false;
            mGame = game;
        }
        public void HandleInput(KeyboardState keyboardState, MouseState mouseState)
        {
            mGame.LastMouseState = mouseState;
            if (mMenu.ActiveField != null)
            {
                mMenu.ActiveField.HandleInput(keyboardState);
            }
            var currentLeft = mouseState.LeftButton == ButtonState.Pressed;
            if (!currentLeft && mLastLeft)
            {
                mMenu.OnClick(mouseState);
            }

            mLastLeft = currentLeft;
        }
    }
}