using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class RevertTimeMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool Revert { get; set; }

        public RevertTimeMessage(int tick,bool revert=true) : base(tick)
        {
            Revert = revert;
        }
        public RevertTimeMessage() : base(0)
        {
            Revert = true;
        }
        public override int ClassNumber => NumberManager.OneHundredNine;
    }
}