#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			DistanceSystem
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
    public class DistanceSystem : JobComponentSystem
    {

        public struct Group
        {
            public ComponentDataArray<EnemyComponent> Enemys;
        }
        public struct GroupCube
        {
            public ComponentDataArray<CubeComponent> Cubes;
        }
        public struct GroupLaser
        {
            public ComponentDataArray<LaserComponent> Lasers;
        }

        [Inject] private Group _Group;
        [Inject] private GroupCube _GroupCube;
        [Inject] private GroupLaser _GroupLaser;


        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, CubeComponent, HealthComponent>
        {
            
            public Group group;
            public GroupLaser groupLaser;

            private half isOutEnemy;

            public void Execute(ref Position position, ref CubeComponent cube, ref HealthComponent health)
            {
                isOutEnemy = 1;
                for (int ii = 0;ii<group.Enemys.Length;ii++)
                {
                    if (group.Enemys[ii].enemyType == 0)
                    {
                        //判断是否在敌人范围内，是就扣血
                        if (math.abs(group.Enemys[ii].position.x - position.Value.x) < 25f &&
                            math.abs(group.Enemys[ii].position.z - position.Value.z) < 25f)
                        {
                            if (cube.isInEnemy == 0)
                            {
                                group.Enemys[ii] = new EnemyComponent { enemyType= 0 , health = group.Enemys[ii].health - 1, position = group.Enemys[ii].position };
                                health.ChangeHealth(-1);
                                cube.isInEnemy = 1;
                            }
                            isOutEnemy = 0;
                        }
                    }
                    else if (group.Enemys[ii].enemyType == 1)
                    {
                        //判断是否在敌人范围内，是就扣血
                        if (math.abs(group.Enemys[ii].position.x - position.Value.x) < 7.5f &&
                            math.abs(group.Enemys[ii].position.z - position.Value.z) < 7.5f)
                        {
                            if (cube.isInEnemy == 0)
                            {
                                group.Enemys[ii] = new EnemyComponent { enemyType = 1, health = group.Enemys[ii].health - 1, position = group.Enemys[ii].position };
                                health.ChangeHealth(-1);
                                cube.isInEnemy = 1;
                            }
                            isOutEnemy = 0;
                        }
                    }

                }
                if (isOutEnemy == 1)
                {   //如果不在任何敌人内部，才是不在敌人内部
                    cube.isInEnemy = 0;
                }

                if (math.length(position.Value) <= 50)
                {
                    //回家回血
                    health.ChangeHealth(1);
                }

                for (int jj = 0; jj < groupLaser.Lasers.Length; jj++)
                {
                    //激光伤害
                    if (groupLaser.Lasers[jj].lifeTime <= 0.2f
                        &&math.length(math.cross(groupLaser.Lasers[jj].direction, position.Value - groupLaser.Lasers[jj].startPoint)) <= 50f)
                    {
                        health.ChangeHealth(-2);
                    }
                }
            }


        }


        //系统会每帧调用这个函数
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //Cube计数
            UIManager.Instance.ChangeCubeNumText(_GroupCube.Cubes.Length);

            //初始化一个job
            var job = new JobProcess { group = _Group , groupLaser = _GroupLaser };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}