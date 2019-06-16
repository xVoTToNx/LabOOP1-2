using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Media;
using Newtonsoft.Json;

namespace LabOOP1
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

    struct Skin
    {
        public char Hero_normal;
        public ConsoleColor chn;
        public char Hero_energizer;
        public ConsoleColor che;
        public char Ghost_normal;
        public ConsoleColor cgn;
        public char Ghost_energizer;
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
            Hero_normal = symb7[0];
            chn = ConsoleColor.Cyan;
            Hero_energizer = symb7[1];
            che = ConsoleColor.Cyan;
            Ghost_normal = symb7[2];
            cgn = ConsoleColor.Red;
            Ghost_energizer = symb7[3];
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

    struct Player
    {
        public string name;

        public string map;

        public string skin;

        public int deltaGhost;

        public bool isNew;

        public void Reset()
        {
            map = "map\\Alpha.txt";
            skin = "skins\\B.txt";
            isNew = true;
            deltaGhost = 7500;
        }

        public void Save()
        {
            string buffer = JsonConvert.SerializeObject(this);
            File.WriteAllText("players\\" + name + ".txt", buffer);
        }
    }

    public static class DeltaTime
    {
        private static long lastTime = System.Environment.TickCount;
        private static int fps = 1;
        private static int frames;

        private static float deltaTime = 0.005f;

        public static void Update()
        {
            var currentTick = System.Environment.TickCount;
            if (currentTick - lastTime >= 1000)
            {
                fps = frames;
                frames = 0;
                lastTime = currentTick;
            }
            frames++;

            deltaTime = currentTick - lastTime;
        }

        public static int getFPS()
        {
            return fps;
        }

        public static float getDeltaTime()
        {
            return (deltaTime / 1000.0f);
        }
    }

    abstract public class Cell
    {
        public Types type;
        public bool isCrossroad;
        public List<int> dirs;
        public char symbol;
        public ConsoleColor color;
        abstract public void ShowMe(int x, int y);

        public Cell(Types p1, char p2, ConsoleColor p3, bool p4 = false, List<int> p5 = null)
        {
            type = p1;
            symbol = p2;
            color = p3;
            isCrossroad = p4;
            dirs = p5;
        }
    }

    public class Wall : Cell
    {
        public Wall(Types p1, char p2, ConsoleColor p3) : base(p1, p2, p3)
        { }

        public override void ShowMe(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }

    public class Food : Cell
    {
        public Food(Types p1, char p2, ConsoleColor p3) : base(p1, p2, p3)
        { }

        public override void ShowMe(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }

    public class Blank : Cell
    {
        public Blank(Types p1, char p2, ConsoleColor p3, bool p4 = false, List<int> p5 = null) : base(p1, p2, p3, p4, p5)
        {}

        public override void ShowMe(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }

    public class Entity : Cell
    {
        public int x;
        public int y;
        public int key;
        public bool mod;
        protected char nskin;
        protected char eskin;
        protected ConsoleColor ncolor;
        protected ConsoleColor ecolor;
        public Stopwatch sw;

        public Entity(Types p1, char p2, char p6, ConsoleColor p3, ConsoleColor p7, int p4, int p5) : base(p1, p2, p3)
        {
            sw = new Stopwatch();
            key = 0;
            nskin = p2;
            eskin = p6;
            ncolor = p3;
            ecolor = p7;
            x = p4;
            y = p5;
            mod = false;
        }

        public void ShowMyself()
        {
            ShowMe(0, 0);
        }

        public override void ShowMe(int p1, int p2)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }

    public class Hero : Entity
    {
        static public int score;
        static public int food;
        public bool saver;
        public bool blackout;
        public Stopwatch saver_watch = new Stopwatch();
        static public float speedModifier = 1.05f;
        public Hero(Types type, char symb_n, char symb_e, ConsoleColor color_n, ConsoleColor color_e,
            int x, int y) : base(type, symb_n, symb_e, color_n, color_e, x, y)
        {
            score = 0;
            food = 0;
            saver = true;
            blackout = false;
        }

        public void ChangeMod(bool flag)
        {
            if (flag)
            {
                mod = true;
                color = ecolor;
                symbol = eskin;
                speedModifier = 0.9f;
                ShowMyself();
            }
            else
            {
                mod = false;
                color = ncolor;
                symbol = nskin;
                speedModifier = 1.05f;
                ShowMyself();
            }
        }

        public void MoveHero(int key,Ghost[] ghosts, Energizer[] energizers, Field field)
        {
            switch (field[x + Ghost.keys[key][0], y + Ghost.keys[key][1]].type)
            {
                case Types.blank:
                    field.Step(key, this);
                    break;
                case Types.food:
                    score++;
                    food++;
                    if (Field.maxFood == food)
                        Game.YouWon();
                    Field.PrintScore();
                    field.Step(key, this);
                    break;
                case Types.wall:
                    break;
                case Types.energizer:
                    score += 10;
                    Field.PrintScore();
                    Energizer ebuffer = (Energizer)field[x + Ghost.keys[key][0], y + Ghost.keys[key][1]];
                    ebuffer.isAlive = false;
                    field.Step(key, this);
                    if (sw.IsRunning)
                    {
                        sw.Restart();
                        ChangeMod(true);
                    }
                    else
                    {
                        sw.Start();
                        ChangeMod(true);
                        for (int j = 0; j < ghosts.Length; j++)
                        {
                            ghosts[j].ChangeMod(true);
                        }
                    }
                    break;
                case Types.ghost:
                    if (mod)
                    {
                        Ghost gbuffer = (Ghost)field[x + Ghost.keys[key][0], y + Ghost.keys[key][1]];
                        gbuffer.isAlive = false;
                        gbuffer.sw.Start();
                        if (gbuffer.undercell.type == Types.food)
                        {
                            food++;
                            if (Field.maxFood == food)
                                Game.YouWon();
                        }
                        gbuffer.undercell = new Blank(Types.blank, ' ', ConsoleColor.Black);
                        score += 50;
                        Field.PrintScore();
                        for (int i = 0; i < energizers.Length; i++)
                            if (!energizers[i].isAlive)
                            {
                                energizers[i].isAlive = true;
                                field[energizers[i].x, energizers[i].y] = energizers[i];
                                energizers[i].ShowMyself();
                            }
                        field.Step(key, this);
                    }
                    else
                        Game.GameOver();
                    break;
            }
        }

        public void EnergizerTimer(Ghost[] ghosts)
        {
            TimeSpan ts = sw.Elapsed;
            if (ts.Seconds > 5)
            {
                sw.Reset();
                ChangeMod(false);
                for (int j = 0; j < ghosts.Length; j++)
                {
                    ghosts[j].ChangeMod(false);
                }
            }
            else if (ts.Seconds > 3)
            {
                color = ((int)(ts.Milliseconds / 200) % 2 == 0) ? ecolor : ncolor;
                ShowMyself();
            }
            if (saver_watch.IsRunning)
            {
                ts = saver_watch.Elapsed;
                if (ts.Seconds > 5)
                {
                    if (!blackout)
                    {
                        saver_watch.Restart();
                        blackout = true;
                    }
                    else
                    {
                        saver_watch.Stop();
                        blackout = false;
                    }
                }
            }
        }
    }

    public class Energizer : Cell
    {
        public bool isAlive;
        public int x;
        public int y;

        public Energizer(Types p1, char p2, ConsoleColor p3, int p4, int p5) : base(p1, p2, p3)
        {
            isAlive = true;
            x = p4;
            y = p5;
        }

        public void ShowMyself()
        {
            ShowMe(0, 0);
        }

        public override void ShowMe(int p1, int p2)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }
    }

    public class Ghost : Entity
    {
        public int start_x;
        public int start_y;
        static public float speedModifier = 1f;
        int targetX;
        int targetY;
        int targetKey;
        public bool isAlive;
        bool hasTarget;
        bool isSeeingTarget;
        bool isTargetGone;

        static public int[][] keys = new int[4][] {
            new int [2] {0, -1},
            new int [2] {-1, 0},
            new int [2] {0, 1},
            new int [2] {1, 0} };

        public Cell undercell = new Blank(Types.blank, ' ', ConsoleColor.Black);

        public Ghost(Types type, char symb_n, char symb_e, ConsoleColor color_n, ConsoleColor color_e,
            int x, int y) : base(type, symb_n, symb_e, color_n, color_e, x, y)
        {
            start_x = x;
            start_y = y;
            isAlive = true;
            hasTarget = false;
        }

        public void ChangeMod(bool flag)
        {
            if (flag)
            {
                color = ecolor;
                symbol = eskin;
                mod = true;
                speedModifier = 1.25f;
                if (isAlive)
                    ShowMyself();
            }
            else
            {
                color = ncolor;
                symbol = nskin;
                speedModifier = 1f;
                mod = false;
                if (isAlive)
                    ShowMyself();
            }
        }

        public void ChangeDir(bool flag, int kkey, Field field)
        {
            if (flag)
            {
                Random vkr = new Random();
                while (true)
                {
                    int buffer = vkr.Next(0, field[x, y].dirs.Count);
                    if (!keys[field[x, y].dirs[buffer]].SequenceEqual(keys[(kkey + 2) % 4]))
                        key = field[x, y].dirs[buffer];
                    return;
                }
            }
            else
            {
                flag = true;
                for(int i = 0; i < keys.Length; i++)
                {
                    if (field[x + keys[i][0], y + keys[i][1]].type != Types.wall && !keys[i].SequenceEqual(keys[(kkey + 2) % 4]))
                    {
                        key = i;
                        flag = false;
                    }
                }
                if(flag)
                {
                    key = (kkey + 2) % 4;
                }
            }
        }

        public void MoveGhost(bool flag, Field field)
        {
            if (isAlive)
            {
                switch (field[x + keys[key][0], y + keys[key][1]].type)
                {
                    case Types.blank:
                        field.Step(key, this);
                        break;
                    case Types.food:
                        field.Step(key, this);
                        break;
                    case Types.wall:
                        ChangeDir(false, key, field);
                        MoveGhost(true, field);
                        break;
                    case Types.energizer:
                        field.Step(key, this);
                        break;
                    case Types.ghost:
                        key = (key + 2) % 4;
                        break;
                    case Types.hero:
                        if (mod)
                        {
                            isAlive = false;
                            Hero.score += 50;
                            Field.PrintScore();
                            sw.Start();
                            field[x, y] = undercell;
                            undercell = new Blank(Types.blank, ' ', ConsoleColor.Black);
                            field[x, y].ShowMe(x, y);
                            break;
                        }
                        else
                            Game.GameOver();
                        break;
                }
                if(flag)
                    if (field[x, y].isCrossroad)
                    {
                        ChangeDir(true, key, field);
                    }
            }
        }

        public void ThinkGhost(Hero hero, Field field)
        {
            int field_x = x;
            int field_y = y;
            while (field[field_x, field_y].type != Types.wall)
            {
                if(field[field_x, field_y].type == Types.hero)
                {
                    hasTarget = true;
                    isSeeingTarget = true;
                    break;
                }
                else
                {
                    field_x += keys[key][0];
                    field_y += keys[key][1];
                }
            }

            if (!hasTarget)
                MoveGhost(true, field);
            else
            {
                if (!isSeeingTarget)
                {
                    if (!isTargetGone)
                    {
                        targetKey = hero.key;
                        targetX = hero.x - keys[targetKey][0];
                        targetY = hero.y - keys[targetKey][1];
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
                    MoveGhost(false, field);
                else
                {
                    key = (key + 2) % 4;
                    hasTarget = false;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    isTargetGone = false;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    MoveGhost(false, field);
                }
            }
        }

        public void GhostsTimer(Field field)
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
                    if (field[x, y].type == Types.hero)
                        Game.GameOver();
                    else
                    {
                        List<int> bdirs = field[x, y].dirs;
                        bool bisCrossroad = field[x, y].isCrossroad;
                        field[x, y] = this;
                        field[x, y].dirs = bdirs;
                        field[x, y].isCrossroad = bisCrossroad;
                        ShowMyself();
                    }
                }
            }
        }
    }

    public class Field
    {
        static public int width;
        static public int heigh;
        static public int maxFood;
        Cell[,] cell;

        public Cell this[int x, int y]
        {
            get { return cell[x, y];  }
            set { cell[x, y] = value; }
        }
        public void CreateField()
        {
            cell = new Cell[width, heigh];
        }

        public void ReWrite()
        {
            for (int i = 0; i < heigh; i++)
                for (int j = 0; j < width; j++)
                    cell[j, i].ShowMe(j, i);
        }

        public void Step(int key, Entity entity)
        {
            if (entity.type == Types.hero && Game.KITTY)
            {
                cell[entity.x, entity.y] = new Blank(Types.blank, ' ', ConsoleColor.Black, 
                    cell[entity.x, entity.y].isCrossroad, cell[entity.x, entity.y].dirs);
                cell[entity.x, entity.y].ShowMe(entity.x, entity.y);
                entity.x += Ghost.keys[key][0];
                entity.y += Ghost.keys[key][1];
                cell[entity.x, entity.y] = entity;
                entity.ShowMyself();
            }
            if (entity.type == Types.ghost && Game.KITTY)
            {
                Ghost ghost = (Ghost)entity;
                cell[ghost.x, ghost.y] = ghost.undercell;
                cell[ghost.x, ghost.y].ShowMe(ghost.x, ghost.y);
                ghost.x += Ghost.keys[key][0];
                ghost.y += Ghost.keys[key][1];
                ghost.undercell = cell[ghost.x, ghost.y];
                cell[ghost.x, ghost.y] = ghost;
                cell[ghost.x, ghost.y].isCrossroad = ghost.undercell.isCrossroad;
                cell[ghost.x, ghost.y].dirs = ghost.undercell.dirs;
                ghost.ShowMyself();
            }
        }

        static public void PrintScore()
        {
            if (Game.KITTY)
            {
                Console.SetCursorPosition(8, heigh + 1);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("{0}", Hero.score);
            }
        }
    }

    static public class Game
    {
        static public bool KITTY = true;
        static int cursor = 1;
        static Field field = new Field();
        static Player currentPlayer;
        static Skin currentSkin;
        static SoundPlayer player = new SoundPlayer();
        static bool music = true;
        static Hero hero;
        static Energizer[] energizers;
        static Ghost[] ghosts;
        static string buffer_map_for_optimisation;


        static void MainMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetBufferSize(49, 25);
            Console.SetWindowSize(49, 25);
            while (true)
            {
                Console.Clear();
                Console.SetCursorPosition(10, 3);
                Console.Write(" _ __ ___   ___  _ __  _   _ ");
                Console.SetCursorPosition(10, 4);
                Console.Write("| '_ ` _ \\ / _ \\| '_ \\| | | |");
                Console.SetCursorPosition(10, 5);
                Console.Write("| | | | | |  __/| | | | |_| |");
                Console.SetCursorPosition(10, 6);
                Console.Write("|_| |_| |_|\\___||_| |_|\\__,_|");

                Console.SetCursorPosition(24 - (int)(5 + currentPlayer.name.Length) / 2, 8);
                Console.Write("Hi, {0}!", currentPlayer.name);
                Console.SetCursorPosition(22, 10);
                Console.Write("START");
                Console.SetCursorPosition(21, 12);
                Console.Write("OPTIONS");
                Console.SetCursorPosition(20, 14);
                Console.Write("TERMINATE");
                Console.ForegroundColor = ConsoleColor.Yellow;
                int frames = 0;
                char[] animationR = new char[4] { '/', '|', '\\', '—' };
                char[] animationL = new char[4] { '\\', '|', '/', '—' };
                int animKey = 0;
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        if (frames == 9000)
                        {
                            Console.SetCursorPosition(20 - cursor, cursor * 2 + 10);
                            Console.Write(animationR[animKey]);
                            Console.SetCursorPosition(28 + cursor, cursor * 2 + 10);
                            Console.Write(animationL[animKey]);
                            animKey++;
                            animKey = animKey % animationR.Length;
                            frames = 0;
                        }
                        frames++;
                    }
                    Console.Beep();
                    if (ChooseMenu(Console.ReadKey(true).Key, 3, true))
                        GameCreation();
                } while (true);
            }
        }

        static bool ChooseMenu(ConsoleKey key, int rows, bool flag)
        {
            Console.SetCursorPosition(20 + rows % 3 / 2 - cursor, cursor * 2 + 13 - rows);
            Console.Write(" ");
            Console.SetCursorPosition(28 - rows % 3 / 2 + cursor, cursor * 2 + 13 - rows);
            Console.Write(" ");

            switch (key)
            {
                case ConsoleKey.W:
                    {
                        cursor--;
                        if (cursor == -1)
                            cursor = rows - 1;
                        break;
                    }
                case ConsoleKey.S:
                    {
                        cursor++;
                        cursor = cursor % rows;
                        break;
                    }
                case ConsoleKey.M:
                    {
                        music = !music;
                        player.SoundLocation = "music\\menu.wav";
                        if (music)
                            player.PlayLooping();
                        else
                            player.Stop();
                        break;
                    }
                case ConsoleKey.Enter:
                    {
                        if (flag)
                            switch (cursor)
                            {
                                case 0:
                                    {
                                        Console.Clear();
                                        return true;
                                    }
                                case 1:
                                    {
                                        OptionsMenu();
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.SetCursorPosition(0, 23);
                                        Console.Write("Bye-bye ^.^\n");
                                        Environment.Exit(0);
                                        break;
                                    }
                            }
                        else
                            switch (cursor)
                            {
                                case 0:
                                    {
                                        MapMenu();
                                        break;
                                    }
                                case 1:
                                    {
                                        SkinsMenu();
                                        break;
                                    }
                                case 2:
                                    {
                                        PlayersMenu(false);
                                        break;
                                    }
                                case 3:
                                    {
                                        cursor = 1;
                                        MainMenu();
                                        break;
                                    }
                                case 4:
                                    {
                                        Configuring(false);
                                        MainMenu();
                                        break;
                                    }
                            }
                        break;
                    }
            }
            return false;
        }

        static void MapMenu()
        {
            int x = 2;
            while (true)
            {
                string[] lines;
                while (true)
                {
                    Console.Clear();
                    int i = 1;
                    foreach (string file in Directory.GetFiles("map"))
                    {
                        if (file != "map\\Test.txt")
                        {
                            Console.SetCursorPosition(5, i * 2);
                            Console.Write("{0}: {1}\n\n", i, file.Split('\\')[1].Split('.')[0]);
                            i++;
                        }
                    }
                    Console.SetCursorPosition(5, 21);
                    Console.Write("Print a number of your next map.");
                    Console.SetCursorPosition(5, 22);
                    Console.Write("Print 0 for terminating.");
                    try
                    {
                        Console.SetCursorPosition(5, 23);
                        string userInput = Console.ReadLine();
                        x = Int32.Parse(userInput);
                        if (x == 0)
                            OptionsMenu();
                        if (Directory.GetFiles("map")[x - 1] == "map\\Test.txt")
                            throw new IndexOutOfRangeException();
                        lines = File.ReadAllLines(Directory.GetFiles("map")[x - 1], Encoding.UTF8);
                        break;
                    }
                    catch
                    {
                    }
                }
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.SetWindowSize(lines[0].Length + 1, lines.Length + 5);
                Console.SetBufferSize(lines[0].Length + 1, lines.Length + 5);
                foreach (string line in lines)
                {
                    foreach (char ch in line)
                        Console.Write(ch);
                    Console.WriteLine();
                }
                Console.WriteLine("\nChoose this map?\nY/N >>");
                while (true)
                {
                    ConsoleKey kk = Console.ReadKey(true).Key;
                    if (kk == ConsoleKey.Y)
                    {
                        currentPlayer.map = Directory.GetFiles("map")[x - 1];
                        currentPlayer.Save();
                        OptionsMenu();
                        break;
                    }
                    else if (kk == ConsoleKey.N)
                    {
                        Console.SetWindowSize(49, 25);
                        Console.SetBufferSize(49, 25); break; }
                }
            }
        }

        static void PlayersMenu(bool flag)
        {
            int x = 2;
            while (true)
            {
                Player buffer = new Player();
                while (true)
                {
                    Console.Clear();
                    int i = 1;
                    foreach (string file in Directory.GetFiles("players"))
                    {
                        Console.SetCursorPosition(5, i * 2);
                        Console.Write("{0}: {1}\n\n", i, file.Split('\\')[1].Split('.')[0]);
                        i++;
                    }
                    Console.SetCursorPosition(5, 21);
                    Console.Write("Print a number of your save.");
                    Console.SetCursorPosition(5, 22);
                    Console.Write("Print 0 for terminating.");
                    Console.SetCursorPosition(5, 23);
                    Console.Write("Print -1 to create new player.");
                    try
                    {
                        if (x != -1 && x != 0)
                        {
                            Console.SetCursorPosition(5, 24);
                            string userInput = Console.ReadLine();
                            x = Int32.Parse(userInput);
                            buffer = JsonConvert.DeserializeObject<Player>(File.ReadAllText(Directory.GetFiles("players")[x - 1]));
                            break;
                        }
                        else 
                            break;
                    }
                    catch
                    {
                    }
                }
                if (x == 0)
                    OptionsMenu();
                if (x == -1)
                {
                    string buffer_name;
                    while (true)
                    {
                        bool fflag = true;
                        Console.Clear();
                        Console.SetCursorPosition(15, 10);
                        Console.Write("Enter player's name:");
                        Console.SetCursorPosition(15, 11);
                        Console.Write(">> ");
                        buffer_name = Console.ReadLine();
                        string[] files = Directory.GetFiles("players");
                        foreach(string str in files)
                        {
                            if (buffer_name == str.Split('\\')[1].Split('.')[0])
                                fflag = false;
                        }
                        if (fflag)
                            break;
                        else
                        {
                            Console.SetCursorPosition(2, 10);
                            Console.Write("A player with the same name already exists.");
                            Console.SetCursorPosition(10, 11);
                            Console.Write("Press any button to continue...");
                            Console.ReadKey();
                        }
                    }
                    Player temp_player = new Player();
                    temp_player.name = buffer_name;
                    temp_player.Reset();
                    temp_player.Save();
                    currentPlayer = temp_player;
                    currentSkin = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(currentPlayer.skin));
                    Console.Clear();
                    Console.SetCursorPosition(2, 10);
                    Console.Write("Done! You can change map and skin in options.");
                    Console.SetCursorPosition(10, 11);
                    Console.Write("Press any button to continue...");
                    Console.ReadKey();
                    Configuring(true);
                    MainMenu();
                    break;
                }
                else
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.SetCursorPosition(5, 4);
                    Console.Write("Name - {0}\n", buffer.name);
                    Console.SetCursorPosition(5, 6);
                    Console.Write("Map - {0}\n", buffer.map.Split('\\')[1].Split('.')[0]);
                    Console.SetCursorPosition(5, 8);
                    Console.Write("Skin - {0}\n", buffer.skin.Split('\\')[1].Split('.')[0]);
                    Console.SetCursorPosition(5, 15);
                    Console.WriteLine("Choose this save?");
                    Console.SetCursorPosition(5, 16);
                    Console.Write("Y/N >>");
                    while (true)
                    {
                        ConsoleKey kk = Console.ReadKey(true).Key;
                        if (kk == ConsoleKey.Y)
                        {
                            currentPlayer = JsonConvert.DeserializeObject<Player>(File.ReadAllText(Directory.GetFiles("players")[x - 1]));
                            currentSkin = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(currentPlayer.skin));
                            if (currentPlayer.isNew)
                                Configuring(true);
                            if (flag)
                            {
                                MainMenu();
                            }
                            else
                                OptionsMenu();
                            break;
                        }
                        else if (kk == ConsoleKey.N)
                        { break; }
                    }
                }
            }
        }

        static void SkinsMenu()
        {
            int x = 2;
            while (true)
            {
                Skin buffer;
                while (true)
                {
                    Console.Clear();
                    int i = 1;
                    foreach (string file in Directory.GetFiles("skins"))
                    {
                        Console.SetCursorPosition(5, i * 2);
                        Console.Write("{0}: {1}\n\n", i, file.Split('\\')[1].Split('.')[0]);
                        i++;
                    }
                    Console.SetCursorPosition(5, 21);
                    Console.Write("Print a number of your skin.");
                    Console.SetCursorPosition(5, 22);
                    Console.Write("Print 0 for terminating.");
                    try
                    {
                        Console.SetCursorPosition(5, 23);
                        string userInput = Console.ReadLine();
                        x = Int32.Parse(userInput);
                        if (x == 0)
                            OptionsMenu();
                        buffer = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(Directory.GetFiles("skins")[x - 1]));
                        break;
                    }
                    catch
                    {
                    }
                }
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.SetCursorPosition(5, 4);
                Console.Write("{0} - Hero\n", buffer.Hero_normal);
                Console.SetCursorPosition(5, 6);
                Console.Write("{0} - Hero (Energized)\n", buffer.Hero_energizer);
                Console.SetCursorPosition(5, 8);
                Console.Write("{0} - Ghost\n", buffer.Ghost_normal);
                Console.SetCursorPosition(5, 10);
                Console.Write("{0} - Ghost (Energized)\n", buffer.Ghost_energizer);
                Console.SetCursorPosition(5, 12);
                Console.Write("{0} - Energized\n", buffer.Energizer);
                Console.SetCursorPosition(5, 14);
                Console.Write("{0} - Coin/Food/Whatever\n", buffer.Food);
                Console.SetCursorPosition(5, 16);
                Console.Write("{0} - Blank(Yes, it's a blank cell)\n", " ");
                Console.SetCursorPosition(5, 18);
                Console.Write("{0} - Wall\n", buffer.Wall);
                Console.SetCursorPosition(5, 20);
                Console.WriteLine("Choose this save?");
                Console.SetCursorPosition(5, 21);
                Console.Write("Y/N >>");
                ConsoleKey kk = Console.ReadKey(true).Key;
                if (kk == ConsoleKey.Y)
                {
                    currentPlayer.skin = Directory.GetFiles("skins")[x - 1];
                    currentSkin = buffer;
                    currentPlayer.Save();
                    OptionsMenu();
                }
                else if (kk == ConsoleKey.N)
                { }
            }
        }

        static void OptionsMenu()
        {
            Console.SetWindowSize(49, 25);
            Console.SetBufferSize(49, 25);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.SetCursorPosition(7, 0);
            Console.Write("             _   _                 ");
            Console.SetCursorPosition(7, 1);
            Console.Write("            | | (_)                ");
            Console.SetCursorPosition(7, 2);
            Console.Write("  ___  _ __ | |_ _  ___  _ __  ___ ");
            Console.SetCursorPosition(7, 3);
            Console.Write(" / _ \\| '_ \\| __| |/ _ \\| '_ \\/ __|");
            Console.SetCursorPosition(7, 4);
            Console.Write("| (_) | |_) | |_| | (_) | | | \\__ \\");
            Console.SetCursorPosition(7, 5);
            Console.Write(" \\___/| .__/ \\__|_|\\___/|_| |_|___/");
            Console.SetCursorPosition(7, 6);
            Console.Write("      |_|   ");

            Console.SetCursorPosition(23, 8);
            Console.Write("MAP");
            Console.SetCursorPosition(22, 10);
            Console.Write("SKINS");
            Console.SetCursorPosition(21, 12);
            Console.Write("PLAYERS");
            Console.SetCursorPosition(20, 14);
            Console.Write("TERMINATE");
            Console.SetCursorPosition(19, 16);
            Console.Write("CONFIGURING");
            Console.ForegroundColor = ConsoleColor.Yellow;
            int frames = 0;
            char[] animationR = new char[4] { '/', '|', '\\', '—' };
            char[] animationL = new char[4] { '\\', '|', '/', '—' };
            int animKey = 0;
            do
            {
                while (!Console.KeyAvailable)
                {
                    if (frames == 9000)
                    {
                        Console.SetCursorPosition(21 - cursor, cursor * 2 + 8);
                        Console.Write(animationR[animKey]);
                        Console.SetCursorPosition(27 + cursor, cursor * 2 + 8);
                        Console.Write(animationL[animKey]);
                        animKey++;
                        animKey = animKey % animationR.Length;
                        frames = 0;
                    }
                    frames++;
                }
                Console.Beep();
                if (ChooseMenu(Console.ReadKey(true).Key, 5, false))
                    return;
            } while (true);
        }

        static void GameCreation()
        {
            Console.Clear();
            Skin symbols = currentSkin;
            player.SoundLocation = currentSkin.Music;
            if (music)
                player.PlayLooping();
            string map = currentPlayer.map;
            int deltaGhost = 7500; 
            string[] lines = File.ReadAllLines(currentPlayer.map, Encoding.UTF8);
            int energizers_number = 0;
            int ghosts_number = 0;
            int heroes = 0;
            foreach (string str in lines)
                foreach (char ch in str)
                {
                    if (ch == ' ')
                        Field.maxFood++;
                    else if (ch == 'E')
                        energizers_number++;
                    else if (ch == 'G')
                        ghosts_number++;
                    if (ch == 'H')
                        heroes++;
                }

            if(heroes > 1)
            {
                Console.WriteLine("There is more that 1 'H'ero in the map.");
                Environment.Exit(0);
            }
            energizers = new Energizer[energizers_number];
            ghosts = new Ghost[ghosts_number];

            Field.width = lines[0].Length;
            Field.heigh = lines.Length;
            field.CreateField();
            Console.SetWindowSize(Field.width + 1, Field.heigh + 2);
            Console.SetBufferSize(Field.width + 1, Field.heigh + 2);
            for (int i = 0; i < Field.heigh; i++)
            {
                if (lines[i].Length == Field.width)
                {
                    for (int j = 0; j < Field.width; j++)
                    {
                        if (lines[i][j] == '@')
                        {
                            field[j, i] = new Wall(Types.wall, symbols.Wall, symbols.cw);
                            field[j, i].ShowMe(j, i);
                        }
                        else if (lines[i][j] == ' ')
                        {
                            
                            field[j, i] = new Food(Types.food, symbols.Food, symbols.cf);
                            field[j, i].ShowMe(j, i);
                        }
                        else if (lines[i][j] == 'E')
                        {
                            field[j, i] = new Energizer(Types.energizer, symbols.Energizer, symbols.ce, j, i);
                            energizers_number--;
                            energizers[energizers_number] = (Energizer)field[j, i];
                            energizers[energizers_number].ShowMyself();
                        }
                        else if (lines[i][j] == 'G')
                        {
                            field[j, i] = new Ghost(Types.ghost, symbols.Ghost_normal, symbols.Ghost_energizer,
                                symbols.cgn, symbols.cge,j, i);
                            ghosts_number--;
                            ghosts[ghosts_number] = (Ghost)field[j, i];
                            ghosts[ghosts_number].ShowMyself();
                        }
                        else if (lines[i][j] == 'H')
                        {
                            field[j, i] = new Hero(Types.hero, symbols.Hero_normal, symbols.Hero_energizer,
                                symbols.chn, symbols.che, j, i);
                            hero = (Hero)field[j, i];
                            hero.ShowMyself();
                        }
                        else if (lines[i][j] == 'b')
                        {
                            field[j, i] = new Blank(Types.blank, ' ', ConsoleColor.Black);
                            field[j, i].ShowMe(j, i);
                        }
                        else
                        {
                            field[j, i] = new Wall(Types.wall, lines[i][j], symbols.cw);
                            field[j, i].ShowMe(j, i);
                        }
                        if (lines[i][j] != '@')
                        {
                            List<int> roads = new List<int>();
                            if (lines[i - 1][j] != '@')
                                roads.Add(0);
                            if (lines[i][j - 1] != '@')
                                roads.Add(1);
                            if (lines[i + 1][j] != '@')
                                roads.Add(2);
                            if (lines[i][j + 1] != '@')
                                roads.Add(3);
                            if (roads.Count >= 3)
                            {
                                if (lines[i][j] == 'G')
                                {
                                    Ghost ghost = (Ghost)field[j, i];
                                    ghost.undercell.isCrossroad = true;
                                    ghost.undercell.dirs = roads;
                                }
                                else
                                {
                                    field[j, i].isCrossroad = true;
                                    field[j, i].dirs = roads;
                                    field[j, i].ShowMe(j, i);
                                }
                            }
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

            Console.SetCursorPosition(1, Field.heigh + 1);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Score: ");
            Console.SetCursorPosition(12, Field.heigh + 1);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Booster");
            Console.SetCursorPosition(1, Field.heigh);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("FPS: ");
            Field.PrintScore();
            Action();
        }

        static void AI_n_Timers()
        {
            hero.EnergizerTimer(ghosts);
            for (int i = 0; i < ghosts.Length; i++)
            {
                if (KITTY)
                {
                    ghosts[i].GhostsTimer(field);
                    ghosts[i].ThinkGhost(hero, field);
                }
            }
        }

        static void ReadKey(ConsoleKey playerKey)
        {
            switch (playerKey)
            {
                case ConsoleKey.W:
                    {
                        hero.MoveHero(0 + 2* Convert.ToInt32(hero.blackout), ghosts, energizers, field);
                        hero.key = 0 + 2 * Convert.ToInt32(hero.blackout);
                        break;
                    }
                case ConsoleKey.A:
                    {
                        hero.MoveHero(1 + 2 * Convert.ToInt32(hero.blackout), ghosts, energizers, field);
                        hero.key = 1 + 2 * Convert.ToInt32(hero.blackout);
                        break;
                    }
                case ConsoleKey.S:
                    {
                        hero.MoveHero(2 - 2 * Convert.ToInt32(hero.blackout), ghosts, energizers, field);
                        hero.key = 2 - 2 * Convert.ToInt32(hero.blackout);
                        break;
                    }
                case ConsoleKey.D:
                    {
                        hero.MoveHero(3 - 2 * Convert.ToInt32(hero.blackout), ghosts, energizers, field);
                        hero.key = 3 - 2 * Convert.ToInt32(hero.blackout);
                        break;
                    }
                case ConsoleKey.M:
                    {
                        music = !music;
                        player.SoundLocation = currentSkin.Music;
                        if (music)
                            player.PlayLooping();
                        else
                            player.Stop();
                        break;
                    }
                case ConsoleKey.Spacebar:
                    {
                        if(hero.saver && !hero.mod)
                        {
                            hero.saver = false;
                            hero.sw.Start();
                            hero.ChangeMod(true);
                            for (int j = 0; j < ghosts.Length; j++)
                            {
                                ghosts[j].ChangeMod(true);
                            }
                            Console.SetCursorPosition(12, Field.heigh + 1);
                            Console.Write("       ");
                            hero.saver_watch.Start();
                        }
                        break;
                    }
            }
        }

        static public void GameOver()
        {
            Console.SetWindowSize(50, 10);
            Console.SetBufferSize(50, 10);
            player.SoundLocation = "music\\nggyu.wav";
            if (music)
                player.PlayLooping();
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Ghosts never gonna give you up!");
            Console.WriteLine("Ghosts never gonna let you down!");
            Console.WriteLine("Ghosts never gonna run around and desert you!");
            Console.WriteLine("Ghosts never gonna make you cry!");
            Console.WriteLine("Ghosts never gonna say goodbye!");
            Console.WriteLine("Ghosts never gonna tell a lie and hurt you!");
            Console.ReadKey(true);
            Console.ReadKey(true);
            KITTY = false;
        }

        static public void YouWon()
        {
            if (currentPlayer.map != "map\\Test.txt")
            {
                player.SoundLocation = "music\\nyan.wav";
                if (music)
                    player.PlayLooping();
                KITTY = false;
                Console.Clear();
                Console.SetWindowSize(80, 25);
                Console.SetBufferSize(80, 25);
                Console.ForegroundColor = ConsoleColor.Gray;
                for (int i = 0; i < 3; i++)
                    Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                Console.WriteLine("::::::::::::::::##############                              :::::::::::::::::::");
                Console.WriteLine("############################  ##############################  :::::::::::::::::");
                Console.WriteLine("#########################  ######???????????????????????######  :::::::::::::::");
                Console.WriteLine("=========================  ####??????????()????()?????????####  :::::::::::::::");
                Console.WriteLine("=========================  ##????()??????????????    ()?????##  ::::    :::::::");
                Console.WriteLine("------------=============  ##??????????????????  ;;;;  ?????##  ::  ;;;;  :::::");
                Console.WriteLine("-------------------------  ##??????????()??????  ;;;;;;?????##    ;;;;;;  :::::");
                Console.WriteLine("-------------------------  ##??????????????????  ;;;;;;         ;;;;;;;;  :::::");
                Console.WriteLine("++++++++++++-------------  ##??????????????????  ;;;;;;;;;;;;;;;;;;;;;;;  :::::");
                Console.WriteLine("+++++++++++++++++++++++++  ##????????????()??  ;;;;;;;;;;;;;;;;;;;;;;;;;;;  :::");
                Console.WriteLine("+++++++++++++++++    ;;;;  ##??()????????????  ;;;;;;@@  ;;;;;;;;@@  ;;;;;  :::");
                Console.WriteLine("~~~~~~~~~~~~~++++;;;;;;;;  ##????????????????  ;;;;;;    ;;;  ;;;    ;;;;;  :::");
                Console.WriteLine("~~~~~~~~~~~~~~~  ;;  ~~~~  ###???????()??????  ;;[];;;;;;;;;;;;;;;;;;;;;[]  :::");
                Console.WriteLine("~~~~~~~~~~~~~~~  ;;  ~~~~  ####??????()??????  ;;;;;;;  ;;;;;;;;;;  ;;;;;   :::");
                Console.WriteLine("$$$$$$$$$$$$$~~~~  ~~~~~~  ######?????????????  ;;;;;;              ;;;;  :::::");
                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$$$    ###################  ;;;;;;;;;;;;;;;;;;;;  :::::::");
                Console.WriteLine("$$$$$$$$$$$$$$$$$$$$$$$  ;;;;                                       :::::::::::");
                Console.WriteLine(":::::::::::::$$$$$$$$$$  ;;;;  ::  ;;  ::::::::::::  ;;  ::  ;;;;  ::::::::::::");
                Console.WriteLine(":::::::::::::::::::::::      ::::::    :::::::::::::     ::::      ::::::::::::");
                Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
                Console.WriteLine("\nYou have won! This is your reward, so don't be shy to look at this cute cat ^.^");
                Console.WriteLine("Your earned {0} points!", Hero.score);
            }
            else
            {
                player.SoundLocation = "music\\menu.wav";
                if (music)
                    player.PlayLooping();
                currentPlayer.map = buffer_map_for_optimisation;
                currentPlayer.isNew = false;
                currentPlayer.Save();
                cursor = 1;
                MainMenu();
            }
        }

        static public void Configuring(bool flag)
        {
            buffer_map_for_optimisation = currentPlayer.map;
            if (flag)
            {
                Console.Clear();
                Console.SetCursorPosition(5, 3);
                Console.Write("Your save is new.");
                Console.SetCursorPosition(5, 4);
                Console.Write("It's recommend to configure it.");
                Console.SetCursorPosition(5, 5);
                Console.Write("Don't you mind?");
                Console.SetCursorPosition(5, 6);
                Console.Write("Y / N >> ");
                Console.SetCursorPosition(5, 7);
                Console.Write("(Just collect all blue circles.)");
                Console.SetCursorPosition(5, 7);
                Console.Write("(After, you will find yourself in menu.)");
                while (true)
                {
                    ConsoleKey buffer = Console.ReadKey(true).Key;
                    if (buffer == ConsoleKey.Y)
                    {
                        currentPlayer.map = "map\\Test.txt";
                        GameCreation();
                    }
                    else if (buffer == ConsoleKey.N)
                    {
                        break;
                    }
                }
            }
            else
            {
                currentPlayer.map = "map\\Test.txt";
                GameCreation();
            }
        }

        static void Action()
        {
            int frames = 0;
            int deltaGhost = currentPlayer.deltaGhost;
            Stopwatch clock = new Stopwatch();
            clock.Start();
            bool flag = false;
            ConsoleKey playerKey = ConsoleKey.Execute;
            do
            {
                while (!Console.KeyAvailable && KITTY)
                {
                    if ((int)(frames % (deltaGhost * Ghost.speedModifier)) == 0)
                    {
                        DeltaTime.Update();
                        Console.SetCursorPosition(6, Field.heigh);
                        Console.ForegroundColor = ConsoleColor.Green;
                        int FPS = DeltaTime.getFPS();
                        Console.Write(FPS + " ");
                        if (currentPlayer.map == "map\\Test.txt")
                        {
                            if (clock.ElapsedMilliseconds > 1250)
                            {
                                flag = true;
                                clock.Stop();
                            }
                            if (flag)
                            {
                                if (FPS > 6)
                                    currentPlayer.deltaGhost += 200;
                                else if (FPS < 6 && currentPlayer.deltaGhost > 200)
                                    currentPlayer.deltaGhost -= 200;
                                Console.SetCursorPosition(9, Field.heigh);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(deltaGhost + "      ");
                            }
                        }
                        AI_n_Timers();
                    }
                    else if ((int)(frames % (deltaGhost * Hero.speedModifier)) == 0 && playerKey != ConsoleKey.Execute)
                    {
                        ReadKey(playerKey);
                        if (playerKey == ConsoleKey.Home)
                            YouWon();
                        playerKey = ConsoleKey.Execute;
                    }
                    else if (frames == 100000)
                        frames = 0;
                    if (deltaGhost != currentPlayer.deltaGhost && frames % (deltaGhost * 1.06) == 0)
                            deltaGhost = currentPlayer.deltaGhost;
                    frames++;
                }
                playerKey = Console.ReadKey(true).Key;
            } while (KITTY);
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.SetWindowSize(49, 25);
            Console.SetBufferSize(49, 25);
            Console.ForegroundColor = ConsoleColor.White;
            player.SoundLocation = "music\\menu.wav";
            if (music)
                player.PlayLooping();
            buffer_map_for_optimisation = "map\\Alpha.txt";
            PlayersMenu(true);
            MainMenu();
        }
    }
}
