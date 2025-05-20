using Microsoft.Xna.Framework.Graphics;

namespace Server.net.main
{
    public sealed class ServerMenuRendering:IRendering
    {
        private readonly Game1 mGame;
        public ServerMenuRendering(Game1 game)
        {
            mGame = game;
        }
        public void Render(SpriteBatch spriteBatch)
        {
            
            foreach (var menu in mGame.ActiveMenus) 
            {
                menu.Draw(spriteBatch, mGame.LastMouseState);
            }
        }
    }
}