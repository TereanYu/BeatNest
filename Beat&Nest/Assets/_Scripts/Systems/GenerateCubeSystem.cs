#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			GenerateCubeSystem
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
    public class GenerateCubeSystem : ComponentSystem
    {
        
        public static EntityArchetype CubeArchetype;
        private static RenderMesh cubeRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {

            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            CubeArchetype = entityManager.CreateArchetype(
                typeof(Position),
                typeof(RenderMesh),
                typeof(CubeComponent),
                typeof(ForceComponent),
                typeof(HealthComponent)
            );
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            cubeRenderer = GetLookFromPrototype("CubePrototype");
        }

        public static RenderMesh GetLookFromPrototype(string protoName)
        {
            var proto = GameObject.Find(protoName);
            var result = proto.GetComponent<RenderMeshComponent>().Value;
            return result;
        }

        protected override void OnUpdate()
        {
            if (MusicController.Instance.isPressAndWaitGenerate)
            {
                MusicController.Instance.isPressAndWaitGenerate = false;

                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i < 500* MusicController.Instance.m_currBeatScore; i++)
                {
                    Entity cube = entityManager.CreateEntity(CubeArchetype);

                    float3 randomVel = UnityEngine.Random.onUnitSphere;

                    //初始化随机点
                    float3 initialPosition = new float3(randomVel.x, 0f, randomVel.z);
                    ;
                    //设置给实体
                    entityManager.SetComponentData(cube, new Position { Value = initialPosition });

                    

                    //cube属性
                    CubeComponent c = new CubeComponent
                    {
                        position = initialPosition,
                        radius = 1,
                        mass = 1,
                        maxLength = 20,
                        velocity = new float3(randomVel.x*100f,0f, randomVel.z*100f),
                        acceration = float3.zero,
                        isInEnemy = 0
                    };

                    entityManager.SetComponentData(cube, c);

                    //边界
                    float4 v = new float4(-960f, -540f, 960f, 540f);

                    //力属性初始化
                    ForceComponent f = new ForceComponent { Mass = 50f, bound = v, frictionCoe = 0.1f };

                    entityManager.SetComponentData(cube, f);

                    HealthComponent h = new HealthComponent { healthValue = 2, currColor = 0 };
                    //生命初始化
                    entityManager.SetComponentData(cube, h);


                    //这里为什么不用Add，因为从Entities中Add或Remove ComponentData会使其archetype改变，大量操作会影响性能
                    //entityManager.AddSharedComponentData(cube, cubeRenderer);
                    //共享数据，用于可以共享，且不经常改变的component
                    entityManager.SetSharedComponentData(cube, cubeRenderer);

                }
            }

            //for test
            if (Input.GetKeyDown(KeyCode.R))
            {
                var entityManager = World.Active.GetOrCreateManager<EntityManager>();
                for (int i = 0; i <10000; i++)
                {
                    Entity cube = entityManager.CreateEntity(CubeArchetype);

                    //初始化随机点
                    float3 initialPosition = new float3(UnityEngine.Random.Range(-960f, 960f), 0f, UnityEngine.Random.Range(-540f, 540f));
;
                    //设置给实体
                    entityManager.SetComponentData(cube, new Position { Value = initialPosition });

                    //cube属性
                    CubeComponent c = new CubeComponent
                    {
                        position = initialPosition,
                        radius = 1,
                        mass = 1,
                        maxLength = 20,
                        velocity = Vector3.zero,
                        acceration = Vector3.zero,
                        isInEnemy = 0
                    };

                    entityManager.SetComponentData(cube,c);

                    //边界
                    float4 v = new float4(-960f, -540f, 960f, 540f);

                    //力属性初始化
                    ForceComponent f = new ForceComponent { Mass = 50f, bound = v, frictionCoe = 0.1f };

                    entityManager.SetComponentData(cube, f);

                    HealthComponent h = new HealthComponent { healthValue = 2, currColor = 0 };
                    //生命初始化
                    entityManager.SetComponentData(cube, h);

                    entityManager.SetSharedComponentData(cube, cubeRenderer);

                }
            }
        }
    }
}