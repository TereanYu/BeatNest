#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			EnemyDistanceSystem
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
    public class EnemyDistanceSystem : JobComponentSystem
    {

        public struct Group
        {
            public ComponentDataArray<HomeComponent> home;
        }

        [Inject] private Group _Group;


        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, EnemyComponent>
        {
            public Group group;
            //判断敌人是否碰到巢穴
            public void Execute(ref Position position, ref EnemyComponent enemy)
            {
                if (math.length(position.Value) <= 50)
                {
                    group.home[0] = new HomeComponent { health = group.home[0].health - 1 };
                    enemy.health = 0;
                }
            }
        }


        //系统会每帧调用这个函数
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            UIManager.Instance.ChangeHomeHPText((int)_Group.home[0].health);

            //初始化一个job
            var job = new JobProcess {group = _Group };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}