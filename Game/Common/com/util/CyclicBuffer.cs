

namespace Common.com.util
{
    public sealed class CyclicBuffer<T>
    {
        private readonly T[] mBuffer;
        private uint mCurrent;
        private uint mBackWards;
        public CyclicBuffer(int size)
        {
            mBuffer = new T[size];
        }
        /// <summary>
        /// Adds the value
        /// </summary>
        /// <param name="t">the value to add</param>
        public void Add(T t)
        {
            mBuffer[mCurrent++] = t;
            mCurrent %= (uint)mBuffer.Length;
            if (mBackWards > 0)
            {
                mBackWards--;
            }
        }
        /// <summary>
        /// Gets the element added i Add()s before.
        /// Returns the default value of T if i>=size or nothing had been added yet
        /// For example calling Add(t) and then Get(0) returns t.
        /// </summary>
        /// <param name="i">How many adds in the past</param>
        /// <returns>The Element</returns>
        public T Get(uint i)
        {
            if (i >= mBuffer.Length-mBackWards)
            {
                return default;
            }
            //Efficient method of getting the positive modulo result
            return mBuffer[GetIndex(i)];
        }

        private uint GetIndex(uint i)
        {
            return (uint)(((((int)mCurrent - (int)i-1)%mBuffer.Length)+mBuffer.Length)%mBuffer.Length);
        }

        public void Back(uint i)
        {
            mBackWards += i;
            var temp = (int)mCurrent-(int)i;
            mCurrent = (uint)(((temp%mBuffer.Length) + mBuffer.Length)%mBuffer.Length);
        }
    }
}