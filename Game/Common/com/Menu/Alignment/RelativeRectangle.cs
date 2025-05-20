using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Common.com.Menu.Alignment
{
    public abstract class RelativeRectangle
    {
        protected Viewport Viewport => mGraphicsDevice.Viewport;
        private readonly GraphicsDevice mGraphicsDevice;
        protected Point mMovement;
        protected RelativeRectangle(GraphicsDevice graphicsDevice)
        {
            mGraphicsDevice = graphicsDevice;
        }
        public abstract Rectangle CurrentBounds { get; }
        public void MoveBy(Point p)
        {
            mMovement += p;
        }

        public virtual bool ToRemove()
        {
            return false;
        }
        public abstract void AddSpace(Size size, int extra);
    }
}