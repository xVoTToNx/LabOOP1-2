using System;
using System.Collections.Generic;

namespace GameSpace
{
    public class Blank : Cell
    {
        public Blank(Types p1, bool p4 = false, List<Directions> p5 = null) : base(p1, p4, p5)
        { }
    }
}
