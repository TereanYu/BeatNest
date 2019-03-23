#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			GameControlSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace YU.ECS
{
    public class GameControlSystem : ComponentSystem
    {

        public struct GroupEnemy
        {
            public ComponentDataArray<EnemyComponent> Enemys;
        }
        public struct GroupCube
        {
            public ComponentDataArray<Position> Pos;
            public ComponentDataArray<CubeComponent> Cubes;
            public ComponentDataArray<HealthComponent> CubeHealths;
        }
        public struct GroupHome
        {
            public ComponentDataArray<HomeComponent> Homes;
        }

        [Inject] private GroupCube _cubes;
        [Inject] private GroupEnemy _enemys;
        [Inject] private GroupHome _homes;

        

        protected override void OnUpdate()
        {
            if (Game.Instance.isResetGame)
            {
                Game.Instance.isResetGame = false;

                float3 _resetPos = new float3(100, 0, 100);

                for (int mm = 0; mm < _cubes.Pos.Length; mm++)
                {
                    _cubes.Pos[mm] = new Position { Value = _resetPos };
                }
                for (int hh = 0; hh < _cubes.Cubes.Length; hh++)
                {
                    _cubes.Cubes[hh] = new CubeComponent { position = _resetPos };
                }
                for (int ii = 0; ii < _cubes.CubeHealths.Length; ii++)
                {
                    _cubes.CubeHealths[ii] = new HealthComponent { currColor = 1, healthValue = 0 };
                }
                for (int jj = 0; jj < _enemys.Enemys.Length; jj++)
                {
                    _enemys.Enemys[jj] = new EnemyComponent { enemyType = 0, health = 0, position = float3.zero } ;
                }
                for (int kk = 0; kk < _homes.Homes.Length; kk++)
                {
                    _homes.Homes[kk] = new HomeComponent { health = 20 };
                }
            }
        }
    }
}

