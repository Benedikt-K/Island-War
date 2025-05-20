using System;

namespace Common.com.game
{
    public sealed class TimeManager
    {
        private readonly long mFirstCall;
        private int mTicksDone;
        private const int MillisecondsPerTick = 50;

        public TimeManager()
        {
            mFirstCall=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        private int GetNewTicks()
        {
            return (int)((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - mFirstCall)/ MillisecondsPerTick)-mTicksDone;
        }

        /// <summary>
        /// elapses the given GameTime
        /// </summary>
        /// <returns>The amount of ticks to calculate</returns>
        public int ElapseTime()
        {
            var ticks=GetNewTicks();
            mTicksDone += ticks;
            
            return ticks;
        }
        /// <summary>
        /// Converts our version of ticks into a TimeSpan.
        /// Theres 20 ticks per second
        /// </summary>
        /// <param name="ticks">The time in ticks to get the TimeSpan of</param>
        /// <returns>The time span of the ticks</returns>
        public static TimeSpan GetTime(int ticks)
        {
            return new TimeSpan(ticks*MillisecondsPerTick*TimeSpan.TicksPerMillisecond);
        }
    }
}