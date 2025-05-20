using System.Collections.Generic;
using System.Drawing;
using Common.com.Menu.Alignment;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Common.com.Menu
{
    public sealed class Menu
    {
        private readonly List<MenuButton> mComponents=new List<MenuButton>();
        private readonly Dictionary<MenuButton, Rectangle> mBounds=new Dictionary<MenuButton, Rectangle>();
        public SpriteFont Font { get; }
        public RelativeRectangle Area { get; }
        private readonly int mButtonDist;
        private readonly Point mExtraMovement, mExtraButtonMovement;
        private readonly Texture2D mMenuTexture;
        private bool mDestructiveSwitches;
        private readonly string mName;
        public Menu(SpriteFont font, RelativeRectangle area,int buttonDist,Point extraMovement,Texture2D menuTexture=null,string name="", Point buttonExtraMovement=new Point())
        {
            mExtraButtonMovement = buttonExtraMovement;
            mMenuTexture = menuTexture;
            Font = font;
            mButtonDist = buttonDist;
            mExtraMovement = extraMovement;
            Area = area;
            Area.MoveBy(extraMovement);
            mName = name;
        }
        private void CalculateBounds(int i,Size size,bool newButton)
        {
            var lastRect = i==0?new Rectangle(Area.CurrentBounds.Left+mExtraButtonMovement.X,Area.CurrentBounds.Top+mExtraButtonMovement.Y,0,0):mBounds[mComponents[i-1]];
            if (!newButton)
            {
                var s = mBounds[mComponents[i]].Size;
                size = new Size(s.X,s.Y);
            }
            var button=mComponents[i];
            if (!HasButtonSpace(size,lastRect)&&newButton)
            {
                Area.AddSpace(size,mButtonDist);
            }
        
            if (SpaceRight(lastRect, size))
            {
                mBounds[button] = new Rectangle(lastRect.Right+mButtonDist, lastRect.Top,size.Width,size.Height);
            }
            else
            {
                mBounds[button] = new Rectangle(Area.CurrentBounds.Left+mButtonDist, lastRect.Bottom+mButtonDist,size.Width,size.Height);
            }
        }

        public void AddMenuButton(MenuButton menuButton, Size size)
        {
            
            mComponents.Add(menuButton);
            CalculateBounds(mComponents.Count-1,size,true);
        }

        private bool SpaceRight(Rectangle lastRect,Size rectangle)
        {
            return Area.CurrentBounds.Right - lastRect.Right>=rectangle.Width;
        }
        private bool SpaceBelow(Rectangle lastRect,Size rectangle)
        {
            return Area.CurrentBounds.Bottom - lastRect.Bottom>=rectangle.Height;
        }

        private bool HasButtonSpace(Size rectangle,Rectangle lastRect)
        {
            return SpaceRight(lastRect,rectangle)||SpaceBelow(lastRect,rectangle);
        }
        public Rectangle GetPosition(MenuButton button)
        {
            return mBounds[button];
        }

        public void SetDestructiveSwitches(bool destructive)
        {
            mDestructiveSwitches = destructive;
        }

        public bool HoveringOverThis(MouseState mouseState)
        {
            foreach (var component in mComponents)
            {
                if (component.Contains(mouseState.Position - mExtraMovement))
                {
                    return true;
                }
            }

            return false;
        }

        public bool OnClick(MouseState mouseState)
        {
            foreach (var component in mComponents)
            {
                
                if (component.OnClick(mouseState.Position-mExtraMovement))
                {
                    if (mDestructiveSwitches&&component.Switch)
                    {
                        SwitchSetIsClick(component);
                    }
                    return true;
                }
            }

            return false;
        }

        private void SwitchSetIsClick(MenuButton menuButton1)
        {
            foreach (var component2 in mComponents)
            {
                if (menuButton1 != component2)
                {
                    component2.SetIsClicked(false);
                }
            }

        }
        public bool SameName(Menu menu)
        {
            return SameName(menu.mName);
        }

        public bool SameName(string name)
        {
            return name == mName;
        }

        public bool SameStart(string name)
        {
            return mName.StartsWith(name);
        }

        public MenuButton[] Switches(bool active)
        {
            var res=new List<MenuButton>();
            foreach (var menuButton in mComponents)
            {
                if (menuButton.Switch && menuButton.IsClick() == active)
                {
                    res.Add(menuButton);
                }
            }

            return res.ToArray();
        }

        public bool ToRemove()
        {
            return Area.ToRemove();
        }
        public void Draw(SpriteBatch spriteBatch,MouseState mouseState)
        {
            var i = 0;
            if (mMenuTexture != null)
            {
                spriteBatch.Draw(mMenuTexture, Area.CurrentBounds, Color.White);
            }

            foreach (var component in mComponents)
            {
                    CalculateBounds(i++, new Size(), false);
                    component.Draw(spriteBatch, mouseState);
                if (i == mComponents.Count && -mBounds[component].Bottom + Area.CurrentBounds.Bottom >= mBounds[component].Height && Area is SideRectangle sideRectangle)
                {
                    sideRectangle.RemoveSpace(mBounds[component].Height+mButtonDist);
                }
                if (i == mComponents.Count && Area.CurrentBounds.Bottom <= mBounds[component].Bottom && Area is SideRectangle s)
                {
                    s.AddSpace(new Size(mBounds[component].Width,mBounds[component].Height),mButtonDist);
                }
            }
        }
    }
}