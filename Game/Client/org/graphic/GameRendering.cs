using System.Collections.Generic;
using System.Linq;
using Common.com.game;
using Common.com.game.settings;
using Common.com.objects;
using Common.com.objects.entities;
using Common.com.objects.entities.FightingUnit;
using Common.com.objects.immovables.Buildings;
using Common.com.objects.immovables.Resources;
using Game.org.gameStates;
using Game.org.main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Priority_Queue;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Game.org.graphic
{
    public sealed class GameRendering : IRendering
    {
        private readonly InGame mGame;
        public GameRendering(InGame game)
        {
            mGame = game;
        }


        public void Render(SpriteBatch spriteBatch)
        {
            spriteBatch.GraphicsDevice.Clear(Color.Black);
            RenderMap(spriteBatch);

            var map = mGame.RollbackManager.CurrentState.Map;
            var secondary = new HashSet<GameObject>();
            ChangeCamera(spriteBatch);
            RenderCorpses(spriteBatch,mGame.RollbackManager.CurrentState.Map);
            var cameraAreaRectangle = mGame.Camera.VisibleArea;
            var priorityQueue = new SimplePriorityQueue<GameObject>();
            for (var x = cameraAreaRectangle.Left; x < cameraAreaRectangle.Right; x++)
            {
                for (var y = cameraAreaRectangle.Top; y < cameraAreaRectangle.Bottom; y++)
                {
                    var ob = map.GetObject(x, y);
                    if (ob == null)
                    {
                        continue;
                    } 
                    if (ob is ObjectImmovable obIm)
                    {
                        if (x == obIm.Location.X && y == obIm.Location.Y)
                        {
                            if (GameMap.FogOfWarListener == null || GameMap.FogOfWarListener != null && obIm.PartiallyVisible(GameMap.FogOfWarListener, mGame.PlayerNumber))
                            {
                                if (!(mGame.SelectedGameObjectId.Contains(obIm.Id)))
                                {

                                    priorityQueue.Enqueue(obIm,x-y);
                                }
                                else
                                {
                                    priorityQueue.Enqueue(obIm,x+y+ NumberManager.OneThousand);
                                }
                            }
                        }
                    }
                    if (GameMap.FogOfWarListener != null)
                    {
                        if (GameMap.FogOfWarListener.FogOfWarTiles[x][y][mGame.PlayerNumber - 1] >= 1 ||
                            ob.PlayerNumber == mGame.PlayerNumber)
                        {

                            // Draws the correct building/resource
                            switch (ob)
                            {

                                case ObjectImmovable _:
                                    ob = map.GetSecondaryObject(x, y);
                                    if (ob != null)
                                    {
                                        secondary.Add(ob);

                                    }

                                    break;
                                // Draws the correct ObjectMoving
                                case ObjectMoving objectMoving:
                                    if (ob.PlayerNumber == mGame.PlayerNumber ||
                                        GameMap.FogOfWarListener.FogOfWarTiles[x][y][mGame.PlayerNumber - 1] >= NumberManager.Two)
                                    { 
                                        if (!(mGame.SelectedGameObjectId.Contains(objectMoving.Id)))
                                        {

                                            priorityQueue.Enqueue(objectMoving,x-y);
                                        }
                                        else
                                        {
                                            priorityQueue.Enqueue(objectMoving,x+y+ NumberManager.OneThousand);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

            }
            while (priorityQueue.Count>0)
            {
                var gameObject=priorityQueue.Dequeue();
                DrawObject(gameObject,spriteBatch, mGame.SelectedGameObjectId.Contains(gameObject.Id) ? Color.Red : Color.White, GetPlayerColor(gameObject));
            }
            foreach (var gameObject in secondary)
            {
                DrawObject(gameObject, spriteBatch, mGame.SelectedGameObjectId.Contains(gameObject.Id) ? Color.Red : Color.White * NumberManager.ZeroPointFiveF, GetPlayerColor(gameObject));
            }
            if (mGame.SelectedStartRoad != new System.Drawing.Point(-NumberManager.OneThousand, -NumberManager.OneThousand))
            {
                DrawRoadStart(spriteBatch, mGame.SelectedStartRoad.X, mGame.SelectedStartRoad.Y, mGame.RollbackManager.CurrentState.Map);
            }

            DrawMenus(spriteBatch);

        }

        private void DrawRoadStart(SpriteBatch spriteBatch, int x, int y, GameMap map)
        {
            var left = Vector2.Transform(new Vector2(x, y), mGame.Camera.Transform);
            var top = Vector2.Transform(new Vector2(x + 1, y), mGame.Camera.Transform);
            var bottom = Vector2.Transform(new Vector2(x, y + 1), mGame.Camera.Transform);
            var right = Vector2.Transform(new Vector2(x + 1, y + 1), mGame.Camera.Transform);
            var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
            var terrain = map.GetTerrainAt((uint) x, (uint) y);
            var grass = Game1.GetGame().ImageManager.Grass;
            var water = Game1.GetGame().ImageManager.Water;
            var mountains = Game1.GetGame().ImageManager.Mountains;
            var road = Game1.GetGame().ImageManager.GetImage("Road");
            var bridge = Game1.GetGame().ImageManager.GetImage("Bridge");
            switch (terrain)
            {
                case Terrain.Grass:
                    spriteBatch.Draw(grass, res2, Color.Purple * NumberManager.OnePointSix);
                    break;
                case Terrain.Mountains:
                    spriteBatch.Draw(mountains, res2, Color.Purple * NumberManager.OnePointSix);
                    break;
                case Terrain.Water:
                    spriteBatch.Draw(water, res2, Color.Purple * NumberManager.OnePointSix);
                    break;
                case Terrain.Road:
                    spriteBatch.Draw(road, res2, Color.Purple * NumberManager.OnePointSix);
                    break;
                case Terrain.Bridge:
                    spriteBatch.Draw(bridge, res2, Color.Purple * NumberManager.OnePointSix);
                    break;
            }
        }

        private void DrawTile(int x,int y,SpriteBatch spriteBatch,GameMap map)
        {
            if (mGame.RollbackManager.CurrentState.Map.InBounds(x, y))
            {
                if (GameMap.FogOfWarListener.FogOfWarTiles[x][y][mGame.PlayerNumber - 1] > 0)
                {
                    var left = Vector2.Transform(new Vector2(x, y), mGame.Camera.Transform);
                    var top = Vector2.Transform(new Vector2(x + 1, y), mGame.Camera.Transform);
                    var bottom = Vector2.Transform(new Vector2(x, y + 1), mGame.Camera.Transform);
                    var right = Vector2.Transform(new Vector2(x + 1, y + 1), mGame.Camera.Transform);
                    var grass = Game1.GetGame().ImageManager.Grass;
                    var water = Game1.GetGame().ImageManager.Water;
                    var mountains = Game1.GetGame().ImageManager.Mountains;
                    var road = Game1.GetGame().ImageManager.GetImage("Road");
                    var bridge = Game1.GetGame().ImageManager.GetImage("Bridge");
                    var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
                    var terrain = map.GetTerrainAt((uint) x, (uint) y);
                    if (GameMap.FogOfWarListener.FogOfWarTiles[x][y][mGame.PlayerNumber - 1] == 1)
                    {
                        switch (terrain)
                        {
                            case Terrain.Grass:
                                spriteBatch.Draw(grass, res2, Color.White * NumberManager.ZeroPointThree);
                                break;
                            case Terrain.Mountains:
                                spriteBatch.Draw(mountains, res2, Color.White * NumberManager.ZeroPointThree);
                                break;
                            case Terrain.Water:
                                spriteBatch.Draw(water, res2, Color.White * NumberManager.ZeroPointThree);
                                break;
                            case Terrain.Road:
                                spriteBatch.Draw(road, res2, Color.White * NumberManager.ZeroPointThree);
                                break;
                            case Terrain.Bridge:
                                spriteBatch.Draw(bridge, res2, Color.White * NumberManager.ZeroPointThree);
                                break;
                        }
                    }
                    else
                    {
                        switch (terrain)
                        {
                            case Terrain.Grass:
                                spriteBatch.Draw(grass, res2, Color.White);
                                break;
                            case Terrain.Mountains:
                                spriteBatch.Draw(mountains, res2, Color.White);
                                break;
                            case Terrain.Water:
                                spriteBatch.Draw(water, res2, Color.White);
                                break;
                            case Terrain.Road:
                                spriteBatch.Draw(road, res2, Color.White);
                                break;
                            case Terrain.Bridge:
                                spriteBatch.Draw(bridge, res2, Color.White);
                                break;
                        }
                    }
                }
            }
        }
        private void RenderMap(SpriteBatch spriteBatch)
        {
            var map = mGame.RollbackManager.CurrentState.Map;
            var cameraAreaRectangle = mGame.Camera.VisibleArea;
            var toDrawLater = new List<Point>();
            for (var x = cameraAreaRectangle.Left; x < cameraAreaRectangle.Right; x++)
            {
                for (var y = cameraAreaRectangle.Top; y < cameraAreaRectangle.Bottom; y++)
                {
                    var terrain = map.GetTerrainAt((uint) x, (uint) y);
                    if (terrain==Terrain.Bridge||terrain==Terrain.Road)
                    {
                        toDrawLater.Add(new Point(x,y));
                    }
                    else
                    {
                        DrawTile(x, y, spriteBatch, map);
                    }

                    DrawRoadScaffolding(mGame.RoadBuildingList, spriteBatch, Color.White);
                    DrawBridgeScaffolding(mGame.BridgeBuildingList, spriteBatch, Color.White);
                    
                    if (mGame.RollbackManager.CurrentState.Map.InBounds(x, y))
                    {
                        if (GameMap.FogOfWarListener.FogOfWarTiles[x][y][mGame.PlayerNumber - 1] == 0)
                        {
                            DrawCompleteFogOfWar(spriteBatch, x, y);
                        }
                    }
                }
            }

            foreach (var point in toDrawLater)
            {
                DrawTile(point.X,point.Y,spriteBatch,map);
            }
        }

        private void RenderCorpses(SpriteBatch spriteBatch, GameMap map)
        {
            var corpses = map.GetCorpses();
            if (corpses != null)
            {
                foreach (var corpse in corpses)
                {
                    var drawAt = Vector2.Transform(new Vector2(corpse.Location.X, corpse.Location.Y), mGame.Camera.Transform);
                    var res = new Rectangle((int)drawAt.X - (int)(NumberManager.SeventyFive * mGame.Camera.Zoom),
                        (int)drawAt.Y - (int)(NumberManager.OneHundredFive * mGame.Camera.Zoom),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom));
                    var corpseTexture = Game1.GetGame().ImageManager.GetImage("Corpse");
                    spriteBatch.Draw(corpseTexture, res, Color.White);
                }
            }
        }
        private static string GetObjectImmovable(ObjectImmovable objectImmovable)
        {
            return objectImmovable.GetType().Name;
        }

        private static string GetObjectMoving(ObjectMoving objectMoving)
        {
            return objectMoving.GetType().Name == "Shieldman" ? "Pikeman" : objectMoving.GetType().Name;
        }

        private string GetPlayerColor(GameObject gameObject)
        {
            return gameObject.PlayerNumber == 1 ? "blue" : "red";
        }

        private string GetAction(ObjectMoving objectMoving)
        {

            if (objectMoving.CurrentAction.FightingWithId >= 0&&objectMoving.OnLand)
            {
                return "attack";
            }

            if (objectMoving.PathDone())
            {
                return "idle";
            }

            if ((objectMoving.CurrentPath.WayPoints.First is { Next: { } } &&
                 objectMoving.CurrentPath.WayPoints.First.Value ==
                 objectMoving.CurrentPath.WayPoints.First.Next.Value))
            {
                return "wait";
            }

            return "walk";
        }

        private int GetAngle(ObjectMoving objectMoving, string action)
        {

            if (action == "walk")
            {
                return objectMoving.CurrentPath.WayPoints.First switch
                {
                    { Next: { } } when objectMoving.CurrentPath.WayPoints.First.Value.X <
                                     objectMoving.CurrentPath.WayPoints.First.Next.Value.X => NumberManager.OneHundredThirtyFive,
                    { Next: { } } when objectMoving.CurrentPath.WayPoints.First.Value.X >
                                     objectMoving.CurrentPath.WayPoints.First.Next.Value.X => NumberManager.ThreeHundredFifteen,
                    { Next: { } } when objectMoving.CurrentPath.WayPoints.First.Value.Y <
                                     objectMoving.CurrentPath.WayPoints.First.Next.Value.Y => NumberManager.FortyFive,
                    { Next: { } } when objectMoving.CurrentPath.WayPoints.First.Value.Y >
                                     objectMoving.CurrentPath.WayPoints.First.Next.Value.Y => NumberManager.TwoHundredTwentyFive,
                    _ => NumberManager.Ninety
                };
            }

            if (action == "attack")
            {
                var enemyEntity = mGame.RollbackManager.CurrentState.Map.GetObject(objectMoving.CurrentAction
                    .FightingWithId);
                var opponent = new Vector2();
                if (enemyEntity is ObjectMoving enemyMoving)
                {
                    opponent = enemyMoving.Position;
                }
                if (enemyEntity is ObjectImmovable enemyImmovable)
                {
                    opponent = new Vector2(enemyImmovable.Location.X, enemyImmovable.Location.Y);
                }

                if (opponent.X > objectMoving.Position.X)
                {
                    var yValue = opponent.Y - objectMoving.Position.Y;
                    if (yValue <= -1)
                    {
                        return NumberManager.OneHundredEighty;
                    }
                    if (yValue >= 1)
                    {
                        return NumberManager.Ninety;
                    }
                    return NumberManager.OneHundredThirtyFive;
                }
                if (opponent.X < objectMoving.Position.X)
                {
                    var yValue = opponent.Y - objectMoving.Position.Y;
                    if (yValue <= -1)
                    {
                        return NumberManager.TwoHundredSeventy;
                    }
                    if (yValue >= 1)
                    {
                        return 0;
                    }
                    return NumberManager.ThreeHundredFifteen;
                }
                var ydValue = opponent.Y - objectMoving.Position.Y;
                if (ydValue <= -1)
                {
                    return NumberManager.TwoHundredTwentyFive;
                }

                if (ydValue >= 1)
                {
                    return NumberManager.FortyFive;
                }
                return 0;
            }

            return 0;
        }

        private int GetAnimationFrame(string action, GameObject gameObject)
        {
            if (action == "death" || action == "wait" || gameObject is ScoutShip || gameObject is TransportShip)
            {
                return 0;
            }

            if (!mGame.GameObjectIdAnimationFrame.ContainsKey(gameObject.Id))
            {
                mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
            }

            return mGame.GameObjectIdAnimationFrame[gameObject.Id];

        }

        private int SetAnimationFrame(string action, GameObject gameObject, int animationFrame)
        {
            switch (action)
            {
                case "idle" when (!(gameObject is ScoutShip) && !(gameObject is TransportShip)):
                    return SetEntityAnimationFrame(gameObject, animationFrame);
                case "walk" when animationFrame > NumberManager.Sixteen && (!(gameObject is ScoutShip) && !(gameObject is TransportShip)):
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                case "attack" when animationFrame > NumberManager.TwelveF:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                default:
                    return animationFrame;
            }
        }

        private int SetEntityAnimationFrame(GameObject gameObject, int animationFrame)
        {
            switch (gameObject)
            {
                case Worker _ when animationFrame > NumberManager.Sixteen:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                case Spy _ when animationFrame > NumberManager.ThirtyEight:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                case Shieldman _ when animationFrame > NumberManager.Eight:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                case Spearman _ when animationFrame > NumberManager.Sixteen:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                case Swordsman _ when animationFrame > NumberManager.TwentyFour:
                    mGame.GameObjectIdAnimationFrame[gameObject.Id] = 1;
                    return 1;
                default:
                    return animationFrame;
            }
        }

        private void ChangeCamera(SpriteBatch spriteBatch)
        {
            if (mGame.SelectedStart.X <= -NumberManager.NineHundredNinetyNine)
            {
                return;
            }

            var inverseViewMatrix = Matrix.Invert(mGame.Camera.Transform);
            var firstPos = Vector2.Transform(
                new Vector2(mGame.SelectedStart.X + NumberManager.ZeroPointFiveF, mGame.SelectedStart.Y + NumberManager.ZeroPointFiveF),
                mGame.Camera.Transform);
            var gamePos = Vector2.Transform(new Vector2(mGame.LastState.Position.X, mGame.LastState.Position.Y),
                inverseViewMatrix);
            gamePos.Floor();

            var secondPos = Vector2.Transform(new Vector2(gamePos.X + NumberManager.ZeroPointFiveF, gamePos.Y + NumberManager.ZeroPointFiveF),
                mGame.Camera.Transform);

            var rect = new Texture2D(spriteBatch.GraphicsDevice, NumberManager.Fifty, NumberManager.Fifty);
            var data = new Color[NumberManager.Fifty * NumberManager.Fifty];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = Color.Blue;
            }

            var res = Camera.GetSurroundingRectangle(firstPos, secondPos, firstPos, secondPos);
            rect.SetData(data);
            spriteBatch.Draw(rect, res, Color.White * (float) NumberManager.ZeroPointTwo);
        }

        private void DrawMenus(SpriteBatch spriteBatch)
        {
            foreach (var menu in mGame.ActiveMenus.ToArray())
            {
                if (menu.ToRemove())
                {
                    mGame.ActiveMenus.Remove(menu);
                }
                else
                {
                    menu.Draw(spriteBatch, mGame.LastState);
                }
            }
        }

        private static int GetAttackType(string action, int attackType, int animationFrame)
        {
            if (action != "attack")
            {
                return 0;
            }

            return attackType switch
            {
                0 => 1,
                1 when animationFrame == NumberManager.Twelve => NumberManager.Two,
                NumberManager.Two when animationFrame == NumberManager.Twelve => 1,
                _ => 0
            };
        }

        private string GetResource(Worker worker)
        {
            return worker.HoldingItem.ToString();
        }

        private void DrawObject(GameObject gameObject, SpriteBatch spriteBatch, Color color, string playerColor)
        {
            if (gameObject is ObjectImmovable objectImmovable)
            {
                DrawObjectImmovable(objectImmovable, spriteBatch, color);
            }
            if (gameObject is ObjectMoving objectMoving)
            {
                DrawObjectMoving(objectMoving, spriteBatch, color, playerColor);
            }
        }

        private Rectangle ModifyDrawingRectangleScaffholding(Rectangle drawTo, Scaffolding scaffolding)
        {
            if (scaffolding.TurnsInto is House)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour );
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThree);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFive );
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwoFive);
            }
            else if (scaffolding.TurnsInto is Sawmill || scaffolding.TurnsInto is StoneProcessing || scaffolding.TurnsInto is IronForge)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFiveFiveF);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (scaffolding.TurnsInto is ForestersLodge)
            {
                drawTo.Height = drawTo.Height * NumberManager.Two;
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = drawTo.Width * NumberManager.Two;
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointThree);
            }
            else if (scaffolding.TurnsInto is Tower)
            {
                drawTo.Height = drawTo.Height * NumberManager.Two;
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointTwo);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOne);
            }
            else if (scaffolding.TurnsInto is Shipyard)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour );
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwoFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThree);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOneFive);
            }
            else if (scaffolding.TurnsInto is Warehouse)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFiveFiveF);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThree);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOneFive);
            }
            else if (scaffolding.TurnsInto is StoneMine)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointSix);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThreeFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFourFiveF);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (scaffolding.TurnsInto is Barracks)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFive);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThree);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThreeFive);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointSevenFive);
            }
            else if (scaffolding.TurnsInto is WorkerTrainingCenter)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwoFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (scaffolding.TurnsInto is IronMine)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointThree);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwo);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }

            return drawTo;
        }
        private Rectangle ModifyDrawingRectangle(Rectangle drawTo, GameObject objectImmovable)
        {
            if (objectImmovable is Scaffolding scaffolding)
            {
                return ModifyDrawingRectangleScaffholding(drawTo, scaffolding);
            }
            if (objectImmovable is MainBuilding)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointSix );
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointOne);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointZeroFive);
            }
            else if (objectImmovable is House)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour );
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThreeFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFiveFiveF);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwoFive);
            }
            else if (objectImmovable is Sawmill || objectImmovable is StoneProcessing || objectImmovable is IronForge)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFiveFiveF);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (objectImmovable is Tree)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointEight);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFive );
                drawTo.X -= (int)(drawTo.Width *NumberManager.ZeroPointTwoFive);
            }
            else if (objectImmovable is ForestersLodge)
            {
                drawTo.Height = drawTo.Height * NumberManager.Two;
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = drawTo.Width * NumberManager.Two;
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointThree);
            }
            else if (objectImmovable is Tower tower)
            {
                drawTo.Height = drawTo.Height * NumberManager.Two;
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointTwo);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOne);
                if (mGame.PlayerNumber == objectImmovable.PlayerNumber && !(mGame.SelectedGameObjectId.Contains(objectImmovable.Id)))
                {
                    mGame.AddIfNewName(mGame.mMenuGenerator.ShowTowerInside(Game1.GetGame().ImageManager, mGame, objectImmovable, false));
                }
                else
                {
                    mGame.RemoveMenus(objectImmovable.Id.ToString());
                }
                if (mGame.PlayerNumber != tower.PlayerNumber && tower.SpyInside.Count != 0)
                {
                    mGame.AddIfNewName(mGame.mMenuGenerator.GetInfiltratedTowerMenu(Game1.GetGame().ImageManager, mGame, tower.Id, tower));
                }
                else
                {
                    mGame.RemoveMenus("infiltrate" + tower.Id);
                }
            }
            else if (objectImmovable is Shipyard)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour );
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwoFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThree);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOneFive);
            }
            else if (objectImmovable is Warehouse)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFiveFiveF);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointFour);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThree);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOneFive);
            }
            else if (objectImmovable is StoneMine)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointSix);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThreeFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFourFiveF);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (objectImmovable is Barracks)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFive);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointThree);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointThreeFive);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointSevenFive);
            }
            else if (objectImmovable is WorkerTrainingCenter)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointFour);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwoFive);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }
            else if (objectImmovable is IronDeposit)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointThree);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointOne);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointTwo);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointOne);
            }
            else if (objectImmovable is IronMine)
            {
                drawTo.Height = (int)(drawTo.Height * NumberManager.OnePointThree);
                drawTo.Y -= (int)(drawTo.Height * NumberManager.ZeroPointTwo);
                drawTo.Width = (int)(drawTo.Width * NumberManager.OnePointFour);
                drawTo.X -= (int)(drawTo.Width * NumberManager.ZeroPointTwo);
            }

            return drawTo;
        }

        private void DrawObjectImmovable(ObjectImmovable objectImmovable, SpriteBatch spriteBatch, Color color)
        {
            var inGameRect = objectImmovable.GetBounds();
            var drawTo = mGame.Camera.GetDrawRect(inGameRect);
            drawTo.X += (int) (NumberManager.Fifty * mGame.Camera.Zoom);
            drawTo.Y -= (int) (NumberManager.Ten * mGame.Camera.Zoom);
            drawTo.Width -= (int) (NumberManager.OneHundred * mGame.Camera.Zoom);

            drawTo = ModifyDrawingRectangle(drawTo, objectImmovable);

            var action = "";
            var attackType = 0;
            var angle = 0;
            var animationFrame = 0;
            string immovable;
            if (mGame.BuildingSelected != null && !(mGame.BuildingSelected is Scaffolding))
            {
                ObjectImmovable buildingSelected = mGame.BuildingSelected;
                var inverseViewMatrix = Matrix.Invert(mGame.Camera.Transform);
                var gamePos = Vector2.Transform(new Vector2(mGame.LastState.Position.X, mGame.LastState.Position.Y),
                    inverseViewMatrix);
                buildingSelected.Location = new System.Drawing.Point((int) gamePos.X, (int) gamePos.Y);

                var inGameRectMouse = buildingSelected.GetBounds();
                var drawToMouse = mGame.Camera.GetDrawRect(inGameRectMouse);
                drawToMouse.X += (int) (NumberManager.Fifty * mGame.Camera.Zoom);
                drawToMouse.Y -= (int) (NumberManager.Ten * mGame.Camera.Zoom);
                drawToMouse.Width -= (int) (NumberManager.OneHundred * mGame.Camera.Zoom);

                immovable = GetObjectImmovable(mGame.BuildingSelected);
                var texture = Game1.GetGame().ImageManager
                    .GetImage(immovable, action, attackType, angle, animationFrame);

                drawToMouse = ModifyDrawingRectangle(drawToMouse, mGame.BuildingSelected);
                spriteBatch.Draw(texture, drawToMouse,
                    mGame.RollbackManager.CurrentState.Map.CanBePut(buildingSelected, mGame.PlayerNumber)
                        ? Color.Green
                        : Color.Red);
            }

            if (objectImmovable is Scaffolding scaffolding)
            {
                if (scaffolding.IsRoad)
                {
                    immovable = "Road";
                }
                else if (scaffolding.IsBridge)
                {
                    immovable = "Bridge";
                }
                else
                {
                    immovable = GetObjectImmovable(scaffolding.TurnsInto);
                }
            }
            else
            {
                immovable = GetObjectImmovable(objectImmovable);
            }

            if (immovable != "Road" && immovable != "Bridge" && immovable != "Corpse")
            {
                var texture = Game1.GetGame().ImageManager
                    .GetImage(immovable, action, attackType, angle, animationFrame);
                spriteBatch.Draw(texture, drawTo, color * (float) (objectImmovable is Scaffolding ? NumberManager.ZeroPointFive : 1f));
                if (!mGame.SelectedGameObjectId.Contains(objectImmovable.Id) || (objectImmovable is Scaffolding))
                {
                    return;
                }
                var maxHp = GetMaxHp(objectImmovable);
                var currentHp = (double) GetCurrentHp(objectImmovable);
                var redColor = new Texture2D(mGame.mGraphicsDeviceManager.GraphicsDevice, 1, 1);
                redColor.SetData(new[] { Color.Red });
                var greenColor = new Texture2D(mGame.mGraphicsDeviceManager.GraphicsDevice, 1, 1);
                greenColor.SetData(new[] { Color.Green });
                spriteBatch.Draw(redColor,
                    new Rectangle(NumberManager.OneHundred,
                        NumberManager.Five,
                        NumberManager.FourHundred,
                        NumberManager.Ten),
                    Color.White);
                spriteBatch.Draw(greenColor,
                    new Rectangle(NumberManager.OneHundred,
                        NumberManager.Five,
                        (int) (NumberManager.FourHundred * (currentHp/maxHp)),
                        NumberManager.Ten),
                    Color.White);
            }
            else
            {
                if (immovable == "Corpse")
                {
                    DrawCorpse(objectImmovable, spriteBatch);
                }
                else if (immovable == "Road")
                {
                    DrawSingleRoadScaffolding(objectImmovable, spriteBatch, Color.White);
                }
                else
                {
                    DrawSingleBridgeScaffolding(objectImmovable, spriteBatch, Color.White);
                }
            }

        }

        private int GetCurrentHp(ObjectImmovable objectImmovable)
        {
            switch (objectImmovable)
            {
                case House ob:
                    return ob.CurrentHp;
                case Barracks ob:
                    return ob.CurrentHp;
                case ForestersLodge ob:
                    return ob.CurrentHp;
                case IronForge ob:
                    return ob.CurrentHp;
                case IronMine ob:
                    return ob.CurrentHp;
                case MainBuilding ob:
                    return ob.CurrentHp;
                case Shipyard ob:
                    return ob.CurrentHp;
                case Sawmill ob:
                    return ob.CurrentHp;
                case StoneMine ob:
                    return ob.CurrentHp;
                case StoneProcessing ob:
                    return ob.CurrentHp;
                case Tower ob:
                    return ob.CurrentHp;
                case Warehouse ob:
                    return ob.CurrentHp;
                case WorkerTrainingCenter ob:
                    return ob.CurrentHp;
            }
            return 0;
        }

        private int GetMaxHp(ObjectImmovable ob)
        {
            if (ob is ResourceBuilding ru)
            {
                return ru.MaxHp;
            }
            if (ob is Building bu)
            {
                return bu.MaxHp;
            }

            return 0;
        }
        private void DrawObjectMoving(ObjectMoving objectMoving,
            SpriteBatch spriteBatch,
            Color color,
            string playerColor)
        {
            var attackType = 0;
            var drawAt = Vector2.Transform(objectMoving.Position, mGame.Camera.Transform);
            var action = GetAction(objectMoving);
            var angle = GetAngle(objectMoving, action);
            var animationFrame = GetAnimationFrame(action, objectMoving);
            animationFrame = SetAnimationFrame(action, objectMoving, animationFrame);
            attackType = GetAttackType(action, attackType, animationFrame);


            var entity = GetObjectMoving(objectMoving);
            
            var texture = Game1.GetGame().ImageManager
                .GetImage(entity, action, attackType, angle, animationFrame, playerColor);
            if (entity == "TransportShip")
            {
                var rColor = CheckTile(objectMoving);
                spriteBatch.Draw(texture,
                    new Rectangle((int) drawAt.X - (int) (NumberManager.FortyFive * mGame.Camera.Zoom),
                        (int) drawAt.Y - (int) (NumberManager.Fifty * mGame.Camera.Zoom),
                        (int) (NumberManager.Seventy * mGame.Camera.Zoom),
                        (int) (NumberManager.Seventy * mGame.Camera.Zoom)),
                    rColor);
                if (mGame.PlayerNumber == objectMoving.PlayerNumber && !(mGame.SelectedGameObjectId.Contains(objectMoving.Id)))
                {
                    mGame.AddIfNewName(
                        mGame.mMenuGenerator.ShowShipInside(Game1.GetGame().ImageManager, mGame, objectMoving, false));
                }
                else
                {
                    mGame.RemoveMenus(objectMoving.Id.ToString());
                }
            }
            else if (entity == "ScoutShip")
            {
                var rColor = CheckTile(objectMoving);
                spriteBatch.Draw(texture,
                    new Rectangle((int)drawAt.X - (int)(NumberManager.FortyThree * mGame.Camera.Zoom),
                        (int)drawAt.Y - (int)(NumberManager.FiftyFive * mGame.Camera.Zoom),
                        (int)(NumberManager.Seventy * mGame.Camera.Zoom),
                        (int)(NumberManager.Seventy * mGame.Camera.Zoom)),
                    rColor);
            }
            else
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)drawAt.X - (int)(NumberManager.SeventyFive * mGame.Camera.Zoom),
                        (int)drawAt.Y - (int)(NumberManager.OneHundredFive * mGame.Camera.Zoom),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom)),
                    color);
            }

            if (objectMoving is Worker worker)
            {
                var resource = GetResource(worker);
                if (resource != "Nothing")
                {
                    var resourceTexture = Game1.GetGame().ImageManager.GetImage(resource);
                    spriteBatch.Draw(resourceTexture,
                        new Rectangle((int)(drawAt.X) - (int)(NumberManager.SeventyFive * mGame.Camera.Zoom / NumberManager.Eight),
                            (int)(drawAt.Y) - (int)(NumberManager.OneHundredFive * mGame.Camera.Zoom / NumberManager.OnePointFour),
                            (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom / NumberManager.Eight),
                            (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom / NumberManager.Eight)),
                        Color.White);
                }
            }

            if (!mGame.SelectedGameObjectId.Contains(objectMoving.Id))
            {
                return;
            }

            var redColor = new Texture2D(mGame.mGraphicsDeviceManager.GraphicsDevice, 1, 1);
            redColor.SetData(new[] { Color.Red });
            var greenColor = new Texture2D(mGame.mGraphicsDeviceManager.GraphicsDevice, 1, 1);
            greenColor.SetData(new[] { Color.Green });
            var proponent = (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom / NumberManager.Eight + NumberManager.SeventyFive) / objectMoving.GetMaxHp(objectMoving);
            spriteBatch.Draw(redColor,
                    new Rectangle((int)(drawAt.X) - NumberManager.TwentyFive,
                        (int)(drawAt.Y) + NumberManager.TwentyFive,
                        (proponent * objectMoving.GetMaxHp(objectMoving)),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom / NumberManager.Eight) / NumberManager.Two),
                    Color.White);
            spriteBatch.Draw(greenColor,
                    new Rectangle((int)(drawAt.X) - NumberManager.TwentyFive,
                        (int)(drawAt.Y) + NumberManager.TwentyFive,
                        (int)(proponent * objectMoving.CurrentHp),
                        (int)(NumberManager.OneHundredFiftyFive * mGame.Camera.Zoom / NumberManager.Eight) / NumberManager.Two),
                    Color.White);
        }

        private Color CheckTile(ObjectMoving ob)
        {

            var tile = mGame.RollbackManager.CurrentState.Map.GetTerrainAt((uint) ob.X, (uint) ob.Y);
            if (tile is Terrain.Bridge)
            {
                return mGame.SelectedGameObjectId.Contains(ob.Id) ? Color.Red : Color.White * (float) NumberManager.OnePointFive;
            }

            return mGame.SelectedGameObjectId.Contains(ob.Id) ? Color.Red : Color.White;
        }

        private void DrawSingleRoadScaffolding(ObjectImmovable ob, SpriteBatch spriteBatch, Color color)
        {
            var texture = Game1.GetGame().ImageManager.GetImage("Road");
            var left = Vector2.Transform(new Vector2(ob.Location.X, ob.Location.Y), mGame.Camera.Transform);
            var top = Vector2.Transform(new Vector2(ob.Location.X + 1, ob.Location.Y), mGame.Camera.Transform);
            var bottom = Vector2.Transform(new Vector2(ob.Location.X, ob.Location.Y + 1), mGame.Camera.Transform);
            var right = Vector2.Transform(new Vector2(ob.Location.X + 1, ob.Location.Y + 1), mGame.Camera.Transform);
            var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
            spriteBatch.Draw(texture, res2, color * NumberManager.ZeroPointFiveF);
        }

        private void DrawSingleBridgeScaffolding(ObjectImmovable ob, SpriteBatch spriteBatch, Color color)
        {
            var texture = Game1.GetGame().ImageManager.GetImage("Bridge");
            var left = Vector2.Transform(new Vector2(ob.Location.X, ob.Location.Y), mGame.Camera.Transform);
            var top = Vector2.Transform(new Vector2(ob.Location.X + 1, ob.Location.Y), mGame.Camera.Transform);
            var bottom = Vector2.Transform(new Vector2(ob.Location.X, ob.Location.Y + 1), mGame.Camera.Transform);
            var right = Vector2.Transform(new Vector2(ob.Location.X + 1, ob.Location.Y + 1), mGame.Camera.Transform);
            var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
            spriteBatch.Draw(texture, res2, color * NumberManager.ZeroPointFiveF);
        }

        private void DrawRoadScaffolding(List<Scaffolding> roadList, SpriteBatch spriteBatch, Color color)
        {
            var texture = Game1.GetGame().ImageManager.GetImage("Road");
            if (roadList != null)
            {
                foreach (var road in roadList)
                {
                    var left = Vector2.Transform(new Vector2(road.Location.X, road.Location.Y), mGame.Camera.Transform);
                    var top = Vector2.Transform(new Vector2(road.Location.X + 1, road.Location.Y), mGame.Camera.Transform);
                    var bottom = Vector2.Transform(new Vector2(road.Location.X, road.Location.Y + 1), mGame.Camera.Transform);
                    var right = Vector2.Transform(new Vector2(road.Location.X + 1, road.Location.Y + 1), mGame.Camera.Transform);
                    var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
                    spriteBatch.Draw(texture, res2, color * NumberManager.ZeroPointFiveF);
                }
            }
        }
        private void DrawBridgeScaffolding(List<Scaffolding> bridgeList, SpriteBatch spriteBatch, Color color)
        {
            var texture = Game1.GetGame().ImageManager.GetImage("Bridge");
            if (bridgeList != null)
            {
                foreach (var road in bridgeList)
                {
                    var left = Vector2.Transform(new Vector2(road.Location.X, road.Location.Y), mGame.Camera.Transform);
                    var top = Vector2.Transform(new Vector2(road.Location.X + 1, road.Location.Y), mGame.Camera.Transform);
                    var bottom = Vector2.Transform(new Vector2(road.Location.X, road.Location.Y + 1), mGame.Camera.Transform);
                    var right = Vector2.Transform(new Vector2(road.Location.X + 1, road.Location.Y + 1), mGame.Camera.Transform);
                    var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
                    spriteBatch.Draw(texture, res2, color * NumberManager.ZeroPointFiveF);
                }
            }
        }

        private void DrawCompleteFogOfWar(SpriteBatch spriteBatch, int x, int y)
        {

            var left = Vector2.Transform(new Vector2(x, y), mGame.Camera.Transform);
            var top = Vector2.Transform(new Vector2(x + 1, y), mGame.Camera.Transform);
            var bottom = Vector2.Transform(new Vector2(x, y + 1), mGame.Camera.Transform);
            var right = Vector2.Transform(new Vector2(x + 1, y + 1), mGame.Camera.Transform);
            var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
            var fogOfWar = Game1.GetGame().ImageManager.GetImage("FogOfWar");
            spriteBatch.Draw(fogOfWar, res2, Color.Black);
        }

        private void DrawCorpse(ObjectImmovable corpse, SpriteBatch spriteBatch)
        {
            var texture = Game1.GetGame().ImageManager.GetImage("Corpse");
            var left = Vector2.Transform(new Vector2(corpse.Location.X, corpse.Location.Y), mGame.Camera.Transform);
            var top = Vector2.Transform(new Vector2(corpse.Location.X + 1, corpse.Location.Y), mGame.Camera.Transform);
            var bottom = Vector2.Transform(new Vector2(corpse.Location.X, corpse.Location.Y + 1), mGame.Camera.Transform);
            var right = Vector2.Transform(new Vector2(corpse.Location.X + 1, corpse.Location.Y + 1), mGame.Camera.Transform);
            var res2 = Camera.GetSurroundingRectangle(left, top, bottom, right);
            spriteBatch.Draw(texture, res2, Color.White);
        }

    }
}
