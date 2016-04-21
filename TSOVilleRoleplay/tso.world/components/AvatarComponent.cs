﻿/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TSO.Vitaboy;
using tso.world.model;
using tso.world.utils;
using tso.common.utils;

namespace tso.world.components
{
    public class AvatarComponent : WorldComponent
    {
        public Avatar Avatar;

        private static Vector2[] PosCenterOffsets = new Vector2[]{
            new Vector2(2+16, 79+8),
            new Vector2(3+32, 158+16),
            new Vector2(5+64, 316+32)
        };

        public override Vector3 GetSLOTPosition(int slot)
        {
            var handpos = Avatar.Skeleton.GetBone("R_FINGER0").AbsolutePosition / 3.0f;
            return Vector3.Transform(new Vector3(handpos.X, handpos.Z, handpos.Y), Matrix.CreateRotationZ((float)(RadianDirection+Math.PI))) + this.Position - new Vector3(0.5f, 0.5f, 0f); //todo, rotate relative to avatar
        }

        public double RadianDirection;
        public Texture2D Headline;
        public Vector2 LastScreenPos; //todo: move this and slots into an abstract class that contains avatars and objects
        public int LastZoomLevel;
        public ushort ObjectID;
        public ushort Room;

        private Direction _Direction;
        public override Direction Direction
        {
            get
            {
                return _Direction;
            }
            set
            {
                _Direction = value;
                switch (value)
                {
                    case Direction.NORTH:
                        RadianDirection = 0;
                        break;
                    case Direction.EAST:
                        RadianDirection = Math.PI*0.5;
                        break;
                    case Direction.SOUTH:
                        RadianDirection = Math.PI;
                        break;
                    case Direction.WEST:
                        RadianDirection = Math.PI*1.5;
                        break;
                }
            }
        }

        public override Vector3 Position
        {
            get
            {
                if (Container == null) return _Position;
                else return Container.GetSLOTPosition(ContainerSlot) + new Vector3(0.5f, 0.5f, -1.4f); //apply offset to snap character into slot
            }
            set
            {
                _Position = value;
                OnPositionChanged();
                _WorldDirty = true;
            }
        }

        public override float PreferredDrawOrder
        {
            get { return 5000.0f;  }
        }

        public override void Initialize(GraphicsDevice device, WorldState world)
        {
            base.Initialize(device, world);
            Avatar.StoreOnGPU(device);
        }

        public override void Draw(GraphicsDevice device, WorldState world)
        {

            var headpos = Avatar.Skeleton.GetBone("HEAD").AbsolutePosition / 3.0f;
            var transhead = Vector3.Transform(new Vector3(headpos.X, headpos.Z, headpos.Y), Matrix.CreateRotationZ((float)(RadianDirection + Math.PI))) + this.Position - new Vector3(0.5f, 0.5f, 0f);


            if (!world.TempDraw)
            {
                LastScreenPos = world.WorldSpace.GetScreenFromTile(Position) + world.WorldSpace.GetScreenOffset();
                LastZoomLevel = (int)world.Zoom;
            }
            /*if (Container != null)
            {
                Direction = Container.Direction;
                _WorldDirty = true;
            }*/
            if (Avatar != null){
                world._3D.DrawMesh(Matrix.CreateRotationY((float)(Math.PI-RadianDirection))*this.World, Avatar); //negated so avatars spin clockwise
            }

            if (Headline != null)
            {
                var headOff = (transhead - Position) + new Vector3(0, 0, 0.66f);
                var headPx = world.WorldSpace.GetScreenFromTile(headOff);

                var item = new _2DSprite();
                item.Pixel = Headline;
                item.Depth = TextureGenerator.GetWallZBuffer(device)[30];
                item.RenderMode = _2DBatchRenderMode.Z_BUFFER;

                item.SrcRect = new Rectangle(0, 0, Headline.Width, Headline.Height);
                item.WorldPosition = headOff;
                var off = PosCenterOffsets[(int)world.Zoom - 1];
                item.DestRect = new Rectangle(
                    ((int)headPx.X - Headline.Width / 2) + (int)off.X,
                    ((int)headPx.Y - Headline.Height / 2) + (int)off.Y, Headline.Width, Headline.Height);
                item.Room = Room;
                world._2D.Draw(item);
            }

        }
    }
}