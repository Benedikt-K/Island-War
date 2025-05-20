using Common.com.game.settings;
using Common.com.objects;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class NewObjectActionMessage:Message
    {
        public int Id{ get; set; }
        public ObjectAction Action { get; set; }

        
        public NewObjectActionMessage(int tick,int id, ObjectAction action) : base(tick)
        {
            Id = id;
            Action = action;
        }
        

        public NewObjectActionMessage() : base(0)
        {
            
        }
        public override int ClassNumber => NumberManager.OneHundredTwo;
    }
}