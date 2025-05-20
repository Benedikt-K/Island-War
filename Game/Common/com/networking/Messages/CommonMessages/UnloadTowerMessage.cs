using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class UnloadTowerMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public ObjectMoving UnloadObjectMoving { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int BuildingId { get; set; }


        public UnloadTowerMessage(int tick, ObjectMoving objectMoving, int buildingId) : base(tick)
        {
            UnloadObjectMoving = objectMoving;
            BuildingId = buildingId;
        }

        public UnloadTowerMessage() : base(0)
        {

        }
        public override int ClassNumber => NumberManager.OneHundredSeventeen;
    }
}
