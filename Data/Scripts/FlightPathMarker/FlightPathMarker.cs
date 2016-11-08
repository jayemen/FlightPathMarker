using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.Input;
using Sandbox.Game;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI.Interfaces;
using System.Collections.Generic;

namespace FlightPathMarker
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class FlightPathMarker : MySessionComponentBase
    {
        private const float WIDTH_ANGLE = 0.0004f;
        private const float CROSSHAIR_DISTANCE = 0.1f;
        private const int AVERAGE_SAMPLES = 10;
        private Vector3[] velocities = new Vector3[AVERAGE_SAMPLES];
        private int nextAverage = 0;
        private IMyCubeGrid frameOfReference;
        private List<IHitInfo> traceBuffer = new List<IHitInfo>();

        public override void UpdateAfterSimulation()
        {
            if (PlayerShip == null)
            {
                return;
            }

            if (MyAPIGateway.Input.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION).IsNewPressed())
            {
                var head = ControlledEntity.GetHeadMatrix(true, true, false);

                MyAPIGateway.Physics.CastRay(
                    head.Translation + head.Forward * 10.0f,
                    head.Translation + head.Forward * 10000.0f,
                    traceBuffer);

                frameOfReference = null;
                foreach (IHitInfo info in traceBuffer) 
                {
                    if (info.HitEntity is IMyCubeGrid && info.HitEntity != PlayerShip) {
                        frameOfReference = info.HitEntity as IMyCubeGrid;
                        break;
                    }
                }
            };
        }

        public override void Draw()
        {
            if (PlayerCockpitPhysics == null)
            {
                return;
            }
            
            var velocity = PlayerCockpitPhysics.LinearVelocity;

            if (frameOfReference?.Physics != null) {
                velocity -= frameOfReference.Physics.LinearVelocity;
                DrawBoundingSphere(frameOfReference, Color.LightGreen);
            }
            
            velocity = UpdateAverageVelocity(velocity);
            if (Vector3.IsZero(velocity))
            {
                return;
            }

            velocity.Normalize();

            DrawCrosshair(velocity, Color.LightGreen);
            DrawCrosshair(-velocity, Color.Red);
        }

        private void DrawBoundingSphere(IMyEntity entity, Color color) {
            var matrix = entity.WorldMatrix;

            MySimpleObjectDraw.DrawTransparentSphere(
                    ref matrix,
                    entity.LocalVolume.Radius,
                    ref color,
                    MySimpleObjectRasterizer.Wireframe,
                    16);
        }

        private Vector3 UpdateAverageVelocity(Vector3 velocity)
        {
            velocities[nextAverage] = velocity;
            nextAverage = (nextAverage + 1) % AVERAGE_SAMPLES;

            var average = velocities[0];

            for (var i = 1; i < AVERAGE_SAMPLES; ++i)
            {
                average += velocities[i];
            }

            return average / AVERAGE_SAMPLES;
        }

        private void DrawCrosshair(Vector3 direction, Color color)
        {
            var camera = MyAPIGateway.Session?.Camera;
            if (camera == null || ControlledEntity == null)
            {
                return;
            }

            var head = ControlledEntity.GetHeadMatrix(true, true, false);

            var width = camera.FovWithZoom * WIDTH_ANGLE;

            MyTransparentGeometry.AddBillboardOriented(
                material: "WhiteDot",
                color: color,
                origin: head.Translation + direction * CROSSHAIR_DISTANCE,
                leftVector: head.Left,
                upVector: head.Up,
                radius: width);
        }


        private MyPhysicsComponentBase PlayerCockpitPhysics
        {
            get
            {
                return PlayerCockpit?.Parent?.Physics;
            }
        }

        private IMyCubeGrid PlayerShip
        {
            get
            {
                return PlayerCockpit?.CubeGrid;
            }
        }

        private IMyCubeBlock PlayerCockpit
        {
            get
            {
                var entity = ControlledEntity?.Entity;
                return (entity as IMyCubeBlock);
            }
        }

        private IMyControllableEntity ControlledEntity
        {
            get
            {
                return MyAPIGateway.Session?.Player?.Controller?.ControlledEntity;
            }
        }
    }
}
