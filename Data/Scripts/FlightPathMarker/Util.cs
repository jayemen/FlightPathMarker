using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace FlightPathMarker
{
    public class Util
    {
        private List<IHitInfo> traceBuffer = new List<IHitInfo>();

        public T LookingAt<T>()
            where T : class, IMyEntity
        {
            var head = CrosshairMatrix();

            MyAPIGateway.Physics.CastRay(
                    from: head.Translation,
                    to: head.Translation + (head.Forward * 100000.0f),
                    toList: traceBuffer);

            return traceBuffer
                .Select(info => info.HitEntity)
                .OfType<T>()
                .Where(hit => hit != ControlledEntity && hit != CameraOwner)
                .FirstOrDefault();
        }

        public IMyEntity CameraOwner
        {
            get
            {
                var cameraController = MyAPIGateway.Session?.CameraController;

                if (cameraController == null) {
                    return null;
                }

                var grid = (cameraController as IMyCubeBlock)?.CubeGrid;
                
                if (grid != null) {
                    return grid;
                }
                
                if (ControlledEntity?.Entity == cameraController && ControlledEntity.EnabledThrusts) {
                    return ControlledEntity.Entity;
                }

                return null;
            }
        }

        public IMyControllableEntity ControlledEntity
        {
            get
            {
                return Player?.Controller?.ControlledEntity;
            }
        }

        public IMyCamera Camera
        {
            get
            {
                return MyAPIGateway.Session?.Camera;
            }
        }

        public MatrixD CrosshairMatrix()
        {
            if (Camera == null)
            {
                return MatrixD.Identity;
            }

            var head = Camera.WorldMatrix;

            if (MyAPIGateway.Session?.CameraController is IMyShipController)
            {
                // See the use of hardcoded DEFAULT_FPS_CAMERA_X_ANGLE variable in MyCockpit.cs in the game source code.
                head.Forward = Vector3D.Transform(head.Forward, MatrixD.CreateFromAxisAngle(head.Right, MathHelper.ToRadians(10)));
            }

            return head;
        }

        public IMyPlayer Player {
            get {
                return MyAPIGateway.Session?.Player;
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
