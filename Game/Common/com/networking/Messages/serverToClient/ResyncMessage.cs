using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;
using Common.com.objects;
using Newtonsoft.Json;

namespace Common.com.networking.Messages.serverToClient
{
    public class ResyncMessage:Message
    {
        [JsonIgnore]
        public bool Resynced
        {
            get;
            set;
        }
        public HashSet<Point> Roads { get; set; }
        public IEnumerable<GameObject> ObjectsMoving { get; set; }
        public IEnumerable<GameObject> ObjectsImmovable { get; set; }
        [JsonIgnore]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool DealtWith { get; set; }
        public ResyncMessage() : base(0)
        {
        }
        

        public override int ClassNumber => NumberManager.OneHundredTwentyTwo;
    }
}