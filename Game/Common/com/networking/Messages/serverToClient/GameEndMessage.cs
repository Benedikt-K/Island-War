using Common.com.game.settings;

namespace Common.com.networking.Messages.serverToClient
{
    public sealed class GameEndMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int Winner { get; set; }

        public GameEndMessage(int tick, int textWinner) : base(tick)
        {
            Winner = textWinner;
        }

        public override int ClassNumber => NumberManager.OneHundredFourteen;
        public GameEndMessage() : base(0)
        {

        }

    }
}

