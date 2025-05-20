using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common.com.Menu
{
    public sealed class NumberBox:MenuButton
    {
        private readonly INumber mNumber;
        public Color Color { private get; set; }
        public NumberBox(Texture2D texture, EventHandler eventHandler, Menu menu, INumber number, string text = "",bool isSwitch = false,Color color=new Color()) : base(texture, eventHandler, menu, text, isSwitch)
        {
            if (color == new Color())
            {
                color = Color.Black;
            }
            Color = color;
            mNumber = number;
        }
        protected override void Draw(SpriteBatch spriteBatch,bool hovering, bool leftDown)
        {
            var c = Color.White;
            var currentText = Text + (mNumber == null ? "" : " " + mNumber.Number);
            if (mTexture != null)
            {
                spriteBatch.Draw(mTexture, Position, c);
            }

            var x = (Position.X + (Position.Width / 2)) - (mFont.MeasureString(currentText).X / 2);
            
            var y = (Position.Y + (Position.Height / 2)) - (mFont.MeasureString(currentText).Y / 2);
            spriteBatch.DrawString(mFont,currentText , new Vector2(x, y), Color);
        }
    }
}
