using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace FlightPathMarker
{
    public class Util
    {
        private List<IHitInfo> traceBuffer = new List<IHitInfo>();
        
        public T LookingAt<T>()
            where T : class, IMyEntity
        {
            var head = ControlledEntity.GetHeadMatrix(true, true, true);

            MyAPIGateway.Physics.CastRay(
                    from: head.Translation,
                    to: head.Translation + (head.Forward * 100000.0f),
                    toList: traceBuffer);

            return traceBuffer
                .Select(info => info.HitEntity)
                .OfType<T>()
                .Where(hit => hit != ControlledEntity && hit != PlayerShip)
                .FirstOrDefault() as T;
        }

        public IMyCubeGrid PlayerShip
        {
            get
            {
                return PlayerCockpit?.CubeGrid;
            }
        }

        public IMyCubeBlock PlayerCockpit
        {
            get
            {
                return (ControlledEntity.Entity as IMyCubeBlock);
            }
        }

        public IMyControllableEntity ControlledEntity
        {
            get
            {
                return MyAPIGateway.Session.Player.Controller.ControlledEntity;
            }
        }

        public IMyCamera Camera
        {
            get
            {
                return MyAPIGateway.Session.Camera;
            }
        }

        public bool GameReady
        {
            get 
            {
                return MyAPIGateway.Utilities != null && MyAPIGateway.Session != null; 
            }
        }
    }
}
