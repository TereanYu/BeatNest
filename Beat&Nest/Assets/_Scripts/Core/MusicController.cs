#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			MusicController
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using DG.Tweening;
using UnityEngine;

namespace YU.ECS {

    public class MusicController : MonoSingleton<MusicController>
    {
        //muisc tool
        public RhythmTool m_rhythmTool;
        public RhythmEventProvider m_eventProvider;
        public AudioClip m_audioClip;

        //controlled stuff
        public Material m_planMaterial;
        public Transform m_homeTransform;

        //music control
        public float m_scoreStandard1;
        public float m_scoreStandard2;
        public float m_scoreStandard3;

        private Sequence m_sequenceBackgroundColor;
        private Sequence m_sequenceHomeScale;


        private float m_lastBeatTime = 0;
        private float m_currBeatTime = 0;
        private float m_pressBeatTime = 0;
        [HideInInspector]
        public float m_currBeatScore = 0;
        [HideInInspector]
        public bool isPressAndWaitGenerate = false;
        [HideInInspector]
        public bool isBadPressAndWaitGenerate = false;

        private void Start()
        {
            m_eventProvider.Onset += OnOnset;
            m_eventProvider.Beat += OnBeat;
            m_eventProvider.Change += OnChange;
            m_eventProvider.SongLoaded += OnSongLoaded;
            m_eventProvider.SongEnded += OnSongEnded;

            m_rhythmTool.audioClip = m_audioClip;

            m_sequenceBackgroundColor = DOTween.Sequence();
            m_sequenceBackgroundColor.SetAutoKill(false);
            m_sequenceBackgroundColor.Append(m_planMaterial.DOColor(new Color(0.09f,0f,0.05f), 0.1f)).Append(m_planMaterial.DOColor(Color.black, 0.5f));
            m_sequenceBackgroundColor.Pause();

            m_sequenceHomeScale = DOTween.Sequence();
            m_sequenceHomeScale.SetAutoKill(false);
            m_sequenceHomeScale.Append(m_homeTransform.DOScale(new Vector3(130f,1f,130f),0.1f))
                .Append(m_homeTransform.DOScale(new Vector3(70f, 1f, 70f), 0.1f))
                .Append(m_homeTransform.DOScale(new Vector3(100f, 1f, 100f), 0.1f));
            m_sequenceHomeScale.Pause();
        }

        private void OnSongEnded()
        {
            m_rhythmTool.Stop();
            Game.Instance.WinGame();
        }

        private void OnSongLoaded()
        {
            Game.Instance.isMuiscReady = true;
        }

        private void OnChange(int arg1, float arg2)
        {

        }

        private int m_lastBeatFrameOld;
        private int m_lastBeatFrame;
        private int m_lastBeatIndex;
        private void OnBeat(Beat obj)
        {
            m_lastBeatFrameOld = m_lastBeatFrame;
            m_lastBeatFrame = m_rhythmTool.currentFrame;
            m_lastBeatIndex = obj.index;

            m_lastBeatTime = m_currBeatTime;
            m_currBeatTime = Time.time;

            m_sequenceBackgroundColor.Restart();
            m_sequenceHomeScale.Restart();
        }

        private float m_beatDelta;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)&&Game.Instance.isStartGame)
            {
                m_pressBeatTime = Time.time;
                m_beatDelta = m_pressBeatTime - m_currBeatTime;
                //if (m_beatDelta <= m_scoreStandard1 || m_beatDelta >= (m_currBeatTime-m_lastBeatTime- m_scoreStandard1))
                //{
                //    m_currBeatScore = 1f;
                //    isPressAndWaitGenerate = true;
                //}
                //else if (m_beatDelta <= m_scoreStandard2 || m_beatDelta >= (m_currBeatTime - m_lastBeatTime - m_scoreStandard2))
                //{
                //    m_currBeatScore = 0.5f;
                //    isPressAndWaitGenerate = true;
                //}
                //else if (m_beatDelta < m_scoreStandard3 || m_beatDelta >= (m_currBeatTime - m_lastBeatTime - m_scoreStandard3))
                //{
                //    m_currBeatScore = 0.1f;
                //    isPressAndWaitGenerate = true;
                //}
                //else
                //{
                //    m_currBeatScore = 0f;
                //    isPressAndWaitGenerate = false;
                //}
                //m_sequenceHomeScale.Restart();

                if (CheckBeat(8))
                {   //往前检测8帧
                    m_currBeatScore = 1f;
                    isPressAndWaitGenerate = true;
                }
                else if ((m_rhythmTool.currentFrame - m_lastBeatFrame) >= (m_lastBeatFrame - m_lastBeatFrameOld - 10))
                {   //根据前两拍间的间隔，预测往后检测10帧
                    m_currBeatScore = 1f;
                    isPressAndWaitGenerate = true;
                }
                else
                {
                    //enemy gene
                    isBadPressAndWaitGenerate = true;

                }
            }
        }

        
        bool isHit = false;
        /// <summary>
        /// 往前检测几帧，放宽打中拍子的条件
        /// </summary>
        /// <param name="_checkLenght">往前检测多少帧</param>
        /// <returns></returns>
        private bool CheckBeat(int _checkLenght)
        {
            isHit = false;
            for (int ii = 0; ii < _checkLenght; ii++)
            {
                Beat beat;
                if (m_rhythmTool.beats.TryGetValue(m_rhythmTool.currentFrame - ii, out beat))
                {
                    //防止检测重
                    if (beat.index != m_lastBeatIndex)
                    {
                        isHit = true;
                        return isHit;
                    }

                }
            }
            return isHit;
        }

        private void OnOnset(OnsetType arg1, Onset arg2)
        {

        }

        private void OnDestroy()
        {
            base.OnDestroy();
            m_eventProvider.Onset += OnOnset;
            m_eventProvider.Beat += OnBeat;
            m_eventProvider.Change += OnChange;
            m_eventProvider.SongLoaded += OnSongLoaded;
            m_eventProvider.SongEnded += OnSongEnded;

            m_planMaterial.color = Color.black;
        }
    }
}