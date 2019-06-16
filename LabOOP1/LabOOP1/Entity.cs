using System;
using System.Diagnostics;

namespace GameSpace
{
    public class Entity : Cell
    {
        public int X;
        public int Y;
        public Directions key;
        public bool Mod = false;
        public static FourMoves Moves = new FourMoves();
        public Stopwatch StopWatch = new Stopwatch();

        public Entity(Types p1, int p4, int p5) : base(p1)
        {
            key = 0;
            X = p4;
            Y = p5;
        }

    }
}
