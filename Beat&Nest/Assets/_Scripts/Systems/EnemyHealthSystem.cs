#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			EnemyHealthSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace YU.ECS
{
    public class EnemyHealthSystem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }

        [Inject] DestroyEntityBarrier barrier;

        //敌人受伤闪烁用
        //private static RenderMesh enemyHurtRenderMesh;
        //private static RenderMesh normalRenderMesh;

        static half results;

        struct JobProcess : IJobProcessComponentDataWithEntity<EnemyComponent>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;

            public half results;

            public void Execute(Entity entity, int index, ref EnemyComponent enemy)
            {
                results = 0;
                if (enemy.health <= 0)
                {
                    results = 1;
                    entityCommandBuffer.DestroyEntity(index, entity);
                }
            }
        }



        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            EntityCommandBuffer.Concurrent entityCommandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            
            //初始化一个job
            var job = new JobProcess
            {
                entityCommandBuffer = entityCommandBuffer,
                results = results
            };

            if (results == 1)
            {
                UIManager.Instance.ChangeHomeHPText(-1);
            }

            //开始job      
            return job.Schedule(this, inputDeps);
        }

        
    }
}