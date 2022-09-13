using System;

namespace Konane
{
    public struct Coord : IComparable<Coord> //Does this even need to be IComparable?
    {
        public readonly int fileIdx;
        public readonly int rankIdx;

        public Coord(int rankIdx, int fileIdx)
        {
            this.fileIdx = fileIdx;
            this.rankIdx = rankIdx;
        }

        public int CompareTo(Coord other)
        {
            return (fileIdx == other.fileIdx && rankIdx == other.rankIdx) ? 0 : 1;
        }
    }
}
