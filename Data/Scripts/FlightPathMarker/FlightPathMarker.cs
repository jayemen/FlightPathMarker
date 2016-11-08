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
using VRage.Audio;

namespace FlightPathMarker
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class FlightPathMarker : MySessionComponentBase
    {
        private const float WIDTH_ANGLE = 0.0004f;
        private const float CROSSHAIR_DISTANCE = 0.1f;
        private const int AVERAGE_SAMPLES = 10;
        private IMyCubeGrid target;
        private List<IHitInfo> traceBuffer = new List<IHitInfo>();
        private AveragedVector averageVelocity = new AveragedVector(AVERAGE_SAMPLES);

        public override void UpdateAfterSimulation()
        {
            if (PlayerShip == null)
            {
                return;
            }

            if (MyAPIGateway.Input.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION).IsNewPressed())
            {
                var head = ControlledEntity.GetHeadMatrix(true, true, true, true);

                MyAPIGateway.Physics.CastRay(
                    head.Translation,
                    head.Translation + (head.Forward * 100000.0f),
                    traceBuffer);

                bool hadTarget = target != null;
                target = null;
                foreach (IHitInfo info in traceBuffer)
                {
                    if (info.HitEntity is IMyCubeGrid && info.HitEntity != PlayerShip)
                    {
                        target = info.HitEntity as IMyCubeGrid;
                        break;
                    }
                }

                if (hadTarget && target == null)
                {
                    MyAPIGateway.Utilities.ShowNotification("Frame of reference cleared");
                }
                else if (target != null)
                {
                    MyAPIGateway.Utilities.ShowNotification("Frame of reference set");
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

            if (target?.Physics != null)
            {
                velocity -= target.Physics.LinearVelocity;
            }

            velocity = averageVelocity.Update(velocity);
            if (Vector3.IsZero(velocity))
            {
                return;
            }

            velocity.Normalize();

            DrawFlightPathMarker(velocity, Color.LightGreen);
            DrawFlightPathMarker(-velocity, Color.Red);

            if (MyAPIGateway.Input.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION).IsPressed()) {
                var head = ControlledEntity.GetHeadMatrix(true, true, true, true);

                head.Translation += head.Forward;
                var color = Color.Yellow;
                MySimpleObjectDraw.DrawTransparentSphere(ref head, .05f, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 8);
            }
        }

        private void DrawFlightPathMarker(Vector3 direction, Color color)
        {
            if (Camera == null || ControlledEntity == null)
            {
                return;
            }

            var head = ControlledEntity.GetHeadMatrix(true, true, true, true);
            var width = Camera.FovWithZoom * WIDTH_ANGLE;

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

        private IMyCamera Camera
        {
            get
            {
                return MyAPIGateway.Session?.Camera;
            }
        }
    }
}
