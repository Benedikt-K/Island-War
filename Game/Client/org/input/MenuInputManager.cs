using Game.org.gameStates;
using Microsoft.Xna.Framework.Input;

namespace Game.org.input
{
    public sealed class MenuInputManager: IInputManager
    {
        private readonly MainMenu mMenu;

        private MouseState mMouseState;
        private bool mLastLeft;
        public MenuInputManager(MainMenu menu)
        {
            mMenu = menu;
            mLastLeft = false;
        }
        
        public void HandleInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (mMenu.ActiveField != null)
            {
                mMenu.ActiveField.HandleInput(keyboardState);
            }
            mMouseState = Mouse.GetState();
            mMenu.LastMouseState = mMouseState;
            var currentLeft = mouseState.LeftButton == ButtonState.Pressed;
            if (!currentLeft && mLastLeft)
            {
                mMenu.OnClick(mouseState);
            }

            mLastLeft = currentLeft;
        }
    }
}