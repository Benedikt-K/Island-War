using System;
using System.Collections.Generic;
using System.Drawing;
using Common.com.game;
using Common.com.game.achievments;
using Common.com.game.settings;
using Common.com.Menu;
using Common.com.Menu.Alignment;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;
using Common.com.rollbacks;
using Game.org.graphic;
using Game.org.main;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Game.org.gameStates
{
    public class MenuGenerator
    {
        public MenuGenerator(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            mFont = contentManager.Load<SpriteFont>("Font/Font");
            mBuildingFont = contentManager.Load<SpriteFont>("Font/BuildingFont");
            mGraphicsDevice = graphicsDevice;
        }

        private static readonly Size sMenuButtonSize = new Size(160, 40);
        private static readonly Size sFightingMenuSize = new Size(180, 80);
        private static readonly Size sCostMenuSize = new Size(120, 40);
        private static readonly Size sBuildingMenuSize = new Size(80, 40);
        private static readonly Size sIngameMenuSize = new Size(30, 35);
        private static readonly Size sRoadConfirmMenuSize = new Size(60, 60);
        private static readonly int sButtonDist = 10;
        private static readonly int sBuildingButtonDist = 1;
        private static readonly int sBuildingMenuHeight = 200;
        private static MenuTextField sField;
        private const ushort StandardPort = 61991;
        public ushort Port => sField != null && ushort.TryParse(sField.Text, out var n) ? n : StandardPort;
        public MenuTextField mConnectField;
        private readonly SpriteFont mFont;
        private readonly SpriteFont mBuildingFont;
        private readonly GraphicsDevice mGraphicsDevice;

        public Menu GetMain(ImageManager imageManager, MainMenu mainMenu)
        {
            var button = imageManager.GetImage("Button");
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point());
            res.AddMenuButton(new MenuButton(button, mainMenu.NewGameButton_Click, res, "Connect to..."), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.DemoGameButton_Click, res, "Connect to localhost"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.SettingButton_Click, res, "Settings"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.AchievementsButton_Click, res, "Achievements"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.QuitGameButton_Click, res, "Quit Game"), sMenuButtonSize);
            return res;
        }

        private sealed class TickNr:INumber
        {
            private readonly RollbackManager mRollbackManager;
            public TickNr(RollbackManager rollbackManager)
            {
                mRollbackManager = rollbackManager;
            }
            public string Number => "" + mRollbackManager.CurrentState.TickNr;
        }

        public Menu GetTimeMenu(RollbackManager rollbackManager)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.TopRight, sMenuButtonSize),
                sButtonDist,
                new Point());
            res.AddMenuButton(new NumberBox(null, null, res, new TickNr(rollbackManager)), sMenuButtonSize);
            return res;
        }

        public Menu GetInputMenu(ImageManager imageManager, MainMenu mainMenu)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize),
                sButtonDist,
                new Point());
            var buttonTexture = imageManager.GetImage("Button");
            var newMenuTextField = new MenuTextField(buttonTexture, mainMenu.ActiveTextField_Click, res);
            if (mainMenu.mLastIpAddress != null)
            {
                newMenuTextField.Text = mainMenu.mLastIpAddress;
            }
            res.AddMenuButton(newMenuTextField, sMenuButtonSize);
            mConnectField = newMenuTextField;
            res.AddMenuButton(new MenuButton(buttonTexture, mainMenu.ConnectButton_Click, res, "Connect to"), sMenuButtonSize);
            return res;
        }

        public Menu GetBackMenu(ImageManager imageManager, MainMenu mainMenu)
        {
            var button = imageManager.GetImage("Button");
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.BottomLeft, sMenuButtonSize), sButtonDist, new Point(0, -sButtonDist));
            res.AddMenuButton(new MenuButton(button, mainMenu.BackButton_Click, res, "Back to main Menu"), sMenuButtonSize);
            return res;
        }

        private sealed class MusicVolume : INumber
        {
            private readonly SoundManager mSoundManager;
            private readonly bool mSfx;
            public MusicVolume(SoundManager soundManager, bool sfx)
            {
                mSfx = sfx;
                mSoundManager = soundManager;
            }
            public string Number => "" + ((int)((mSfx ? mSoundManager.GetSfxVolume() : mSoundManager.GetMusicVolume()) * NumberManager.Twenty));
        }

        public Menu GetMainMenuSettingsMenu(ImageManager imageManager, MainMenu mainMenu)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize),
                sButtonDist, new Point(), null, "Menu_Settings");
            var button = imageManager.GetImage("Button");
            res.AddMenuButton(new MenuButton(button, mainMenu.NumberBox_Click, res, "Music Volume"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.MusicVolumeDecrease_Click, res, "-"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new NumberBox(button, mainMenu.NumberBox_Click, res, new MusicVolume(Game1.GetGame().SoundManager, false)), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, mainMenu.MusicVolumeIncrease_Click, res, "+"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            // Effects Volume
            res.AddMenuButton(new MenuButton(button, mainMenu.NumberBox_Click, res, "Effects Volume"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, mainMenu.EffectsVolumeDecrease_Click, res, "-"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new NumberBox(button, mainMenu.NumberBox_Click, res, new MusicVolume(Game1.GetGame().SoundManager, true)), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, mainMenu.EffectsVolumeIncrease_Click, res, "+"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, mainMenu.DisplaymodeButton_Click, res, "Display Mode"), sMenuButtonSize);
            var b = new MenuTextField(button, mainMenu.ActiveTextField_Click, res)
            {
                Text = "" + Game1.GetGame().mSettings.mPort
            };
            sField = b;
            res.AddMenuButton(new NumberBox(button, null, res, INumber.GetConst("Port:")), new Size(sMenuButtonSize.Width / NumberManager.Two - sButtonDist, sMenuButtonSize.Height));
            res.AddMenuButton(b, new Size(sMenuButtonSize.Width / NumberManager.Two, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, mainMenu.BackButton_Click, res, "Back"), sMenuButtonSize);
            return res;
        }

        public Menu GetIngameMenu(ImageManager imageManager, InGame inGame)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.TopLeft, sIngameMenuSize), sButtonDist,
                new Point());
            var pauseButton = imageManager.GetImage("break_menu_button");
            var workerButton = imageManager.GetImage("worker_button");
            var buildButton = imageManager.GetImage("build_button");
            var unitsButton = imageManager.GetImage("units_button");
            res.AddMenuButton(new MenuButton(pauseButton, inGame.PauseButton_Click, res), new Size(sIngameMenuSize.Width, sIngameMenuSize.Height));
            res.AddMenuButton(new MenuButton(workerButton, inGame.WorkerButton_Click, res), new Size(sIngameMenuSize.Width, sIngameMenuSize.Height));
            res.AddMenuButton(new MenuButton(buildButton, inGame.BuildButton_Click, res), new Size(sIngameMenuSize.Width, sIngameMenuSize.Height));
            res.AddMenuButton(new MenuButton(unitsButton, inGame.UnitsButton_Click, res), new Size(sIngameMenuSize.Width, sIngameMenuSize.Height));
            return res;
        }

        public Menu GetPauseMenu(ImageManager imageManager, InGame inGame)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize), sButtonDist, new Point(),
                null, "pause");
            var button = imageManager.GetImage("Button");
            res.AddMenuButton(new MenuButton(button, inGame.PauseButton_Click, res, "Back to Game"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.SaveGameButton_Click, res, "Save Game"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.SettingsButton_Click, res, "Settings"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.BackToMainMenuButton_Click, res, "Return to Main Menu"), sMenuButtonSize);
            return res;
        }

        public Menu GetIngameSettingsMenu(ImageManager imageManager, InGame inGame)
        {
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize),
                sButtonDist, new Point(), null, "In-game_Settings");
            var button = imageManager.GetImage("Button");
            // Music Volume
            res.AddMenuButton(new MenuButton(button, inGame.NumberBox_Click, res, "Music Volume"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.MusicVolumeDecrease_Click, res, "-"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new NumberBox(button, inGame.NumberBox_Click, res, new MusicVolume(Game1.GetGame().SoundManager, false)), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, inGame.MusicVolumeIncrease_Click, res, "+"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            // Effects Volume
            res.AddMenuButton(new MenuButton(button, inGame.NumberBox_Click, res, "Effects Volume"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.EffectsVolumeDecrease_Click, res, "-"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new NumberBox(button, inGame.NumberBox_Click, res, new MusicVolume(Game1.GetGame().SoundManager, true)), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));
            res.AddMenuButton(new MenuButton(button, inGame.EffectsVolumeIncrease_Click, res, "+"), new Size(sMenuButtonSize.Width / NumberManager.Four, sMenuButtonSize.Height));

            res.AddMenuButton(new MenuButton(button, inGame.DisplayModeButton_Click, res, "Display Mode"), sMenuButtonSize);
            res.AddMenuButton(new MenuButton(button, inGame.BackToPauseMenuButton_Click, res, "Back"), sMenuButtonSize);
            return res;
        }

        public Menu GetFightingMenu(ImageManager imageManager, InGame inGame)
        {
            var res = new Menu(mFont, 
                new AlignedRectangle(mGraphicsDevice, Alignment.Right, sFightingMenuSize), sButtonDist, 
                new Point(), imageManager.GetImage("hud_box_1"), "", new Point(0, NumberManager.Twenty));
            res.SetDestructiveSwitches(true);
            var button = imageManager.GetImage("Button");
            res.AddMenuButton(new MenuButton(button, inGame.AttackButton_Click, res, "Attack", true), sMenuButtonSize);
            return res;
        }

        public Menu GetBuildingsMenu(ImageManager imageManager, InGame inGame)
        {
            var res = new Menu(mFont, new SideRectangle(mGraphicsDevice, Side.Bottom, sBuildingMenuHeight),
                sButtonDist, new Point(0, -sButtonDist), imageManager.GetImage("hud_box_1"), "BuildingSwitches",
                new Point(40, 40));
            res.SetDestructiveSwitches(true);
            var barracksButton = imageManager.GetImage("Barracks_button");
            var houseButton = imageManager.GetImage("House_button_text");
            var workerTrainingCenterButton = imageManager.GetImage("WorkerTrainingCenter_button");
            var towerButton = imageManager.GetImage("Tower_button");
            var roadButton = imageManager.GetImage("RoadTile_button");
            var lodgeButton = imageManager.GetImage("ForestersLodge_button");
            var sawmillButton = imageManager.GetImage("Sawmill_button");
            var stoneMineButton = imageManager.GetImage("StoneMine_button");
            var stoneProcessingButton = imageManager.GetImage("StoneProcessing_button");
            var ironMineButton = imageManager.GetImage("IronMine_button");
            var ironForgeButton = imageManager.GetImage("IronForge_button");
            var warehouseButton = imageManager.GetImage("Warehouse_button");
            var shipyardButton = imageManager.GetImage("Shipyard_button");
            res.AddMenuButton(new MenuButton(houseButton, inGame.HouseButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(barracksButton, inGame.BarracksButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(workerTrainingCenterButton, inGame.WorkerTrainingCenterButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(shipyardButton, inGame.ShipyardButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(towerButton, inGame.TowerButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(roadButton, inGame.RoadButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(lodgeButton, inGame.LodgeButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(sawmillButton, inGame.SawmillButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(ironMineButton, inGame.IronMineButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(ironForgeButton, inGame.IronForgeButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(stoneMineButton, inGame.StoneMine_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(stoneProcessingButton, inGame.StoneProcessingButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(warehouseButton, inGame.WarehouseButton_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            return res;
        }

        public Menu GetResourcesMenu(ImageManager imageManager, List<int> resources)
        {
            var res = new Menu(mFont, new SideRectangle(mGraphicsDevice, Side.Bottom, sBuildingMenuHeight), sButtonDist, new Point(0, -sButtonDist), imageManager.GetImage("hud_box_1"), "ResourcesAmounts", new Point(40, 40));
            var woodIcon = imageManager.GetImage("Wood");
            var woodPlankIcon = imageManager.GetImage("Plank");
            var ironOreIcon = imageManager.GetImage("IronOre");
            var ironIcon = imageManager.GetImage("Iron");
            var rawStoneIcon = imageManager.GetImage("RawStone");
            var stoneIcon = imageManager.GetImage("Stone");
            var transparentButton = imageManager.GetImage("Button_transparent");
            res.AddMenuButton(new MenuButton(woodIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[0]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(woodPlankIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[1]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(ironOreIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[NumberManager.Two]), "", true), new Size(sBuildingMenuHeight /NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(ironIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[NumberManager.Three]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(rawStoneIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[NumberManager.Four]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new MenuButton(stoneIcon, InGame.Icon_Click, res, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + resources[NumberManager.Five]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            return res;
        }

        public Menu GetStatisticsMenu(ImageManager imageManager, InGame inGame, int winner)
        {
            
            var res = new Menu(mFont, new AlignedRectangle(mGraphicsDevice, Alignment.Middle, sMenuButtonSize * NumberManager.Four), 0, new Point(0, 0), null, "Statistics", new Point(0, 0));
            var button = imageManager.GetImage("Button");
            var text = "You lost!";
            var color = Color.Red;
            if (winner == inGame.PlayerNumber)
            {
                text = "You won!";
                color = Color.Green;
            }
            else if(winner == 0)
            {
                text = "Its a draw!";
                color = Color.Gold;
            }
            res.AddMenuButton(new NumberBox(button, null, res, null, text, false, color), new Size(sMenuButtonSize.Width * NumberManager.Four, sMenuButtonSize.Height));
            foreach (Statistic statistic in Enum.GetValues(typeof(Statistic)))
            {
                if (statistic == Statistic.BlueprintSpaces)
                {
                    continue;
                }
                res.AddMenuButton(new NumberBox(button, null, res, null, statistic.ToString()), new Size(sMenuButtonSize.Width * NumberManager.Two, sMenuButtonSize.Height));
                if (statistic == Statistic.GameTime)
                {
                    var boxTime = new NumberBox(button, null, res, null,
                        "" + TimeManager.GetTime(GameMap.StatisticsManager.GetStatistic(statistic, 1)).ToString())
                    {
                        Color = Color.Blue
                    };
                    res.AddMenuButton(boxTime, sMenuButtonSize);
                    boxTime = new NumberBox(button, null, res, null,
                        "" + TimeManager.GetTime(GameMap.StatisticsManager.GetStatistic(statistic, 1)).ToString())
                    {
                        Color = Color.Red
                    };
                    res.AddMenuButton(boxTime, sMenuButtonSize);
                }
                else
                {
                    var box = new NumberBox(button, null, res, null,
                            "" + GameMap.StatisticsManager.GetStatistic(statistic, 1))
                    {
                        Color = Color.Blue
                    };
                    res.AddMenuButton(box, sMenuButtonSize);
                    box = new NumberBox(button, null, res, null,
                        "" + GameMap.StatisticsManager.GetStatistic(statistic, NumberManager.Two))
                    {
                        Color = Color.Red
                    };
                    res.AddMenuButton(box, sMenuButtonSize);
                }
            }
            res.AddMenuButton(new MenuButton(button, inGame.BackToMainMenuButton_Click, res, "Return to Main Menu"), sMenuButtonSize);
            return res;
        }

        public Menu GetAchievementsMenu(ImageManager imageManager, AchievementManager achievementManager)
        {
            Dictionary<Achievement, string> achievementTexts = new Dictionary<Achievement, string>
            {
                { Achievement.FirstBlood , "First Blood: Win the first fight of the game" },
                { Achievement.Conqueror, "Conqueror: Win a game" },
                { Achievement.KingOfTheIslands, "King of the Islands: Posess 10 islands in one game" },
                { Achievement.Rolled, "Rolled: Win a game in less than 1/3 of the time" },
                { Achievement.BobTheBuilder, "Bob the Builder: Build 100 buildings in one game and win" },
                { Achievement.BlindWarrior, "Blind Warrior: Win a game with no spies on your side" },
                { Achievement.BloodWillFlow, "Blood Will Flow: Kill 200 enemies in one game" },
                { Achievement.TheFleetAdmiral, "The Fleet Admiral: Build 10 ships in one game and win" },
                { Achievement.SuperiorSpy, "Superior Spy: Kill an enemy spy" },
                { Achievement.WhoNeedsBridges, "Who Needs Bridges: Win a game with no bridges built" },
                { Achievement.TheBridgeMaster, "The Bridge Master: Build 100 bridgetiles in one game and win" },
                { Achievement.TheFallOfTheCitadel, "The Fall of the Citadel: Destroy enemy's main building" },
                { Achievement.SlowAndSteady, "Slow and Steady: Win the game by destroying enemy's main building in the last minute of the game" }
            };
            var button = imageManager.GetImage("Button_transparent");
            var res = new Menu(mFont, new SideRectangle(mGraphicsDevice, Side.Top, 80), sButtonDist, new Point(0, -sButtonDist));
            
            foreach (var key in achievementTexts.Keys)
            {
                res.AddMenuButton(
                    achievementManager.Achievements.Contains(key)
                        ? new NumberBox(button, null, res, null, achievementTexts[key], false, Color.Green)
                        : new NumberBox(button, null, res, null, achievementTexts[key], false, Color.White),
                    new Size(sMenuButtonSize.Width * NumberManager.Five, sMenuButtonSize.Height));
            }
            return res;
        }

        public Menu GetUnitsMenu(ImageManager imageManager, InGame inGame, Dictionary<string, int> units)
        {
            var res = new Menu(mFont, new SideRectangle(mGraphicsDevice, Side.Bottom, sBuildingMenuHeight), sButtonDist, new Point(0, -sButtonDist), imageManager.GetImage("hud_box_1"), "UnitAmounts", new Point(40, 40));
            var workerIcon = imageManager.GetImage("creat_worker_button");
            var swordsmanIcon = imageManager.GetImage("creat_swordman_button");
            var spearmanIcon = imageManager.GetImage("creat_spearman_button");
            var shieldmanIcon = imageManager.GetImage("creat_shieldman_button"); // Pikeman!
            var spyIcon = imageManager.GetImage("creat_spy_button");
            var transporthipIcon = imageManager.GetImage("TransportShip_button");
            var scoutshipIcon = imageManager.GetImage("ScoutShip_button");
            var transparentButton = imageManager.GetImage("Button_transparent");
            res.AddMenuButton(new NumberBox(workerIcon, InGame.Icon_Click, res,null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res,INumber.GetConst("" + units["Worker"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(swordsmanIcon, InGame.Icon_Click, res,null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res,INumber.GetConst("" + units["Swordsman"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(spearmanIcon, InGame.Icon_Click, res,null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res,INumber.GetConst("" + units["Spearman"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(shieldmanIcon, InGame.Icon_Click, res,null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res,INumber.GetConst("" + units["Shieldman"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(spyIcon, InGame.Icon_Click, res,null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res,INumber.GetConst("" + units["Spy"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transporthipIcon, InGame.Icon_Click, res, null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + units["TransportShip"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(scoutshipIcon, InGame.Icon_Click, res, null, "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, INumber.GetConst("" + units["ScoutShip"]), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            res.AddMenuButton(new NumberBox(transparentButton, InGame.Icon_Click, res, new CapTracker(inGame.PlayerNumber), "", true), new Size(sBuildingMenuHeight / NumberManager.Two, sBuildingMenuHeight / NumberManager.Two));
            return res;
        }

        public Menu GetWorkerBuildingMenu(ImageManager imageManager, InGame inGame, int id)
        {
            var button = imageManager.GetImage("Button");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sMenuButtonSize), sBuildingButtonDist,
                new Point(), null, "worker-menu");
            res.SetDestructiveSwitches(true);
            res.AddMenuButton(new MenuButton(button, inGame.Deposit_Click, res, "Deposit", isSwitch: false), sBuildingMenuSize);
            res.AddMenuButton(new MenuButton(button, inGame.Withdraw_Click, res, "Withdraw", isSwitch: false), sBuildingMenuSize);
            return res;
        }

        public Menu GetOwnInfiltratedTowerMenu(ImageManager imageManager, InGame inGame, int id)
        {
            var button = imageManager.GetImage("Button");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sMenuButtonSize), sBuildingButtonDist,
                new Point(), null, "unitmenu");
            res.SetDestructiveSwitches(true);
            res = ShowTowerInside(imageManager, inGame, inGame.RollbackManager.CurrentState.Map.GetObject(id), true);
            res.AddMenuButton(new MenuButton(button, inGame.Kill_Spy_Click, res, "Kill Spy", isSwitch: false), sBuildingMenuSize);
            return res;
        }

        private sealed class StoreUnits : INumber
        {
            private readonly string mUnitName;
            private readonly string mUnitNumber;

            public StoreUnits(string name, string number)
            {
                mUnitName = name;
                mUnitNumber = number;
            }

            public string Number => mUnitName + ": " + mUnitNumber;
        }

        public Menu GetSelectedBuildingsMenu(ImageManager imageManager, InGame inGame, int id, GameObject gameObject)
        {
            var button = imageManager.GetImage("Button");
            var whiteButton = imageManager.GetImage("Button_white");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "selected" + id);
            res.SetDestructiveSwitches(true);
            if (gameObject is ResourceBuilding building)
            {
                foreach (var key in building.CurrentResourcesStored.ItemAmounts.Keys)
                {
                    res.AddMenuButton(new NumberBox(whiteButton, InGame.Icon_Click, res,building.GetTracker(inGame.RollbackManager,key), key + ":", isSwitch: false), sBuildingMenuSize);
                }
                res.AddMenuButton(new MenuButton(button,inGame.RemovePriorityClick,res,"-"), new Size(sBuildingMenuSize.Width / NumberManager.Five, sBuildingMenuSize.Height));
                res.AddMenuButton(new NumberBox(whiteButton, null, res, building.GetPriorityTracker(inGame.RollbackManager)), new Size(NumberManager.Three * sBuildingMenuSize.Width / NumberManager.Five - sBuildingButtonDist * NumberManager.Two, sBuildingMenuSize.Height));
                res.AddMenuButton(new MenuButton(button, inGame.AddPriorityClick, res, "+"), new Size(sBuildingMenuSize.Width / NumberManager.Five, sBuildingMenuSize.Height));
            }

            if (gameObject is Tower tower)
            {
                GetTowerNumbers(tower, res, whiteButton);
                res.AddMenuButton(new MenuButton(button, inGame.Unman_Tower_Click, res, "Unman Tower", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is Barracks)
            {
                res.AddMenuButton(new MenuButton(button, inGame.TrainShieldman_Click, res, "Train Pikeman", isSwitch: false), sBuildingMenuSize);
                res.AddMenuButton(new MenuButton(button, inGame.TrainSpearman_Click, res, "Train Spearman", isSwitch: false), sBuildingMenuSize);
                res.AddMenuButton(new MenuButton(button, inGame.TrainSwordsman_Click, res, "Train Swordsman", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is WorkerTrainingCenter)
            {
                res.AddMenuButton(new MenuButton(button, inGame.TrainWorker_Click, res, "Train Worker", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is Shipyard)
            {
                res.AddMenuButton(new MenuButton(button, inGame.TrainTransportShip_Click, res, "Train Transportship", isSwitch: false), sBuildingMenuSize);
                res.AddMenuButton(new MenuButton(button, inGame.TrainScoutShip_Click, res, "Train Scoutship", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is MainBuilding)
            {
                res.AddMenuButton(new MenuButton(button, inGame.TrainSpy_Click, res, "Train Spy", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is ForestersLodge)
            {
                res.AddMenuButton(new MenuButton(button, inGame.Plant_Harvest_Mode_Button_Click, res, "Plant + Harvest", isSwitch: false), sBuildingMenuSize);
                res.AddMenuButton(new MenuButton(button, inGame.Harvest_Only_Button_Click, res, "Harvest only", isSwitch: false), sBuildingMenuSize);
            }
            if (gameObject is Building && !(gameObject is MainBuilding))
            {
                res.AddMenuButton(new MenuButton(button, inGame.DestroyBuildingButton_Click, res, "Destroy", isSwitch: false), sBuildingMenuSize);
            }
            else if (gameObject is MainBuilding)
            {
                res.AddMenuButton(new MenuButton(button, inGame.DestroyBuildingButton_Click, res, "Surrender", isSwitch: false), sBuildingMenuSize);
            }
            return res;
        }

        public Menu GetTransportShipMenu(ImageManager imageManager, InGame inGame, int id, GameObject gameObject)
        {
            var button = imageManager.GetImage("Button");
            var whiteButton = imageManager.GetImage("Button_white");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "transportship" + id);
            res.SetDestructiveSwitches(true);
            if (gameObject is TransportShip transportShip)
            {
                GetShipNumber(transportShip, res, whiteButton);
            }
            res.AddMenuButton(new MenuButton(button, inGame.Unload_TransportShip_Click, res, "Unman Ship", isSwitch: false), sBuildingMenuSize);
            return res;
        }

        public Menu GetSpyTowerMenu(ImageManager imageManager, InGame inGame, int id)
        {
            var button = imageManager.GetImage("Button");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "spytower" + id);
            res.SetDestructiveSwitches(true);
            res.AddMenuButton(new MenuButton(button, inGame.Infiltrate_Click, res, "Infiltrate", isSwitch: false), sBuildingMenuSize);
            return res;
        }

        public Menu GetInfiltratedTowerMenu(ImageManager imageManager, InGame inGame, int id, GameObject gameObject)
        {
            var button = imageManager.GetImage("Button");
            var whiteButton = imageManager.GetImage("Button_white");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "infiltrate" + id);
            res.SetDestructiveSwitches(true);
            if (gameObject is Tower tower && tower.SpyInside.Count == 1)
            {
                GetTowerNumbers(tower, res, whiteButton);
                res.AddMenuButton(new MenuButton(button, inGame.Leave_Tower_Click, res, "Leave Tower", isSwitch: false), sBuildingMenuSize);
            }
            return res;
        }

        public Menu GetRoadConfirmMenu(ImageManager imageManager, InGame inGame)
        {
            var check = imageManager.GetImage("GreenCheck_button");
            var cross = imageManager.GetImage("RedCross_button");
            var res = new Menu(mBuildingFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.TopRight, sRoadConfirmMenuSize),
                sButtonDist, new Point(-NumberManager.Twenty, NumberManager.Twenty), null, "RoadConfirmMenu");
            res.SetDestructiveSwitches(true);
            res.AddMenuButton(new MenuButton(check, inGame.RoadCheck_Click, res), sRoadConfirmMenuSize);
            res.AddMenuButton(new MenuButton(cross, inGame.RoadCross_Click, res), sRoadConfirmMenuSize);
            return res;
        }

        public Menu GetTimer(ImageManager imageManager, InGame inGame, RollbackManager rollbackManager)
        {
            var whiteButton = imageManager.GetImage("Button_white");
            var size=new Size((int)(sMenuButtonSize.Width*1.5), sMenuButtonSize.Height);
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.Top, size),
                sButtonDist,
                new Point());
            res.AddMenuButton(new Counter(whiteButton, null, res, new TickNr(rollbackManager), "Time Left: "),size);
            res.AddMenuButton(new NumberBox(whiteButton, null, res, GameMap.StatisticsManager.GetTracker(Statistic.BlueprintSpaces, inGame.PlayerNumber, " / " + Scaffolding.MaxBlueprintSpaces), "Used Blueprint Spaces: "), size);
            return res;
        }

        public Menu GetStoredItems(ImageManager imageManager, InGame inGame, int needed, ResourceBuilding gameObject)
        {
            var items = new List<Item>()
            {
                Item.Wood,
                Item.Plank,
                Item.IronOre,
                Item.Iron,
                Item.RawStone,
                Item.Stone
            };
            var button = imageManager.GetImage("Button");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, gameObject.Id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "StoredItems");
            foreach (var item in items)
            {
                if (gameObject.MaxResourcesStorable.ItemAmounts.ContainsKey(item) && gameObject.CurrentResourcesStored.ItemAmounts[item] > needed)
                {
                    GetButton(res, item, button, inGame);
                }
            }
            return res;
        }

        private void GetButton(Menu menu, Item item, Texture2D button, InGame inGame)
        {
            switch (item)
            {
                case Item.Stone:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutStone, menu, "Take " + item), sBuildingMenuSize);
                    break;
                case Item.Iron:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutIron, menu, "Take " + item), sBuildingMenuSize);
                    break;
                case Item.IronOre:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutIronOre, menu, "Take " + item), sBuildingMenuSize);
                    break;
                case Item.Plank:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutPlanks, menu, "Take " + item), sBuildingMenuSize);
                    break;
                case Item.RawStone:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutRawStone, menu, "Take " + item), sBuildingMenuSize);
                    break;
                case Item.Wood:
                    menu.AddMenuButton(new MenuButton(button, inGame.GetItemOutWood, menu, "Take " + item), sBuildingMenuSize);
                    break;
            }
        }

        private void GetTowerNumbers(Tower tower, Menu menu, Texture2D button)
        {
            if (tower.PikemenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Pikemen", tower.PikemenInsideString())), sBuildingMenuSize);
            }
            if (tower.SpearmenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Spearmen", tower.SpearmenInsideString())), sBuildingMenuSize);
            }
            if (tower.SwordsmenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Swordsmen", tower.SwordsmenInsideString())), sBuildingMenuSize);
            }
        }

        private void GetShipNumber(TransportShip transportShip, Menu menu, Texture2D button)
        {
            if (transportShip.PikemenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Pikemen", transportShip.PikemenInsideString())), sBuildingMenuSize);
            }
            if (transportShip.SpearmenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Spearmen", transportShip.SpearmenInsideString())), sBuildingMenuSize);
            }
            if (transportShip.SwordsmenInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Swordsmen", transportShip.SwordsmenInsideString())), sBuildingMenuSize);
            }

            if (transportShip.WorkerInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Worker", transportShip.WorkerInsideString())), sBuildingMenuSize);
            }
            if (transportShip.SpiesInsideInt() > 0)
            {
                menu.AddMenuButton(new NumberBox(button, InGame.Icon_Click, menu, new StoreUnits("Spies", transportShip.SpiesInsideString())), sBuildingMenuSize);
            }
        }

        public Menu ShowShipInside(ImageManager image, InGame inGame, GameObject gameObject, bool rmb)
        {
            var whiteButton = image.GetImage("Button_white");
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, gameObject.Id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "ship" + gameObject.Id);
            if (gameObject is TransportShip ship)
            {
                GetShipNumber(ship, res, whiteButton);
            }

            foreach (var id in inGame.SelectedGameObjectId)
            {
                if (inGame.RollbackManager.CurrentState.Map.GetObject(id) is ObjectMoving objectMoving &&
                    !(objectMoving is TransportShip || objectMoving is ScoutShip) && rmb)
                {
                    res.AddMenuButton(new MenuButton(whiteButton, inGame.Man_TransportShip_Click, res, "Man Transportship", isSwitch: false), sBuildingMenuSize);
                    break;
                }
            }
            return res;
        }

        public Menu ShowTowerInside(ImageManager image, InGame inGame, GameObject gameObject, bool rmb)
        { 
            var whiteButton = image.GetImage("Button_white"); 
            var res = new Menu(mBuildingFont, new GameObjectRectangle(mGraphicsDevice, gameObject.Id, inGame, sBuildingMenuSize), sBuildingButtonDist,
                new Point(), null, "tower" + gameObject.Id); 
            if (gameObject is Tower tower) 
            { 
                GetTowerNumbers(tower, res, whiteButton);
                if (tower.SpyInside.Count == 1)
                {
                    res.AddMenuButton(new NumberBox(whiteButton, InGame.Icon_Click, res, null, "Infiltrated!", false, Color.Purple), sBuildingMenuSize);
                }
            }
            foreach (var id in inGame.SelectedGameObjectId)
            {
                if (inGame.RollbackManager.CurrentState.Map.GetObject(id) is ObjectMoving objectMoving && (objectMoving is Shieldman || objectMoving is Spearman || objectMoving is Swordsman) && rmb)
                {
                    res.AddMenuButton(new MenuButton(whiteButton, inGame.Man_Tower_Click, res, "Man Tower", isSwitch: false), sBuildingMenuSize);
                    break;
                }
            }
            return res;
        }

        public void AddCost(Building building, InGame inGame, ImageManager image)
        {
            inGame.AddIfNewName(CostFor(building,inGame,image));
        }

        private Menu CostFor(Building building, InGame inGame, ImageManager image)
        {
            var res = new Menu(mFont,
                new AlignedRectangle(mGraphicsDevice, Alignment.TopRight, sRoadConfirmMenuSize),
                sButtonDist,
                new Point(-250,0),
                null,
                building.ClassNumber.ToString());
            var whiteButton = image.GetImage("Button_white");
            foreach (var items in building.ResourceCost.ItemAmounts)
            {
                res.AddMenuButton(new NumberBox(whiteButton,InGame.Icon_Click,res,new StoreUnits(items.Key.ToString(),items.Value.ToString()),"",false,IsEnough(items.Key,items.Value,inGame.PlayerNumber,inGame)),sCostMenuSize);
            }
            return res;
        }

        private Color IsEnough(Item item, int needed,int playerNumber,InGame inGame)
        {
            if (item is Item.IronOre || item is Item.RawStone || item is Item.Wood)
            {
                return Color.Gray;
            }
            var isStored =
                inGame.RollbackManager.CurrentState.Map.AmountOfAllStoredResourcesOfKind(item, playerNumber);
            if (needed < isStored)
            {
                return Color.Green;
            }
            return Color.Red;
        }
    }
}