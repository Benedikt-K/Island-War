using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game.org.input
{
    public enum KeyboardInputOption
    {
        Left, Right, Up, Down, TimeBack, TimeForward, LogisticIgnore, Freeze, LogisticIgnoreAll,
        Esc,
        Jump
    }

    public static class InputInterpretation
    {
        private static Vector2 GetVector(KeyboardInputOption keyboardInputOption)
        {
            switch (keyboardInputOption)
            {
                case KeyboardInputOption.Up: return new Vector2(0,-1);
                case KeyboardInputOption.Down: return new Vector2(0,1);
                case KeyboardInputOption.Left: return new Vector2(-1,0);
                case KeyboardInputOption.Right: return new Vector2(1,0);
            }
            return Vector2.Zero;
        }

        public static Vector2 GetVector(KeyboardState keyboardState)
        {
            var res=Vector2.Zero;
            foreach (KeyboardInputOption option in Enum.GetValues(typeof(KeyboardInputOption)))
            {
                if (keyboardState.IsKeyDown(GameInputManager.GetKeyOf(option)))
                {
                    res += GetVector(option);
                }
            }

            return res;
        }
    }
}