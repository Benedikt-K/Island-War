using Common.com.game.settings;

namespace Common.com.networking.Messages.ClientToServer
{
    public sealed class ResyncRequestMessage:Message
    {
        public ResyncRequestMessage() : base(0)
        {
        }

        public override int ClassNumber => NumberManager.OneHundredTwentyThree;
    }
}