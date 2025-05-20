using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.objects.entities;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class LogisticIgnoreMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Worker> ToUnIgnore { get; set; }

        public LogisticIgnoreMessage() : base(0)
        {
            
        }
        public LogisticIgnoreMessage(int tick,List<Worker> toUnIgnore):base(tick)
        {
            ToUnIgnore = toUnIgnore;
        }

        public override int ClassNumber => NumberManager.OneHundredTwelve;
    }
}