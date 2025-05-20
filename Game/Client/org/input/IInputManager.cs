using Microsoft.Xna.Framework.Input;

namespace Game.org.input
{
    public interface IInputManager
    {
        public void HandleInput(KeyboardState keyboardState, MouseState mouseState);
    }
}