using System;
using System.Collections.Generic;
using System.Drawing;
using Common.com.game.settings;
using Common.com.objects;
using Common.com.objects.immovables.Buildings;

namespace Common.com.game.Map
{
    public class FogOfWar 
    {

        // first two are coordinates of FogOfWar Tile
        // third is distinction between players
        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public int[][][] FogOfWarTiles { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public SortedDictionary<int, Point> MovingPosition { get; set; }//Serialization
        public void AddCurrentVision(GameObject gameObject, GameMap gameMap)
        {
            if (gameObject is Building building && !(gameObject is Scaffolding))
            {
                var vision = building.VisionRange;
                for (var buildingWidth = 0; buildingWidth < building.TileSize.Width; buildingWidth++)
                {
                    for (var buildingHeight = 0; buildingHeight < building.TileSize.Height; buildingHeight++)
                    {
                        for (var x = -vision; x <= vision; x++)
                        {
                            for (var y = -vision; y <= vision; y++)
                            {
                                if (Math.Abs(x) + Math.Abs(y) <= vision &&
                                    gameMap.InBounds(building.Location.X + x + buildingWidth, building.Location.Y + y + buildingHeight))
                                {
                                    if (FogOfWarTiles[building.Location.X + x + buildingWidth][building.Location.Y + y + buildingHeight][
                                        building.PlayerNumber - 1] < 1)
                                    {
                                        FogOfWarTiles[building.Location.X + x + buildingWidth][building.Location.Y + y + buildingHeight][
                                            building.PlayerNumber - 1] = NumberManager.Two;
                                    }
                                    else
                                    {
                                        FogOfWarTiles[building.Location.X + x + buildingWidth][
                                            building.Location.Y + y + buildingHeight][
                                            building.PlayerNumber - 1] += 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (gameObject is ObjectMoving objectMoving)
            {
                MovingPosition[objectMoving.Id] = new Point((int)objectMoving.Position.X, (int)objectMoving.Position.Y);
                var vision = objectMoving.VisionRange;
                for (var x = -vision; x <= vision; x++)
                {
                    for (var y = -vision; y <= vision; y++)
                    {
                        var movingPositionX = (int) (objectMoving.Position.X) + x;
                        var movingPositionY = (int)(objectMoving.Position.Y) + y;
                        if (Math.Abs(x) + Math.Abs(y) <= vision && gameMap.InBounds(movingPositionX,
                            movingPositionY))
                        {
                            if (FogOfWarTiles[movingPositionX][movingPositionY][
                                objectMoving.PlayerNumber - 1] < 1)
                            {
                                FogOfWarTiles[movingPositionX][movingPositionY][
                                    objectMoving.PlayerNumber - 1] = NumberManager.Two;
                            }
                            else
                            {
                                FogOfWarTiles[movingPositionX][movingPositionY][
                                    objectMoving.PlayerNumber - 1] += 1;
                            }
                        }
                    }
                }
            }
        }

        public void RemoveCurrentVision(GameObject gameObject, GameMap gameMap)
        {
            if (gameObject is Building building && !(gameObject is Scaffolding))
            {
                var vision = building.VisionRange;
                for (var buildingWidth = 0; buildingWidth < building.TileSize.Width; buildingWidth++)
                {
                    for (var buildingHeight = 0; buildingHeight < building.TileSize.Height; buildingHeight++)
                    {
                        for (var x = -vision; x <= vision; x++)
                        {
                            for (var y = -vision; y <= vision; y++)
                            {
                                if (Math.Abs(x) + Math.Abs(y) <= vision &&
                                    gameMap.InBounds(building.Location.X + x + buildingWidth, building.Location.Y + y + buildingHeight))
                                {
                                    if (FogOfWarTiles[building.Location.X + x + buildingWidth][
                                        building.Location.Y + y + buildingHeight][
                                        building.PlayerNumber - 1] < NumberManager.Two)
                                    {
                                        FogOfWarTiles[building.Location.X + x + buildingWidth][ building.Location.Y + y + buildingHeight][
                                            building.PlayerNumber - 1] = 1;
                                    }
                                    else
                                    {
                                        FogOfWarTiles[building.Location.X + x + buildingWidth][
                                            building.Location.Y + y + buildingHeight][
                                            building.PlayerNumber - 1] -= 1;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            else if (gameObject is ObjectMoving objectMoving && MovingPosition.ContainsKey(objectMoving.Id))
            {
                var vision = objectMoving.VisionRange;
                for (var x = -vision; x <= vision; x++)
                {
                    for (var y = -vision; y <= vision; y++)
                    {
                        var movingPositionX = MovingPosition[objectMoving.Id].X + x;
                        var movingPositionY = MovingPosition[objectMoving.Id].Y + y;
                        if (Math.Abs(x) + Math.Abs(y) <= vision && gameMap.InBounds(movingPositionX, movingPositionY))
                        {
                            if (FogOfWarTiles[movingPositionX][movingPositionY][objectMoving.PlayerNumber - 1] < NumberManager.Two)
                            {
                                FogOfWarTiles[movingPositionX][movingPositionY][objectMoving.PlayerNumber - 1] = 1;
                            }
                            else
                            {
                                FogOfWarTiles[movingPositionX][movingPositionY][objectMoving.PlayerNumber - 1] -= 1;
                            }
                        }
                    }
                }
            }
        }
        public static FogOfWar CreateNewFogOfWar(Size size)
        {
            var fogOfWar = new FogOfWar();
            var newFogOfWarTiles = new int[size.Width][][];
            for (var i = 0; i < size.Width; i++)
            {
                newFogOfWarTiles[i] = new int[size.Height][];
            }
            for (var i = 0; i < size.Width; i++)
            {
                for (var j = 0; j < size.Height; j++)
                {
                    newFogOfWarTiles[i][j] = new int[NumberManager.Two];
                    newFogOfWarTiles[i][j][0] = 0;
                    newFogOfWarTiles[i][j][1] = 0;
                }
            }
            fogOfWar.FogOfWarTiles = newFogOfWarTiles;
            fogOfWar.MovingPosition = new SortedDictionary<int, Point>();
            return fogOfWar;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        //JsonSerialization uses both the properties public getter and setter and therefore this Resharper error can be ignored
        public FogOfWar()//Serialization
        {
            MovingPosition = new SortedDictionary<int, Point>();
        }


    }
}