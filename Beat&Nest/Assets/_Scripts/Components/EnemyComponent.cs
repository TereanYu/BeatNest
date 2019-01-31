#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			EnemyComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Mathematics;

namespace YU.ECS
{
    public struct EnemyComponent : IComponentData
    {
        //怪物类型0是大红怪，1是小红怪
        public half enemyType;
        public float3 position;
        public float3 velocity { get; set; }

        public half health;

    }
}