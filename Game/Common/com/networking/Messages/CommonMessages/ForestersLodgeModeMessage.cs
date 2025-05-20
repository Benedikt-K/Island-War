using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class ForestersLodgeModeMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int Id { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool Mode { get; set; }

        public ForestersLodgeModeMessage(int tick, int id, bool mode) : base(tick)
        {
            Id = id;
            Mode = mode;
        }

        public ForestersLodgeModeMessage() : base(0)
        {

        }

        public override int ClassNumber => NumberManager.OneHundredTwenty;
    }
}