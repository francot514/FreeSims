using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Common.rendering.framework.camera;
using Microsoft.Xna.Framework.Graphics;
using TSO.Common.rendering.framework.model;
using Microsoft.Xna.Framework;

namespace TSO.Common.rendering.framework
{
    public class _3DTargetScene : _3DScene
    {
        public RenderTarget2D Target;
        private GraphicsDevice Device;
        private int Multisample = 0;
        public _3DTargetScene(GraphicsDevice device, ICamera camera, Point size, int multisample) : this(device, size, multisample) { Camera = camera; }
        public _3DTargetScene(GraphicsDevice device, Point size, int multisample)
            : base(device)
        {
            Device = device;
            Multisample = multisample;
            SetSize(size);
        }

        public void SetSize(Point size)
        {
            if (Target != null) Target.Dispose();
            Target = new RenderTarget2D(Device, size.X, size.Y, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, Multisample, RenderTargetUsage.PreserveContents);
        }

        public override void Draw(GraphicsDevice device)
        {
            var oldTargets = device.GetRenderTargets();
            device.SetRenderTarget(Target);
            device.Clear(Color.Transparent);
            Camera.ProjectionDirty();
            base.Draw(device);
            device.SetRenderTargets(oldTargets);
        }


    }
}
