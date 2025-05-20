using System;
using System.Collections.Generic;
using System.Drawing;
using Common.com.game;
using Common.com.game.settings;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;
using Common.com.objects.immovables.Resources;
using Common.com.path;
using Microsoft.Xna.Framework;
using Path = Common.com.path.Path;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Server.net.map
{
    public static class MapGeneration
    {
        public static GameMap TestMap => GetTestMap();
        public static GameMap TechDemo => GetTechDemo();

        public static GameMap GenerateMap()
        {
            var res = new GameMap(new Size(200, 200));
            GenerateTiles(res);
            res.UpdateIslands();
            GenerateOres(res);
            AddSpawns(res);
            return res;
        }

        private static void AddSpawns(GameMap gameMap)
        {
            
            AddSpawn(gameMap,GameMap.IslandMapper.BestSpawnBottom,1);
            AddSpawn(gameMap,GameMap.IslandMapper.BestSpawnTop,NumberManager.Two);
        }

        private static void GenerateOres(GameMap gameMap,int seed=-1)
        {
            seed = seed == -1 ? new Random().Next() : seed;
            var random = new Random(seed); 
            for (var x = 0; x < gameMap.GetSize().Width-1; x++)
            {
                for (var y = 0; y < gameMap.GetSize().Height-1; y++)
                {
                    var ore=new IronDeposit {Location = new Point(x, y), Id = IdGenerator.NewId};
                    var required = new List<Terrain> {Terrain.Grass,Terrain.Mountains};
                    if (gameMap.CanBePut(ore,0, required))
                    {
                        var chance = Perlin(x * 0.1f, y * 0.1f, seed+1);
                        var check1 = NumberManager.ZeroPointTwo + random.NextDouble() / NumberManager.Two;
                        var check2 = NumberManager.ZeroPointTwo + random.NextDouble() / NumberManager.Two;
                        PlaceOre(chance, check1, check2, gameMap,ore);
                    }
                    
                    var tree=new Tree {Location = new Point(x, y), ForestersLodgeId = -1, Id = IdGenerator.NewId};
                    required = new List<Terrain> {Terrain.Grass};
                    if (gameMap.CanBePut(tree,0, required))
                    {
                        var chance = Perlin(x * 0.1f, y * 0.1f, seed);
                        var check1 = random.NextDouble() / NumberManager.Two;
                        var check2 = random.NextDouble() / NumberManager.Two;
                        PlaceTree(chance, check1, check2, gameMap, tree);
                    }
                }
            }
        }

        private static void ClearRect(GameMap gameMap,Rectangle rectangle)
        {
            for (var x = rectangle.Left; x < rectangle.Right; x++)
            {
                for (var y = rectangle.Top; y < rectangle.Bottom; y++)
                {
                    if (gameMap.GetObject(x, y) is { } gameObject)
                    {
                        gameMap.RemoveObject(gameObject);
                    }
                }
            }
        }
        private static void AddSpawn(GameMap gameMap, Point p, int playerId)
        {
            ClearRect(gameMap,new Rectangle(p.X-NumberManager.Four,p.Y- NumberManager.Four,new MainBuilding().GetBounds().Width+ NumberManager.Three,new MainBuilding().GetBounds().Width+ NumberManager.Three));
            gameMap.AddObject(new MainBuilding {Id = IdGenerator.NewId, IsBlueprint = false, Location = new Point(p.X - 1, p.Y - 1), PlayerNumber = playerId});
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive + NumberManager.Four,Y=p.Y- (float)NumberManager.OnePointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive + NumberManager.Three,Y=p.Y- (float)NumberManager.OnePointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive + NumberManager.Two,Y=p.Y- (float)NumberManager.OnePointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive,Y=p.Y- (float)NumberManager.OnePointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive,Y=p.Y- (float)NumberManager.ZeroPointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.ZeroPointFive,Y=p.Y- (float)NumberManager.OnePointFive });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive,Y=p.Y- (float)NumberManager.OnePointFive + NumberManager.Two });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive,Y=p.Y- (float)NumberManager.OnePointFive + NumberManager.Three });
            gameMap.AddObject(new Worker(){Id=IdGenerator.NewId,PlayerNumber = playerId,X=p.X- (float)NumberManager.OnePointFive,Y=p.Y- (float)NumberManager.OnePointFive + NumberManager.Four });
        }

        private static GameMap GetTestMap()
        {
            var res = new GameMap(new Size(NumberManager.Twenty, NumberManager.Twenty));
            for (uint x = 0; x < NumberManager.Twenty; x++)
            {
                for (uint y = 0; y < NumberManager.Twenty; y++)
                {
                    res.SetTerrain(x,y,Terrain.Grass);
                }
            }
            res.SetTerrain(NumberManager.Five, NumberManager.Five,Terrain.Mountains);
            
            res.SetTerrain(NumberManager.Five, NumberManager.Four,Terrain.Water);
            
            res.SetTerrain(NumberManager.Five, NumberManager.Six,Terrain.Road);
            var path = new LinkedList<Point>();
            path.AddLast(new Point(NumberManager.Five, 0));
            path.AddLast(new Point(NumberManager.Five, 1));
            path.AddLast(new Point(NumberManager.Five, NumberManager.Two));
            path.AddLast(new Point(NumberManager.Four, NumberManager.Two));
            var p2=new PathAbstract(new Point(0, NumberManager.Five),new Point(NumberManager.Eight, NumberManager.Five),false).FindPath(res,new Path[]{});
            var p=new PathAbstract(new Point(NumberManager.Five,0),new Point(NumberManager.Five, NumberManager.Eight),false).FindPath(res,new[]{p2});
            var p3=new PathAbstract(new Point(0,0),new Point(NumberManager.Nine, NumberManager.Ten),false).FindPath(res,new[]{p2,p});
            res.AddObject(new Worker(1,IdGenerator.NewId,new Vector2(NumberManager.ZeroPointFiveF, NumberManager.FivePointFive),p2,1));
            res.AddObject(new Worker(NumberManager.Two,IdGenerator.NewId,new Vector2(NumberManager.FivePointFive, NumberManager.OnePointFiveF),p,1));
            res.AddObject(new Spearman(1,IdGenerator.NewId,new Vector2(NumberManager.TwelveF, NumberManager.TwelveF),p3, NumberManager.Fifteen));
            res.AddObject(new Spearman(1, IdGenerator.NewId, new Vector2(NumberManager.SeventeenF, NumberManager.TwelveF), p3, NumberManager.Fifteen));
            res.AddObject(new Spearman(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.NineteenF, NumberManager.TwelveF), p3, NumberManager.Fifteen));
            res.AddObject(new Spearman(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.FifteenF, NumberManager.TwelveF), p3, NumberManager.Fifteen));
            res.AddObject(new Worker(1,IdGenerator.NewId,new Vector2(NumberManager.OnePointFiveF, NumberManager.ZeroPointFiveF),null,1));
            res.AddObject(new Spy(1,IdGenerator.NewId,new Vector2(NumberManager.TwoPointFive, NumberManager.ZeroPointFiveF),null,1));
            res.AddObject(new Spy(NumberManager.Two,IdGenerator.NewId,new Vector2(NumberManager.ThreePointFive, NumberManager.ZeroPointFiveF),null,1));
            res.AddObject(new Spy(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.FourPointFive, NumberManager.ZeroPointFiveF), null, 1));
            res.AddObject(new Swordsman(1, IdGenerator.NewId, new Vector2(NumberManager.FivePointFive, NumberManager.ZeroPointFiveF), null, 1));
            res.AddObject(new Spearman(1, IdGenerator.NewId, new Vector2(NumberManager.SixPointFive, NumberManager.ZeroPointFiveF), null, 1));
            res.AddObject(new Shieldman(1, IdGenerator.NewId, new Vector2(NumberManager.SevenPointFive, NumberManager.ZeroPointFiveF), null, 1));
            res.AddObject(new Swordsman(1, IdGenerator.NewId, new Vector2(NumberManager.EightPointFive, NumberManager.ZeroPointFiveF), null, 1));
            res.AddObject(new Spearman(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.ThreePointFive, NumberManager.ThreePointFive), null, NumberManager.Two));
           
            var barracks = new Barracks()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.Six, NumberManager.Eight),
                Id = IdGenerator.NewId
                
            };
            res.AddObject(barracks);
            var tower = new Tower()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.Two, NumberManager.Eight),
                Id = IdGenerator.NewId
            };
            res.AddObject(tower);
            var house = new House()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.Ten, NumberManager.Ten),
                Id = IdGenerator.NewId,
            };
            res.AddObject(house);
            return res;
        }

        private static bool CheckX1(uint x)
        {
            if (x == NumberManager.Twenty || x == NumberManager.SixtyNine)
            {
                return true;
            }

            return CheckX2(x);
        }

        private static bool CheckX2(uint x)
        { 
            if (x == NumberManager.Ten || x == NumberManager.ThirtyOne)
            {
                return true;
            }

            if (x == NumberManager.FiftyEight || x == NumberManager.SeventyNine)
            {
                return true;
            }

            return false;
        }

        private static bool CheckY1(uint y)
        {
            if (y == NumberManager.Ten ||
                y == NumberManager.ThirtyOne)
            {
                return true;
            }

            if (y == NumberManager.FiftyEight || y == NumberManager.SeventyNine)
            {
                return true;
            }

            return CheckY2(y);
        }

        private static bool CheckY2(uint y)
        {
            if (y == NumberManager.Twenty || y == NumberManager.SixtyNine)
            {
                return true;
            }

            return false;
        }
        private static bool CheckBridgeSet1(GameMap game, uint x, uint y)
        {
            if (CheckX1(x) && CheckY1(y))
            {
                game.SetTerrain(x,y,Terrain.Bridge);
                return true;
            }

            return CheckBridgeSet2(game, x, y);
        }

        private static bool CheckBridgeSet2(GameMap game, uint x, uint y)
        {
            if (CheckY1(y) && CheckX1(x))
            {
                game.SetTerrain(x,y,Terrain.Bridge);
                return true;
            }

            return false;
        }

        private static bool CheckWaterSet1(GameMap game, uint x, uint y)
        {
            if ((x < NumberManager.TwentyOne && x > NumberManager.Nineteen) ||
                (y < NumberManager.TwentyOne && y > NumberManager.Nineteen))
            {
                game.SetTerrain(x,y,Terrain.Water);
                return true;
            }

            return CheckWaterSet2(game, x, y);
        }

        private static bool CheckWaterSet2(GameMap game, uint x, uint y)
        {
            if((x < NumberManager.FortyNine && x > NumberManager.Forty) || (y < NumberManager.FortyNine && y > NumberManager.Forty))
            {
                game.SetTerrain(x,y,Terrain.Water);
                return true;
            }

            return CheckWaterSet3(game, x, y);
        }

        private static bool CheckWaterSet3(GameMap game, uint x, uint y)
        {
            if ((x < NumberManager.Seventy && x > NumberManager.SixtyEight) ||
                (y < NumberManager.Seventy && y > NumberManager.SixtyEight))
            {
                game.SetTerrain(x,y,Terrain.Water);
                return true;
            }

            return false;
        }

        private static bool SetMountainSet(GameMap game, uint x, uint y)
        {
            if (x < NumberManager.FiftyNine && x > NumberManager.FortyNine && y < NumberManager.FortyOne &&
                y > NumberManager.Thirty)
            {
                game.SetTerrain(x,y,Terrain.Mountains);
                return true;
            }

            if (x < NumberManager.FortyOne && x > NumberManager.Thirty && y < NumberManager.FiftyNine &&
                y > NumberManager.FortyEight)
            {
                game.SetTerrain(x,y,Terrain.Mountains);
                return true;
            }

            return false;

        }

        private static void ChangeWater(GameMap game)
        {

            for (uint x = NumberManager.ThirtyFive; x < NumberManager.FiftyTwo; x++)
            {
                for (int y = -3; y < NumberManager.Four; y++)
                {
                    if (game.GetTerrainAt(x, (uint)(x - y)) == Terrain.Water)
                    {
                        game.SetTerrain(x, (uint)(x - y), Terrain.Bridge);
                    }
                }
            }
        }

        private static void SetShieldmanPlayerOne(GameMap game)
        {

            for (uint x = 0; x < NumberManager.Twenty; x += NumberManager.Two)
            {
                for (uint y = 0; y < NumberManager.Twenty; y += NumberManager.Two)
                {
                    game.AddObject(new Shieldman(1, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.TwentyOne));
                }
            }
        }

        private static void SetSpearmanPlayerOne(GameMap game)
        {
            for (uint x = 0; x < NumberManager.Twenty; x += NumberManager.Two)
            {
                for (uint y = NumberManager.TwentyOne; y < NumberManager.FortyOne; y += NumberManager.Two)
                {
                    game.AddObject(new Spearman(1, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Fifteen));
                }
            }
            for (uint x = NumberManager.TwentyOne; x < NumberManager.FortyOne; x += NumberManager.Two)
            {
                for (uint y = 0; y < NumberManager.Twenty; y += NumberManager.Two)
                {
                    game.AddObject(new Spearman(1, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Fifteen));
                }
            }
        }

        private static void SetSwordmanPlayerOne(GameMap game)
        {
            for (uint x = NumberManager.TwentyOne; x < NumberManager.FortyOne; x += NumberManager.Two)
            {
                for (uint y = NumberManager.TwentyOne; y < NumberManager.FortyOne; y += NumberManager.Two)
                {
                    game.AddObject(new Swordsman(1, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Ten));
                }
            }
        }

        private static void SetWorkerPlayerOen(GameMap game)
        {
            for (uint y = 0; y < NumberManager.Seven; y++)
            {
                game.AddObject(new Worker(1, IdGenerator.NewId, new Vector2(NumberManager.TwentyOne + NumberManager.ZeroPointFiveF, NumberManager.Eighty + y + NumberManager.ZeroPointFiveF),
                    null, NumberManager.Three));
            }
        }
        private static void SetSpyPlayerOne(GameMap game)
        {
            for (uint y = 1; y < NumberManager.Five; y++)
            {
                game.AddObject(new Spy(1, IdGenerator.NewId, new Vector2(NumberManager.TwentyFive + NumberManager.ZeroPointFiveF, NumberManager.Eighty + y + NumberManager.ZeroPointFiveF),
                    null, 1));
            }
        }
        private static void SetTransportShipPlayerOen(GameMap game)
        {
            for (uint x = 0; x < NumberManager.Three; x += NumberManager.Two)
            {
                for (uint y = 0; y < NumberManager.Five; y += NumberManager.Two)
                {
                    var transportShip1 = new TransportShip()
                    {
                        PlayerNumber = 1,
                        Id = IdGenerator.NewId,

                        X = NumberManager.FortyTwo + x + NumberManager.ZeroPointFiveF,
                        Y = NumberManager.FiftyFive + NumberManager.Two + y + NumberManager.ZeroPointFiveF
                    };
                    game.AddObject(transportShip1);
                }
            }
        }
        private static void SetShieldmanPlayerTwo(GameMap game)
        {
            for (uint x = NumberManager.FortyNine; x < NumberManager.SixtyNine; x += NumberManager.Two)
            {
                for (uint y = NumberManager.FortyNine; y < NumberManager.SixtyNine; y += NumberManager.Two)
                {
                    game.AddObject(new Shieldman(NumberManager.Two, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.TwentyOne));
                }
            }
        }
        private static void SetSpearmanPlayerTwo(GameMap game)
        {
            for (uint x = NumberManager.FortyNine; x < NumberManager.SixtyNine; x += NumberManager.Two)
            {
                for (uint y = NumberManager.Seventy; y < NumberManager.Ninety; y += NumberManager.Two)
                {
                    game.AddObject(new Spearman(NumberManager.Two, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Fifteen));
                }
            }
            for (uint x = NumberManager.Seventy; x < NumberManager.Ninety; x += NumberManager.Two)
            {
                for (uint y = NumberManager.FortyNine; y < NumberManager.SixtyNine; y += NumberManager.Two)
                {
                    game.AddObject(new Spearman(NumberManager.Two, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Fifteen));
                }
            }
        }
        private static void SetSwordmanPlayerTwo(GameMap game)
        {
            for (uint x = NumberManager.Seventy; x < NumberManager.Ninety; x += NumberManager.Two)
            {
                for (uint y = NumberManager.Seventy; y < NumberManager.Ninety; y += NumberManager.Two)
                {
                    game.AddObject(new Swordsman(NumberManager.Two, IdGenerator.NewId, new Vector2(x + NumberManager.ZeroPointFiveF, y + NumberManager.ZeroPointFiveF),
                        null, NumberManager.Ten));
                }
            }
        }

        private static void SetWorkerPlayerTwo(GameMap game)
        {
            for (uint x = 0; x < NumberManager.Seven; x++)
            {
                game.AddObject(new Worker(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.Eighty + x + NumberManager.ZeroPointFiveF, NumberManager.TwentyOne + NumberManager.ZeroPointFiveF),
                    null, NumberManager.Three));
            }
        }
        private static void SetSpyPlayerTwo(GameMap game)
        {
            for (uint x = 0; x < NumberManager.Four; x++)
            {
                game.AddObject(new Spy(NumberManager.Two, IdGenerator.NewId, new Vector2(NumberManager.Eighty + x + NumberManager.ZeroPointFiveF, NumberManager.TwentyFive + NumberManager.ZeroPointFiveF),
                    null, 1));
            }
        }
        private static void SetTransportShipPlayerTwo(GameMap game)
        {
            for (uint x = 0; x < NumberManager.Three; x += NumberManager.Two)
            {
                for (uint y = 0; y < NumberManager.Five; y += NumberManager.Two)
                {
                    var transportShip1 = new TransportShip()
                    {
                        PlayerNumber = NumberManager.Two,
                        Id = IdGenerator.NewId,

                        X = NumberManager.FiftyFive + NumberManager.Two + y + NumberManager.ZeroPointFiveF,
                        Y = NumberManager.FortyTwo + x + NumberManager.ZeroPointFiveF

                    };
                    game.AddObject(transportShip1);
                }
            }
        }

        private static void SetBuildingsPlayerOne1(GameMap game)
        {
            var main1 = new MainBuilding()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.TwentyThree, NumberManager.SeventyTwo),
                Id = IdGenerator.NewId
            };
            game.AddObject(main1);
            var scoutShip1 = new ScoutShip()
            {
                PlayerNumber = 1,
                Id = IdGenerator.NewId,

                X = NumberManager.FortyTwo + NumberManager.ZeroPointFiveF,
                Y = NumberManager.FiftyFive + NumberManager.ZeroPointFiveF

            };
            game.AddObject(scoutShip1);
            var barracks1 = new Barracks()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.TwentyEight, NumberManager.SeventyNine),
                Id = IdGenerator.NewId
            };
            game.AddObject(barracks1);
            var ironForge1 = new IronForge()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtyFive, NumberManager.SeventyTwo),
                Id = IdGenerator.NewId
            };
            game.AddObject(ironForge1);
            var sawmill1 = new Sawmill()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtyFive, NumberManager.SeventySix),
                Id = IdGenerator.NewId
            };
            game.AddObject(sawmill1);
            var stoneProcessing1 = new StoneProcessing()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtyFive, NumberManager.Eighty),
                Id = IdGenerator.NewId
            };
            game.AddObject(stoneProcessing1);
            var warehouse1 = new Warehouse()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtyOne, NumberManager.SixtyOne),
                Id = IdGenerator.NewId
            };
            game.AddObject(warehouse1);
            var forestersLodge1 = new ForestersLodge()
            {
                PlayerNumber = 1,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.TwentyFour, NumberManager.SixtyFour)
            };
            game.AddObject(forestersLodge1);
            var house11 = new House()
            {
                PlayerNumber = 1,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.TwentyNine, NumberManager.SeventyOne)
            };
            game.AddObject(house11);
            var house12 = new House()
            {
                PlayerNumber = 1,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.TwentyNine, NumberManager.SeventyThree)
            };
            game.AddObject(house12);
            SetBuildingsPlayerOne2(game);
        }
        private static void SetBuildingsPlayerOne2(GameMap game)
        {
            var house13 = new House()
            {
                PlayerNumber = 1,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.TwentyNine, NumberManager.SeventyFive)
            }; 
            game.AddObject(house13);
            var worker1 = new WorkerTrainingCenter()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.TwentyEight, NumberManager.EightyThree),
                Id = IdGenerator.NewId
            };
            game.AddObject(worker1);
            var stoneMine1 = new StoneMine()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtyOne, NumberManager.FiftySix),
                Id = IdGenerator.NewId
            };
            game.AddObject(stoneMine1);
            var ironMine = new IronMine()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.ThirtySix, NumberManager.SixtyOne),
                Id = IdGenerator.NewId
            };
            game.AddObject(ironMine);
            var shipyard1 = new Shipyard()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.Forty, NumberManager.SixtyFour),
                Id = IdGenerator.NewId
            };
            game.AddObject(shipyard1);
            var tower1 = new Tower()
            {
                PlayerNumber = 1,
                Location = new Point(NumberManager.TwentyFive, NumberManager.FiftySeven),
                Id = IdGenerator.NewId
            };
            game.AddObject(tower1);

        }
        private static void SetBuildingsPlayerTwo1(GameMap game){
            var main2 = new MainBuilding()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SeventyTwo, NumberManager.TwentyThree),
                Id = IdGenerator.NewId
            };
            game.AddObject(main2);
            var scoutShip2 = new ScoutShip()
            {
                PlayerNumber = NumberManager.Two,
                Id = IdGenerator.NewId,

                X = NumberManager.FiftyFive + NumberManager.ZeroPointFiveF,
                Y = NumberManager.FortyTwo + NumberManager.ZeroPointFiveF
            };
            game.AddObject(scoutShip2);
            var barracks2 = new Barracks()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SeventyNine, NumberManager.TwentyEight),
                Id = IdGenerator.NewId
            };
            game.AddObject(barracks2);
            var house22 = new House()
            {
                PlayerNumber = NumberManager.Two,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.SeventyOne, NumberManager.ThirtyOne)
            };
            game.AddObject(house22);
            var house32 = new House()
            {
                PlayerNumber = NumberManager.Two,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.SeventyThree, NumberManager.ThirtyOne)
            };
            game.AddObject(house32);
            var house42 = new House()
            {
                PlayerNumber = NumberManager.Two,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.SeventyFive, NumberManager.ThirtyOne)
            };
            game.AddObject(house42);
            var worker2 = new WorkerTrainingCenter()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.EightyThree, NumberManager.TwentyEight),
                Id = IdGenerator.NewId
            };
            game.AddObject(worker2);
            var shipyard2 = new Shipyard()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SixtyFour, NumberManager.Forty),
                Id = IdGenerator.NewId
            };
            game.AddObject(shipyard2);
            var tower2 = new Tower()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.FiftySeven, NumberManager.TwentyFive),
                Id = IdGenerator.NewId
            };
            game.AddObject(tower2);
            var ironForge2 = new IronForge()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SeventyTwo, NumberManager.ThirtyFive),
                Id = IdGenerator.NewId
            };
            game.AddObject(ironForge2);
            var sawmill2 = new Sawmill()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SeventySix, NumberManager.ThirtyFive),
                Id = IdGenerator.NewId
            };
            game.AddObject(sawmill2);
            SetBuildingsPlayerTwo2(game);
        }
        private static void SetBuildingsPlayerTwo2(GameMap game)
        {
            var stoneProcessing2 = new StoneProcessing()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.Eighty, NumberManager.ThirtyFive),
                Id = IdGenerator.NewId
            };
            game.AddObject(stoneProcessing2);
            var warehouse2 = new Warehouse()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SixtyOne, NumberManager.ThirtyOne),
                Id = IdGenerator.NewId
            };
            game.AddObject(warehouse2);
            var forestersLodge2 = new ForestersLodge()
            {
                PlayerNumber = NumberManager.Two,
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.SixtyFour, NumberManager.TwentyFour)
            };
            game.AddObject(forestersLodge2);
            var stoneMine2 = new StoneMine()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.FiftySix, NumberManager.ThirtyOne),
                Id = IdGenerator.NewId
            };
            game.AddObject(stoneMine2);
            var ironMine2 = new IronMine()
            {
                PlayerNumber = NumberManager.Two,
                Location = new Point(NumberManager.SixtyOne, NumberManager.ThirtySix),
                Id = IdGenerator.NewId
            };
            game.AddObject(ironMine2);
        }

        private static GameMap GetTechDemo()
        {
            var res = new GameMap(new Size(NumberManager.TechDemoSize, NumberManager.TechDemoSize));

            // Terrain
            for (uint x = 0; x < NumberManager.TechDemoSize; x++)
            {
                for (uint y = 0; y < NumberManager.TechDemoSize; y++)
                {
                    if(!CheckBridgeSet1(res,x,y) && !CheckWaterSet1(res,x,y) && !SetMountainSet(res,x,y))
                    { 
                        res.SetTerrain(x, y, Terrain.Grass);
                    }
                }
            }
            // Bridge in middle
            ChangeWater(res);
            var ore1 = new IronDeposit()
            {
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.Three, NumberManager.Fifty)
            };
            res.AddObject(ore1);
            var ore2 = new IronDeposit()
            {
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.Fifty, NumberManager.Three)
            };
            res.AddObject(ore2);
            var ore3 = new IronDeposit()
            {
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.ThirtySix, NumberManager.SixtyOne)
            };
            res.AddObject(ore3);
            var ore4 = new IronDeposit()
            {
                Id = IdGenerator.NewId,
                Location = new Point(NumberManager.SixtyOne, NumberManager.ThirtySix)
            };
            res.AddObject(ore4);

            // Player 1
            // Shieldmen
            SetShieldmanPlayerOne(res);
            // Spearmen
            SetSpearmanPlayerOne(res);
            // Swordsmen
            SetSwordmanPlayerOne(res);
            //Worker
            SetWorkerPlayerOen(res);
            // Spy
            SetSpyPlayerOne(res);
            //Transport Ships
            SetTransportShipPlayerOen(res);
            // Rest
            SetBuildingsPlayerOne1(res);
            // Player 2
            // Shieldmen
            SetShieldmanPlayerTwo(res);
            // Spearmen
            SetSpearmanPlayerTwo(res);
            // Swordsmen
            SetSwordmanPlayerTwo(res);
            // Worker
            SetWorkerPlayerTwo(res);
            // Spy
            SetSpyPlayerTwo(res);
            //Transport Ships
            SetTransportShipPlayerTwo(res);
            // Rest
            SetBuildingsPlayerTwo1(res);
            return res;
        }

        private static void SetTerrainCorrect(GameMap game, int seed, int x, int y)
        {

            var perlinX = x / NumberManager.ThirtyTwoF;
            var perlinY = y / NumberManager.ThirtyTwoF;
            var height = Perlin(perlinX, perlinY, seed);
            var terrain = Terrain.Mountains;
            if (height < NumberManager.ZeroPointOne)
            {
                terrain = Terrain.Water;

            }
            else
            {
                if (height < NumberManager.ZeroPointFour)
                {
                    terrain = Terrain.Grass;
                }
            }
            game.SetTerrain((uint)x, (uint)y, terrain);
        }
        private static void GenerateTiles(GameMap gameMap,int seed=-1)
        {
            seed = seed == -1 ? new Random().Next() : seed;
            for (var x = 0; x < gameMap.GetSize().Width; x++)
            {
                for (var y = 0; y < gameMap.GetSize().Height; y++)
                {
                    SetTerrainCorrect(gameMap, seed,x,y);
                }
            }
        }
        private static float Interpolate(float a0, float a1, float x)
        {
            if (x < 0.0)
            {
                return a0;
            }

            if (x > 1.0)
            {
                return a1;
            }

            return (a1 - a0) * ((x * (x * NumberManager.SixPointZero - NumberManager.FifteenPointZero) + NumberManager.TenPointZero) * x * x * x) + a0; // Alternative:  Interpolation mit dem Polynom 6 * x^5 - 15 * x^4 + 10 * x^3
        }


        private static Vector2 RandomGradient(int ix, int iy, int seed)
        {
            var random = new Random((ix+seed)*(seed+ix));
            var random2 = new Random((iy+seed)*(seed+iy));
            random = new Random(random.Next()+random2.Next()+ NumberManager.Eleven *seed+iy+ix);
            var r = NumberManager.Two *3.14159265 * random.NextDouble();
            var v=new Vector2((float)Math.Sin(r),(float)Math.Cos(r));
            return v;
        }

        private static float DotGridGradient(int ix, int iy, float x, float y,int seed)
        {
            var gradient = RandomGradient(ix, iy,seed);
            var dx = x - ix;
            var dy = y - iy;
            return dx * gradient.X + dy * gradient.Y;
        }
        private static float Perlin(float x, float y, int seed)
        {
            var x0 = (int) x;
            var x1 = x0 + 1;
            var y0 = (int) y;
            var y1 = y0 + 1;
            var sx = x - x0;
            var sy = y - y0;
            var n0 = DotGridGradient(x0, y0, x, y,seed);
            var n1 = DotGridGradient(x1, y0, x, y,seed);
            var ix0 = Interpolate(n0, n1, sx);
            n0 = DotGridGradient(x0, y1, x, y,seed);
            n1 = DotGridGradient(x1, y1, x, y,seed);
            var ix1 = Interpolate(n0, n1, sx);
            return Interpolate(ix0, ix1, sy);
        }

        private static void PlaceOre(float chance, double check1, double check2, GameMap gameMap, GameObject ore)
        {

            if (chance > check1 && chance > check2)
            {
                gameMap.AddObject(ore);
            }
        }

        private static void PlaceTree(float chance, double check1, double check2, GameMap gameMap, GameObject tree)
        {

            if (chance > check1 && chance > check2)
            {
                gameMap.AddObject(tree);
            }
        }

    }
}