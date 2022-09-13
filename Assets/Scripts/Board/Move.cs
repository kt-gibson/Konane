namespace Konane
{
    public readonly struct Move
    {
        public readonly Coord startPos;
        public readonly Coord targetPos;

        public Move (Coord startPos, Coord targetPos)
        {
            this.startPos = startPos;
            this.targetPos = targetPos;
        }
    }
}