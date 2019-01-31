#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			LaserComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Mathematics;

namespace YU.ECS
{
    public struct LaserComponent : IComponentData
    {
        //激光是一条直线，这是直线上随意一点
        public float3 startPoint;
        //直线的方向
        public float3 direction;
        //激光存在的时间
        public half lifeTime; 
    }
}
