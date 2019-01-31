#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			HomeComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Mathematics;

namespace YU.ECS
{
    public struct HomeComponent : IComponentData
    { 
        //用一个组件来跟主线程进行通信
        public half health;
    }
}