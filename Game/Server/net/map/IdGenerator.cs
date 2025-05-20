namespace Server.net.map
{
    public static class IdGenerator
    {
        public static int CurrentId { get; set; }
        public static int NewId => GetNewId();

        static IdGenerator()
        {
            CurrentId = 1;
        }

        private static int GetNewId()
        {
            return CurrentId++;
        }
    }
}