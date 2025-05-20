using Common.com.game.settings;
using Common.com.objects.immovables.Resources;

namespace Common.com.networking.Messages.serverToClient
{
    public class TreeGrowthMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public Tree Tree { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool Remove { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool GivesResources { get; set; }

        public TreeGrowthMessage(int tick,Tree tree,bool remove,bool givesResources):base(tick)
        {
            Tree = tree;
            Remove = remove;
            GivesResources = givesResources;
        }
        public TreeGrowthMessage() : base(0)
        {
            
        }

        public override int ClassNumber => NumberManager.OneHundredEight;
    }
}