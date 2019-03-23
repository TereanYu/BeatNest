#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			Game
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YU.ECS
{
    //游戏逻辑控制
    public class Game : MonoSingleton<Game>
    {
        [HideInInspector]
        public bool isMuiscReady = false;
        [HideInInspector]
        public bool isStartGame = false;
        [HideInInspector]
        public bool isResetGame = false;


        private void Awake()
        {
            
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGame();
            }
        }

        public void StartGame()
        {
            isStartGame = true;
            MusicController.Instance.m_rhythmTool.Play();
            LevelController.Instance.StartLevelTimer();
            UIManager.Instance.HideStartUI();
        }

        public void EndGame()
        {
            isStartGame = false;
            LevelController.Instance.CancelInvoke("MyTimer");
            UIManager.Instance.ShowLoseText();
            StartCoroutine("WaitAndReset");
        }

        public void WinGame()
        {
            isStartGame = false;
            LevelController.Instance.CancelInvoke("MyTimer");
            UIManager.Instance.ShowSuccessText();
            StartCoroutine("WaitAndReset");
        }

        public IEnumerator WaitAndReset()
        {
            yield return new WaitForSeconds(3f);
            ResetGame();
        }

        public void ResetGame()
        {
            SceneManager.LoadSceneAsync("Beat&Nest");
            UIManager.Instance.ResetText();
            isResetGame = true;
        }
    }
}