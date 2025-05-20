using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public class PauseMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool Pause { get; set; }

        public PauseMessage(int tick, bool pause) : base(tick)
        {
            Pause = pause;
        }

        public PauseMessage():base(0)
        {
            
        }

        public override int ClassNumber => NumberManager.OneHundredThree;
    }
}