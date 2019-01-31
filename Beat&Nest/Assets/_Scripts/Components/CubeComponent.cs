#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			CubeComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Mathematics;

namespace YU.ECS
{
    public struct CubeComponent : IComponentData
    {
        public float3 position;
        public float3 velocity { get; set; }
        public float3 acceration;

        public float mass { get; set; }
        public float radius { get; set; }

        //最大速度模长
        public float maxLength { get; set; }

        public half isInEnemy;
    }
}