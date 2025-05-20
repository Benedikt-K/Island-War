using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class NewRemoveBuildingMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public Building RemoveBuilding { get; set; }
        public NewRemoveBuildingMessage(int tick, Building building) : base(tick)
        {
            RemoveBuilding = building;
        }

        public NewRemoveBuildingMessage() : base(0)
        {

        }

        public override int ClassNumber => NumberManager.OneHundredEleven;
    }
}
