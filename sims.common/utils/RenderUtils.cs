/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TSO.Common.utils
{
    public class RenderUtils
    {
        
        private static GraphicsDevice GD;
        private static RenderTarget2D BackbufferDepth;
        private static RenderTarget2D Backbuffer;
        private static SpriteBatch SB;

        public static void InitGD(GraphicsDevice gd)
        {
            GD = gd;
            SB = new SpriteBatch(gd);
        }
        public static void InitScreenTargets(GraphicsDevice gd)
        {
            if (BackbufferDepth != null) BackbufferDepth.Dispose();
            if (Backbuffer != null) Backbuffer.Dispose();
            var scale = 1;
            BackbufferDepth = CreateRenderTarget(gd, 1, 0, SurfaceFormat.Color, gd.Viewport.Width / scale, gd.Viewport.Height / scale, DepthFormat.None);
            Backbuffer = CreateRenderTarget(gd, 1, 0, SurfaceFormat.Color, gd.Viewport.Width / scale, gd.Viewport.Height / scale, DepthFormat.Depth24Stencil8);
        }

        private static RenderTarget2D ActiveColor;
        private static RenderTarget2D ActiveDepth;
        private static int StencilValue;

        public static void SetPPXTarget(RenderTarget2D color, RenderTarget2D depth, bool clear)
        {
            if (color == null && depth == null && Backbuffer != null) color = Backbuffer;
            ActiveColor = color;
            if (color == Backbuffer && depth == null && BackbufferDepth != null) depth = BackbufferDepth;
            ActiveDepth = depth;

            var gd = GD;
            gd.SetRenderTarget(color); //can be null
            if (clear)
            {
                StencilValue = 1;
                gd.Clear(Color.TransparentBlack);
                if (depth != null)
                {
                    gd.SetRenderTarget(depth);
                    gd.Clear(Color.White);
                }
            }
            
            if (depth != null) gd.SetRenderTargets(color, depth);
           
        }

        public delegate void RenderPPXProcedureDelegate(bool depthPass);
        public static void RenderPPXDepth(Effect effect, bool forceDepth,
            RenderPPXProcedureDelegate proc)
        {
            var color = ActiveColor;
            var depth = ActiveDepth;
            var gd = GD;
            if (depth != null)
            {
                var oldDS = gd.DepthStencilState;
                //completely special case.
                gd.SetRenderTarget(color);
                gd.DepthStencilState = new DepthStencilState
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Always,
                    StencilFail = StencilOperation.Keep,
                    StencilPass = StencilOperation.Replace,
                    CounterClockwiseStencilPass = StencilOperation.Replace,
                    StencilDepthBufferFail = StencilOperation.Keep,
                    DepthBufferEnable = forceDepth, //(ActiveColor == null),
                    DepthBufferWriteEnable = forceDepth, //(ActiveColor == null),
                    ReferenceStencil = StencilValue,
                    TwoSidedStencilMode = true
                };
                effect.Parameters["depthMap"].SetValue(depth);
                effect.Parameters["depthOutMode"].SetValue(false);
                proc(false);

                //now draw the depth using the depth test information we got previously.

                //unbind depth map since we are writing to it
                effect.Parameters["depthMap"].SetValue((Texture2D)null);
                effect.Parameters["depthOutMode"].SetValue(true);
                gd.SetRenderTarget(depth);
                gd.DepthStencilState = new DepthStencilState
                {
                    StencilEnable = true,
                    StencilFunction = CompareFunction.Equal,
                    DepthBufferEnable = forceDepth,
                    DepthBufferWriteEnable = forceDepth,
                    ReferenceStencil = StencilValue,
                };
                proc(true);

                gd.DepthStencilState = oldDS;
                StencilValue++; //can increment up to 254 times. Assume we're not going to be rendering that much between clears.
                if (StencilValue > 255) StencilValue = 1;
                gd.SetRenderTarget(color);
                effect.Parameters["depthOutMode"].SetValue(false);
            }
            else if (depth != null)
            {
                //draw color then draw depth
                gd.SetRenderTarget(color);
                proc(false);
                gd.SetRenderTarget(depth);
                proc(true);
            }
            else
            {
                //mrt already bound. draw in both.
                proc(false);
            }
        }

        public static void DrawBackbuffer()
        {
            if (Backbuffer == null) return; //this gfx mode does not use a rendertarget backbuffer
            SB.Begin();
            SB.Draw(Backbuffer, new Vector2(), null, Color.White, 0f, new Vector2(), new Vector2(1, 1),
                SpriteEffects.None, 0);
            SB.End();
        }
        
        public static RenderTarget2D CreateRenderTarget(GraphicsDevice device, int numberLevels, int multisample, SurfaceFormat surface, int width, int height,  DepthFormat dformat)
        {
            // Create our render target
            return new RenderTarget2D(device,
                width, height, (numberLevels>1), surface,
                dformat, 0, RenderTargetUsage.PreserveContents);
        }
    }
}
