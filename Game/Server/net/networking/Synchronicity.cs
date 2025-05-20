using Common.com.game;
using Common.com.rollbacks;
using Server.net.main;

namespace Server.net.networking
{
    public sealed class Synchronicity:ITickListener
    {
        private readonly MessageWrapper mMessageWrapper;
        public Synchronicity(MessageWrapper messageWrapper)
        {
            mMessageWrapper = messageWrapper;
        }
        public void OnTick(GameMap gameMap, int tick)
        {
            if (tick%100==99)
            {
                mMessageWrapper.SpreadMessage(gameMap.GetSyncMessage(tick),true);
            }
        }
    }
}