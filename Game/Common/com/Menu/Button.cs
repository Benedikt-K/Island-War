using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common.com.Menu
{
    public class Button:Component
    {
        protected readonly Texture2D mTexture;
        internal bool Switch { get; }
        private readonly EventHandler mEventHandler;
        private bool IsClicked { get; set; }
        protected virtual Rectangle Position { get; }
        protected readonly SpriteFont mFont;
        public string Text { get; set; }
        protected Button(Texture2D texture,SpriteFont font,string text,bool isSwitch,EventHandler eventHandler)
        {
            mFont = font;
            Text = text;
            mEventHandler = eventHandler;
            mTexture = texture;
            Switch = isSwitch;
            IsClicked = false;
        }

        public override void OnClick()
        {
            if (Switch)
            {
                IsClicked = !IsClicked;
            }
            mEventHandler?.Invoke(this,EventArgs.Empty);
        }

        public bool IsClick()
        {
            return IsClicked;
        }

        public void SetIsClicked(bool b)
        {
            IsClicked = b;
        }
        public override bool Contains(Point p)
        {
            return Position.Contains(p);
        }

        protected override void Draw(SpriteBatch spriteBatch,bool hovering, bool leftDown)
        {
            var c = hovering ? Color.DarkGray : Color.White;
            c = IsClicked||(leftDown && hovering) ? Color.Gray : c;
            spriteBatch.Draw(mTexture,Position,c);var x = (Position.X + (Position.Width / 2)) - (mFont.MeasureString(Text).X / 2);
            var y = (Position.Y + (Position.Height / 2)) - (mFont.MeasureString(Text).Y / 2);
            spriteBatch.DrawString(mFont, Text, new Vector2(x, y), Color.Black);
        }
    }
}