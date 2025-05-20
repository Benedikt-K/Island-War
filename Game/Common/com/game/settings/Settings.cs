using Common.com.serialization;

namespace Common.com.game.settings
{
    public sealed class Settings : JsonSerializable
    {
        public float mEffectsVolume;
        public float mMusicVolume;
        public string mLastIpAddress;
        public ushort mPort=61991;


        public Settings(float effectsVolume, float musicVolume, ushort port, string lastIpAddress = null) 
        {
            mEffectsVolume = effectsVolume;
            mMusicVolume = musicVolume;
            mLastIpAddress = lastIpAddress;
            mPort = port;
        }

        public Settings()
        {

        }

        public override int ClassNumber => NumberManager.TwoHundredOne;
    }
}