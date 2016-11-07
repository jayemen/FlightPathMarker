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
        const float WIDTH_ANGLE = 0.001f;
        const float LENGTH_ANGLE = .005f;
        const float CROSSHAIR_DISTANCE = .5f;

        public override void Draw()
        {
            var physics = ShipPhysics();
            if (physics == null)
            {
                return;
            }
            
            var offset = physics.LinearVelocity;
            offset.Normalize();
            offset *= CROSSHAIR_DISTANCE;

            DrawCross(offset, Color.LightGreen);
            DrawCross(-offset, Color.Red);
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
