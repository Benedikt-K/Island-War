using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common.com.Menu
{
    public class MenuButton:Button
    {
        public Menu Menu { get; }
        protected override Rectangle Position => Menu.GetPosition(this);
        public MenuButton(Texture2D texture,  EventHandler eventHandler, Menu menu, string text="",bool isSwitch=false) : base(texture, menu.Font, text, isSwitch, eventHandler)
        {
            Menu = menu;
        }
    }
}