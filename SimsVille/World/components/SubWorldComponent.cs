using FSO.Common.Rendering.Framework;
using FSO.LotView.Model;
using FSO.LotView.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FSO.LotView.Components
{
    public class SubWorldComponent : World
    {
        public WorldArchitecture Architecture;
        public BoundingBox Bounds;
        //public WorldEntities Entities;
        /// <summary>
        /// Creates a new World instance.
        /// </summary>
        /// <param name="Device">A GraphicsDevice instance.</param>
        public SubWorldComponent(GraphicsDevice Device)
            : base(Device)
        {
        }

        public Vector2 GlobalPosition;

        private List<_2DDrawBuffer> StaticObjectsCache = new List<_2DDrawBuffer>();
        private List<_2DDrawBuffer> StaticArchCache = new List<_2DDrawBuffer>();
        private int TicksSinceLight = 0;

        public void UpdateBounds()
        {
            float minAlt = 0;
            float maxAlt = 0;
            foreach (var height in Blueprint.Altitude)
            {
                var alt = height * Blueprint.TerrainFactor - Blueprint.BaseAlt;
                if (alt < minAlt)
                {
                    minAlt = alt;
                }
                if (alt > maxAlt)
                {
                    maxAlt = alt;
                }
            }
            //calculate the maximum floor. shave 1 off to account for buildable area being smaller, but minimum 1 floor as trees are likely on floor 1
            maxAlt += Math.Max(1, 2) * 2.95f * 3;
            Bounds = new BoundingBox(new Vector3(GlobalPosition.X * -3, minAlt, GlobalPosition.Y * -3), new Vector3(GlobalPosition.X * -3 + Blueprint.Width * 3, maxAlt, GlobalPosition.Y * -3 + Blueprint.Height * 3));
        }

        public void RefreshLighting()
        {
            Blueprint.Changes.SetFlag(BlueprintGlobalChanges.OUTDOORS_LIGHTING_CHANGED);
        }

        /// <summary>
        /// Setup anything that needs a GraphicsDevice
        /// </summary>
        /// <param name="layer"></param>
        public void Initialize(GraphicsDevice device)
        {
            /**
             * Setup world state, this object acts as a facade
             * to world objects as well as providing various
             * state settings for the world and helper functions
             */
            State = new WorldState(device, device.Viewport.Width, device.Viewport.Height, this);
            State.AmbientLight = new Texture2D(device, 256, 256);

            HasInitGPU = true;
            HasInit = HasInitGPU & HasInitBlueprint;
        }

        public override void InitBlueprint(Blueprint blueprint)
        {
            this.Blueprint = blueprint;
            Architecture = new WorldArchitecture(blueprint);
            HasInitBlueprint = true;
            HasInit = HasInitGPU & HasInitBlueprint;
        }

        public void SubDraw(GraphicsDevice gd, WorldState parentState, Action<Vector2> action)
        {
            var parentScroll = parentState.CenterTile;
            //if (parentState.CameraMode > CameraRenderMode._2D)
               // parentState.Cameras.ModelTranslation = new Vector3(GlobalPosition.X * 3, 0, GlobalPosition.Y * 3);
            //else parentState.CenterTile += GlobalPosition; //TODO: vertical offset
            var pxOffset = -parentState.WorldSpace.GetScreenOffset();

            
            var oldLevel = parentState.Level;
            parentState.SilentLevel = State.Level;

            action(pxOffset);

            parentState.SilentLevel = oldLevel;


        }

        public bool DoDraw(WorldState state)
        {
            return state.Frustum.Intersects(Bounds);
        }

        /// <summary>
        /// Prep work before screen is painted
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="state"></param>
        public void PreDraw(GraphicsDevice gd, WorldState state)
        {
            if (Blueprint == null) return;
            var pxOffset = -state.WorldSpace.GetScreenOffset();
            var damage = Blueprint.Damage;
            var _2d = state._2D;
            var oldLevel = state.Level;
            var oldBuild = state.BuildMode;
            state.SilentLevel = State.Level;
            state.SilentBuildMode = 0;

            /**
             * This is a little bit different from a normal 2d world. All objects are part of the static 
             * buffer, and they are redrawn into the parent world's scroll buffers.
             */

            var recacheWalls = false;
            var recacheObjects = false;

            if (TicksSinceLight++ > 60 * 4) damage.Add(new BlueprintDamage(BlueprintDamageType.LIGHTING_CHANGED));

            foreach (var item in damage)
            {
                switch (item.Type)
                {
                    case BlueprintDamageType.ROTATE:
                    case BlueprintDamageType.ZOOM:
                    case BlueprintDamageType.LEVEL_CHANGED:
                        recacheObjects = true;
                        recacheWalls = true;
                        break;
                    case BlueprintDamageType.SCROLL:
                        break;
                    case BlueprintDamageType.LIGHTING_CHANGED:
                        Blueprint.OutsideColor = state.OutsideColor;
                        Blueprint.GenerateRoomLights();
                        State.OutsideColor = Blueprint.RoomColors[1];
                        State.AmbientLight.SetData(Blueprint.RoomColors);
                        TicksSinceLight = 0;
                        break;
                    case BlueprintDamageType.OBJECT_MOVE:
                    case BlueprintDamageType.OBJECT_GRAPHIC_CHANGE:
                    case BlueprintDamageType.OBJECT_RETURN_TO_STATIC:
                        recacheObjects = true;
                        break;
                    case BlueprintDamageType.WALL_CUT_CHANGED:
                    case BlueprintDamageType.FLOOR_CHANGED:
                    case BlueprintDamageType.WALL_CHANGED:
                        recacheWalls = true;
                        break;
                }
            }
            damage.Clear();

            state._2D.End();
            state._2D.Begin(state.Camera);
            if (recacheWalls)
            {
                //clear the sprite buffer before we begin drawing what we're going to cache
                Blueprint.Terrain.RegenTerrain(gd, Blueprint);
                Blueprint.FloorComp.Draw(gd, state);
                Blueprint.WallComp.Draw(gd, state);
                StaticArchCache.Clear();
                state._2D.End(StaticArchCache, true);
            }

            if (recacheObjects)
            {
                state._2D.Pause();
                state._2D.Resume();

                foreach (var obj in Blueprint.Objects)
                {
                    if (obj.Level > state.Level) continue;
                    var tilePosition = obj.Position;
                    state._2D.OffsetPixel(state.WorldSpace.GetScreenFromTile(tilePosition));
                    state._2D.OffsetTile(tilePosition);
                    state._2D.SetObjID(obj.ObjectID);
                    obj.Draw(gd, state);
                }
                StaticObjectsCache.Clear();
                state._2D.End(StaticObjectsCache, true);
            }

            state.SilentBuildMode = oldBuild;
            state.SilentLevel = oldLevel;
        }

        public void DrawArch(GraphicsDevice gd, WorldState parentState)
        {
            var parentLight = parentState._2D.AmbientLight;
            parentState._2D.AmbientLight = State.AmbientLight;

            var parentScroll = parentState.CenterTile;
            parentState.CenterTile += GlobalPosition; //TODO: vertical offset
            
            var pxOffset = -parentState.WorldSpace.GetScreenOffset();

            parentState._2D.SetScroll(pxOffset);
            Blueprint.Terrain.Draw(gd, parentState);
            parentState._2D.RenderCache(StaticArchCache);
            parentState._2D.Pause();
            var level = parentState.SilentLevel;
            parentState.SilentLevel = 5;
            Blueprint.RoofComp.Draw(gd, parentState);
            parentState.SilentLevel = level;

            parentState.CenterTile = parentScroll;
            parentState._2D.AmbientLight = parentLight;
        }

        public void DrawObjects(GraphicsDevice gd, WorldState parentState)
        {
            var parentLight = parentState._2D.AmbientLight;
            parentState._2D.AmbientLight = State.AmbientLight;

            var parentScroll = parentState.CenterTile;
            parentState.CenterTile += GlobalPosition; //TODO: vertical offset

            var pxOffset = -parentState.WorldSpace.GetScreenOffset();

            parentState._2D.SetScroll(pxOffset);
            parentState._2D.RenderCache(StaticObjectsCache);

            parentState.CenterTile = parentScroll;
            parentState._2D.AmbientLight = parentLight;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
