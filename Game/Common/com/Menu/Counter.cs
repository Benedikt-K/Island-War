using System;
using Common.com.game;
using Common.com.game.settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common.com.Menu
{
    public sealed class Counter : MenuButton
    {
        private readonly INumber mNumber;
        public static readonly int sMinStart=300, sSecondStart=0;
        public Counter(Texture2D texture, EventHandler eventHandler, Menu menu,INumber number,string text = "", bool isSwitch = false) : base(texture, eventHandler, menu, text, isSwitch)
        {
            mNumber = number;
        }
        protected override void Draw(SpriteBatch spriteBatch, bool hovering, bool leftDown)
        {
            var num = Int32.Parse(mNumber.Number);
            var min = (sMinStart*60-num/20+sSecondStart)/60;
            var sec = ((sSecondStart - num / 20)%60+60)%60;
            var c = Color.Black; 
            string currentText;
            if (sec < NumberManager.Ten)
            {
                currentText = Text + (min + ":0" +sec);
            }
            else
            {
                currentText = Text + (min + ":" + sec);
            }

            if (min < 0)
            {
                currentText = Text + ("0:00");
            }
            if (mTexture != null)
            {
                spriteBatch.Draw(mTexture, Position, Color.White);
            }

            if ((sec <= 0 && min <= 0)||min<0)
            {
                GameMap.EndGame(GameMap.StatisticsManager.MoreTowers());
            }

            var x = (Position.X + (Position.Width / 2)) - (mFont.MeasureString(currentText).X / 2);

            var y = (Position.Y + (Position.Height / 2)) - (mFont.MeasureString(currentText).Y / 2);
            spriteBatch.DrawString(mFont, currentText, new Vector2(x, y), c);
        }
    }
}
