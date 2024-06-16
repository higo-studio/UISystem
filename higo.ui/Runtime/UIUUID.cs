using System;

namespace Higo.UI
{
    public struct UIUUID : IEquatable<UIUUID>
    {
        public int LayerIndex { get; }
        public int UuidInLayer { get; }
        public UIUUID(int layerIndex, int uuid)
        {
            LayerIndex = layerIndex;
            UuidInLayer = uuid;
        }

        public bool Equals(UIUUID other)
        {
            return LayerIndex == other.LayerIndex && UuidInLayer == other.UuidInLayer;
        }

        public override bool Equals(object typelessOther)
        {
            return typelessOther is UIUUID other && LayerIndex == other.LayerIndex && UuidInLayer == other.UuidInLayer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LayerIndex.GetHashCode(), UuidInLayer.GetHashCode());
        }

        public static implicit operator UIUUID((int layerIndex, int uuid) entry)
        {
            return new UIUUID(entry.layerIndex, entry.uuid);
        }

        public override string ToString()
        {
            return $"{{Layer:{LayerIndex}, UUID:{UuidInLayer}}}";
        }

        public static bool operator ==(UIUUID a, UIUUID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIUUID a, UIUUID b)
        {
            return !(a == b);
        }
    }
}