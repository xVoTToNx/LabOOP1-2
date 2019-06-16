using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Media;
using Newtonsoft.Json;
using System.Threading;

using GameSpace;

namespace ConsoleView
{
    public struct ColorSymbol
    {
        ConsoleColor color;
        char symbol;

        public ConsoleColor Color { get { return color; } }

        public char Symbol { get { return symbol; } }

        public ColorSymbol(ConsoleColor c, char ch)
        {
            color = c;
            symbol = ch;
        }
    }

    static class Data
    {
        static public Dictionary<Types, ColorSymbol> dictionary;

        static public void ConsoleRender(int X, int Y, Types type)
        {
            Console.SetCursorPosition(X, Y);
            Console.ForegroundColor = dictionary[type].Color;
            Console.Write(dictionary[type].Symbol);
        }
    }

    public class GameConsole : Game
    {
         int cursor = 1;
         int animKey = 0;
         string bufferMapForOptimisation;
         char[] animationR = new char[4] { '/', '|', '\\', '—' };
         char[] animationL = new char[4] { '\\', '|', '/', '—' };
        const int normalHeroTime = 150;
        const int normalGhostTime = 200;

         void animation(int leftPart = 0, int rightPart = 0, MenuTypes menu = MenuTypes.otherMenu)  // Works until user's input
        {
            int frames = 0;
            while (!Console.KeyAvailable)
            {
                if (frames == 6000)
                {
                    int yCoefficient = 1;
                    if (menu == MenuTypes.mainMenu)
                    {
                        leftPart = 20 - cursor;
                        rightPart = 28 + cursor;
                        yCoefficient = 5;
                    }
                    else if (menu == MenuTypes.optionMenu)
                    {
                        leftPart = 21 - cursor;
                        rightPart = 27 + cursor;
                        yCoefficient = 4;
                    }

                    Console.SetCursorPosition(leftPart, cursor * 2 + yCoefficient * 2);
                    Console.Write(animationR[animKey]);
                    Console.SetCursorPosition(rightPart, cursor * 2 + yCoefficient * 2);
                    Console.Write(animationL[animKey]);
                    animKey++;
                    animKey = animKey % animationR.Length;


                    frames = 0;
                }
                frames++;
            }
        }

        void mainMenu()
        {
            cursor = 0;
            while (true)
            {
                writeMainMenu();
                do
                {
                    animation(menu: MenuTypes.mainMenu);

                    Console.Beep();
                    if (chooseMainMenu(Console.ReadKey(true).Key, 3))
                    {
                        gameCreation();
                    }
                    break;
                } while (true);
            }
        }

         void writeMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetWindowSize(49, 25);
            Console.SetBufferSize(49, 25);
            Console.SetCursorPosition(10, 3);
            Console.Write(" _ __ ___   ___  _ __  _   _ ");
            Console.SetCursorPosition(10, 4);
            Console.Write("| '_ ` _ \\ / _ \\| '_ \\| | | |");
            Console.SetCursorPosition(10, 5);
            Console.Write("| | | | | |  __/| | | | |_| |");
            Console.SetCursorPosition(10, 6);
            Console.Write("|_| |_| |_|\\___||_| |_|\\__,_|");

            Console.SetCursorPosition(24 - (int)(5 + currentPlayer.Name.Length) / 2, 8);
            Console.Write("Hi, {0}!", currentPlayer.Name);
            Console.SetCursorPosition(22, 10);
            Console.Write("START");
            Console.SetCursorPosition(21, 12);
            Console.Write("OPTIONS");
            Console.SetCursorPosition(20, 14);
            Console.Write("TERMINATE");

            Console.ForegroundColor = ConsoleColor.Yellow;
        }

         bool chooseMainMenu(ConsoleKey key, int rows)
        {
            if (chooseMenu(key, rows))
                switch (cursor)
                {
                    case 0:
                        Console.Clear();
                        return true;
                    case 1:
                        OptionsMenu();
                        break;
                    case 2:
                        Console.SetCursorPosition(0, 23);
                        Console.Write("Bye-bye ^.^\n");
                        Environment.Exit(0);
                        break;
                }
            return false;
        }

         bool chooseOptionMenu(ConsoleKey key, int rows)
        {
            if (chooseMenu(key, rows))
                switch (cursor)
                {
                    case 0:
                        mapMenu();
                        cursor = 0;
                        break;
                    case 1:
                        SkinsMenu();
                        cursor = 0;
                        break;
                    case 2:
                        playersMenu(false);
                        cursor = 0;
                        break;
                    case 3:
                        cursor = 1;
                        return true;
                    case 4:
                        Configuring(false);
                        cursor = 0;
                        mainMenu();
                        break;
                }
            return false;
        }

