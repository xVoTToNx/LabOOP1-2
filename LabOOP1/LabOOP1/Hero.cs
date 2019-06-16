using System;
using System.Diagnostics;

namespace GameSpace
{
    public class Hero : Entity
    {
        const float normalSpeedModifier = 0.9f;
        const float energizedSpeedModifier = 1.05f;
        static public int scoreForKill = 50;
        const int energizerDuration = 5;

        static public int score;
        static public int food;

        public bool booster;
        public bool isAfterBooster;

        public Stopwatch BoosterWatch = new Stopwatch();

        static public float speedModifier = 1.05f;

        public Hero(Types type, int x, int y) : base(type, x, y)
        {
            score = 0;
            food = 0;
            booster = true;
            isAfterBooster = false;
        }

        public void ChangeMod(bool flag)
        {
            if (flag)
            {
                Mod = true;
                type = Types.heroEnerg;
                speedModifier = normalSpeedModifier;
                Cell.ShowMe(X, Y, type);
            }
            else
            {
                Mod = false;
                type = Types.heroNormal;
                speedModifier = energizedSpeedModifier;
                Cell.ShowMe(X, Y, type);
            }
        }

        public void MoveHero(Directions dkey, Ghost[] ghosts, Energizer[] energizers)
        {
            key = (Directions)(((int)dkey + 2 * Convert.ToInt32(isAfterBooster)) % 4); // Reverse direction if booster has been used recently.
            Cell currentCell = Field.cell[X + Moves[key][0], Y + Moves[key][1]];
            switch (currentCell.type)
            {
                case Types.blank:
                    Field.StepHero(key, this);
                    break;
                case Types.food:
                    Food.takingFood(key, this);
                    break;
                case Types.wall:
                    break;
                case Types.energizer:
                    Energizer energ = (Energizer) currentCell;
                    energ.takingEnergizer(ghosts, key, this);
                    break;
                case Types.ghostNormal:
                    game.GameOver();
                    break;
                case Types.ghostEnerg:
                    Ghost ghst = (Ghost)currentCell;
                    ghst.killingGhost(energizers);
                    break;
            }
        }

        public void energizerTimer(Ghost[] ghosts)
        {
            TimeSpan ts = StopWatch.Elapsed;
            if (ts.Seconds > energizerDuration)
            {
                StopWatch.Reset();
                ChangeMod(false);
                for (int j = 0; j < ghosts.Length; j++)
                {
                    ghosts[j].ChangeMode(false);
                }
            }
            else if (ts.Seconds > energizerDuration - 2)
            {
                type = ((int)(ts.Milliseconds / 200) % 2 == 0) ? Types.heroEnerg : Types.heroNormal;
                Cell.ShowMe(X, Y, type);
            }
            if (BoosterWatch.IsRunning)
            {
                ts = BoosterWatch.Elapsed;
                if (ts.Seconds > energizerDuration)
                {
                    if (!isAfterBooster)
                    {
                        BoosterWatch.Restart();
                        isAfterBooster = true;
                    }
                    else
                    {
                        BoosterWatch.Stop();
                        isAfterBooster = false;
                    }
                }
            }
        }
    }
}
