using System;

using System.Drawing;
using Common.com.game.settings;
using Game.org.input;
using Game.org.main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Game.org.graphic
{
    public sealed class Camera
    {
        public float Zoom { get; private set; }
        private static readonly float sZoomMult = 100f;
        private static readonly Matrix sDirections=new Matrix(new Vector4(1/2f,1/4f,0,0),new Vector4(1/2f,-1/4f,0,0),new Vector4(0,0,1,0),new Vector4(0,0,0,1));
        private Vector2 Position { get; set; }
        private Viewport mViewport;
        public Rectangle VisibleArea { get; private set; }
        public Matrix Transform { get; private set; }
        private Size MapBounds { get; }
        private float mPrevMouseWheelValue;
        private readonly Game1 mGame;




        public Camera(Viewport viewport,Vector2 startPosition, Size mapBounds, float zoom = 1f)
        {
            MapBounds = mapBounds;
            mViewport = viewport;
            Zoom = zoom;
            Position = startPosition;
            mGame = Game1.GetGame();
        }


        private void UpdateVisibleArea()
        {
            var inverseViewMatrix = Matrix.Invert(Transform);
            var bounds = mGame.GraphicsDevice.PresentationParameters.Bounds;
            var topLeft = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var topRight = Vector2.Transform(new Vector2(bounds.Width, 0), inverseViewMatrix);
            var bottomLeft = Vector2.Transform(new Vector2(0, bounds.Height), inverseViewMatrix);
            var bottomRight = Vector2.Transform(new Vector2(bounds.Width, bounds.Height), inverseViewMatrix);
            VisibleArea= GetSurroundingRectangle(topLeft, topRight, bottomLeft, bottomRight);
            VisibleArea = new Rectangle(VisibleArea.X-1,VisibleArea.Y-1,VisibleArea.Width+ NumberManager.Two,VisibleArea.Height+ NumberManager.Two);
        }

        public Rectangle GetDrawRect(System.Drawing.Rectangle inGameRect)
        {
            var topLeft = Vector2.Transform(new Vector2(inGameRect.Left,inGameRect.Top), Transform);
            var topRight = Vector2.Transform(new Vector2(inGameRect.Left,inGameRect.Bottom), Transform);
            var bottomLeft = Vector2.Transform(new Vector2( inGameRect.Right,inGameRect.Top), Transform);
            var bottomRight = Vector2.Transform(new Vector2( inGameRect.Right,inGameRect.Bottom), Transform);
            return GetSurroundingRectangle(topLeft, topRight, bottomLeft, bottomRight);
        }
        public static Rectangle GetSurroundingRectangle(Vector2 corner1, Vector2 corner2, Vector2 corner3, Vector2 corner4)
        {
            var min = new Vector2(
                Math.Min(corner1.X, Math.Min(corner2.X, Math.Min(corner3.X, corner4.X))),
                Math.Min(corner1.Y, Math.Min(corner2.Y, Math.Min(corner3.Y, corner4.Y))));
            var max = new Vector2(
                Math.Max(corner1.X, Math.Max(corner2.X, Math.Max(corner3.X, corner4.X))),
                Math.Max(corner1.Y, Math.Max(corner2.Y, Math.Max(corner3.Y, corner4.Y))));
            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
        private void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                        Matrix.CreateScale(sZoomMult) *
                        Matrix.CreateScale(Zoom)*
                        Matrix.CreateTranslation(new Vector3(mViewport.Bounds.Width * NumberManager.ZeroPointFiveF, mViewport.Bounds.Height * NumberManager.ZeroPointFiveF, 0))*
                        
                        sDirections;
            UpdateVisibleArea();
        }

        private void MoveCamera(Vector2 movePosition)
        {
            var transformedMovePos = Vector2.Transform(movePosition, Matrix.Invert(sDirections));
            var cameraRectangle = VisibleArea;
            var cameraCenterPoint = cameraRectangle.Center;

            if (transformedMovePos.X < 0 && cameraCenterPoint.X < 0)
            {
                transformedMovePos.X = 0;
            }
            if (transformedMovePos.X > 0 && cameraCenterPoint.X > MapBounds.Width)
            {
                transformedMovePos.X = 0;
            }
            if (transformedMovePos.Y < 0 && cameraCenterPoint.Y < 0)
            {
                transformedMovePos.Y = 0;
            }
            if (transformedMovePos.Y > 0 && cameraCenterPoint.Y > MapBounds.Height)
            {
                transformedMovePos.Y = 0;
            }

            if (mGame.IsActive)
            {
                Position += transformedMovePos;
                UpdateMatrix();
            }
            else
            {
                movePosition.X = 0;
                movePosition.Y = 0;
            }
        }

        private void AdjustZoom(float zoomAmount)
        {
            Zoom += zoomAmount;
            Zoom = Math.Max(NumberManager.ZeroPointThreeFive,Zoom);
            Zoom = Math.Min(NumberManager.TwoF,Zoom);
        }

        private int GetBaseSpeed(float currentZoom)
        {
            if (currentZoom > NumberManager.ZeroPointEight)
            {
                return NumberManager.Fifteen;
            }
            if (currentZoom >= NumberManager.ZeroPointSix)
            {
                return NumberManager.Twenty;
            }
            if (currentZoom > NumberManager.ZeroPointThreeFive)
            {
                return NumberManager.TwentyFive;
            }
            return NumberManager.Thirty;
            
        }
        public void UpdateCamera(MouseState mouseState, KeyboardState keyboardState)
        {
            var cameraMovement = InputInterpretation.GetVector(keyboardState);
            if (mouseState.Position.X > mGame.GraphicsDevice.PresentationParameters.Bounds.Width * NumberManager.ZeroPointNineNine)
            {
                cameraMovement.X += NumberManager.ZeroPointFiveF;
            }
            if (mouseState.Position.X < mGame.GraphicsDevice.PresentationParameters.Bounds.Width * NumberManager.ZeroPointZeroOne)
            {
                cameraMovement.X -= NumberManager.ZeroPointFiveF;
            }
            if (mouseState.Position.Y > mGame.GraphicsDevice.PresentationParameters.Bounds.Height * NumberManager.ZeroPointNineNine)
            {
                cameraMovement.Y += NumberManager.ZeroPointFiveF;
            }
            if (mouseState.Position.Y < mGame.GraphicsDevice.PresentationParameters.Bounds.Height * NumberManager.ZeroPointZeroOne)
            {
                cameraMovement.Y -= NumberManager.ZeroPointFiveF;
            }
            
            //auf dem Menü oben links kann nun nicht mehr verschoben werden
            if (mouseState.Position.X <= NumberManager.FortyFive && mouseState.Position.Y <= NumberManager.OneHundredSeventy && mouseState.Position.X > NumberManager.Two && mouseState.Position.Y > NumberManager.Two)
            {
                cameraMovement.X = 0f;
                cameraMovement.Y = 0f;
            }

            AdjustZoom(NumberManager.ZeroPointZeroFive *Math.Sign(mouseState.ScrollWheelValue-mPrevMouseWheelValue));

            mPrevMouseWheelValue=mouseState.ScrollWheelValue;
            
            MoveCamera(cameraMovement*GetBaseSpeed(Zoom)/sZoomMult);
        }
    }
}