         bool chooseMenu(ConsoleKey key, int rows, int leftPart = 0, int rightPart = 0, MenuTypes menu = MenuTypes.otherMenu)
        {
            int yCoefficient = 1;
            if (menu == MenuTypes.mainMenu)
            {
                leftPart = 20 - cursor;
                rightPart = 28 + cursor;
                yCoefficient = 5;
            }
            else if (menu == MenuTypes.optionMenu)
            {
                leftPart = 21 - cursor;
                rightPart = 27 + cursor;
                yCoefficient = 4;
            }

            Console.SetCursorPosition(leftPart, cursor * 2 + yCoefficient * 2);
            Console.Write(" ");
            Console.SetCursorPosition(rightPart, cursor * 2 + yCoefficient * 2);
            Console.Write(" ");


            if (key == ConsoleKey.W || key == ConsoleKey.UpArrow)
            {
                cursor--;
                if (cursor == -1)
                    cursor = rows - 1;
            }
            else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
            {
                cursor++;
                cursor = cursor % rows;
            }
            else if (key == ConsoleKey.M)
            {
                music = !music;
                player.SoundLocation = "music\\menu.wav";
                if (music)
                    player.PlayLooping();
                else
                    player.Stop();
            }
            else if (key == ConsoleKey.Enter)
            {
                return true;
            }
            return false;
        }

         void mapMenu()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;

                string[] lines;
                int[][] positionOfAnimation = new int[Directory.GetFiles("map").Length + 1][];
                cursor = 0;

