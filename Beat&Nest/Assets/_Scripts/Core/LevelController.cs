#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			LevelController
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace YU.ECS
{
    public enum LevelType
    {
        GenerateEnemy,
        GenerateLittleEnemy,
        GenerateLaser,
        GenerateBombLaser
    }
    
    public class Level
    {
        public float m_triggerTime;
        public LevelType m_levelType;
        public int m_generateNum;
        //for enemy
        public List<float3> m_enemyPositions;

        //for laser
        public List<float3> m_laserStartPositions;
        public List<float3> m_laserDirections;

    }

    public struct Teach
    {
        public float m_triggerTime;
        public string m_chineseText;
        public string m_englishText;
    }

    /// <summary>
    /// 关卡控制器
    /// </summary>
    public class LevelController : MonoSingleton<LevelController>
    {
        [HideInInspector]
        public int m_currLevelIndex;
        [HideInInspector]
        public int m_currTeachIndex;
        [SerializeField]
        public List<Level> m_levelList = new List<Level>();
        [SerializeField]
        public List<Teach> m_teachist = new List<Teach>();

        private void Awake()
        {
            //初始化
            m_currLevelIndex = -1;
            m_currTeachIndex = -1;

            //初始化教程
            InitTeach();

            Level_LoopLittleEnemy(3f);

            //初始化关卡
            Level_TwoBigEnemy(15f);
            Level_TwoUpCrossLaser(29.5f);
            Level_FourLittleEnemy(44.5f);
            Level_HighQiLai(59f);
            Level_VerticalLaser(75.5f);
            Level_HorizontalLaser(85f);
            Level_RotateGenerateBigEnemy(111f);
            Level_LoopLittleEnemy(124f);
            //过渡
            Level_LoopLittleEnemy(140f);
            Level_HighQiLai(143f);
            Level_VerticalLaser(154f);
            Level_HorizontalLaser(156f);
            Level_RotateGenerateBigEnemy(160f);
            Level_LoopLittleEnemy(170f);
            Level_LoopLittleEnemy(171f);
            //大高潮
            Level_CrossLaser(184f);
            Level_LoopLittleEnemy(186f);
            Level_LoopLittleEnemy(187f);
            Level_LoopLittleEnemy(188f);
            Level_LoopLittleEnemy(189f);
            Level_RotateGenerateBigEnemy(195f);
            Level_HorizontalLaser(198f);
            Level_VerticalLaser(200f);
            Level_HighQiLai(204f);

        }

        private void InitTeach()
        {
            Teach teach1 = new Teach();
            teach1.m_triggerTime = 0.1f;
            teach1.m_chineseText = "保护你的巢穴！";
            teach1.m_englishText = "Protect Your Nest！";
            m_teachist.Add(teach1);

            Teach teach2 = new Teach();
            teach2.m_triggerTime = 3f;
            teach2.m_chineseText = "跟着节奏按空格就可以生成士兵";
            teach2.m_englishText = "Generate your soldier by pressing SPACE along with the Beat";
            m_teachist.Add(teach2);

            Teach teach10 = new Teach();
            teach10.m_triggerTime = 6f;
            teach10.m_chineseText = "按错节奏会引来敌人哦";
            teach10.m_englishText = "The wrong rhythm will attract enemies";
            m_teachist.Add(teach10);

            Teach teach3 = new Teach();
            teach3.m_triggerTime = 9f;
            teach3.m_chineseText = "按住鼠标左键控制士兵移动";
            teach3.m_englishText = "Hold down the left mouse button to control soldiers";
            m_teachist.Add(teach3);

            Teach teach4 = new Teach();
            teach4.m_triggerTime = 15f;
            teach4.m_chineseText = "让你的士兵撞击敌人，别让敌人靠近你的巢穴";
            teach4.m_englishText = "Let your soldiers hit them and keep enemies away from your nest";
            m_teachist.Add(teach4);

            Teach teach5 = new Teach();
            teach5.m_triggerTime = 23f;
            teach5.m_chineseText = "你的士兵受伤了！让它们回家可以治愈它们";
            teach5.m_englishText = "Your soldiers are wounded! Bringing them home can cure them";
            m_teachist.Add(teach5);

            Teach teach6 = new Teach();
            teach6.m_triggerTime = 30f;
            teach6.m_chineseText = "小心激光，它们不会伤害巢穴，但会伤害士兵";
            teach6.m_englishText = "Be careful with lasers, they don't damage nests, but they do damage soldiers";
            m_teachist.Add(teach6);

            Teach teach7 = new Teach();
            teach7.m_triggerTime = 35f;
            teach7.m_chineseText = "你可以按住鼠标右键让士兵散开";
            teach7.m_englishText = "You can hold down the right mouse button to disperse the soldiers";
            m_teachist.Add(teach7);

            Teach teach8 = new Teach();
            teach8.m_triggerTime = 50f;
            teach8.m_chineseText = "巢穴护佑着你，它也期待你的保护，加油！";
            teach8.m_englishText = "The nest guards you, and it expects you to protect it.Fighting！ ";
            m_teachist.Add(teach8);
        }

        public void StartLevelTimer()
        {
            InvokeRepeating("MyTimer", 0f, 0.1f);
        }

        private float m_currTime = 0f;
        public void MyTimer()
        {
            if (m_levelList.Count <= m_currLevelIndex + 1)
            {
                CancelInvoke("MyTimer");
                return;
            }
            if (m_levelList[m_currLevelIndex + 1].m_triggerTime <= m_currTime)
            {
                m_currLevelIndex++;
            }

            if (m_currTeachIndex+1<m_teachist.Count && m_teachist[m_currTeachIndex + 1].m_triggerTime <= m_currTime)
            {
                m_currTeachIndex++;
            }

            m_currTime += 0.1f;
        }

        #region 关卡模式，后续可用于随机关卡生成，且关卡模式间可以嵌套

        //2个大怪物，教学用
        private void Level_TwoBigEnemy(float _startTime)
        {
            /*Level1 Init*/
            Level level1 = new Level();
            level1.m_triggerTime = _startTime;
            level1.m_levelType = LevelType.GenerateEnemy;
            level1.m_generateNum = 2;
            //for Enemy
            level1.m_enemyPositions = new List<float3>();
            level1.m_enemyPositions.Add(new float3(-980f, 0f, 570f));
            level1.m_enemyPositions.Add(new float3(980f, 0f, 570f));
            m_levelList.Add(level1);
            /*Level1 End Init*/

        }

        //2道激光，教学用
        private void Level_TwoUpCrossLaser(float _startTime)
        {
            /*Level2 Init*/
            Level level2 = new Level();
            level2.m_triggerTime = _startTime;
            level2.m_levelType = LevelType.GenerateLaser;
            level2.m_generateNum = 2;
            //for Laser
            level2.m_laserStartPositions = new List<float3>();
            level2.m_laserStartPositions.Add(new float3(-665f, 0f, 0f));
            level2.m_laserStartPositions.Add(new float3(665f, 0f, 0f));
            level2.m_laserDirections = new List<float3>();
            level2.m_laserDirections.Add(new float3(1.5f, 0f, 1f));
            level2.m_laserDirections.Add(new float3(-1.5f, 0f, 1f));
            m_levelList.Add(level2);
            /*Level2 End Init*/
        }

        //4小怪物,教学用
        private void Level_FourLittleEnemy(float _startTime)
        {
            Level level2 = new Level();
            level2.m_triggerTime = _startTime;
            level2.m_levelType = LevelType.GenerateLittleEnemy;
            level2.m_generateNum = 4;
            //for Enemy
            level2.m_enemyPositions = new List<float3>();
            level2.m_enemyPositions.Add(new float3(-980f, 0f, 570f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, -570f));
            level2.m_enemyPositions.Add(new float3(-980f, 0f, -570f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, 570f));
            m_levelList.Add(level2);
        }

        //生成八个小怪
        private void Level_EightLittleEnemy(float _startTime)
        {
            Level level2 = new Level();
            level2.m_triggerTime = _startTime;
            level2.m_levelType = LevelType.GenerateLittleEnemy;
            level2.m_generateNum = 8;
            //for Enemy
            level2.m_enemyPositions = new List<float3>();
            level2.m_enemyPositions.Add(new float3(-980f, 0f, 570f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, -570f));
            level2.m_enemyPositions.Add(new float3(-980f, 0f, -570f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, 570f));
            level2.m_enemyPositions.Add(new float3(-980f, 0f, 220f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, -220f));
            level2.m_enemyPositions.Add(new float3(-980f, 0f, -220f));
            level2.m_enemyPositions.Add(new float3(980f, 0f, 220f));
            m_levelList.Add(level2);
        }

        //棱形激光
        private void Level_DiamondLaser(float _startTime)
        {
            Level level3 = new Level();
            level3.m_triggerTime = _startTime;
            level3.m_levelType = LevelType.GenerateLaser;
            level3.m_generateNum = 4;
            //for Laser
            level3.m_laserStartPositions = new List<float3>();
            level3.m_laserStartPositions.Add(new float3(-665f, 0f, 0f));
            level3.m_laserStartPositions.Add(new float3(665f, 0f, 0f));
            level3.m_laserStartPositions.Add(new float3(-665f, 0f, 0f));
            level3.m_laserStartPositions.Add(new float3(665f, 0f, 0f));
            level3.m_laserDirections = new List<float3>();
            level3.m_laserDirections.Add(new float3(1.5f, 0f, 1f));
            level3.m_laserDirections.Add(new float3(-1.5f, 0f, 1f));
            level3.m_laserDirections.Add(new float3(-1.5f, 0f, 1f));
            level3.m_laserDirections.Add(new float3(1.5f, 0f, 1f));
            m_levelList.Add(level3);
        }

        //4个大怪生成
        private void Level_FourBigEnemy(float _startTime)
        {
            Level level1 = new Level();
            level1.m_triggerTime = _startTime;
            level1.m_levelType = LevelType.GenerateEnemy;
            level1.m_generateNum = 4;
            //for Enemy
            level1.m_enemyPositions = new List<float3>();
            level1.m_enemyPositions.Add(new float3(-980f, 0f, 570f));
            level1.m_enemyPositions.Add(new float3(980f, 0f, -570f));
            level1.m_enemyPositions.Add(new float3(-980f, 0f, -570f));
            level1.m_enemyPositions.Add(new float3(980f, 0f, 570f));
            m_levelList.Add(level1);
        }

        //高潮用混合模式，嵌套其他关卡模式
        private void Level_HighQiLai(float _startTime)
        {
            Level_EightLittleEnemy(_startTime);

            Level_EightLittleEnemy(_startTime + 3f);

            Level_DiamondLaser(_startTime + 8f);

            Level_FourBigEnemy(_startTime + 9f);

        }

        //竖向激光大阵
        private void Level_VerticalLaser(float _startTime)
        {
            Level level6 = new Level();
            level6.m_triggerTime = _startTime;
            level6.m_levelType = LevelType.GenerateLaser;
            level6.m_generateNum = 1;
            level6.m_laserStartPositions = new List<float3>();
            level6.m_laserStartPositions.Add(new float3(-650f, 0f, 0f));
            level6.m_laserDirections = new List<float3>();
            level6.m_laserDirections.Add(new float3(0, 0f, 1f));
            m_levelList.Add(level6);

            Level level7 = new Level();
            level7.m_triggerTime = _startTime + 0.5f;
            level7.m_levelType = LevelType.GenerateLaser;
            level7.m_generateNum = 1;
            level7.m_laserStartPositions = new List<float3>();
            level7.m_laserStartPositions.Add(new float3(-350f, 0f, 0f));
            level7.m_laserDirections = new List<float3>();
            level7.m_laserDirections.Add(new float3(0, 0f, 1f));
            m_levelList.Add(level7);

            Level level8 = new Level();
            level8.m_triggerTime = _startTime + 1f;
            level8.m_levelType = LevelType.GenerateLaser;
            level8.m_generateNum = 1;
            level8.m_laserStartPositions = new List<float3>();
            level8.m_laserStartPositions.Add(new float3(350f, 0f, 0f));
            level8.m_laserDirections = new List<float3>();
            level8.m_laserDirections.Add(new float3(0, 0f, 1f));
            m_levelList.Add(level8);

            Level level9 = new Level();
            level9.m_triggerTime = _startTime + 1.5f;
            level9.m_levelType = LevelType.GenerateLaser;
            level9.m_generateNum = 1;
            level9.m_laserStartPositions = new List<float3>();
            level9.m_laserStartPositions.Add(new float3(650f, 0f, 0f));
            level9.m_laserDirections = new List<float3>();
            level9.m_laserDirections.Add(new float3(0, 0f, 1f));
            m_levelList.Add(level9);
        }

        //横向激光大阵
        private void Level_HorizontalLaser(float _startTime)
        {
            Level level6 = new Level();
            level6.m_triggerTime = _startTime;
            level6.m_levelType = LevelType.GenerateLaser;
            level6.m_generateNum = 1;
            level6.m_laserStartPositions = new List<float3>();
            level6.m_laserStartPositions.Add(new float3(0f, 0f, 400f));
            level6.m_laserDirections = new List<float3>();
            level6.m_laserDirections.Add(new float3(1f, 0f, 0f));
            m_levelList.Add(level6);

            Level level7 = new Level();
            level7.m_triggerTime = _startTime + 0.5f;
            level7.m_levelType = LevelType.GenerateLaser;
            level7.m_generateNum = 1;
            level7.m_laserStartPositions = new List<float3>();
            level7.m_laserStartPositions.Add(new float3(0f, 0f, 220f));
            level7.m_laserDirections = new List<float3>();
            level7.m_laserDirections.Add(new float3(1f, 0f, 0f));
            m_levelList.Add(level7);

            Level level8 = new Level();
            level8.m_triggerTime = _startTime + 1f;
            level8.m_levelType = LevelType.GenerateLaser;
            level8.m_generateNum = 1;
            level8.m_laserStartPositions = new List<float3>();
            level8.m_laserStartPositions.Add(new float3(0f, 0f, -220f));
            level8.m_laserDirections = new List<float3>();
            level8.m_laserDirections.Add(new float3(1f, 0f, 0f));
            m_levelList.Add(level8);

            Level level9 = new Level();
            level9.m_triggerTime = _startTime + 1.5f;
            level9.m_levelType = LevelType.GenerateLaser;
            level9.m_generateNum = 1;
            level9.m_laserStartPositions = new List<float3>();
            level9.m_laserStartPositions.Add(new float3(0f, 0f, -400f));
            level9.m_laserDirections = new List<float3>();
            level9.m_laserDirections.Add(new float3(1f, 0f, 0f));
            m_levelList.Add(level9);
        }

        //旋转生成大怪
        private void Level_RotateGenerateBigEnemy(float _startTime)
        {
            Level level1 = new Level();
            level1.m_triggerTime = _startTime;
            level1.m_levelType = LevelType.GenerateEnemy;
            level1.m_generateNum = 1;
            level1.m_enemyPositions = new List<float3>();
            level1.m_enemyPositions.Add(new float3(-700f, 0f, 450f));
            m_levelList.Add(level1);

            Level level2 = new Level();
            level2.m_triggerTime = _startTime + 0.5f;
            level2.m_levelType = LevelType.GenerateEnemy;
            level2.m_generateNum = 1;
            level2.m_enemyPositions = new List<float3>();
            level2.m_enemyPositions.Add(new float3(700f, 0f, 450f));
            m_levelList.Add(level2);

            Level level3 = new Level();
            level3.m_triggerTime = _startTime + 1f;
            level3.m_levelType = LevelType.GenerateEnemy;
            level3.m_generateNum = 1;
            level3.m_enemyPositions = new List<float3>();
            level3.m_enemyPositions.Add(new float3(700f, 0f, -450f));
            m_levelList.Add(level3);

            Level level4 = new Level();
            level4.m_triggerTime = _startTime + 1.5f;
            level4.m_levelType = LevelType.GenerateEnemy;
            level4.m_generateNum = 1;
            level4.m_enemyPositions = new List<float3>();
            level4.m_enemyPositions.Add(new float3(-700f, 0f, -450f));
            m_levelList.Add(level4);
        }

        //生成一圈小怪物
        private void Level_LoopLittleEnemy(float _startTime)
        {
            Level level2 = new Level();
            level2.m_triggerTime = _startTime;
            level2.m_levelType = LevelType.GenerateLittleEnemy;
            level2.m_generateNum = 8;
            //for Enemy
            level2.m_enemyPositions = new List<float3>();
            level2.m_enemyPositions.Add(new float3(-1000f, 0f, 0f));
            level2.m_enemyPositions.Add(new float3(-750f, 0f, 750f));
            level2.m_enemyPositions.Add(new float3(0f, 0f, 1000f));
            level2.m_enemyPositions.Add(new float3(750f, 0f, 750f));
            level2.m_enemyPositions.Add(new float3(1000f, 0f, 0f));
            level2.m_enemyPositions.Add(new float3(750f, 0f, -750f));
            level2.m_enemyPositions.Add(new float3(0f, 0f, -1000f));
            level2.m_enemyPositions.Add(new float3(-750f, 0f, -750f));
            m_levelList.Add(level2);
        }

        //十字激光
        private void Level_CrossLaser(float _startTime)
        {
            Level level6 = new Level();
            level6.m_triggerTime = _startTime;
            level6.m_levelType = LevelType.GenerateLaser;
            level6.m_generateNum = 1;
            level6.m_laserStartPositions = new List<float3>();
            level6.m_laserStartPositions.Add(new float3(0f, 0f, 0f));
            level6.m_laserDirections = new List<float3>();
            level6.m_laserDirections.Add(new float3(1f, 0f, 0f));
            m_levelList.Add(level6);

            Level level7 = new Level();
            level7.m_triggerTime = _startTime + 0.5f;
            level7.m_levelType = LevelType.GenerateLaser;
            level7.m_generateNum = 1;
            level7.m_laserStartPositions = new List<float3>();
            level7.m_laserStartPositions.Add(new float3(0f, 0f, 0f));
            level7.m_laserDirections = new List<float3>();
            level7.m_laserDirections.Add(new float3(0f, 0f, -1f));
            m_levelList.Add(level7);

            Level level8 = new Level();
            level8.m_triggerTime = _startTime + 1f;
            level8.m_levelType = LevelType.GenerateLaser;
            level8.m_generateNum = 1;
            level8.m_laserStartPositions = new List<float3>();
            level8.m_laserStartPositions.Add(new float3(0f, 0f, 0f));
            level8.m_laserDirections = new List<float3>();
            level8.m_laserDirections.Add(new float3(-1f, 0f, 0f));
            m_levelList.Add(level8);

            Level level9 = new Level();
            level9.m_triggerTime = _startTime + 1.5f;
            level9.m_levelType = LevelType.GenerateLaser;
            level9.m_generateNum = 1;
            level9.m_laserStartPositions = new List<float3>();
            level9.m_laserStartPositions.Add(new float3(0f, 0f, 0f));
            level9.m_laserDirections = new List<float3>();
            level9.m_laserDirections.Add(new float3(0f, 0f, 1f));
            m_levelList.Add(level9);
        }
        #endregion
    }
}