using System.Collections.Generic;
using System.Linq;
using Common.com.game.achievments;
using Common.com.game.Map;
using Common.com.game.settings;
using Common.com.objects;
using Microsoft.Xna.Framework;

namespace Common.com.networking.Messages.serverToClient
{
    public sealed class MapMessage:Message
    {
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public Terrain[][] Terrains { get;  set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public GameObject[] Objects { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int PlayerNumber { get;  set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public bool IsPaused { get; set; }
        public Vector2[] CameraStart { get;  set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public FogOfWar FogOfWar { get; set; }
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int MaxPlayers { get; set; }
        public int CurrentId { get; set; }
        public StatisticsManager StatisticsManager{
            get;
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global

            //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
            set;
        }

        public MapMessage(Terrain[][] terrains, HashSet<GameObject> objects, int tick,int playerNumber,Vector2[] cameraStarts,bool isPaused,int maxPlayers, FogOfWar fogOfWar,StatisticsManager statisticsManager):base(tick)
        {
            Terrains = terrains;
            MaxPlayers = maxPlayers;
            Objects = objects.ToArray();
            PlayerNumber = playerNumber;
            CameraStart = cameraStarts;
            IsPaused = isPaused;
            FogOfWar = fogOfWar;
            StatisticsManager = statisticsManager;
        }
        public override int ClassNumber => NumberManager.OneHundredOne;
        public MapMessage():base(0)
        {
            
        }
    }
}