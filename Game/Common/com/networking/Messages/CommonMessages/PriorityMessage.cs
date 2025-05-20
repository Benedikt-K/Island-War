using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public class PriorityMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int NewPriority { get; set; }
        public int OldPriority { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int BuildingId { get; set; }

        public PriorityMessage() : base(0)
        {
            
        }
        public PriorityMessage(int tick,int newPriority, int buildingId) : base(tick)
        {
            NewPriority = newPriority;
            BuildingId = buildingId;
        }

        public override int ClassNumber => NumberManager.OneHundredFifteen;
    }
}