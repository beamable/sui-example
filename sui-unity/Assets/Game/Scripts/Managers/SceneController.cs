using System;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoeBeam.Game.Scripts.Managers
{
    
    public class SceneController : GenericSingleton<SceneController>
    {
        public enum ScenesEnum
        {
            MainMenu = 0,
            Game = 1
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void LoadScene(ScenesEnum scene)
        {
            SceneManager.LoadScene((int) scene);
            EventCenter.InvokeEvent(GameData.OnSceneLoadedEvent, scene);
        }

    }
}