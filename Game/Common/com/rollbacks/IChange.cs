using Common.com.game;

namespace Common.com.rollbacks
{
    public interface IChange
    {
        public void DoChange(GameState gameState);
        public void RevertChange(GameState gameState);
    }
}