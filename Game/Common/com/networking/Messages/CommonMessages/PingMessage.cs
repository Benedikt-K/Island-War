using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class PingMessage:Message
    {
        public override int ClassNumber => NumberManager.OneHundredFive;

        public PingMessage() : base(0)
        {
        }
    }
}