                do
                {
                    writeOtherMenu(ref positionOfAnimation, "map");

                    int leftPositon = positionOfAnimation[cursor][(int)Positions.left];
                    int rightPosition = positionOfAnimation[cursor][(int)Positions.right];

                    animation(leftPositon, rightPosition);
                    Console.Beep();

                    if (chooseMenu(Console.ReadKey(true).Key, positionOfAnimation.Length,
                        leftPositon, rightPosition))
                    {
                        if (cursor == positionOfAnimation.Length - 1)
                            return;

                        lines = File.ReadAllLines(Directory.GetFiles("map")[cursor]);
                        writeMapMenu(lines);

                        while (true)
                        {
                            ConsoleKey kk = Console.ReadKey(true).Key;
                            if (kk == ConsoleKey.Y)
                            {
                                currentPlayer.Map = Directory.GetFiles("map")[cursor];
                                currentPlayer.Save();
                                return;
                            }
                            else if (kk == ConsoleKey.N)
                            {
                                Console.SetWindowSize(49, 25);
                                Console.SetBufferSize(49, 25); break;
                            }
                        }
                    }

                } while (true);
            }
        }

         void writeMapMenu(string[] lines)
        {
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
        }

         void writeOtherMenu(ref int[][] positionOfAnimation, string directory, bool isPlayerMenu = false)
        {
            Console.Clear();
            int i = 0;
            foreach (string file in Directory.GetFiles(directory))
            {
                string name;
                if (directory == "players")
                {
                    name = makeNameNormal(file.Split('\\')[1].Split('.')[0]);
                }
                else
                {
                    name = file.Split('\\')[1].Split('.')[0];
                }
                int nameLength = name.Length;
                int nameStartPosition = 23 - nameLength / 2;
                int nameEndPosition = 26 - nameLength / 2 + nameLength;
                positionOfAnimation[i] = new int[2] { nameStartPosition, nameEndPosition };

                Console.SetCursorPosition(25 - nameLength / 2, (i + 1) * 2);
                Console.Write(name);
                i++;
            }

            if (isPlayerMenu)
            {
                positionOfAnimation[i] = new int[2] { 14, 36 };
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(16, (i + 1) * 2);
                Console.Write("<Create New Player>");
                i++;
            }
            positionOfAnimation[i] = new int[2] { 18, 32 };
            Console.SetCursorPosition(20, (i + 1) * 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("<Terminate>");
            Console.ForegroundColor = ConsoleColor.White;
        }

         string makeNameNormal(string name)
        {
            name = name.Replace("{lt}", "<");

            name = name.Replace("{gt}", ">");

            name = name.Replace("{cl}", ":");

            name = name.Replace("{dq}", "\"");

            name = name.Replace("{fs}", "/");

            name = name.Replace("{bs}", "\\");

            name = name.Replace("{vb}", "|");

            name = name.Replace("{qm}", "?");

            name = name.Replace("{ar}", "*");

            return name;
        }

         void playersMenu(bool flag)
        {
            while (true)
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    int[][] positionOfAnimation = new int[Directory.GetFiles("players").Length + 2][];
                    cursor = 0;

                    do
                    {
                        writeOtherMenu(ref positionOfAnimation, "players", isPlayerMenu: true);

                        int leftPositon = positionOfAnimation[cursor][(int)Positions.left];
                        int rightPosition = positionOfAnimation[cursor][(int)Positions.right];

                        animation(leftPositon, rightPosition);

                        Console.Beep();

                        if (chooseMenu(Console.ReadKey(true).Key, positionOfAnimation.Length,
                            leftPositon, rightPosition))
                        {
                            Console.Clear();
                            if (cursor == positionOfAnimation.Length - 1)
                            {
                                if (currentPlayer.Map == null)
                                    throw new IndexOutOfRangeException();
                                return;
                            }
                            if (cursor == positionOfAnimation.Length - 2)
                            {
                                creatingNewPlayer();
                            }
                            else
                            {
                                writePlayerMenu();
                                while (true)
                                {
                                    ConsoleKey kk = Console.ReadKey(true).Key;
                                    if (kk == ConsoleKey.Y)
                                    {
                                        currentPlayer = JsonConvert.DeserializeObject<Player>(File.ReadAllText(Directory.GetFiles("players")[cursor]));
                                        currentSkin = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(currentPlayer.Skin));
                                        if (currentPlayer.IsNew)
                                            Configuring(true);
                                        if (flag)
                                        {
                                            mainMenu();
                                        }
                                        else
                                            return;
                                        break;
                                    }
                                    else if (kk == ConsoleKey.N)
                                    { break; }
                                }
                            }
                        }

                    } while (true);
                }
            }
        }

         void writePlayerMenu()
        {
            Player buffer = JsonConvert.DeserializeObject<Player>(File.ReadAllText(Directory.GetFiles("players")[cursor]));
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.SetCursorPosition(5, 4);
            Console.Write("Name - {0}\n", buffer.Name);
            Console.SetCursorPosition(5, 6);
            Console.Write("Map - {0}\n", buffer.Map.Split('\\')[1].Split('.')[0]);
            Console.SetCursorPosition(5, 8);
            Console.Write("Skin - {0}\n", buffer.Skin.Split('\\')[1].Split('.')[0]);
            Console.SetCursorPosition(5, 15);
            Console.WriteLine("Choose this save?");
            Console.SetCursorPosition(5, 16);
            Console.Write("Y/N >>");
        }

         void creatingNewPlayer()
        {
            string bufferName;
            Player tempPlayer = new Player();
            while (true)
            {
                bool fflag = true;
                Console.Clear();
                Console.SetCursorPosition(15, 10);
                Console.Write("Enter player's name:");
                Console.SetCursorPosition(15, 11);
                Console.Write(">> ");
                bufferName = Console.ReadLine();
                string[] files = Directory.GetFiles("players");
                foreach (string str in files)
                {
                    if (bufferName == str.Split('\\')[1].Split('.')[0])
                        fflag = false;
                }
                if (fflag)
                {
                    try
                    {
                        tempPlayer.Name = bufferName;
                        tempPlayer.Reset();
                        tempPlayer.Save();
                        break;
                    }
                    catch
                    {
                        Console.SetCursorPosition(15, 10);
                        Console.Write("   Invalid name.           ");
                        Console.SetCursorPosition(10, 11);
                        Console.Write("Press any button to continue...");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.SetCursorPosition(2, 10);
                    Console.Write("A player with the same name already exists.");
                    Console.SetCursorPosition(10, 11);
                    Console.Write("Press any button to continue...");
                    Console.ReadKey();
                }
            }
            currentPlayer = tempPlayer;
            currentSkin = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(currentPlayer.Skin));
            Console.Clear();
            Console.SetCursorPosition(2, 10);
            Console.Write("Done! You can change map and skin in options.");
            Console.SetCursorPosition(10, 11);
            Console.Write("Press any button to continue...");
            Console.ReadKey();
            Configuring(true);
            mainMenu();
        }

         void SkinsMenu()
        {
            while (true)
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    int[][] positionOfAnimation = new int[Directory.GetFiles("skins").Length + 1][];
                    cursor = 0;
                    Skin buffer = new Skin();
                    do
                    {
                        writeOtherMenu(ref positionOfAnimation, "skins");

                        int leftPositon = positionOfAnimation[cursor][(int)Positions.left];
                        int rightPosition = positionOfAnimation[cursor][(int)Positions.right];

                        animation(leftPositon, rightPosition);
                        Console.Beep();

                        if (chooseMenu(Console.ReadKey(true).Key, positionOfAnimation.Length,
                            leftPositon, rightPosition))
                        {
                            if (cursor == positionOfAnimation.Length - 1)
                                return;

                            writeSkinMenu(ref buffer);
                            ConsoleKey kk = Console.ReadKey(true).Key;
                            if (kk == ConsoleKey.Y)
                            {
                                currentPlayer.Skin = Directory.GetFiles("skins")[cursor];
                                currentSkin = buffer;
                                currentPlayer.Save();
                                return;
                            }
                            else if (kk == ConsoleKey.N)
                            {
                            }
                        }
                    } while (true);
                }

            }
        }

         void writeSkinMenu(ref Skin buffer)
        {
            buffer = JsonConvert.DeserializeObject<Skin>(File.ReadAllText(Directory.GetFiles("skins")[cursor]));
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.SetCursorPosition(5, 4);
            Console.Write("{0} - Hero\n", buffer.HeroNormal);
            Console.SetCursorPosition(5, 6);
            Console.Write("{0} - Hero (Energized)\n", buffer.HeroEnergizer);
            Console.SetCursorPosition(5, 8);
            Console.Write("{0} - Ghost\n", buffer.GhostNormal);
            Console.SetCursorPosition(5, 10);
            Console.Write("{0} - Ghost (Energized)\n", buffer.GhostEnergizer);
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
        }

         void OptionsMenu()
        {
            cursor = 0;
            while (true)
            {
                writeOptionMenu();
                do
                {
                    animation(menu: MenuTypes.optionMenu);
                    Console.Beep();
                    if (chooseOptionMenu(Console.ReadKey(true).Key, 5))
                        return;
                    else
                        break;
                } while (true);
            }
        }

         void writeOptionMenu()
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
        }

         void gameCreation()
        {
            isGameRunning = true;
            Console.Clear();
            player.SoundLocation = currentSkin.Music;
            if (music)
                player.PlayLooping();
            string map = currentPlayer.Map;
            string[] lines = File.ReadAllLines(currentPlayer.Map, Encoding.UTF8);
            int energizersNumber = 0;
            int ghostsNumber = 0;
            int heroes = 0;
            foreach (string str in lines)
                foreach (char ch in str)
                {
                    if (ch == ' ')
                        Field.MaxFood++;
                    else if (ch == 'E')
                        energizersNumber++;
                    else if (ch == 'G')
                        ghostsNumber++;
                    if (ch == 'H')
                        heroes++;
                }

            if (heroes > 1)
            {
                Console.WriteLine("There is more that 1 'H'ero in the map.");
                Environment.Exit(0);
            }
            energizers = new Energizer[energizersNumber];
            ghosts = new Ghost[ghostsNumber];

            Field.Width = lines[0].Length;
            Field.Height = lines.Length;
            Field.CreateField();
            Console.SetWindowSize(Field.Width + 1, Field.Height + 2);
            Console.SetBufferSize(Field.Width + 1, Field.Height + 2);


            makingField(lines, currentSkin, energizersNumber, ghostsNumber);
            writingHUD();
            action();
        }

         void makingField(string[] lines, Skin symbols, int energizers_number, int ghosts_number)
        {
            Cell.ShowMe = Data.ConsoleRender;
            getKey = delegate (ref int key)
            {
                if (Console.KeyAvailable)
                    key = (int)Console.ReadKey(true).Key;
            };

            Data.dictionary = new Dictionary<Types, ColorSymbol>() {
                {Types.blank, new ColorSymbol(ConsoleColor.Black, ' ')},
                {Types.heroNormal, new ColorSymbol(symbols.chn, symbols.HeroNormal)},
                {Types.heroEnerg, new ColorSymbol(symbols.che, symbols.HeroEnergizer)},
                {Types.ghostNormal, new ColorSymbol(symbols.cgn, symbols.GhostNormal)},
                {Types.ghostEnerg, new ColorSymbol(symbols.cge, symbols.GhostEnergizer)},
                {Types.wall, new ColorSymbol(symbols.cw, symbols.Wall)},
                {Types.food, new ColorSymbol(symbols.cf, symbols.Food)},
                {Types.energizer, new ColorSymbol(symbols.ce, symbols.Energizer)}
            };

            for (int i = 0; i < Field.Height; i++)
            {
                if (lines[i].Length == Field.Width)
                {
                    for (int j = 0; j < Field.Width; j++)
                    {
                        if (lines[i][j] == '@')
                        {
                            Field.cell[j, i] = new Wall(Types.wall);
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else if (lines[i][j] == ' ')
                        {

                            Field.cell[j, i] = new Food(Types.food);
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else if (lines[i][j] == 'E')
                        {
                            Field.cell[j, i] = new Energizer(Types.energizer, j, i);
                            energizers_number--;
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else if (lines[i][j] == 'G')
                        {
                            Field.cell[j, i] = new Ghost(Types.ghostNormal, j, i);
                            ghosts_number--;
                            ghosts[ghosts_number] = (Ghost)Field.cell[j, i];
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else if (lines[i][j] == 'H')
                        {
                            Field.cell[j, i] = new Hero(Types.heroNormal, j, i);
                            hero = (Hero)Field.cell[j, i];
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else if (lines[i][j] == 'b')
                        {
                            Field.cell[j, i] = new Blank(Types.blank);
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        else
                        {
                            Field.cell[j, i] = new Wall(Types.wall);
                            Cell.ShowMe(j, i, Field.cell[j, i].type);
                        }
                        if (lines[i][j] != '@')
                        {
                            List<Directions> roads = new List<Directions>();
                            if (lines[i - 1][j] != '@')
                                roads.Add(Directions.up);
                            if (lines[i][j - 1] != '@')
                                roads.Add(Directions.left);
                            if (lines[i + 1][j] != '@')
                                roads.Add(Directions.down);
                            if (lines[i][j + 1] != '@')
                                roads.Add(Directions.right);
                            if (roads.Count >= 3)
                            {
                                if (lines[i][j] == 'G')
                                {
                                    Ghost ghost = (Ghost)Field.cell[j, i];


                                    ghost.undercell.isCrossroad = true;
                                    ghost.undercell.dirs = roads;
                                }
                                else
                                {
                                    Field.cell[j, i].isCrossroad = true;
                                    Field.cell[j, i].dirs = roads;
                                    Cell.ShowMe(j, i, Field.cell[j, i].type);
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
        }

         void writingHUD()
        {
            Console.SetCursorPosition(1, Field.Height + 1);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Score: ");
            Console.SetCursorPosition(12, Field.Height + 1);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Booster");
            Console.SetCursorPosition(1, Field.Height);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("FPS: ");
            Field.PrintScore();
        }

        public override void GameOver()
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
            Console.WriteLine("Ghosts never gonna tell a lie and hurt you!\n");
            Console.WriteLine("Your earned {0} points!", Hero.score);
            Thread.Sleep(1500);
            Console.WriteLine("Press any button to continue...");
            Console.ReadKey();
            isGameRunning = false;
        }

        public override void YouWon()
        {
            if (currentPlayer.Map != "Test.txt")
            {
                player.SoundLocation = "music\\nyan.wav";
                if (music)
                    player.PlayLooping();
                isGameRunning = false;
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
                currentPlayer.Map = bufferMapForOptimisation;
                currentPlayer.IsNew = false;
                currentPlayer.Save();
                cursor = 1;
                mainMenu();
            }
        }

         public void Configuring(bool flag)
        {
            bufferMapForOptimisation = currentPlayer.Map;
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
                        currentPlayer.Map = "Test.txt";
                        gameCreation();
                    }
                    else if (buffer == ConsoleKey.N)
                    {
                        break;
                    }
                }
            }
            else
            {
                currentPlayer.Map = "Test.txt";
                gameCreation();
            }
        }

        public void Start()
        {
            Cell.game = this;
            Field.game = this;
            heroTime = normalHeroTime;
            ghostTime = normalGhostTime;
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.SetWindowSize(49, 25);
            Console.SetBufferSize(49, 25);
            Console.ForegroundColor = ConsoleColor.White;
            player.SoundLocation = "music\\menu.wav";
            if (music)
                player.PlayLooping();
            bufferMapForOptimisation = "map\\Alpha.txt";
            //try
            { playersMenu(true); }
            /*catch
            {
                Console.SetCursorPosition(0, 23);
                Console.Write("Bye-bye ^.^\n");
                Environment.Exit(0);
                return;
            }*/
        }

        static void Main()
        {
            GameConsole game = new GameConsole();
            game.Start();
        }
    }
}

