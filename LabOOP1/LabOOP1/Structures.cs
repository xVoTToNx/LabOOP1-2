using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Media;


namespace LabOOP1Struct
{
    public enum Types
    {
        blank,
        hero,
        ghost,
        wall,
        food,
        energizer
    }
    public struct Cell
    {
        public Types type;
    }

    public struct Hero
    {
        public int x;
        public int y;
        public bool mod;
        public string symbol;
        public ConsoleColor color;
        public int score;
        public Stopwatch sw;
        public Hero(int p1, int p2, string p3)
        {
            x = p1;
            y = p2;
            mod = false;
            symbol = p3;
            score = 0;
            color = ConsoleColor.Cyan;
            sw = new Stopwatch();
        }
    }

    public struct Blank
    {
        public string symbol;
    }

    public struct Wall
    {
        public string symbol;
        public ConsoleColor color;
    }

    public struct Food
    {
        public string symbol;
        public ConsoleColor color;
    }

    public struct Energizer
    {
        public int x;
        public int y;
        public string symbol;
        public ConsoleColor color;
        public bool isAlive;
        public Energizer(int p1, int p2, string p3)
        {
            x = p1;
            y = p2;
            symbol = p3;
            color = ConsoleColor.Green;
            isAlive = true;
        }
    }

    public struct Ghost
    {
        public int x;
        public int y;
        public int start_x;
        public int start_y;
        public bool mod;
        public string symbol;
        public ConsoleColor color;
        public bool isAlive;
        public Stopwatch sw;
        public Ghost(int p1, int p2, string p3)
        {
            x = p1;
            y = p2;
            start_x = p1;
            start_y = p2;
            mod = false;
            symbol = p3;
            color = ConsoleColor.Red;
            isAlive = true;
            sw = new Stopwatch();
        }
    }
    public struct Field
    {
        public int width;
        public int heigh;
        public Cell[,] cell;
        public Field(int p1, int p2)
        {
            width = p1;
            heigh = p2;
            cell = new Cell[p1, p2];
        }
    }

    public class Game
    {
        Dictionary<string, string> symbols = new Dictionary<string, string>{
                {"hero_normal","\u2692" },
                {"hero_energizer", "\u2694" },
                {"ghost_normal", "\u262d" },
                {"ghost_energizer", "\u262e" },
                {"food", "\u2022" },
                {"wall", "\u233a" },
                {"energizer", "\u0024" } };

        public Hero hero;
        public Blank blank;
        public Wall wall;
        public Food food;
        public Energizer[] energizers;
        public Ghost[] ghosts;
        public Field field;

        public void GameCreation()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            string[] lines = File.ReadAllLines("map\\Alpha.txt", Encoding.UTF8);
            int energizers_number = 0;
            int ghosts_number = 0;
            Console.SetWindowSize(lines[0].Length + 1, lines.Length + 2);
            Console.SetBufferSize(lines[0].Length + 1, lines.Length + 2);
            foreach (string str in lines)
                foreach (char ch in str)
                {
                    if (ch == 'E')
                        energizers_number++;
                    if (ch == 'G')
                        ghosts_number++;
                }
            blank.symbol = " ";
            wall.symbol = symbols["wall"];
            wall.color = ConsoleColor.White;
            food.symbol = symbols["food"];
            food.color = ConsoleColor.DarkYellow;
            energizers = new Energizer[energizers_number];
            ghosts = new Ghost[ghosts_number];

