using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Common.com.Menu
{
    public abstract class Component
    {
        internal bool OnClick(Point p)
        {
            
            if (!Contains(p))
            {
                return false;
            }
            OnClick();
            return true;

        }

        public abstract void OnClick();
        public abstract bool Contains(Point p);
        protected abstract void Draw(SpriteBatch spriteBatch,bool hovering, bool leftDown);

        internal void Draw(SpriteBatch spriteBatch, MouseState mouseState)
        {
            Draw(spriteBatch,Contains(mouseState.Position),mouseState.LeftButton == ButtonState.Pressed);
        }
    }
}