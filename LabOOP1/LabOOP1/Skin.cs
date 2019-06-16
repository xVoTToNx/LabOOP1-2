using System;

namespace GameSpace
{
    public struct Skin
    {
        public char HeroNormal;
        public ConsoleColor chn;
        public char HeroEnergizer;
        public ConsoleColor che;
        public char GhostNormal;
        public ConsoleColor cgn;
        public char GhostEnergizer;
        public ConsoleColor cge;
        public char Food;
        public ConsoleColor cf;
        public char Wall;
        public ConsoleColor cw;
        public char Energizer;
        public ConsoleColor ce;
        public string Music;

        public Skin(char[] symb7)
        {
            HeroNormal = symb7[0];
            chn = ConsoleColor.Cyan;
            HeroEnergizer = symb7[1];
            che = ConsoleColor.Cyan;
            GhostNormal = symb7[2];
            cgn = ConsoleColor.Red;
            GhostEnergizer = symb7[3];
            cge = ConsoleColor.Green;
            Food = symb7[4];
            cf = ConsoleColor.DarkYellow;
            Wall = symb7[5];
            cw = ConsoleColor.White;
            Energizer = symb7[6];
            ce = ConsoleColor.Green;
            Music = "music\\megalovania.wav";
        }
    }
}
