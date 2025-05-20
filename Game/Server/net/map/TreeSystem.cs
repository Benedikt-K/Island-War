using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.com.game;
using Common.com.networking.Messages.serverToClient;
using Common.com.objects;
using Common.com.objects.immovables.Buildings;
using Common.com.objects.immovables.Resources;
using Common.com.rollbacks;
using Server.net.main;

namespace Server.net.map
{
    public sealed class TreeSystem:ITickListener
    {
        private readonly MessageWrapper mMessageWrapper;
        private readonly Random mRandom = new Random();
        private static readonly int sMinRemoveTime=30*10, sMaxRemoveTime=210*10,
            sMinAddTime=5*10, sMaxAddTime=15*10;
        private readonly Dictionary<int, int> mTreeDeathTick = new Dictionary<int, int>();
        private readonly Dictionary<int, int> mForestersLodgeSpawnTick = new Dictionary<int, int>();
        private readonly HashSet<int> mLodges;
        public TreeSystem(MessageWrapper messageWrapper,int startTick,GameMap gameMap)
        {
            mMessageWrapper = messageWrapper;
            mLodges = new HashSet<int>();
            
            foreach (var lodge in gameMap.GetLodges())
            {
                OnAdd(lodge,startTick);
            }
        }
        private void RenewTreeDeathTick(int treeId,int tick)
        {
            var deathTick=mRandom.Next(sMinRemoveTime, sMaxRemoveTime);
            Console.WriteLine(deathTick+tick);
            mTreeDeathTick[treeId]=deathTick+tick;
        }
        private void RenewForestersLodgeSpawn(int lodgeId,int tick)
        {
            var spawnTick=mRandom.Next(sMinAddTime, sMaxAddTime);
            mForestersLodgeSpawnTick[lodgeId]=spawnTick+tick;
            
        }
        private void KillTrees(GameMap gameMap,int tick)
        {
            var toRemove=new List<int>();
            foreach (var keyVal in mTreeDeathTick)
            {
                if (keyVal.Value <= tick)
                {
                    var tree = (Tree)gameMap.GetObject(keyVal.Key);
                    var forestersLodge=(ForestersLodge)gameMap.GetObject(tree.ForestersLodgeId);
                    SpawnTree(tree,tick,true,forestersLodge.StorageAvailable(Item.Wood));
                    toRemove.Add(keyVal.Key);
                }
            }
            foreach (var treeId in toRemove)
            {
                mTreeDeathTick.Remove(treeId);
            }
        }

        private void OnTreeChange(Tree tree,int tick)
        {
            if (tree.ForestersLodgeId == -1)
            {
                mTreeDeathTick.Remove(tree.Id);
            }
            else
            {
                RenewTreeDeathTick(tree.Id,tick);
            }
        }
        private void TreeSpawn(GameMap gameMap, int tick)
        {
            foreach (var key in mForestersLodgeSpawnTick.Keys.ToArray())
            {
                if (mForestersLodgeSpawnTick[key] <= tick)
                {
                    var forestersLodge = (ForestersLodge)gameMap.GetObject(key);
                    SpawnAround(gameMap,forestersLodge.Location.X,forestersLodge.Location.Y,forestersLodge,tick);
                    RenewForestersLodgeSpawn(key,tick);
                }
            }
        }
        private void OnAdd(int forestersLodgeId,int tick)
        {
            if (!mLodges.Contains(forestersLodgeId))
            {
                mLodges.Add(forestersLodgeId);
                RenewForestersLodgeSpawn(forestersLodgeId, tick);
            }
        }

        private void OnRemove(int forestersLodgeId)
        {
            mLodges.Remove(forestersLodgeId);
            mForestersLodgeSpawnTick.Remove(forestersLodgeId);
            
        }
        public void OnTick(GameMap gameMap,int tick)
        {
            if (GameMap.Added != null)
            {
                foreach (var added in GameMap.Added)
                {
                    Console.WriteLine("another test");
                    OnAdd(added, tick);
                }
                GameMap.Added.Clear();
            }

            if (GameMap.Removed != null)
            {
                foreach (var removed in GameMap.Removed)
                {
                    OnRemove(removed);
                }
                GameMap.Removed.Clear();
            }

            if (GameMap.ChangedTree != null)
            {
                foreach (var tree in GameMap.ChangedTree)
                {
                    OnTreeChange(tree, tick);
                }
                GameMap.ChangedTree.Clear();
            }

            KillTrees(gameMap,tick);
            TreeSpawn(gameMap,tick);
        }
        private void SpawnTree(Tree tree, int tick, bool remove,bool givesResources)
        {
            if (!remove)
            {
                RenewTreeDeathTick(tree.Id,tick);
            }
            mMessageWrapper.Handle(new TreeGrowthMessage(tick,tree,remove,givesResources),0,null);
        }
        private void SpawnAround(GameMap gameMap, int x, int y, ForestersLodge forestersLodge, int tick)
        {
            if (!forestersLodge.Plants)
            {
                return;
            }
            var relX = mRandom.Next(-ForestersLodge.sRadius, ForestersLodge.sRadius);
            var relY = mRandom.Next(-ForestersLodge.sRadius, ForestersLodge.sRadius);
            var tree = new Tree
            {
                Location = new Point(x + relX, y + relY),
                ForestersLodgeId = forestersLodge.Id,
                Id = IdGenerator.NewId,
                PlayerNumber = 0
            };
            for (var xPos = tree.GetBounds().Left; xPos < tree.GetBounds().Right; xPos++)
            {
                for (var yPos = tree.GetBounds().Top; yPos < tree.GetBounds().Bottom; yPos++)
                {
                    if (gameMap.GetObject(xPos, yPos) != null || !gameMap.InBounds(xPos, yPos) ||
                        gameMap.GetTerrainAt((uint) xPos, (uint) yPos) != Terrain.Grass)
                    {
                        return;
                    }
                }
            }

            SpawnTree(tree, tick, false, false);
            
        }
    }
}