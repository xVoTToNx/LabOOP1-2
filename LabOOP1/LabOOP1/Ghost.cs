using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSpace
{
    public class Ghost : Entity
    {
        const float normalSpeedModifier = 1.25f;
        const float energizedSpeedModifier = 1f;

        int startX;
        int startY;
        static public float speedModifier = 1f;
        int targetX = 0;
        int targetY = 0;
        Directions targetKey;
        public bool IsAlive = true;
        bool hasTarget = false;
        bool isSeeingTarget;
        bool isTargetGone;
        Random vkr = new Random(DateTime.Now.Millisecond);

        public Cell undercell = new Blank(Types.blank);

        public Ghost(Types type, int x, int y) : base(type, x, y)
        {
            startX = x;
            startY = y;
        }

        Directions reverseDirection(Directions direction)
        {
            return (Directions)(((int)direction + 2) % 4);
        }

        public void ChangeMode(bool flag)
        {
            if (flag)
            {
                type = Types.ghostEnerg;
                Mod = true;
                speedModifier = normalSpeedModifier;
            }
            else
            {
                type = Types.ghostNormal;
                Mod = false;
                speedModifier = energizedSpeedModifier;
            }
            if (IsAlive)
                Cell.ShowMe(X, Y, type);
        }

        bool isFollowingTarger()
        {
            if (Field.cell[X, Y].isCrossroad)
            {
                lookingForHero();
            }
            return isSeeingTarget;
        }

        public void ChangeDirCrossroad(Directions kkey)
        {
            if (!isFollowingTarger())
                while (true)
                {
                    int buffer = vkr.Next(0, Field.cell[X, Y].dirs.Count);
                    if (!Moves[Field.cell[X, Y].dirs[buffer]].SequenceEqual(Moves[reverseDirection(kkey)]))
                        key = Field.cell[X, Y].dirs[buffer];
                    return;
                }
        }

        public void ChangeDirWall(Directions kkey)
        {
            if (!isFollowingTarger())
            {
                bool isStucked = true;
                for (int i = 0; i < Moves.Length; i++)
                {
                    if (Field.cell[X + Moves[(Directions)i][0], Y + Moves[(Directions)i][1]].type != Types.wall &&
                        !Moves[(Directions)i].SequenceEqual(Moves[reverseDirection(kkey)]))
                    {
                        key = (Directions)i;
                        isStucked = false;
                    }
                }

                if (isStucked)
                {
                    key = reverseDirection(kkey);
                }
            }
        
        }

        public void MoveGhost(bool isFreeToChangeDirection)
        {
            if (IsAlive)
            {
                Types cellType = Field.cell[X + Moves[key][0], Y + Moves[key][1]].type;
                switch (cellType)
                {
                    case Types.wall:
                        if(isFreeToChangeDirection)
                            ChangeDirWall(key);
                        MoveGhost(isFreeToChangeDirection: true);
                        break;

                    case Types.ghostNormal:
                        wentOnGhost();
                        break;


                    case Types.ghostEnerg:
                        wentOnGhost();
                        break;

                    case Types.heroEnerg:
                            death();
                            break;

                    case Types.heroNormal:
                        game.GameOver();
                        break;

                    default:
                        Field.StepGhost(key, this);
                        break;
                }
                if (isFreeToChangeDirection && cellType != Types.wall)
                    if (Field.cell[X, Y].isCrossroad)
                    {
                        ChangeDirCrossroad(key);
                    }
            }
        }

        private void wentOnGhost()
        {
            key = reverseDirection(key);
            hasTarget = false;
            isSeeingTarget = false;
        }

        private void death()
        {
            IsAlive = false;
            Hero.score += 50;
            Field.PrintScore();
            StopWatch.Start();
            Field.cell[X, Y] = undercell;
            undercell = new Blank(Types.blank);
            Cell.ShowMe(X, Y, type);
        }

        public void ThinkGhost(Hero hero)
        {
            lookingForHero();

            if (isSeeingTarget)
            {
                isTargetGone = false;

                if (!Mod)
                    MoveGhost(isFreeToChangeDirection:false);
                else
                {
                    key = reverseDirection(key);
                    hasTarget = false;
                    isSeeingTarget = false;
                    MoveGhost(isFreeToChangeDirection:false);
                }
            }
            else
            {
                if (hasTarget)
                {
                    if (!isTargetGone)
                    {
                        targetKey = hero.key;
                        targetX = hero.X - Moves[targetKey][(int)Coordinates.x];
                        targetY = hero.Y - Moves[targetKey][(int)Coordinates.y];
                        isTargetGone = true;
                        MoveGhost(isFreeToChangeDirection: false);
                    }
                    else
                    {
                        MoveGhost(isFreeToChangeDirection: false);
                    }

                    if (targetX == X && targetY == Y)
                    {
                        key = targetKey;
                        hasTarget = false;
                        isSeeingTarget = false;
                    }
                }
                else
                {
                    MoveGhost(isFreeToChangeDirection: true);
                }
            }
        }

        public void GhostsTimer()
        {
            TimeSpan ts;
            if (!IsAlive)
            {
                ts = StopWatch.Elapsed;
                if (ts.Seconds > 15)
                {
                    StopWatch.Reset();
                    IsAlive = true;
                    X = startX;
                    Y = startY;
                    if (Field.cell[X, Y].type == Types.heroNormal)
                        game.GameOver();
                    else
                    {
                        List<Directions> bdirs = Field.cell[X, Y].dirs;
                        bool bisCrossroad = Field.cell[X, Y].isCrossroad;
                        Field.cell[X, Y] = this;
                        Field.cell[X, Y].dirs = bdirs;
                        Field.cell[X, Y].isCrossroad = bisCrossroad;
                        Cell.ShowMe(X, Y, type);
                    }
                }
            }
        }

        void lookingForHero()
        {
            isSeeingTarget = false;

            if (!Field.cell[X, Y].isCrossroad)
            {
                lookOneDirection(key);
                return;
            }

            foreach (Directions dir in Field.cell[X, Y].dirs)
            {
                if (dir != reverseDirection(key))
                {
                    if (lookOneDirection(dir))
                        break;
                }
            }
        }

        bool lookOneDirection(Directions dir)
        {
            int fieldX = X;
            int fieldY = Y;
            while (Field.cell[fieldX, fieldY].type != Types.wall)
            {
                if (Field.cell[fieldX, fieldY].type == Types.heroNormal || Field.cell[fieldX, fieldY].type == Types.heroEnerg)
                {
                    hasTarget = true;
                    isSeeingTarget = true;
                    key = dir;
                    return true;
                }
                else
                {
                    fieldX += Moves[dir][(int)Coordinates.x];
                    fieldY += Moves[dir][(int)Coordinates.y];

                }
            }
            return false;
        }

        public void killingGhost(Energizer[] energizers)
        {
            IsAlive = false;
            StopWatch.Start();
            if (undercell.type == Types.food)
            {
                Hero.food++;
                if (Field.MaxFood == Hero.food)
                    game.YouWon();
            }
            undercell = new Blank(Types.blank);
            Hero.score += Hero.scoreForKill;
            Field.PrintScore();
            for (int i = 0; i < energizers.Length; i++)
                if (energizers[i] != null)
                    if (!energizers[i].isAlive)
                    {
                        energizers[i].isAlive = true;
                        Field.cell[energizers[i].X, energizers[i].Y] = energizers[i];
                        Cell.ShowMe(X, Y, type);
                    }
            Field.StepHero(key, this);
        }
    }
}
