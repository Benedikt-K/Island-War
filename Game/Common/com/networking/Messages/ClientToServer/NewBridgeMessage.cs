using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.objects.immovables.Buildings;

namespace Common.com.networking.Messages.ClientToServer
{
    public class NewBridgeMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Scaffolding> NewBridgeTiles { get; set; }
        public NewBridgeMessage(int tick, List<Scaffolding> bridgeTiles) : base(tick)
        {
            NewBridgeTiles = bridgeTiles;
        }

        public NewBridgeMessage() : base(0)
        {
            
        }
        
        public override int ClassNumber => NumberManager.OneHundredThirteen;
    }
}