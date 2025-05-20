using System.Collections.Generic;
using Common.com.game.settings;
using Common.com.path;

namespace Common.com.networking.Messages.CommonMessages
{
    public sealed class NewPathsMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<int> Ids { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public List<Path> Paths { get; set; }
        public bool Aggressive { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool ChangeAggressiveness { get; set; }
        public NewPathsMessage(int tick, bool changeAggressiveness,bool aggressive) : base(tick)
        {
            Ids = new List<int>();
            Paths = new List<Path>();
            Aggressive = aggressive;
            ChangeAggressiveness = changeAggressiveness;
        }

        public void AddPath(int id, Path path)
        {
            Ids.Add(id);
            Paths.Add(path);
        }
        public NewPathsMessage():base(0)
        {
            Ids = new List<int>();
            Paths = new List<Path>();
        }
        public override int ClassNumber => NumberManager.OneHundredFour;
    }
}