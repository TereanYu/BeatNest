#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			LaserHealthSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;

namespace YU.ECS
{
    public class LaserHealthSystem : JobComponentSystem
    {
        public class DestroyEntityBarrier : BarrierSystem
        { }
        public struct Group
        {
            // A long list of different ComponentDataArray
            public ComponentDataArray<LaserComponent> lasers;
        }

        [Inject] private Group _Group;

        [Inject] DestroyEntityBarrier barrier;


        //控制激光的生命周期变化
        struct JobProcess : IJobProcessComponentDataWithEntity<LaserComponent,Scale>
        {
            public EntityCommandBuffer.Concurrent entityCommandBuffer;
            public float deltaTime;

            public void Execute(Entity entity, int index, ref LaserComponent laser, ref Scale scale)
            {
                laser.lifeTime = laser.lifeTime - deltaTime;


                if (laser.lifeTime > 0.5 && laser.lifeTime <= 2)
                {
                    //do not change
                }
                else if (laser.lifeTime > 0.2 && laser.lifeTime <= 0.25)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (0.25f - laser.lifeTime) * 1000);
                }
                else if (laser.lifeTime > 0.1 && laser.lifeTime <= 0.2)
                {
                    //do not change
                }
                else if (laser.lifeTime > 0 && laser.lifeTime <= 0.1)
                {
                    scale.Value = new float3(scale.Value.x, 1f, (laser.lifeTime) * 500);
                }
                else if (laser.lifeTime <= 0)
                {
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
                deltaTime = Time.deltaTime
            };

            //开始job      
            return job.Schedule(this, inputDeps);
        }


    }
}