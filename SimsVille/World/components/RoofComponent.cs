using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using tso.world;
using tso.world.Model;
using SimsHomeMaker.ContentManager;
using tso.world.Utils;
using TSO.Common.utils;

namespace tso.world.Components
{

        

        public class RoofComponent : WorldComponent, IDisposable
        {
            private static Point[] advanceByDir = new Point[] { new Point(8, 0), new Point(0, 8), new Point(-8, 0), new Point(0, -8) };
            public Blueprint blueprint;
            private RoofDrawGroup[] Drawgroups;
            private Microsoft.Xna.Framework.Graphics.Effect Effect;
            private static int[] ExpandOrder = new int[] { 2, 3, 1, -1 };
            public float RoofPitch;
            private List<RoofRect>[] RoofRects;
            public uint RoofStyle;
            public bool ShapeDirty = true;
            public bool StyleDirty;
            private Texture2D Texture;

            public RoofComponent(Blueprint bp)
            {
                this.blueprint = bp;
                this.RoofRects = new List<RoofRect>[bp.Stories];
                this.Drawgroups = new RoofDrawGroup[bp.Stories];
                this.Effect = WorldContent.GrassEffect;
            }

            public void Dispose()
            {
                foreach (RoofDrawGroup buf in this.Drawgroups)
                {
                    if ((buf != null) && (buf.NumPrimitives > 0))
                    {
                        buf.IndexBuffer.Dispose();
                        buf.VertexBuffer.Dispose();
                    }
                }
            }

