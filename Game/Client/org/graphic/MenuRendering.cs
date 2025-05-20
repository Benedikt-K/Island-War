using Game.org.gameStates;
using Microsoft.Xna.Framework.Graphics;

namespace Game.org.graphic
{
    public sealed class MenuRendering:IRendering
    {
        private MainMenu Menu { get; }
        public MenuRendering(MainMenu menu)
        {
            Menu = menu;
        }
        public void Render(SpriteBatch spriteBatch)
        {
            

            foreach (var menu in Menu.ActiveMenus)
            {
                menu.Draw(spriteBatch,Menu.LastMouseState);
            }
        }
    }
}