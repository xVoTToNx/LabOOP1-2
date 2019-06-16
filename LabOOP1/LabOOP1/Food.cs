using System;

namespace GameSpace
{
    public class Food : Cell
    {
        public Food(Types p1) : base(p1)
        {
        }

        public static void takingFood(Directions key, Hero hero)
        {
            Hero.score++;
            Hero.food++;
            if (Field.MaxFood == Hero.food)
                game.YouWon();
            Field.PrintScore();
            Field.StepHero(key, hero);
        }
    }
}
