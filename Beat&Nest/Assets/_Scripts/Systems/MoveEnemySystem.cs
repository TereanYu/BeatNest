#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			MoveEnemySystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

namespace YU.ECS
{
    public class MoveEnemySystem : JobComponentSystem
    {
        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, EnemyComponent>
        {

            //Job的执行函数，作用是控制enemy的运动
            public void Execute(ref Position position, ref EnemyComponent enemy)
            {

                if (enemy.enemyType == 0)
                {
                    float3 forceDir = (float3.zero - enemy.position);
                    forceDir = math.normalize(forceDir) * 1f;
                    enemy.velocity = forceDir;

                    //根据速度变化位置
                    enemy.position += enemy.velocity;

                    //应用cube位置到Position
                    position.Value = enemy.position;
                }
                else
                {
                    float3 forceDir = (float3.zero - enemy.position);
                    forceDir = math.normalize(forceDir) * 5f;
                    enemy.velocity = forceDir;

                    //根据速度变化位置
                    enemy.position += enemy.velocity;

                    //应用cube位置到Position
                    position.Value = enemy.position;
                }

            }

        }

        //系统会每帧调用这个函数
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            //初始化一个job
            var job = new JobProcess { };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}