            public override void Draw(GraphicsDevice device, WorldState world)
            {
                if (this.ShapeDirty)
                {
                    this.RegenRoof(device);
                    this.ShapeDirty = false;
                    this.StyleDirty = false;
                }
                else if (this.StyleDirty)
                {
                    this.RemeshRoof(device);
                    this.StyleDirty = false;
                }
                for (int i = 0; i < this.Drawgroups.Length; i++)
                {
                    if (i > (world.Level - 1))
                    {
                        return;
                    }
                    if (this.Drawgroups[i] != null)
                    {
                        RoofDrawGroup dg = this.Drawgroups[i];
                        if (dg.NumPrimitives != 0)
                        {
                            RenderUtils.RenderPPXDepth(this.Effect, true, delegate(bool depthMode)
                            {
                                world._3D.ApplyCamera(this.Effect);
                                this.Effect.Parameters["World"].SetValue(Matrix.Identity);
                                this.Effect.Parameters["DiffuseColor"].SetValue(new Vector4(((float)world.OutsideColor.R) / 255f, ((float)world.OutsideColor.G) / 255f, ((float)world.OutsideColor.B) / 255f, 1f));
                                //this.Effect.Parameters["UseTexture"].SetValue(true);
                                //this.Effect.Parameters["BaseTex"].SetValue(this.Texture);
                                device.SetVertexBuffer(dg.VertexBuffer);
                                device.Indices = dg.IndexBuffer;
                                this.Effect.CurrentTechnique = this.Effect.Techniques["DrawBase"];
                                using (EffectPassCollection.Enumerator enumerator = this.Effect.CurrentTechnique.Passes.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        enumerator.Current.Apply();
                                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 0, 0, dg.NumPrimitives);
                                    }
                                }
                            });
                        }
                    }
                }
            }

            public bool IndoorsOrFloor(int x, int y, int level)
            {
                if (level > this.blueprint.Stories)
                {
                    return false;
                }
                if (!this.TileIndoors(x, y, level))
                {
                    return (this.blueprint.GetFloor((short)x, (short)y, (sbyte)level).Pattern > 0);
                }
                return true;
            }

            public bool IsRoofable(LotTilePos pos)
            {
                if (pos.Level == 1)
                {
                    return false;
                }
                short tileX = pos.TileX;
                short tileY = pos.TileY;
                sbyte level = pos.Level;
                if (((tileX <= 0) || (tileX >= (this.blueprint.Width - 1))) || ((tileY <= 0) || (tileY >= (this.blueprint.Height - 1))))
                {
                    return false;
                }
                bool halftile = false;
                if (!this.TileIndoors(tileX, tileY, level - 1))
                {
                    bool found = false;
                    if ((pos.x % 0x10) == 8)
                    {
                        if (this.TileIndoors(tileX + 1, tileY, level - 1))
                        {
                            found = true;
                        }
                    }
                    else if (this.TileIndoors(tileX - 1, tileY, level - 1))
                    {
                        found = true;
                    }
                    if ((pos.y % 0x10) == 8)
                    {
                        if (this.TileIndoors(tileX, tileY + 1, level - 1))
                        {
                            found = true;
                        }
                    }
                    else if (this.TileIndoors(tileX, tileY - 1, level - 1))
                    {
                        found = true;
                    }
                    if (this.TileIndoors(tileX + (((pos.x % 0x10) == 8) ? 1 : -1), tileY + (((pos.y % 0x10) == 8) ? 1 : -1), level - 1))
                    {
                        found = true;
                    }
                    if (!found)
                    {
                        return false;
                    }
                    halftile = true;
                }
                if (this.IndoorsOrFloor(tileX, tileY, level))
                {
                    return false;
                }
                if (halftile)
                {
                    if ((pos.x % 0x10) == 8)
                    {
                        if (this.IndoorsOrFloor(tileX + 1, tileY, level))
                        {
                            return false;
                        }
                    }
                    else if (this.IndoorsOrFloor(tileX - 1, tileY, level))
                    {
                        return false;
                    }
                    if ((pos.y % 0x10) == 8)
                    {
                        if (this.IndoorsOrFloor(tileX, tileY + 1, level))
                        {
                            return false;
                        }
                    }
                    else if (this.IndoorsOrFloor(tileX, tileY - 1, level))
                    {
                        return false;
                    }
                    if (this.IndoorsOrFloor(tileX + (((pos.x % 0x10) == 8) ? 1 : -1), tileY + (((pos.y % 0x10) == 8) ? 1 : -1), level))
                    {
                        return false;
                    }
                }
                return true;
            }

            public void MeshRects(int level, GraphicsDevice device)
            {
                List<RoofRect> rects = this.RoofRects[level - 2];
                if (rects != null)
                {
                    if ((this.Drawgroups[level - 2] != null) && (this.Drawgroups[level - 2].NumPrimitives > 0))
                    {
                        this.Drawgroups[level - 2].VertexBuffer.Dispose();
                        this.Drawgroups[level - 2].IndexBuffer.Dispose();
                    }
                    int num1 = rects.Count * 4;
                    TerrainVertex[] Geom = new TerrainVertex[num1 * 4];
                    int[] Indexes = new int[num1 * 6];
                    int numPrimitives = num1 * 2;
                    int geomOffset = 0;
                    int indexOffset = 0;
                    foreach (RoofRect rect in rects)
                    {
                        int height = Math.Min((int)(rect.x2 - rect.x1), (int)(rect.y2 - rect.y1)) / 2;
                        float heightMod = ((float)height) / 400f;
                        float pitch = this.RoofPitch;
                        Vector3 tl = this.ToWorldPos(rect.x1, rect.y1, 0, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 tr = this.ToWorldPos(rect.x2, rect.y1, 0, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 bl = this.ToWorldPos(rect.x1, rect.y2, 0, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 br = this.ToWorldPos(rect.x2, rect.y2, 0, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 m_tl = this.ToWorldPos(rect.x1 + height, rect.y1 + height, height, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 m_tr = this.ToWorldPos(rect.x2 - height, rect.y1 + height, height, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 m_bl = this.ToWorldPos(rect.x1 + height, rect.y2 - height, height, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Vector3 m_br = this.ToWorldPos(rect.x2 - height, rect.y2 - height, height, level, pitch) + new Vector3(0f, heightMod, 0f);
                        Color topCol = Color.Lerp(Color.White, new Color(0xaf, 0xaf, 0xaf), pitch);
                        Color rightCol = Color.White;
                        Color btmCol = Color.Lerp(Color.White, new Color(200, 200, 200), pitch);
                        Color leftCol = Color.Lerp(Color.White, new Color(150, 150, 150), pitch);
                        Vector4 darken = new Vector4(0.8f, 0.8f, 0.8f, 1f);
                        for (int j = 0; j < 0x10; j += 4)
                        {
                            Indexes[indexOffset++] = geomOffset + j;
                            Indexes[indexOffset++] = (geomOffset + 1) + j;
                            Indexes[indexOffset++] = (geomOffset + 2) + j;
                            Indexes[indexOffset++] = (geomOffset + 2) + j;
                            Indexes[indexOffset++] = (geomOffset + 3) + j;
                            Indexes[indexOffset++] = geomOffset + j;
                        }
                        Vector2 texScale = new Vector2(0.6666667f, 1f);
                        Geom[geomOffset++] = new TerrainVertex(tl, topCol.ToVector4() * darken, new Vector2(tl.X, tl.Z * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(tr, topCol.ToVector4() * darken, new Vector2(tr.X, tr.Z * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_tr, topCol.ToVector4(), new Vector2(m_tr.X, m_tr.Z * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_tl, topCol.ToVector4(), new Vector2(m_tl.X, m_tl.Z * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(tr, rightCol.ToVector4() * darken, new Vector2(tr.Z, tr.X) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(br, rightCol.ToVector4() * darken, new Vector2(br.Z, br.X) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_br, rightCol.ToVector4(), new Vector2(m_br.Z, m_br.X) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_tr, rightCol.ToVector4(), new Vector2(m_tr.Z, m_tr.X) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(br, btmCol.ToVector4() * darken, new Vector2(br.X, br.Z) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(bl, btmCol.ToVector4() * darken, new Vector2(bl.X, bl.Z) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_bl, btmCol.ToVector4(), new Vector2(m_bl.X, m_bl.Z) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_br, btmCol.ToVector4(), new Vector2(m_br.X, m_br.Z) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(bl, leftCol.ToVector4() * darken, new Vector2(bl.Z, bl.X * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(tl, leftCol.ToVector4() * darken, new Vector2(tl.Z, tl.X * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_tl, leftCol.ToVector4(), new Vector2(m_tl.Z, m_tl.X * -1f) * texScale, 0f);
                        Geom[geomOffset++] = new TerrainVertex(m_bl, leftCol.ToVector4(), new Vector2(m_bl.Z, m_bl.X * -1f) * texScale, 0f);
                    }
                    RoofDrawGroup result = new RoofDrawGroup();
                    if (numPrimitives > 0)
                    {
                        result.VertexBuffer = new VertexBuffer(device, typeof(TerrainVertex), Geom.Length, BufferUsage.None);
                        if (Geom.Length != 0)
                        {
                            result.VertexBuffer.SetData<TerrainVertex>(Geom);
                        }
                        result.IndexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, 4 * Indexes.Length, BufferUsage.None);
                        if (Geom.Length != 0)
                        {
                            result.IndexBuffer.SetData<int>(Indexes);
                        }
                    }
                    result.NumPrimitives = numPrimitives;
                    this.Drawgroups[level - 2] = result;
                }
            }

            private bool RangeCheck(RoofRect me, RoofRect into, int dir)
            {
                switch ((dir % 2))
                {
                    case 0:
                        return ((me.y1 > into.y1) && (me.y2 < into.y2));

                    case 1:
                        return ((me.x1 > into.x1) && (me.x2 < into.x2));
                }
                return false;
            }

            public void RegenRoof(GraphicsDevice device)
            {
                WorldRoofProvider roofs = TSO.Content.Content.Get().WorldRoofs;
                this.Texture = roofs.Get((ulong)this.RoofStyle).Sprite;
                for (int i = 1; i <= this.blueprint.Stories; i++)
                {
                    this.RegenRoof((sbyte)(i + 1), device);
                }
            }

            public void RegenRoof(sbyte level, GraphicsDevice device)
            {
                int width = this.blueprint.Width * 2;
                int height = this.blueprint.Height * 2;
                bool[] evaluated = new bool[width * height];
                List<RoofRect> result = new List<RoofRect>();
                for (int y = 2; y < height; y++)
                {
                    for (int x = 2; x < width; x++)
                    {
                        int off = x + (y * width);
                        if (!evaluated[off])
                        {
                            evaluated[off] = true;
                            LotTilePos tilePos = new LotTilePos((short)(x * 8), (short)(y * 8), level);
                            if (this.IsRoofable(tilePos))
                            {
                                this.RoofSpread(tilePos, evaluated, width, height, level, result);
                            }
                        }
                    }
                }
                this.RoofRects[level - 2] = result;
                this.MeshRects(level, device);
            }

            public void RemeshRoof(GraphicsDevice device)
            {
                WorldRoofProvider roofs = TSO.Content.Content.Get().WorldRoofs;
                this.Texture = roofs.Get((ulong)this.RoofStyle).Sprite;
                for (int i = 1; i < this.blueprint.Stories; i++)
                {
                    this.MeshRects((sbyte)(i + 1), device);
                }
            }

            private void RoofSpread(LotTilePos start, bool[] evaluated, int width, int height, sbyte level, List<RoofRect> result)
            {
                RoofRect rect = new RoofRect(start.x, start.y, start.x + 8, start.y + 8);
                Point toCtr = new Point(4, 4);
                while (rect.ExpandDir != -1)
                {
                    int dir = rect.ExpandDir;
                    Point startPt = this.StartLocation(rect, dir);
                    Point testPt = startPt;
                    Point inc = advanceByDir[(dir + 1) % 4];
                    int count = Math.Abs((int)(rect.GetByDir((dir + 1) % 4) - rect.GetByDir((dir + 3) % 4))) / 8;
                    bool canExpand = true;
                    for (int i = 0; i < count; i++)
                    {
                        LotTilePos tile = new LotTilePos((short)testPt.X, (short)testPt.Y, level);
                        if (!this.IsRoofable(tile))
                        {
                            canExpand = false;
                            break;
                        }
                        testPt += inc;
                    }
                    if (!canExpand)
                    {
                        rect.ExpandDir = ExpandOrder[rect.ExpandDir];
                    }
                    else
                    {
                        testPt = startPt;
                        for (int i = 0; i < count; i++)
                        {
                            evaluated[(testPt.X / 8) + ((testPt.Y / 8) * width)] = true;
                            testPt += inc;
                        }
                        Point midPt = (startPt + new Point((inc.X * count) / 2, (inc.Y * count) / 2)) + toCtr;
                        RoofRect expandInto = result.FirstOrDefault<RoofRect>(x => x.Contains(midPt) && this.RangeCheck(rect, x, dir));
                        if (expandInto != null)
                        {
                            rect.SetByDir(dir, expandInto.GetByDir(dir));
                            continue;
                        }
                        rect.SetByDir(dir, rect.GetByDir(dir) + ((dir > 1) ? -8 : 8));
                    }
                }
                result.Add(rect);
            }

            public void SetStylePitch(uint style, float pitch)
            {
                this.RoofStyle = style;
                this.RoofPitch = pitch;
                this.blueprint.Damage.Add(new BlueprintDamage(BlueprintDamageType.ROOF_STYLE_CHANGED, 0, 0, 1));
            }

            private Point StartLocation(RoofRect rect, int dir)
            {
                switch (dir)
                {
                    case 0:
                        return new Point(rect.x2, rect.y1);

                    case 1:
                        return new Point(rect.x2 - 8, rect.y2);

                    case 2:
                        return new Point(rect.x1 - 8, rect.y2 - 8);

                    case 3:
                        return new Point(rect.x1, rect.y1 - 8);
                }
                return new Point();
            }

            public bool TileIndoors(int x, int y, int level)
            {
                uint num1 = this.blueprint.RoomMap[level - 1][x + (y * this.blueprint.Width)];
                uint room1 = num1 & 0xffff;
                uint room2 = num1 >> 0x10;
                return (((room1 < this.blueprint.Rooms.Count) && !this.blueprint.Rooms[(int)room1].IsOutside) || (((room2 > 0) && (room2 < this.blueprint.Rooms.Count)) && !this.blueprint.Rooms[(int)room2].IsOutside));
            }

            private Vector3 ToWorldPos(int x, int y, int z, int level, float pitch)
            {
                return new Vector3((((float)x) / 16f) * 3f, (((z * pitch) / 16f) * 3f) + (((level - 1) * 2.95f) * 3f), (((float)y) / 16f) * 3f);
            }

            public override float PreferredDrawOrder
            {
                get
                {
                    return 0f;
                }
            }

            public class RoofRect
            {
                public int ExpandDir;
                public int x1;
                public int x2;
                public int y1;
                public int y2;

                public RoofRect(int x1, int y1, int x2, int y2)
                {
                    if (x1 > x2)
                    {
                        x1 = x2;
                        x2 = x1;
                    }
                    if (y1 > y2)
                    {
                        y1 = y2;
                        y2 = y1;
                    }
                    this.x1 = x1;
                    this.y1 = y1;
                    this.x2 = x2;
                    this.y2 = y2;
                }

                public Point Closest(int x, int y)
                {
                    return new Point(Math.Max(Math.Min(this.x2, x), this.x1), Math.Max(Math.Min(this.y2, y), this.y1));
                }

                public bool Contains(Point pt)
                {
                    if ((pt.X < this.x1) || (pt.X > this.x2))
                    {
                        return false;
                    }
                    return ((pt.Y >= this.y1) && (pt.Y <= this.y2));
                }

                public int GetByDir(int dir)
                {
                    switch (dir)
                    {
                        case 0:
                            return this.x2;

                        case 1:
                            return this.y2;

                        case 2:
                            return this.x1;

                        case 3:
                            return this.y1;
                    }
                    return 0;
                }

                public bool HardContains(Point pt)
                {
                    if ((pt.X <= this.x1) || (pt.X >= this.x2))
                    {
                        return false;
                    }
                    return ((pt.Y > this.y1) && (pt.Y < this.y2));
                }

                public bool Intersects(RoofComponent.RoofRect other)
                {
                    if ((other.x1 >= this.x2) || (other.x2 <= this.x1))
                    {
                        return false;
                    }
                    return ((other.y1 < this.y2) && (other.y2 > this.y1));
                }

                public void SetByDir(int dir, int value)
                {
                    switch (dir)
                    {
                        case 0:
                            this.x2 = value;
                            return;

                        case 1:
                            this.y2 = value;
                            return;

                        case 2:
                            this.x1 = value;
                            return;

                        case 3:
                            this.y1 = value;
                            return;
                    }
                }
            }
        }


        public class RoofDrawGroup
        {
            public Microsoft.Xna.Framework.Graphics.IndexBuffer IndexBuffer;
            public int NumPrimitives;
            public Microsoft.Xna.Framework.Graphics.VertexBuffer VertexBuffer;
        }

}
