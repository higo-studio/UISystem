using System;

namespace Higo.UI
{
    public struct Uuid : IEquatable<Uuid>
    {
        public int LayerIndex { get; }
        public int UuidInLayer { get; }
        public Uuid(int layerIndex, int uuid)
        {
            LayerIndex = layerIndex;
            UuidInLayer = uuid;
        }

        public bool Equals(Uuid other)
        {
            return LayerIndex == other.LayerIndex && UuidInLayer == other.UuidInLayer;
        }

        public override bool Equals(object typelessOther)
        {
            return typelessOther is Uuid other && LayerIndex == other.LayerIndex && UuidInLayer == other.UuidInLayer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LayerIndex.GetHashCode(), UuidInLayer.GetHashCode());
        }

        public static implicit operator Uuid((int layerIndex, int uuid) entry)
        {
            return new Uuid(entry.layerIndex, entry.uuid);
        }

        public override string ToString()
        {
            return $"{{Layer:{LayerIndex}, UUID:{UuidInLayer}}}";
        }

        public static bool operator ==(Uuid a, Uuid b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Uuid a, Uuid b)
        {
            return !(a == b);
        }
    }
}