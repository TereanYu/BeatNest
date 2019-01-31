#region Author
///-----------------------------------------------------------------
///   Namespace:		YU.ECS
///   Class:			HomeManagerSystem
///   Author: 		    yutian
///-----------------------------------------------------------------
#endregion
using Unity.Entities;

namespace YU.ECS
{
    public class HomeManagerSystem : ComponentSystem
    {
        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            Entity home = EntityManager.CreateEntity(typeof(HomeComponent));
            EntityManager.SetComponentData(home, new HomeComponent { health = 20 });
        }

        protected override void OnUpdate()
        {
            //throw new System.NotImplementedException();
        }
    }
}
