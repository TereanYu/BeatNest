#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			HealthSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;

namespace YU.ECS
{
    public class HealthSystem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }

        [Inject] DestroyEntityBarrier barrier;

        private static RenderMesh hurtRenderMesh;
        private static RenderMesh normalRenderMesh;

        struct JobProcess : IJobProcessComponentDataWithEntity<HealthComponent>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public void Execute(Entity entity, int index, ref HealthComponent health)
            {
                if (health.healthValue == 1 && health.currColor == 0)
                {
                    entityCommandBuffer.SetComponent(index, entity, new HealthComponent() { healthValue = 1, currColor = 1 });
                    entityCommandBuffer.SetSharedComponent<RenderMesh>(index, entity, hurtRenderMesh);
                }
                else if (health.healthValue == 2 && health.currColor == 1)
                {
                    entityCommandBuffer.SetComponent(index, entity, new HealthComponent() { healthValue = 2, currColor = 0 });
                    entityCommandBuffer.SetSharedComponent<RenderMesh>(index, entity, normalRenderMesh);
                }
                else if (health.healthValue <= 0)
                {
                    entityCommandBuffer.DestroyEntity(index, entity);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            var proto = GameObject.Find("HurtCubePrototype");
            hurtRenderMesh = proto.GetComponent<RenderMeshComponent>().Value;
            var proto2 = GameObject.Find("CubePrototype");
            normalRenderMesh = proto2.GetComponent<RenderMeshComponent>().Value;
            
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //CommandBuff不支持burst，所以这里没有burst compiler
            EntityCommandBuffer.Concurrent entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var job = new JobProcess {
                entityCommandBuffer = entityCommandBuffer
            };

            return job.Schedule(this, inputDeps);
        }
    }
}