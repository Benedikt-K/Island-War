using System.Collections.Generic;
using System.IO;
using Common.com.game.settings;
using Common.com.serialization;
using Common.com.Menu;

namespace Common.com.game.achievments
{

    public sealed class AchievementManager : JsonSerializable
    {
        public static int PlayerNumber { get; set; }
        private static AchievementManager sAchievementManager;
        public AchievementManager()
        {
            Achievements = new HashSet<Achievement>();
        }

        public HashSet<Achievement> Achievements { get; }

        private void Save()
        {
            var fileName = "Achievements.json";
            File.WriteAllBytes(fileName, Serialize());
        }

        public static AchievementManager GetManager()
        {
            if (sAchievementManager == null)
            {
                if (!File.Exists("Achievements.json"))
                {
                    var manager = new AchievementManager();
                    manager.Save();
                    return manager;
                }

                var achievementsByte = File.ReadAllBytes("Achievements.json");
                sAchievementManager = (AchievementManager) Deserialize(achievementsByte);
            }

            return sAchievementManager;

        }
        private void UnlockAchievement(Achievement achievement)
        {
            Achievements.Add(achievement);
            Save();
        }

        public override int ClassNumber => NumberManager.TwoHundred;
        
        public void OnStatisticUpdate(int playerNumber)
        {
            if (GameMap.StatisticsManager.GetStatistic(Statistic.UnitsKilled, playerNumber) == 1 && GameMap.StatisticsManager.GetStatistic(Statistic.UnitsKilled, NumberManager.Three - playerNumber) == 0)
            {
                UnlockAchievement(Achievement.FirstBlood);
            }
            if (GameMap.StatisticsManager.GetStatistic(Statistic.TowersStanding, playerNumber) == NumberManager.Ten)
            {
                UnlockAchievement(Achievement.KingOfTheIslands);
            }
            if (GameMap.StatisticsManager.GetStatistic(Statistic.UnitsKilled, playerNumber) == NumberManager.TwoHundred) 
            {
                UnlockAchievement(Achievement.BloodWillFlow);
            }
            if (GameMap.StatisticsManager.GetStatistic(Statistic.SpiesKilled, playerNumber) == 1)
            {
                UnlockAchievement(Achievement.SuperiorSpy);
            }
        }

        public void AfterGameEnding(int tickNr, int winnerNumber)
        {
            var gameTime = TimeManager.GetTime(tickNr);
            if (winnerNumber == PlayerNumber)
            {
                UnlockAchievement(Achievement.Conqueror);

                if (gameTime < TimeManager.GetTime((int)(Counter.sMinStart * NumberManager.ZeroPointThreeD * NumberManager.OneThousandTwoHundred)))
                {
                    UnlockAchievement(Achievement.Rolled);
                }

                if (tickNr > Counter.sMinStart * NumberManager.OneThousandTwoHundred - NumberManager.OneThousandTwoHundred && tickNr < Counter.sMinStart * NumberManager.OneThousandTwoHundred)
                {
                    UnlockAchievement(Achievement.SlowAndSteady);
                }

                if (tickNr < Counter.sMinStart * NumberManager.OneThousandTwoHundred)
                {
                    UnlockAchievement(Achievement.TheFallOfTheCitadel);
                }

                if (GameMap.StatisticsManager.GetStatistic(Statistic.BuildingsBuilt, winnerNumber) >= NumberManager.OneHundred)
                {
                    UnlockAchievement(Achievement.BobTheBuilder);
                }

                if (GameMap.StatisticsManager.GetStatistic(Statistic.SpiesProduced, winnerNumber) == 0)
                {
                    UnlockAchievement(Achievement.BlindWarrior);
                }

                if (GameMap.StatisticsManager.GetStatistic(Statistic.ShipsProduced, winnerNumber) >= NumberManager.Ten)
                {
                    UnlockAchievement(Achievement.TheFleetAdmiral);
                }

                if (GameMap.StatisticsManager.GetStatistic(Statistic.BridgesBuilt, winnerNumber) >= NumberManager.OneHundred)
                {
                    UnlockAchievement(Achievement.TheBridgeMaster);
                }

                if (GameMap.StatisticsManager.GetStatistic(Statistic.BridgesBuilt, winnerNumber) == 0)
                {
                    UnlockAchievement(Achievement.WhoNeedsBridges);
                }
            }
        }
    }
}