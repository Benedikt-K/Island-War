using System;

namespace Game.org.main
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = Game1.GetGame();
            game.Run();
        }
    }
}