using System;
using System.Drawing;
using Common.com.game.settings;
using Microsoft.Xna.Framework.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Common.com.Menu.Alignment
{
    public sealed class AlignedRectangle:RelativeRectangle
    {
        private readonly Alignment mAlignment;
        private Size mSize;
        public AlignedRectangle(GraphicsDevice graphicsDevice, Alignment alignment, Size size):base(graphicsDevice)
        {
            mSize = size;
            mAlignment = alignment;
        }

        public override Rectangle CurrentBounds => 
            new Rectangle(mMovement+new Point(GetStart(true),GetStart(false)),new Point(mSize.Width,mSize.Height));

        

        public override void AddSpace(Size size, int extra)
        {
            mSize.Height += size.Height+extra;
            
        }

        private int GetStart(bool vertical)
        {
            return vertical ? GetStart(Viewport.Width,mSize.Width,GetEnd(mAlignment,true)):
                GetStart(Viewport.Height,mSize.Height,GetEnd(mAlignment,false));
        }
        private static int GetStart(int totalLength,int length, End end)
        {
            switch (end)
            {
                case End.Start:
                    return 0;
                case End.Middle:
                    return totalLength / NumberManager.Two - length / NumberManager.Two;
                case End.Finish:
                    return totalLength - length;
                default:
                    throw new ArgumentOutOfRangeException(nameof(end), end, null);
            } 
        }
        private static End GetEnd(Alignment alignment, bool vertical)
        {
            switch (alignment)
            {
                case Alignment.Left:
                    return vertical?End.Start:End.Middle;
                case Alignment.Right:
                    return vertical?End.Finish:End.Middle;
                case Alignment.Top:
                    return vertical?End.Middle:End.Start;
                case Alignment.Bottom:
                    return vertical?End.Middle:End.Finish;
                case Alignment.Middle:
                    return End.Middle;
                case Alignment.TopLeft:
                    return End.Start;
                case Alignment.TopRight:
                    return vertical ? End.Finish : End.Start;
                case Alignment.BottomLeft:
                    return vertical ? End.Start:End.Finish ;
                case Alignment.BottomRight:
                    return End.Finish;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }

        }
    }
    
    enum End
    {
        Start, Middle, Finish
    }
    public enum Alignment
    {
        Left, Right, Top, Bottom, Middle, TopLeft, TopRight, BottomLeft, BottomRight
    }
}