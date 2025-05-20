using Common.com.game;

namespace Common.com.rollbacks
{
    public interface ITickListener
    {
        public void OnTick(GameMap gameMap, int tick);
    }
}