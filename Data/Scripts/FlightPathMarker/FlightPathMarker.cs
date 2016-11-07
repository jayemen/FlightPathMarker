using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace FlightPathMarker
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class FlightPathMarker : MySessionComponentBase
    {
        private const float WIDTH_ANGLE = 0.002f;
        private const float LENGTH_ANGLE = 0.01f;
        private const float CROSSHAIR_DISTANCE = 1.0f;
        private const float SLERP_MS = 250;

        private Vector3? lastOffset;
        private long lastTick = 0;

        public override void Draw()
        {
            var tick = DateTime.UtcNow.Ticks;
            var tickDelta = tick - lastTick;

            var physics = ShipPhysics();
            if (physics == null)
            {
                lastOffset = null;
                return;
            }
            
            
            var offset = physics.LinearVelocity;
            if (offset.LengthSquared() == 0)
            {
                lastOffset = null;
                return;
            }

            offset.Normalize();
            offset *= CROSSHAIR_DISTANCE;
            
            if (lastOffset.HasValue)
            {
                // (100ns/tick) * (1 slerp / SLERP_MS) * (0.0001 ms / 100 ns) = (0.0001 ms / SLERP_MS) slerps/tick
                var frac = 0.0001f / SLERP_MS * tickDelta;

                Slerp(
                    from: lastOffset.Value, 
                    to: offset, 
                    frac: frac, 
                    output: ref offset);

                offset.Normalize();
            }

            
            DrawCross(offset, Color.LightGreen);
            DrawCross(-offset, Color.Red);

            lastTick = tick;
            lastOffset = offset;
        }

        private void Slerp(Vector3 from, Vector3 to, float frac, ref Vector3 output)
        {
            output = (to - from) * frac + from;
        }

        private void DrawCross(Vector3 offset, Color color)
        {
            var camera = MyAPIGateway.Session?.Camera;
            if (camera == null)
            {
                return;
            }

            var width = camera.FovWithZoom * WIDTH_ANGLE;
            var length = camera.FovWithZoom * LENGTH_ANGLE;

            MyTransparentGeometry.AddBillboardOriented(
                material: "SquareIgnoreDepth", 
                color: color,
                origin: camera.WorldMatrix.Translation + offset, 
                leftVector: camera.WorldMatrix.Left, 
                upVector: camera.WorldMatrix.Up, 
                width: width, 
                height: length);

            MyTransparentGeometry.AddBillboardOriented(
               material: "SquareIgnoreDepth",
               color: color,
               origin: camera.WorldMatrix.Translation + offset,
               leftVector: camera.WorldMatrix.Left,
               upVector: camera.WorldMatrix.Up,
               width: length,
               height: width);
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
