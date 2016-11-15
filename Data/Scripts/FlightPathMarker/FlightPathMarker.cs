using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.Game;
using VRage.Game;

namespace FlightPathMarker
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class FlightPathMarker : MySessionComponentBase
    {
        private const float WIDTH_ANGLE = 0.0004f;
        private const float CROSSHAIR_DISTANCE = 0.1f;
        private const int AVERAGE_SAMPLES = 10;
        private IMyCubeGrid target;
        readonly private AveragedVector averageVelocity = new AveragedVector(AVERAGE_SAMPLES);
        readonly private Util util = new Util();
        
        public override void UpdateAfterSimulation()
        {
            if (util.CameraGrid == null)
            {
                return;
            }

            if (MyAPIGateway.Input.GetGameControl(MyControlsSpace.SECONDARY_TOOL_ACTION).IsNewPressed())
            {
                bool hadTarget = target != null;
                target = util.LookingAt<IMyCubeGrid>();

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
            if (!util.GameReady || util.CameraGrid?.Physics == null) {
                return;
            }
            
            var velocity = util.CameraGrid.Physics.LinearVelocity;

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
        }

        private void DrawFlightPathMarker(Vector3 direction, Color color)
        {
            var head = util.Camera.WorldMatrix;
            var width = util.Camera.FovWithZoom * WIDTH_ANGLE;

            MyTransparentGeometry.AddBillboardOriented(
                material: "WhiteDot",
                color: color,
                origin: head.Translation + direction * CROSSHAIR_DISTANCE,
                leftVector: head.Left,
                upVector: head.Up,
                radius: width);
        }
    }
}
