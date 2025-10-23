using static FSO.LotView.World2D;

namespace FSO.LotView.Model
{
    public class WorldObjectRenderInfo
    {
        public int DynamicCounter;
        public int DynamicRemoveCycle;
        public WorldObjectRenderLayer Layer = WorldObjectRenderLayer.STATIC;
      
    }

    public enum WorldObjectRenderLayer
    {
        STATIC,
        DYNAMIC
    }
}
