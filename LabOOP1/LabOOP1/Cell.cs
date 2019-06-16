using System;
using System.Collections.Generic;

namespace GameSpace
{
    abstract public class Cell
    {
        public Types type;
        public bool isCrossroad;
        public List<Directions> dirs;
        public static View ShowMe;
        public static Game game;

        public Cell(Types p1, bool p4 = false, List<Directions> p5 = null)
        {
            type = p1;
            isCrossroad = p4;
            dirs = p5;
        }
    }
}
