using Game.org.graphic;
using Game.org.input;
using Microsoft.Xna.Framework;

namespace Game.org.gameStates
{
    public interface IGameState
    {
        public void Update(GameTime gameTime);
        public IRendering GetRenderer();

        public void Stop();
        public IInputManager GetInputManager();
    }
}