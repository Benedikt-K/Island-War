using Common.com.game.settings;

namespace Common.com.networking.Messages.ClientToServer
{
    public class SaveGameMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public string SaveName{ get; set; }
        public SaveGameMessage(int tick, string saveName) : base(tick)
        {
            SaveName = saveName;
        }
        public override int ClassNumber => NumberManager.OneHundredSeven;
    }
}