using System;
using UnityEngine;

namespace Grid
{
    public struct GridPosition : IEquatable<GridPosition>
    {
        private static readonly GridPosition Invalid = new (int.MinValue, int.MinValue);
        
        public bool IsValid() => this != Invalid;

        public int x;
        public int z;
    
        public GridPosition(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override string ToString()
        {
            return $"x: {x}, z: {z}";
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.z == b.z;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public bool Equals(GridPosition other)
        {
            return x == other.x && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, z);
        }

        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.x + b.x, a.z + b.z);
        }

        public static GridPosition operator -(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.x - b.x, a.z - b.z);
        }
        public static int GetDistance(GridPosition a, GridPosition b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
        }
    }
}
