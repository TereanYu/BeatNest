#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			GenerateEnemySystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

namespace YU.ECS
{
    public class GenerateEnemySystem : ComponentSystem
    {
        public static EntityArchetype EnemyArchetype;
        public static EntityArchetype LaserArchetype;
        private static RenderMesh enemyRenderer;
        private static RenderMesh laserRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {

            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            EnemyArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(EnemyComponent)
            );

            LaserArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(LaserComponent)
            );
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            enemyRenderer = GetLookFromPrototype("EnemyPrototype");
            laserRenderer = GetLookFromPrototype("LaserPrototype");
        }

        public static RenderMesh GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<RenderMeshComponent>().Value;
            return result;
        }


        private static int oldLevelIndex = -1;
        protected override void OnUpdate()
        {
            if (LevelController.Instance.m_currLevelIndex > oldLevelIndex)
            {
                //更新上一次的levelindex
                oldLevelIndex = LevelController.Instance.m_currLevelIndex;

                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                if (LevelController.Instance.m_levelList[oldLevelIndex].m_levelType == LevelType.GenerateEnemy)
                {
                    foreach (float3 pos in LevelController.Instance.m_levelList[oldLevelIndex].m_enemyPositions)
                    {
                        Entity enemy = entityManager.CreateEntity(EnemyArchetype);
                        //设置给实体
                        entityManager.SetComponentData(enemy, new Position { Value = pos });
                        entityManager.SetComponentData(enemy, new Scale { Value = new float3(50, 1, 50) });

                        //enemy属性
                        EnemyComponent c = new EnemyComponent
                        {
                            enemyType = 0,
                            position = pos,
                            velocity = Vector3.zero,
                            health = 20000
                        };

                        entityManager.SetComponentData(enemy, c);

                        entityManager.SetSharedComponentData(enemy, enemyRenderer);
                    }
                }
                else if (LevelController.Instance.m_levelList[oldLevelIndex].m_levelType == LevelType.GenerateLittleEnemy)
                {
                    foreach (float3 pos in LevelController.Instance.m_levelList[oldLevelIndex].m_enemyPositions)
                    {
                        Entity enemy = entityManager.CreateEntity(EnemyArchetype);
                        //设置给实体
                        entityManager.SetComponentData(enemy, new Position { Value = pos });
                        entityManager.SetComponentData(enemy, new Scale { Value = new float3(15, 1, 15) });

                        //enemy属性
                        EnemyComponent c = new EnemyComponent
                        {
                            enemyType = 1,
                            position = pos,
                            velocity = Vector3.zero,
                            health = 100
                        };

                        entityManager.SetComponentData(enemy, c);

                        entityManager.SetSharedComponentData(enemy, enemyRenderer);
                    }
                }
                else if (LevelController.Instance.m_levelList[oldLevelIndex].m_levelType == LevelType.GenerateLaser)
                {
                    for (int jj = 0; jj < LevelController.Instance.m_levelList[oldLevelIndex].m_generateNum; jj++)
                    {
                        Entity laser = entityManager.CreateEntity(LaserArchetype);

                        float3 dir = LevelController.Instance.m_levelList[oldLevelIndex].m_laserDirections[jj];

                        //设置给实体
                        entityManager.SetComponentData(laser, new Position { Value = LevelController.Instance.m_levelList[oldLevelIndex].m_laserStartPositions[jj] });
                        entityManager.SetComponentData(laser, new Rotation { Value = Quaternion.FromToRotation(Vector3.right, dir) });
                        entityManager.SetComponentData(laser, new Scale { Value = new float3(5000, 1, 5) });

                        //laser属性
                        LaserComponent c = new LaserComponent
                        {
                            startPoint = LevelController.Instance.m_levelList[oldLevelIndex].m_laserStartPositions[jj],
                            direction = dir,
                            lifeTime = 2
                        };

                        entityManager.SetComponentData(laser, c);

                        entityManager.SetSharedComponentData(laser, laserRenderer);
                    }
                }
                else
                {

                }


            }

            //如果按错节奏，生成新敌人
            if (MusicController.Instance.isBadPressAndWaitGenerate)
            {
                MusicController.Instance.isBadPressAndWaitGenerate = false;
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < 1; i++)
                {
                    Entity enemy = entityManager.CreateEntity(EnemyArchetype);

                    //初始化随机点
                    float3 initialPosition = math.normalize( new float3(UnityEngine.Random.insideUnitCircle.x, 0f, UnityEngine.Random.insideUnitCircle.y));
                    //设置给实体
                    entityManager.SetComponentData(enemy, new Position { Value = initialPosition*750f });
                    entityManager.SetComponentData(enemy, new Scale { Value = new float3(50,1,50) });

                    //enemy属性
                    EnemyComponent c = new EnemyComponent
                    {
                        enemyType = 0,
                        position = initialPosition * 750f,
                        velocity = Vector3.zero,
                        health = 20000
                    };

                    entityManager.SetComponentData(enemy, c);

                    entityManager.SetSharedComponentData(enemy, enemyRenderer);

                }
            }
        }
    }
}