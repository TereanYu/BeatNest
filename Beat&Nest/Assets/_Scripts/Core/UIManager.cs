#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			UIManager
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace YU.ECS {

    public class UIManager : MonoSingleton<UIManager>
    {
        public float m_textShowTime = 3f;
        public float m_textStayTime = 4f;
        public float m_textFadeTime = 3f;

        public Text m_cubeNumText;
        public Text m_homeHPText;
        public Text m_showTextEnglish;
        public Text m_showTextChinese;
        public Text m_successText;
        public Text m_loseText;

        public GameObject StartUI;

        private int oldTeachIndex;

        private void Awake()
        {
            oldTeachIndex = -1;
        }

        private void Update()
        {
            if (LevelController.Instance.m_currTeachIndex > oldTeachIndex)
            {
                //更新上一次的levelindex
                oldTeachIndex = LevelController.Instance.m_currTeachIndex;
                ShowTeachText(LevelController.Instance.m_teachist[oldTeachIndex].m_englishText, LevelController.Instance.m_teachist[oldTeachIndex].m_chineseText);
            }
        }

        public void ShowStartUI()
        {
            StartUI.SetActive(true);
        }

        public void HideStartUI()
        {
            StartUI.SetActive(false);
        }

        public void ShowSuccessText()
        {
            m_successText.enabled = true;
        }

        public void ShowLoseText()
        {
            m_loseText.enabled = true;
        }

        public void ResetText()
        {
            m_successText.enabled = false;
            m_loseText.enabled = false;
        }

        public void ShowTeachText(string contentEnglish,string contentChinese)
        {
            m_showTextEnglish.text = contentEnglish;
            m_showTextEnglish.DOFade(1f, m_textShowTime);
            m_showTextChinese.text = contentChinese;
            m_showTextChinese.DOFade(1f, m_textShowTime);
            StartCoroutine(WaitToTextFade());
        }

        public void ChangeCubeNumText(int num)
        {
            m_cubeNumText.text = num.ToString();
        }

        public void ChangeHomeHPText(int num)
        {
            m_homeHPText.text = num.ToString();
            if (num <= 0)
            {
                Game.Instance.EndGame();
            }
        }

        private IEnumerator WaitAndQuit()
        {
            yield return new WaitForSeconds(3f);
            Application.Quit();
        }

        private IEnumerator WaitToTextFade()
        {
            yield return new WaitForSeconds(m_textStayTime);
            m_showTextEnglish.DOFade(0f, m_textFadeTime);
            m_showTextChinese.DOFade(0f, m_textFadeTime);
        }
    }
}

