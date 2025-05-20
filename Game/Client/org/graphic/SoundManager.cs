using System.Security.Cryptography;
using Common.com.game.settings;
using Game.org.gameStates;
using Game.org.main;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Game.org.graphic
{
    public class SoundManager
    {
        private readonly ContentManager mContentManager;

        public SoundEffect Building1 { get; private set; }
        public SoundEffect Building2 { get; private set; }
        public SoundEffect GameLost { get; private set; }
        public SoundEffect GameWon { get; private set; }
        public SoundEffect MouseClick { get; private set; }
        public SoundEffect Sailing { get; private set; }
        
        public SoundEffect UnitStartedMoving { get; private set; }

        public Song MusicMainMenu { get; private set; }
        private Song MusicPeace1 { get; set; }
        private Song MusicPeace2 { get; set; }
        private Song MusicPeace3 { get; set; }
        private Song MusicWar1 { get; set; }
        private Song MusicWar2 { get; set; }
        private Song MusicWar3 { get; set; }


        public SoundManager(ContentManager contentManager)
        {
            mContentManager = contentManager;
        }

        public void LoadContent()
        {
            Building1 = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_building");
            Building2 = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_building_2");
            GameLost = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_game_lost");
            GameWon = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_game_won");
            MouseClick = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_mouseclick");
            Sailing = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_sailing");
           
            UnitStartedMoving = mContentManager.Load<SoundEffect>("Sound/SFX/sfx_unit_started_moving");

            MusicMainMenu = mContentManager.Load<Song>("Sound/Soundtrack/Main_Menu");
            MusicPeace1 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_Peace_1");
            MusicPeace2 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_Peace_2");
            MusicPeace3 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_Peace_3");
            MusicWar1 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_War_1");
            MusicWar2 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_War_2");
            MusicWar3 = mContentManager.Load<Song>("Sound/Soundtrack/InGame_War_3");
            }

        public void PlayMusic(Song music)
        {
            MediaPlayer.Play(music);
            MediaPlayer.IsRepeating = true;
        }

        public void PlayRandomMusic()
        {
            var songNumber = RandomNumberGenerator.GetInt32(0, 6);
            if (songNumber == 0)
            {
                PlayMusic(MusicPeace1);
            }

            if (songNumber == 1)
            {
                PlayMusic(MusicPeace2);
            }

            if (songNumber == NumberManager.Two)
            {
                PlayMusic(MusicPeace3);
            }

            if (songNumber == NumberManager.Three)
            {
                PlayMusic(MusicWar1);
            }

            if (songNumber == NumberManager.Four)
            {
                PlayMusic(MusicWar2);
            }

            if (songNumber == NumberManager.Five)
            {
                PlayMusic(MusicWar3);
            }
        }

        public void StopMusic()
        {
            if ((int) MediaPlayer.State == 1)
            {
                MediaPlayer.Stop();
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (volume == 0)
            {
                StopMusic();
                MediaPlayer.Volume = volume;
            }
            else if (Game1.GetGame().mGameState is MainMenu && MediaPlayer.Volume == 0)
            {
                PlayMusic(MusicMainMenu);
                MediaPlayer.Volume = volume;
            }
            else if (Game1.GetGame().mGameState is InGame && MediaPlayer.Volume == 0)
            {
                PlayRandomMusic();
                MediaPlayer.Volume = volume;
            }
            else
            {
                MediaPlayer.Volume = volume;
            }
        }

        public void PlaySfx(SoundEffect effect)
        {
            var soundEffectInstance = effect.CreateInstance();
            soundEffectInstance.Volume = SoundEffect.MasterVolume;
            soundEffectInstance.Play();
        }

        public void SetSfxMasterVolume(float volume)
        {
            SoundEffect.MasterVolume = volume;
        }
        
        public float GetSfxVolume()
        {
            return SoundEffect.MasterVolume;
        }

        public float GetMusicVolume()
        {
            return MediaPlayer.Volume;
        }
    }
}