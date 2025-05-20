using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class NewObjectMovingCreationMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public ObjectMoving NewObjectMoving { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int BuildingId { get; set; }

        public NewObjectMovingCreationMessage(int tick, ObjectMoving objectMoving, int buildingId) : base(tick)
        {
            NewObjectMoving = objectMoving;
            BuildingId = buildingId;
        }

        public NewObjectMovingCreationMessage() : base(0)
        {

        }
        public override int ClassNumber => NumberManager.OneHundredSeven;
    }
}