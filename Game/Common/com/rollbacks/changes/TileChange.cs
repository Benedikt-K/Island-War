using System.Drawing;
using Common.com.game;
using Common.com.objects;

namespace Common.com.rollbacks.changes
{
    public class TileChange:IChange
    {
        private readonly Terrain mTerrainOld;
        private readonly Terrain mTerrainNew;
        private readonly uint mX;
        private readonly uint mY;
        private bool mDone;

        public TileChange(Terrain terrainOld,Terrain terrainNew,uint x,uint y)
        {
            mTerrainOld = terrainOld;
            mTerrainNew = terrainNew;
            mX = x;
            mY = y;
        }
        public void DoChange(GameState gameState)
        {
            if (mDone)
            {
                return;
            }
            mDone = true;
            if (mTerrainOld == Terrain.Mountains)
            {
                GameMap.sMountains.Add(new Point((int) mX, (int) mY));
            }
            gameState.Map.SetTerrain(mX,mY,mTerrainNew);
            GameMap.sRoads.Add(new Point((int)mX,(int)mY));
        }

        public void RevertChange(GameState gameState)
        {
            gameState.Map.SetTerrain(mX,mY,mTerrainOld);
            GameMap.sRoads.Remove(new Point((int)mX,(int)mY));
        }
    }
}