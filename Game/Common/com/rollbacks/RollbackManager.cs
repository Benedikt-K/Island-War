using System;
using System.Collections.Generic;
using System.Linq;
using Common.com.game;
using Common.com.networking.Messages;
using Common.com.networking.Messages.CommonMessages;
using Common.com.networking.Messages.serverToClient;
using Common.com.rollbacks.changes;
using Common.com.util;

namespace Common.com.rollbacks
{
    public sealed class RollbackManager:IGameEndListener
    {
        public static bool HasEnded { get; private set; }
        public const int DefaultSize = 1000;
        private const int SavedStates = 20; 
        private readonly int mSize;
        private readonly int mTickDiff;
        private readonly List<ITickListener> mTickListeners = new List<ITickListener>();
        public RollbackManager(GameState start, int size)
        {
            HasEnded = false;
            mTickDiff = start.TickNr % SavedStates;
            mSize = size;
            mPastStates = new CyclicBuffer<GameState>(size);
            mPastStates.Add(start);
            UpdateCurrentGameState();
            DoAllChanges(start);
            GameMap.mGameEndListeners.Add(this);
        }

        public void AddTickListener(ITickListener listener)
        {
            mTickListeners.Add(listener);
        }

        private readonly CyclicBuffer<GameState> mPastStates;
        private Dictionary<int, List<IChange>> mChanges=new Dictionary<int, List<IChange>>();
        public GameState CurrentState { get; private set; }
        private readonly object mSyncLock = new object();
        private void UpdateCurrentGameState()
        {
            CurrentState = mPastStates.Get(0);
        }

        private void AddMessage(Message message, int tickNr)
        {
            AddChange(new MessageChange(message),tickNr);
        }
        private void AddChange(IChange change, int tickNr)
        {
            if (!mChanges.ContainsKey(tickNr))
            {
                mChanges[tickNr] = new List<IChange>();
            }
            
            mChanges[tickNr].Add(change);
        }

        private void ReverseTicks(int ticks)
        {
            lock (mSyncLock)
            {
                ticks = mPastStates.Get(0).TickNr - ticks;
                CalculateTicks((uint)ticks,0);
            }
        }
        /// <summary>
        /// Changes the GameState ticksAgo ticks ago and calculates the new gameStates
        /// </summary>
        /// <param name="message">The message to change the gameState with</param>
        /// <param name="tickNr">The tick that should be replaced</param>
        /// <returns>whether the state was accessible</returns>
        public bool RollbackState(Message message, int tickNr)
        {
            if (message is RevertTimeMessage revertTimeMessage)
            {
                if (revertTimeMessage.Revert)
                {
                    ReverseTicks(tickNr);
                }
                else
                {
                    CalculateTicks(0,(uint)tickNr);
                }

                return true;
            }
            int ticks;
            lock (mSyncLock)
            {
                ticks=mPastStates.Get(0).TickNr - tickNr;
                if (ticks < 0)
                {
                    if (message != null)
                    {
                        AddMessage(message, tickNr);
                    }

                    return true;
                }
            }
            
            
            lock (mSyncLock)
            {
                
                ticks = mPastStates.Get(0).TickNr - tickNr;
                if (ticks < 0) //Should be a very rare case
                {
                    return RollbackState(message, tickNr);
                    
                }
                var ticksAgo = (uint)(ticks);
                if (mPastStates.Get(ticksAgo) != null)
                {
                    if (message is PauseMessage)
                    {
                        CalculateTicks(ticksAgo, 0);
                        UpdateCurrentGameState();
                        CurrentState.Handle(message);
                        return true;
                    }
                    CalculateTicks(ticksAgo+1, ticksAgo+1,message);
                    UpdateCurrentGameState();
                    
                    return true;
                    
                }

                return false;
            }
        }
        
        public void CalculateNextTicks(uint ticks)
        {
            
                lock (mSyncLock)
                {
                    if (!IsPaused())
                    {
                        CalculateTicks(0, ticks);
                        UpdateCurrentGameState();
                    }
                }
            
        }