            int fieldx = lines[0].Length;
            int fieldy = lines.Length;
            field = new Field(fieldx, fieldy);
            for (int i = 0; i < field.heigh; i++)
            {
                if (lines[i].Length == fieldx)
                {
                    for (int j = 0; j < field.width; j++)
                    {
                        if (lines[i][j] == '@')
                        {
                            field.cell[j, i].type = Types.wall;
                            Console.ForegroundColor = wall.color;
                            Console.Write(wall.symbol);
                        }
                        else if (lines[i][j] == ' ')
                        {
                            field.cell[j, i].type = Types.food;
                            Console.ForegroundColor = food.color;
                            Console.Write(food.symbol);
                        }
                        else if (lines[i][j] == 'E')
                        {
                            field.cell[j, i].type = Types.energizer;
                            energizers_number--;
                            energizers[energizers_number] = new Energizer(j, i, symbols["energizer"]);
                            energizers[energizers_number].x = j;
                            energizers[energizers_number].y = i;
                            Console.ForegroundColor = energizers[energizers_number].color;
                            Console.Write(energizers[energizers_number].symbol);
                        }
                        else if (lines[i][j] == 'G')
                        {
                            field.cell[j, i].type = Types.ghost;
                            ghosts_number--;
                            ghosts[ghosts_number] = new Ghost(j, i, symbols["ghost_normal"]);
                            Console.ForegroundColor = ghosts[ghosts_number].color;
                            Console.Write(ghosts[ghosts_number].symbol);
                        }
                        else if (lines[i][j] == 'H')
                        {
                            field.cell[j, i].type = Types.hero;
                            hero = new Hero(j, i, symbols["hero_normal"]);
                            Console.ForegroundColor = hero.color;
                            Console.Write(hero.symbol);
                        }
                        else
                        {
                            Console.WriteLine("\nError. Wrong symbols in map! Only 'G', 'H', ' ', '@', 'E' can be there!");
                            Environment.Exit(0);
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("\nError. Wrong size of the field!");
                    Environment.Exit(0);
                }

            }

            Console.SetCursorPosition(field.width / 2 - 5, field.heigh + 1);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Score: ");
        }

        public void ReadKey()
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.W:
                    {
                        MoveHero(0, -1);
                        break;
                    }
                case ConsoleKey.A:
                    {
                        MoveHero(-1, 0);
                        break;
                    }
                case ConsoleKey.S:
                    {
                        MoveHero(0, 1);
                        break;
                    }
                case ConsoleKey.D:
                    {
                        MoveHero(1, 0);
                        break;
                    }
            }
        }

        public void MoveHero(int xkey, int ykey)
        {
            switch (field.cell[hero.x + xkey, hero.y + ykey].type)
            {
                case Types.blank:
                    ReWriteCell(xkey, ykey);
                    break;
                case Types.food:
                    hero.score++;
                    PrintScore();
                    ReWriteCell(xkey, ykey);
                    break;
                case Types.wall:
                    break;
                case Types.energizer:
                    hero.score += 10;
                    PrintScore();
                    for (int i = 0; i < energizers.Length; i++)
                    {
                        if (hero.x + xkey == energizers[i].x && hero.y + ykey == energizers[i].y)
                        {
                            if (energizers[i].isAlive)
                            {
                                energizers[i].isAlive = false;
                                hero.sw.Start();
                                hero.symbol = symbols["hero_energizer"];
                                hero.mod = true;
                                for (int j = 0; j < ghosts.Length; j++)
                                {
                                    ghosts[j].mod = true;
                                    ghosts[j].color = ConsoleColor.Green;
                                    ghosts[j].symbol = symbols["ghost_energizer"];
                                    Console.SetCursorPosition(ghosts[j].x, ghosts[j].y);
                                    Console.ForegroundColor = ghosts[j].color;
                                    Console.Write(ghosts[j].symbol);
                                }
                            }
                            ReWriteCell(xkey, ykey);
                        }
                    }
                    break;
                case Types.ghost:
                    if (hero.mod)
                    {
                        for (int i = 0; i < ghosts.Length; i++)
                            if (hero.x + xkey == ghosts[i].x && hero.y + ykey == ghosts[i].y)
                            {
                                ghosts[i].isAlive = false;
                                ghosts[i].sw.Start();
                            }
                        hero.score += 50;
                        PrintScore();
                        for (int i = 0; i < energizers.Length; i++)
                            if (!energizers[i].isAlive)
                            {
                                energizers[i].isAlive = true;
                                field.cell[energizers[i].x, energizers[i].y].type = Types.energizer;
                                Console.SetCursorPosition(energizers[i].x, energizers[i].y);
                                Console.ForegroundColor = energizers[i].color;
                                Console.Write(energizers[i].symbol);
                            }
                        ReWriteCell(xkey, ykey);
                    }
                    else
                        GameOver();
                    break;
            }
        }

