using Common.com.game;
using Common.com.networking.Messages;

namespace Common.com.rollbacks.changes
{
    public sealed class MessageChange:IChange
    {
        private readonly Message mMessage;

        public MessageChange(Message message)
        {
            mMessage = message;
        }

        public void DoChange(GameState gameState)
        {
            gameState.Handle(mMessage);
        }

        public void RevertChange(GameState gameState)
        {
            gameState.Revert(mMessage);
        }
    }
}