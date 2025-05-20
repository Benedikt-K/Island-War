using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.objects.immovables.Buildings;

namespace Common.com.networking.Messages.ClientToServer
{
    public class NewRoadMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Scaffolding> NewRoadTiles { get; set; }

        public NewRoadMessage(int tick, List<Scaffolding> roadTiles) : base(tick)
        {
            NewRoadTiles = roadTiles;
        }

        public NewRoadMessage() : base(0)
        {
            
        }
        public override int ClassNumber => NumberManager.OneHundredTen;
    }
}