        public void ReWriteCell(int xkey, int ykey)
        {
                field.cell[hero.x, hero.y].type = Types.blank;
                Console.SetCursorPosition(hero.x, hero.y);
                Console.Write(blank.symbol);
                hero.x += xkey;
                hero.y += ykey;
                field.cell[hero.x, hero.y].type = Types.hero;
                Console.SetCursorPosition(hero.x, hero.y);
                Console.ForegroundColor = hero.color;
                Console.Write(hero.symbol);
        }

        public void EnergizerTime()
        {
            TimeSpan ts = hero.sw.Elapsed;
            if (ts.Seconds > 15)
            {
                hero.sw.Reset();
                hero.mod = false;
                for (int j = 0; j < ghosts.Length; j++)
                {
                    ghosts[j].mod = false;
                    ghosts[j].color = ConsoleColor.Red;
                    ghosts[j].symbol = symbols["ghost_normal"];
                    Console.SetCursorPosition(ghosts[j].x, ghosts[j].y);
                    Console.ForegroundColor = ghosts[j].color;
                    if(ghosts[j].isAlive)
                        Console.Write(ghosts[j].symbol);
                }
                hero.symbol = symbols["hero_normal"];
                Console.SetCursorPosition(hero.x, hero.y);
                Console.ForegroundColor = hero.color;
                Console.Write(hero.symbol);
            }
        }

        public void GhostsTime()
        {
            TimeSpan ts;
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (!ghosts[i].isAlive)
                {
                    ts = ghosts[i].sw.Elapsed;
                    if (ts.Seconds > 30)
                    {
                        ghosts[i].sw.Reset();
                        ghosts[i].isAlive = true;
                        ghosts[i].x = ghosts[i].start_x;
                        ghosts[i].y = ghosts[i].start_y;
                        field.cell[ghosts[i].x, ghosts[i].y].type = Types.ghost;
                        Console.SetCursorPosition(ghosts[i].x, ghosts[i].y);
                        Console.ForegroundColor = ghosts[i].color;
                        Console.Write(ghosts[i].symbol);
                    }
                }
            }
        }

        public void PrintScore()
        {
            Console.SetCursorPosition(field.width / 2 + 2, field.heigh + 1);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("{0}     ", hero.score);
        }

        public void GameOver()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Soyuz nerushimy respublik svobodnykh\nSplotila naveki velikaya Rus'!\n" +
                "Da zdravstvuyet sozdanny voley narodov\nYediny, moguchy Sovetsky Soyuz!\n\n" +
                "Slav'sya, Otechestvo nashe svobodnoye,\nDruzhby narodov nadyozhny oplot!" +
                "Znamya Sovetskoye, znamya narodnoye\nPust' ot pobedy k pobede vedyot!\n\n" +
                "Skvoz' grozy siyalo nam solntse svobody,\nI Lenin veliky nam put' ozaril:" +
                "Nas vyrastil Stalin – na vernost' narodu,\nNa trud i na podvigi nas vdokhnovil!");
        }

        public void Action()
        {
            do
            {
                while (!Console.KeyAvailable)
                {
                    EnergizerTime();
                    GhostsTime();
                }
                ReadKey();
            } while (true);
        }
    }

    /*class Program
    {
        static void Main()
        {
            Game game = new Game();
            game.GameCreation();
            game.Action();
        }
    }*/
}
