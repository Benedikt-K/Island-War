using System.Drawing;
using Common.com.Menu.Alignment;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Game.org.gameStates
{
    sealed class GameObjectRectangle : RelativeRectangle
    {
        private readonly InGame mGame;
        private Size mSize;
        public int TargetId { get; }
        public GameObjectRectangle(GraphicsDevice graphicsDevice, int gameObjectId, InGame inGame, Size size) : base(graphicsDevice)
        {
            TargetId = gameObjectId;
            mGame = inGame;
            mSize = size;
        }

        public override bool ToRemove()
        {
            return mGame.GetCurrentState().Map.GetObject(TargetId) == null;
        }

        public override Rectangle CurrentBounds
        {
            get
            {
                var target = mGame.GetCurrentState().Map.GetObject(TargetId);
                var location = new Point(target.GetBounds().Location.X, target.GetBounds().Location.Y);
                var drawAt = Vector2.Transform(
                    new Vector2(location.X, location.Y),
                    mGame.Camera.Transform);
                return new Rectangle(new Point((int)drawAt.X, (int)drawAt.Y), new Point(mSize.Width, mSize.Height));
            }
        } 

        public override void AddSpace(Size size, int extra)
        {
            mSize.Height += size.Height + extra;
        }
    }
}
