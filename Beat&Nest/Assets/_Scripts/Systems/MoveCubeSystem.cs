#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			MoveCubeSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

namespace YU.ECS
{
    public class MoveCubeSystem : JobComponentSystem
    {
        [BurstCompile]
        struct JobProcess : IJobProcessComponentData<Position, CubeComponent, ForceComponent,HealthComponent>
        {
            public bool isForceOn;
            public ForceComponent.ForceMode forceMode;
            public float3 mousePosition;

            //Job的执行函数，作用是控制cube的运动
            public void Execute(ref Position position, ref CubeComponent cube, ref ForceComponent forcefield ,ref HealthComponent health)
            {

                //加力
                if (isForceOn)
                {
                    float3 f = forcefield.CastForce(ref mousePosition, ref cube, forceMode);
                    ApplyForce(ref cube, f);
                }

                //速度小于0.1f直接抛弃
                if (math.length(cube.velocity) >= 0.1f)
                { 
                    //应用摩擦力
                    ApplyForce(ref cube, CalculateFriction(forcefield.frictionCoe, ref cube));
                }
                else
                {
                    Stop(ref cube);
                }

                //加速度
                cube.velocity += cube.acceration;

                //最大速度限制
                if (math.length(cube.velocity) > cube.maxLength)
                {
                    cube.velocity = math.normalize(cube.velocity);
                    cube.velocity *= cube.maxLength;
                }

                //边缘传送
                //CheckEdge(ref forcefield, ref cube);

                //边缘扣血测试用
                //if (cube.position.x > forcefield.bound.z)
                //{
                //    health.ChangeHealth(-1) ;
                //}

                //根据速度变化位置
                cube.position += cube.velocity;

                //应用cube位置到Position
                position.Value = cube.position;

                //重设加速度
                cube.acceration *= 0;
            }

            public void ApplyForce(ref CubeComponent b, float3 force)
            {
                //F = ma
                b.acceration = b.acceration + (force / b.mass);
            }

            public void Stop(ref CubeComponent b)
            {
                b.velocity *= 0;
            }

            float3 CalculateFriction(float coe, ref CubeComponent b)
            {
                float3 friction = b.velocity;
                friction *= -1;
                friction = math.normalize(friction);
                friction *= coe;

                return friction;
            }

            //边缘传送
            public void CheckEdge(ref ForceComponent forcefield, ref CubeComponent b)
            {
                if (forcefield.bound.z == 0) return;

                if (b.position.x > forcefield.bound.z)
                {
                    b.position.x = forcefield.bound.x;
                }
                else if (b.position.x < forcefield.bound.x)
                {
                    b.position.x = forcefield.bound.z;
                }

                if (b.position.z > forcefield.bound.w)
                {
                    b.position.z = forcefield.bound.y;
                }
                else if (b.position.z < forcefield.bound.y)
                {
                    b.position.z = forcefield.bound.w;
                }
            }
        }

        bool _isForceOn = false;
        ForceComponent.ForceMode _forceMode = ForceComponent.ForceMode.PULL;
        float3 mousePos;

        //系统会每帧调用这个函数
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
//#if UNITY_STANDALONE
            // ------- input handle ------
            //left - pull
            if (Input.GetMouseButtonDown(0))
            {
                _isForceOn = true;
                _forceMode = ForceComponent.ForceMode.PULL;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isForceOn = false;
            }

            //right - push
            if (Input.GetMouseButtonDown(1))
            {
                _isForceOn = true;
                _forceMode = ForceComponent.ForceMode.PUSH;
            }

            if (Input.GetMouseButtonUp(1))
            {
                _isForceOn = false;
            }

            if (Input.GetKey(KeyCode.W))
            {
                Camera.main.transform.SetPositionAndRotation(Camera.main.transform.position + Vector3.down*5f, Camera.main.transform.rotation);
            }
            if (Input.GetKey(KeyCode.S))
            {
                Camera.main.transform.SetPositionAndRotation(Camera.main.transform.position + Vector3.up * 5f, Camera.main.transform.rotation);
            }
            mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y));
            mousePos.y = 0;
            // ------- end input handle ------
//#endif
#if UNITY_ANDROID
            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Moved)
                {
                    _isForceOn = true;
                    mousePos = Camera.main.ScreenToWorldPoint(new float3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, Camera.main.transform.position.y));
                    mousePos.y = 0;
                }
            }
            else
            {
                //_isForceOn = false;
            }
#endif


            //初始化一个job
            var job = new JobProcess { isForceOn = _isForceOn, forceMode = _forceMode, mousePosition = mousePos };

            //开始job      
            return job.Schedule(this, inputDeps);
        }
    }
}