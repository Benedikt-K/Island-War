using System.Collections.Generic;
using System.IO;
using Common.com.game.settings;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game.org.graphic
{
    public sealed class ImageManager
    {
        private readonly ContentManager mContentManager;
        
        public Texture2D Grass { get; private set; }
        public Texture2D Water { get; private set; }
        public Texture2D Mountains { get; private set; }
        private Dictionary<string,Texture2D> Images { get; set; }

            public ImageManager(ContentManager contentManager)
        {
            
            mContentManager = contentManager;
        }

        
        public void LoadContent()
        {
            Water = mContentManager.Load<Texture2D>("Water");
            Mountains = mContentManager.Load<Texture2D>("Mountains");
            Grass = mContentManager.Load<Texture2D>("Grass");

            //sets folder in which images are located
            const string contentFolder = "graphics";  
            var dir = new DirectoryInfo(mContentManager.RootDirectory + "/" + contentFolder);
            //Load all files in Dictionary (with file Name as key) that matches the file filter (all files here)
            var result=new List<FileInfo>();
            
            GetLowestFiles(result,dir);
            System.Diagnostics.Debug.WriteLine("loaded "+result.Count+" images");
            Images = new Dictionary<string, Texture2D>();
            
            foreach (var file in result)
            {
                var key = Path.GetFileNameWithoutExtension(file.Name);
                string res;
                if (file.DirectoryName != null && file.DirectoryName.Contains("Content/"))
                {
                    res = file.DirectoryName.Split("Content/")[1] + "/" + key;
                }
                else
                {
                    res = file.DirectoryName?.Split("Content\\")[1] + "\\" + key;
                }

                Images[key] = mContentManager.Load<Texture2D>(res);
                
            }
            
        }

        
        private void GetLowestFiles(List<FileInfo> res, DirectoryInfo dir)
        {
            var dirs=dir.GetDirectories();
            foreach (var subDir in dirs)
            {
                GetLowestFiles(res,subDir);
                
            }
            res.AddRange(dir.GetFiles("*.*"));
        }
        // returns requested image at given action/attackType/angle/animationFrame/playerColor, depending on which values are given
        public Texture2D GetImage(string imageName, string action = "", int attackType = 0, int angle = 0, int animationFrame = 0, string playerColor = "")
        {

            if (action != "")
            {
                // attacking entities
                if (attackType != 0)
                {
                    return GetAttackImage(imageName, action, attackType, angle, animationFrame, playerColor);
                }
                // dead/idle entities without animation

                if (angle == 0 && animationFrame == 0)
                {
                    return Images[imageName + "_" + action + "_" + playerColor];
                }
                // walking entities with animation

                if (angle != 0 && animationFrame != 0)
                {
                    return Images[imageName + "_" + action + "_" + angle + "_" + animationFrame + "_" + playerColor];
                }
                // walking entities without animation (ScoutShip + TransportShip)

                if (angle != 0 && animationFrame == 0)
                {
                    return Images[imageName + "_" + action + "_" + angle + "_" + playerColor];
                }
                // animated idle Spy

                return Images[imageName + "_" + action + "_" + animationFrame + "_" + playerColor];

            }
            // buildings

            return Images[imageName];
        }

        private Texture2D GetAttackImage(string imageName,
            string action = "",
            int attackType = 0,
            int angle = 0,
            int animationFrame = 0,
            string playerColor = "")
        {
            if (action == "attack" && imageName != "Worker" && imageName != "Spy")
            {
                if (animationFrame < NumberManager.Ten)
                {
                    return Images[
                        imageName + "_" + action + attackType + "_" + angle + "_00" +
                        animationFrame + "_" + playerColor];
                }

                if (animationFrame < NumberManager.OneHundred)
                {
                    return Images[
                        imageName + "_" + action + attackType + "_" + angle + "_0" +
                        animationFrame + "_" + playerColor];
                }
                return Images[
                    imageName + "_" + action + attackType + "_" + angle + "_" +
                    animationFrame + "_" + playerColor];
            }

            if (imageName == "Worker" || imageName == "Spy")
            {
                action = "idle";
                return Images[imageName + "_" + action + "_" +
                              animationFrame + "_" + playerColor];
            }
            return Images[
                imageName + "_" + action + attackType + "_" + angle + "_" +
                animationFrame + "_" + playerColor];
        }
    }
}