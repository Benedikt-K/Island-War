using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Common.com.game.settings;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Common.com.Menu
{
    public sealed class MenuTextField : MenuButton
    {
        private static readonly TimeSpan sTime = new TimeSpan(0,0,0,0,400);
        private static readonly TimeSpan sTime2 = new TimeSpan(0,0,0,0,100);
        private static readonly DateTime sDef=new DateTime(0);
        private DateTime mInputStart=sDef;
        private Keys mLastKey;
        
        public MenuTextField(Texture2D texture, EventHandler eventHandler, Menu menu) : base(texture,
            eventHandler,
            menu,
            "",
            true)
        {
            
        }

        public void HandleInput(KeyboardState keyboardState)
        {
            var currentDateTime = DateTime.Now;
            if (!keyboardState.GetPressedKeys().ToList().Contains(mLastKey))
            {
                mInputStart=sDef;
            }
            
            if (keyboardState.GetPressedKeys().Length == 0)
            {
                mInputStart=sDef;
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.LeftControl)&&(keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl)))
                {
                    
                }
                if (mInputStart == sDef)
                {
                    mInputStart=currentDateTime;
                    OnKeyboardInput(keyboardState);
                }
                else
                {
                    if (currentDateTime - mInputStart <= sTime)
                    {
                        return;
                    }

                    mInputStart += sTime2;
                    OnKeyboardInput(keyboardState);
                }
            }
            
            
        }

        private void OnKeyboardInput(KeyboardState keyboardState)
        {
            foreach (var key in keyboardState.GetPressedKeys())
            {
                if ( key == Keys.Back)
                {
                    if (Text.Length > 0)
                    {
                        Text = Text.Remove(Text.Length - 1);
                    }
                }
                else
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Text += KeyCodeToUnicodeLinux(key);
                    }
                    else
                    {
                        Text += KeyCodeToUnicodeWindows(key);
                    }
                }
                mLastKey = key;
            }
            
        }

        private static string KeyCodeToUnicodeLinux(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return key.ToString().Replace("D","");
            }

            if (key == Keys.OemPeriod)
            {
                return ".";
            }

            if (key == Keys.Space)
            {
                return " ";
            }

            if (key == Keys.OemComma)
            {
                return ",";
            }

            if (key >= Keys.A && key <= Keys.Z)
            {
                return key.ToString();
            }

            return "";
        }
        private static string KeyCodeToUnicodeWindows(Keys key)
        {
            var keyboardState = new byte[255];
            var keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return "";
            }

            var virtualKeyCode = (uint)key;
            var scanCode = MapVirtualKey(virtualKeyCode, 0);
            var inputLocaleIdentifier = GetKeyboardLayout(0);

            var result = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, NumberManager.Five, 0, inputLocaleIdentifier);

            return result.ToString();
        }

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint vKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pBuff, int cchBuff, uint wFlags, IntPtr intPtr);
    }
}