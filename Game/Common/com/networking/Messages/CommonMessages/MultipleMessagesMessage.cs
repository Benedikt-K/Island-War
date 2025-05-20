using System.Collections.Generic;
using Common.com.game.settings;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class MultipleMessagesMessage : Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Message> MessageList{ get; set; }

        public MultipleMessagesMessage(int tick, List<Message> messageList) : base(tick)
        {
            MessageList = messageList;
        }


        public MultipleMessagesMessage() : base(0)
        {

        }
        public override int ClassNumber => NumberManager.OneHundredTwentyOne;
    }
}