        private void ChangesBack(uint ticksAgo)
        {
            lock (mSyncLock)
            {
                var current = mPastStates.Get(0);
                for (var i = 0; i < ticksAgo; i++)
                {
                    if (mChanges.ContainsKey(current.TickNr - i))
                    {
                        var list = mChanges[current.TickNr - i];
                        for (var j=list.Count-1;j>=0;j-- )
                        {
                            var tileChange = list[j];
                            if (!(tileChange is MessageChange))
                            {
                                list.RemoveAt(j);
                            }
                            tileChange.RevertChange(current);
                        }
                    }
                }
            }
        }

        private void CalculateTicksWait(uint ticksAgo, uint toCalculate, Message newMessage = null) 
        {
            lock (mSyncLock)
            {
                var current=mPastStates.Get(0).TickNr;
                if (ticksAgo != 0)
                {
                    uint oldTicksAgo = ticksAgo;
                    ticksAgo = (uint) current-(uint)mTickDiff-ticksAgo;
                    ticksAgo /= SavedStates;
                    ticksAgo *= SavedStates;
                    ticksAgo = (uint) current-(uint)mTickDiff-ticksAgo;
                    toCalculate += ticksAgo - oldTicksAgo;
                }

                if (mPastStates.Get(ticksAgo) == null)
                {
                    throw new Exception("Rollback manager went out of bounds");
                }
                ChangesBack(ticksAgo);
                if (newMessage != null)
                {
                    AddMessage(newMessage, newMessage.Tick);
                }

                
                mPastStates.Back(ticksAgo);
                var pastState = mPastStates.Get(0);
                if (current-pastState.TickNr!=ticksAgo)
                {
                    throw new Exception("An issue with the rollback manager has occured:\n The tick Number of the rolled back state was "+pastState.TickNr+" when it should have been "+(current-ticksAgo)+" when rolling back "+ticksAgo+" ticks with an offset of "+mTickDiff);
                }
                while (toCalculate-- > 0)
                {
                    if (pastState.TickNr % SavedStates == mTickDiff)
                    {
                        GameMap.OnEntityReset();
                    }
                    
                    GameState next = pastState.TickNr%SavedStates==mTickDiff? pastState.Clone():pastState;
                    next.Tick();
                    if (ticksAgo == 0)
                    {
                        mChanges.Remove(next.TickNr- 1 - mSize);
                    }

                    

                    mPastStates.Add(next);
                    if (mChanges.ContainsKey(next.TickNr))
                    {
                        
                        foreach (var message in mChanges[next.TickNr])
                        {
                            message.DoChange(next);
                        }

                        if (newMessage is ResyncMessage {Resynced: true})
                        {
                            mChanges = new Dictionary<int, List<IChange>>
                            {
                                [newMessage.Tick] = new List<IChange> {new MessageChange(newMessage)}
                            };
                        }
                    }

                    DoAllChanges(next);
                    if (ticksAgo == 0)
                    {
                        foreach (var listener in mTickListeners)
                        {
                            listener.OnTick(next.Map,next.TickNr);
                        }

                        
                    }

                    pastState = next;
                }
            }
        }
        private void CalculateTicks(uint ticksAgo, uint toCalculate,Message newMessage=null)
        {
            CalculateTicksWait(ticksAgo, toCalculate, newMessage);
        }

        private void DoAllChanges(GameState gameState)
        {
            if (gameState.Map.Changes == null)
            {
                return;
            }
            while (gameState.Map.Changes.Any())
            {
                foreach (var changeNew in gameState.Map.Changes.ToArray())
                {
                    AddChange(changeNew, gameState.TickNr);
                    gameState.Map.Changes.Remove(changeNew);
                    changeNew.DoChange(gameState);
                }
            }
        }
        public bool IsPaused()
        {
            return CurrentState.IsPaused;
        }

        public void OnGameEnd()
        {
            GameMap.StatisticsManager.OnEndGame(CurrentState);
        }

        public void OnGameEnd(int playerIdWon)
        {
            HasEnded = true;
        }
    }
}