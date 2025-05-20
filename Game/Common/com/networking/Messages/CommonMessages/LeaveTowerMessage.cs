using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class LeaveTowerMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public ObjectMoving LeaveObjectMoving { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int BuildingId { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool KillClick { get; set; }


        public LeaveTowerMessage(int tick, ObjectMoving objectMoving, int buildingId, bool killClick) : base(tick)
        {
            LeaveObjectMoving = objectMoving;
            BuildingId = buildingId;
            KillClick = killClick;
        }

        public LeaveTowerMessage() : base(0)
        {

        }
        public override int ClassNumber => NumberManager.OneHundredNineteen;
    }
}