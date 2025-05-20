using System;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Common.com.Menu.Alignment
{
    public sealed class SideRectangle:RelativeRectangle
    {
        private readonly Side mSide;
        private int mLength;
        public SideRectangle(GraphicsDevice graphicsDevice,Side side, int length):base(graphicsDevice)
        {
            mLength = length;
            mSide = side;
        }


        public override Rectangle CurrentBounds
        {
            get
            {
                var sideName = nameof(mSide);
                return mSide switch
                {
                    Side.Left => new Rectangle(0, 0, mLength + mMovement.X, Viewport.Height),
                    Side.Right => new Rectangle(Viewport.Width - mLength - mMovement.X,
                        0,
                        mLength + mMovement.X,
                        Viewport.Height),
                    Side.Top => new Rectangle(0, 0, Viewport.Width, mLength + mMovement.Y),
                    Side.Bottom => new Rectangle(0,
                        Viewport.Height - mLength - mMovement.Y,
                        Viewport.Width,
                        mLength + mMovement.Y),
                    _ => throw new ArgumentOutOfRangeException(sideName)
                };
            }
        }

        public override void AddSpace(Size size, int extra)
        {
            if (mSide == Side.Left || mSide == Side.Right)
            {
                mLength += size.Width+extra;
            }
            else
            {
                mLength += size.Height+extra;
            }
        }

        public void RemoveSpace(int height)
        {
            mLength -= height;
        }
    }
    public enum Side
    {
        Left,Right,Top,Bottom
    }
}