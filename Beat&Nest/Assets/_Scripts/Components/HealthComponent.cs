#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			HealthComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;
using Unity.Mathematics;

namespace YU.ECS
{
    public struct HealthComponent : IComponentData
    {
        //生命值组件，用在cube身上
        public half healthValue;


        /// <summary>
        /// 0 = normal
        /// 1 = red
        /// </summary>
        public half currColor;

        public void ChangeHealth(half changeValue)
        {
            healthValue += changeValue;
            if (healthValue < 0)
            {
                healthValue = 0;
            }
            if (healthValue > 2)
            {
                healthValue = 2;
            }
        }
    }
}