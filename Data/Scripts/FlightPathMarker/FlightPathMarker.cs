using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;

namespace FlightPathMarker
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class FlightPathMarker : MySessionComponentBase
    {
        private const float WIDTH_ANGLE = 0.0004f;
        private const float CROSSHAIR_DISTANCE = 0.1f;
        private const int AVERAGE_SAMPLES = 5;
        private Vector3[] velocities = new Vector3[AVERAGE_SAMPLES];
        private int nextAverage = 0;

        public override void Draw()
        {
            var physics = ShipPhysics();
            if (physics == null)
            {
                return;
            }

            var camera = MyAPIGateway.Session?.Camera;
            if (camera == null)
            {
                return;
            }

            var velocity = UpdateAverageVelocity(physics.LinearVelocity);
            if (Vector3.IsZero(velocity))
            {
                return;
            }

            velocity.Normalize();

            DrawCrosshair(velocity, Color.LightGreen);
            DrawCrosshair(-velocity, Color.Red);
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
            if (camera == null)
            {
                return;
            }

            var width = camera.FovWithZoom * WIDTH_ANGLE;
            
            MyTransparentGeometry.AddBillboardOriented(
                material: "WhiteDot",
                color: color,
                origin: camera.WorldMatrix.Translation + direction * CROSSHAIR_DISTANCE,
                leftVector: camera.WorldMatrix.Left,
                upVector: camera.WorldMatrix.Up,
                radius: width);
        }

        private MyPhysicsComponentBase ShipPhysics()
        {
            var entity = MyAPIGateway.Session?.Player?.Controller?.ControlledEntity?.Entity;
            if (entity == null)
            {
                return null;
            }

            return entity is IMyCubeBlock
                ? entity.Parent.Physics
                : null;
        }
    }
}
