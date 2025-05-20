using System;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.networking.Messages.ClientToServer;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables;
using Common.com.objects.immovables.Buildings;
using Common.com.objects.immovables.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.com.serialization
{
    public class GameObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonSerializable).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            
            
            JsonSerializable ob;
            switch ((int)jo["ClassNumber"])
            {
                case 1: ob = new Worker();
                    break;
                case NumberManager.Two: ob = new TransportShip();
                    break;
                case NumberManager.Three: ob = new ScoutShip();
                    break;
                case NumberManager.Four: ob = new Spy();
                    break;
                case NumberManager.Five: ob = new Spearman();
                    break;
                case NumberManager.Six: ob = new Swordsman();
                    break;
                case NumberManager.Seven: ob = new Shieldman();
                    break;
                case NumberManager.Eight: ob = new MainBuilding();
                    break;
                case NumberManager.Nine: ob = new Barracks();
                    break;
                case NumberManager.Ten: ob = new WorkerTrainingCenter();
                    break;
                case NumberManager.Eleven: ob = new Warehouse();
                    break;
                case NumberManager.Twelve: ob = new House();
                    break;
                case NumberManager.Thirteen: ob = new Scaffolding();
                    break;
                case NumberManager.Fourteen: ob = new Shipyard();
                    break;
                case NumberManager.Fifteen: ob = new Tower();
                    break;
                case NumberManager.Sixteen: ob = new StoneMine();
                    break;
                case NumberManager.Seventeen: ob = new IronMine();
                    break;
                case NumberManager.Eighteen: ob = new ForestersLodge();
                    break;
                case NumberManager.Nineteen: ob = new StoneProcessing();
                    break;
                case NumberManager.Twenty: ob = new IronForge();
                    break;
                case NumberManager.TwentyOne: ob = new Sawmill();
                    break;
                case NumberManager.TwentyTwo: ob = new IronDeposit();
                    break;
                case NumberManager.TwentyThree: ob = new Tree();
                    break;
                case NumberManager.TwentyFour: ob = new Corpse();
                    break;
                case NumberManager.OneHundredOne: ob = new MapMessage();
                    break;
                case NumberManager.OneHundredTwo: ob = new NewObjectActionMessage();
                    break;
                case NumberManager.OneHundredThree: ob = new PauseMessage();
                    break;
                case NumberManager.OneHundredFour: ob = new NewPathsMessage();
                    break;
                case NumberManager.OneHundredFive: ob = new PingMessage();
                    break;
                case NumberManager.OneHundredSix: ob = new NewBuildingPlacementMessage();
                    break;
                case NumberManager.OneHundredSeven:
                    ob = new NewObjectMovingCreationMessage();
                    break;
                case NumberManager.OneHundredEight:
                    ob = new TreeGrowthMessage();
                    break;
                case NumberManager.OneHundredNine:
                    ob = new RevertTimeMessage();
                    break;
                case NumberManager.OneHundredTen:
                    ob = new NewRoadMessage();
                    break;
                case NumberManager.OneHundredEleven:
                    ob = new NewRemoveBuildingMessage();
                    break;
                case NumberManager.OneHundredTwelve:
                    ob = new LogisticIgnoreMessage();
                    break;
                case NumberManager.OneHundredThirteen:
                    ob = new NewBridgeMessage();
                    break;
                case NumberManager.OneHundredFourteen:
                    ob = new GameEndMessage();
                    break;
                case NumberManager.OneHundredFifteen:
                    ob = new PriorityMessage();
                    break;
                case NumberManager.TwoHundred: ob = new AchievementManager();
                    break;
                case NumberManager.TwoHundredOne: ob = new Settings();
                    break;
                case NumberManager.OneHundredSixteen: ob = new ItemOutMessage();
                    break;
                case NumberManager.OneHundredSeventeen:
                    ob = new UnloadTowerMessage();
                    break;
                case NumberManager.OneHundredEighteen:
                    ob = new UnloadTransportShipMessage();
                    break;
                case NumberManager.OneHundredNineteen:
                    ob = new LeaveTowerMessage();
                    break;
                case NumberManager.OneHundredTwenty:
                    ob = new ForestersLodgeModeMessage();
                    break;
                case NumberManager.OneHundredTwentyOne:
                    ob = new MultipleMessagesMessage();
                    break;
                case NumberManager.OneHundredTwentyTwo:
                    ob = new ResyncMessage();
                    break;
                case NumberManager.OneHundredTwentyThree:
                    ob = new ResyncRequestMessage();
                    break;
                default: ob = null;
                    break;
            }
            if (ob != null)
            {
                serializer.Populate(jo.CreateReader(), ob);
            }
            return ob;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, 
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}