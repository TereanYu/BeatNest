#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			Game
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using System.Collections;
using UnityEngine;

namespace YU.ECS
{
    //游戏逻辑控制
    public class Game : MonoSingleton<Game>
    {
        [HideInInspector]
        public bool isMuiscReady = false;
        [HideInInspector]
        public bool isStartGame = false;


        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public void StartGame()
        {
            MusicController.Instance.m_rhythmTool.Play();
            LevelController.Instance.StartLevelTimer();
            UIManager.Instance.HideStartUI();
        }

        public void EndGame()
        {
            isStartGame = false;
            LevelController.Instance.CancelInvoke("MyTimer");
            UIManager.Instance.ShowLoseText();
            StartCoroutine("WaitAndQuit");
        }

        public void WinGame()
        {
            isStartGame = false;
            LevelController.Instance.CancelInvoke("MyTimer");
            UIManager.Instance.ShowSuccessText();
            StartCoroutine("WaitAndQuit");
        }

        public IEnumerator WaitAndQuit()
        {
            yield return new WaitForSeconds(3f);
            Application.Quit();
        }
    }
}