#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			ForceComponent
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Mathematics;
using Unity.Entities;


namespace YU.ECS
{        
    //力组件
    public struct ForceComponent : IComponentData
    {

        public enum ForceMode
        {
            PUSH,
            PULL
        }

        //重力常量
        public const float G = 9.8f;

        //摩擦因数
        public float frictionCoe;

        //鼠标质量
        public float Mass;

        //x, y, w, h
        public float4 bound;

        //加力
        public float3 CastForce(ref float3 position, ref CubeComponent b, ForceMode forceMode)
        {
            float3 forceDir = (position - b.position);
            float d = math.length(forceDir);
            d = math.clamp(d, 1, 25);
            forceDir = math.normalize(forceDir);
            // F = GMm / d^2
            float strength = (G * Mass * b.mass) / (d * d);
            forceDir *= strength;
            return (forceMode == ForceMode.PUSH) ? forceDir * -1 : forceDir;
        }
       
    }
}