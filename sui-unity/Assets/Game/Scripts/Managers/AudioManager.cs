using System;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MoeBeam.Game.Scripts.Managers
{
    public class AudioManager : GenericSingleton<AudioManager>
    {
        #region EXPOSED_VARIABLES

        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        
        [Header("SFX")]
        [SerializeField] private AudioClip winSfx;
        [SerializeField] private AudioClip deathSfx;
        
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip demoMusic;

        #endregion

        #region PRIVATE_VARIABLES

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            DontDestroyOnLoad(this);
            
            PlayMusic(menuMusic);
            
            EventCenter.Subscribe(GameData.OnSceneLoadedEvent, OnSceneLoaded);
            EventCenter.Subscribe(GameData.OnPlayerDiedEvent, OnPlayerDied);
            EventCenter.Subscribe(GameData.OnBossDiedEvent, OnPlayerWin);
        }

        private void OnPlayerWin(object obj)
        {
            PlaySfx(winSfx);
        }

        private void OnPlayerDied(object obj)
        {
            PlaySfx(deathSfx);
        }

        private void OnSceneLoaded(object scene)
        {
            switch ((int)scene)
            {
                case 0:
                    PlayMusic(menuMusic);
                    break;
                case 1:
                    PlayMusic(demoMusic);
                    break;
            }
        }

        #endregion

        public void StopMusic()
        {
            musicAudioSource.Stop();
        }

        public void PlayMusic(AudioClip music)
        {
            musicAudioSource.clip = music;
            musicAudioSource.Play();
        }
        
        public void PlaySfx(AudioClip sfx)
        {
            sfxAudioSource.PlayOneShot(sfx);
        }

        
    }
}