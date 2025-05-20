using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.objects;
using Common.com.objects.entities;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class ItemOutMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public ObjectAction Action { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Worker> Workers { get; set; }

        public ItemOutMessage() : base(0)
        {
            
        }
        public ItemOutMessage(int tick,ObjectAction action,List<Worker> workers) : base(tick)
        {
            Action = action;
            Workers = workers;
        }

        public override int ClassNumber => NumberManager.OneHundredSixteen;
    }
}