namespace Common.com.Menu
{
    public interface INumber
    {
        public string Number { get; }

        public static INumber GetConst(string str)
        {
            return new Const(str);
        }

        private sealed class Const:INumber
        {
            private readonly string mNum;
            public Const(string num)
            {
                mNum = num;
            }

            public string Number => mNum;
        }
    }
}