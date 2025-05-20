using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class NewBuildingPlacementMessage:Message
    {
        public Building NewBuilding { get; set; }

        public NewBuildingPlacementMessage(int tick, Building building) : base(tick)
        {
            NewBuilding = building;
        }

        public NewBuildingPlacementMessage() : base(0)
        {
            
        }
        public override int ClassNumber => NumberManager.OneHundredSix;
    }
}