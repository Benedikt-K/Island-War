using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class UnloadTransportShipMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public ObjectMoving UnloadObjectMoving { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int TransportShipId { get; set; }


        public UnloadTransportShipMessage(int tick, ObjectMoving objectMoving, int transportShipId) : base(tick)
        {
            UnloadObjectMoving = objectMoving;
            TransportShipId = transportShipId;
        }

        public UnloadTransportShipMessage() : base(0)
        {

        }
        public override int ClassNumber => NumberManager.OneHundredEighteen;
    }
}