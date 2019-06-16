using System;
using System.Collections.Generic;
using System.Linq;

namespace LabOOP1
{
    public class Ghost : Entity
    {
        public int start_x;
        public int start_y;
        static public float speedModifier = 1f;
        int targetX = 0;
        int targetY = 0;
        Directions targetKey;
        public bool isAlive = true;
        bool hasTarget = false;
        bool isSeeingTarget;
        bool isTargetGone;
        Random vkr = new Random(DateTime.Now.Millisecond);

        public Cell undercell = new Blank(Types.blank, ' ', ConsoleColor.Black);

        public Ghost(Types type, char symb_n, char symb_e, ConsoleColor color_n, ConsoleColor color_e,
            int x, int y) : base(type, symb_n, symb_e, color_n, color_e, x, y)
        {
            start_x = x;
            start_y = y;
        }

        public override void ShowMe(int p1, int p2)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write((int)key);
        }

        Directions ReverseDirection(int direction)
        {
            return (Directions)((direction + 2) % 4);
        }

        Directions ReverseDirection(Directions direction)
        {
            return (Directions)(((int)direction + 2) % 4);
        }

        public void ChangeMode(bool flag)
        {
            if (flag)
            {
                color = ecolor;
                symbol = eskin;
                mod = true;
                speedModifier = 1.25f;
            }
            else
            {
                color = ncolor;
                symbol = nskin;
                mod = false;
                speedModifier = 1f;
            }
            if (isAlive)
                ShowMyself();
        }

        bool isFollowingTarger()
        {
            if (Field.cell[x, y].isCrossroad)
            {
                LookingForHero();
            }
            return isSeeingTarget;
        }

        public void ChangeDirCrossroad(int kkey)
        {
            if (!isFollowingTarger())
                while (true)
                {
                    int buffer = vkr.Next(0, Field.cell[x, y].dirs.Count);
                    if (!moves[Field.cell[x, y].dirs[buffer]].SequenceEqual(moves[ReverseDirection(kkey)]))
                        key = Field.cell[x, y].dirs[buffer];
                    return;
                }
        }

        public void ChangeDirWall(int kkey)
        {
            if (!isFollowingTarger())
            {
                bool isStucked = true;
                for (int i = 0; i < moves.Length; i++)
                {
                    if (Field.cell[x + moves[(Directions)i][0], y + moves[(Directions)i][1]].type != Types.wall &&
                        !moves[(Directions)i].SequenceEqual(moves[ReverseDirection(kkey)]))
                    {
                        key = (Directions)i;
                        isStucked = false;
                    }
                }

                if (isStucked)
                {
                    key = ReverseDirection(kkey);
                }
            }
        
        }

        public void MoveGhost(bool flag)
        {
            if (isAlive)
            {
                switch (Field.cell[x + moves[key][0], y + moves[key][1]].type)
                {
                    case Types.blank:
                        Field.StepGhost(key, this);
                        break;
                    case Types.food:
                        Field.StepGhost(key, this);
                        break;
                    case Types.wall:
                        ChangeDirWall((int)key);
                        MoveGhost(true);
                        break;
                    case Types.energizer:
                        Field.StepGhost(key, this);
                        break;
                    case Types.ghost:
                        key = ReverseDirection(key);
                        break;
                    case Types.hero:
                        if (mod)
                        {
                            Death();
                            break;
                        }
                        else
                            Game.GameOver();
                        break;
                }
                if (flag)
                    if (Field.cell[x, y].isCrossroad)
                    {
                        ChangeDirCrossroad((int)key);
                    }
            }
        }

        private void Death()
        {
            isAlive = false;
            Hero.score += 50;
            Field.PrintScore();
            sw.Start();
            Field.cell[x, y] = undercell;
            undercell = new Blank(Types.blank, ' ', ConsoleColor.Black);
            Field.cell[x, y].ShowMe(x, y);
        }

        public void kek(int i)
        {
            Console.SetCursorPosition(0, 26 + i);
            Console.Write(x + " " + y + " "  + targetX + " " + targetY + "      ");
        }

        public void ThinkGhost(Hero hero)
        {
            if (Field.cell[x, y].isCrossroad)
            {
                LookingForHero();
            }


            if (!hasTarget)
                MoveGhost(true);
            else
            {
                if (!isSeeingTarget)
                {
                    if (!isTargetGone)
                    {
                        targetKey = hero.key;
                        targetX = hero.x - moves[targetKey][(int)Coordinates.x];
                        targetY = hero.y - moves[targetKey][(int)Coordinates.y];
                        isTargetGone = true;
                    }
                    if (targetX == x && targetY == y)
                    {
                        key = targetKey;
                        hasTarget = false;
                        isTargetGone = false;
                    }
                }
                isSeeingTarget = false;
                if (!mod)
                    MoveGhost(false);
                else
                {
                    key = ReverseDirection(key);
                    hasTarget = false;
                    isTargetGone = false;
                    MoveGhost(false);
                }
            }
        }

        public void GhostsTimer()
        {
            TimeSpan ts;
            if (!isAlive)
            {
                ts = sw.Elapsed;
                if (ts.Seconds > 15)
                {
                    sw.Reset();
                    isAlive = true;
                    x = start_x;
                    y = start_y;
                    if (Field.cell[x, y].type == Types.hero)
                        Game.GameOver();
                    else
                    {
                        List<Directions> bdirs = Field.cell[x, y].dirs;
                        bool bisCrossroad = Field.cell[x, y].isCrossroad;
                        Field.cell[x, y] = this;
                        Field.cell[x, y].dirs = bdirs;
                        Field.cell[x, y].isCrossroad = bisCrossroad;
                        ShowMyself();
                    }
                }
            }
        }

        void LookingForHero()
        {
            int field_x = x;
            int field_y = y;
            bool hasFoundHero = false;
            foreach (Directions dir in Field.cell[x, y].dirs)
            {
                if (dir != ReverseDirection(key))
                {
                    while (Field.cell[field_x, field_y].type != Types.wall)  //Looking for hero.
                    {
                        if (Field.cell[field_x, field_y].type == Types.hero)
                        {
                            hasTarget = true;
                            isSeeingTarget = true;
                            hasFoundHero = true;
                            key = dir;
                            break;
                        }
                        else
                        {
                            field_x += moves[dir][(int)Coordinates.x];
                            field_y += moves[dir][(int)Coordinates.y];
                            //Console.SetCursorPosition(field_x, field_y);
                            //Console.Write((int)key);

                        }
                    }
                    field_x = x;
                    field_y = y;
                }
                if (hasFoundHero)
                    break;
            }
        }
    }
}
