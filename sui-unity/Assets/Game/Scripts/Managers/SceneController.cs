using System;
using MoeBeam.Game.Input;
using MoeBeam.Game.Scripts.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoeBeam.Game.Scripts.Managers
{
    
    public class SceneController : GenericSingleton<SceneController>
    {
        [SerializeField] private InputReader inputReader;
        
        public enum ScenesEnum
        {
            MainMenu = 0,
            Game = 1
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            inputReader.EnablePlayerInputActions();
            inputReader.ForceRestartEvent += RestartGame;
        }

        private void RestartGame()
        {
            //check current scene is not MainMenu
            if (SceneManager.GetActiveScene().buildIndex == 0) return;
            
            SceneManager.LoadScene(0); 
            EventCenter.ResetEventCenter();
        }

        public void LoadScene(ScenesEnum scene)
        {
            SceneManager.LoadScene((int) scene);
            EventCenter.InvokeEvent(GameData.OnSceneLoadedEvent, scene);
        }

    }
}