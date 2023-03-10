﻿using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq.Expressions;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Tetris
{
    internal class Program
    {
        // ======================================================================================================== \\
        // if you see a variable that's not const but is named with ALL_CAPS_SNAKE_CASE then don't change its value \\
        // ======================================================================================================== \\


        // score
        static int score = 0;

        // reserving ( these aren't the actual dimensions, these are for checking if the tetromino will be out of border )
        const byte LEFT_SPARE = 3;
        const byte RIGHT_SPARE = 2;
        const byte UP_SPARE = 2;
        const byte DOWN_SPARE = 2;

        // some keybinds
        static List<ConsoleKey> upInput = new() { ConsoleKey.UpArrow, ConsoleKey.W, ConsoleKey.LeftArrow, ConsoleKey.A };
        static List<ConsoleKey> downInput = new() { ConsoleKey.DownArrow, ConsoleKey.S, ConsoleKey.RightArrow, ConsoleKey.D };
        static List<ConsoleKey> confirmInput = new() { ConsoleKey.Enter, ConsoleKey.Spacebar, ConsoleKey.Z };
        static List<ConsoleKey> exitInput = new() { ConsoleKey.Escape, ConsoleKey.Backspace, ConsoleKey.X };
        // be aware when changing these values, you might need to change the part below "5943"
        static ConsoleKey[] moveLeft = { ConsoleKey.A, ConsoleKey.LeftArrow };
        static ConsoleKey[] moveRight = { ConsoleKey.D, ConsoleKey.RightArrow };

        // music
        // background is normal game music, background2 is intense game music, ending is the game over music
        // highScore is the game over music if the player gets a new high score, gameOver is the game over sound effect
        static SoundPlayer background = new(), background2 = new(), ending = new(), highScore = new(), gameOver = new();

        // time
        const byte REFRESH_RATE = 25; // interval between each refresh, in milliseconds, default is 25 ( it affects game speed )
        const ushort PAUSE = 500; // in milliseconds

        // miscellaneous
        const string BLOCK = "██"; // what tetrominoes is consisted of
        const string BLANK = "  "; // for erasing tetrominoes
        const byte BLOC = 4; // number of blocks in each tetromino, default is 4
        const byte HEIGHT = 26; // height for the tetris screen ( the entire game interface needs to be changed if HEIGHT is changed )
        const byte WIDTH = 10;
        static Random rnd = new(123); // 420 is a good seed
        static byte[] lowY = new byte[BLOC]; // lowest y position possible if dropped

        // some statistics ( how many times each type of tetromino occur )
        static ushort iNum = 0, lNum = 0, jNum = 0, tNum = 0, oNum = 0, sNum = 0, zNum = 0;

        // 3 bag method ( for selecting pieces )
        static byte bag = 0; // which bag to choose from
        static List<byte> bag0 = new();
        static List<byte> bag1 = new();
        static List<byte> bag2 = new();

        // file-read data, scoreboard and settings
        const ushort MAX_QUANTITY = 255; // the maximum number of names / scores allowed
        const ushort MAX_LENGTH = 15; // name length allowed
        static List<string> names = new();
        static List<int> scores = new();
        static byte music; // whether play music and sounds or not / which piece to play
        static byte invert; // whether invert left and right movement keys
        static byte ghost; // whether show ghost pieces
        static byte colour; // whether black and white or colourful

        static void Main(string[] args)
        {
            // tetris screen 10 x 26 grids
            // tuple for coordinates!
            // x: 0-19; y: 0-25

            if (invert == 1) // 5943 ( invert keys )
            {
                moveLeft[0] = ConsoleKey.D;
                moveLeft[1] = ConsoleKey.RightArrow;
                moveRight[0] = ConsoleKey.A;
                moveRight[1] = ConsoleKey.LeftArrow;
            }

            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.White;

            // loading
            LoadSettings();
            LoadScoreboard();

            byte selection;
            do
            {
                // if the player goes to fullscreen, the CURSOR will occur,
                // but if he goes back to the main menu, the CURSOR will disappear once again
                Console.CursorVisible = false;

                selection = Start(); // show menu (options) and get the user's selection
                switch (selection)
                {
                    case 0: NewGame(LevelSelection()); break;
                    case 1: Settings(); break;
                    case 2: Scoreboard(); break;
                    case 3: Tutorial(); break;
                }
                Console.Clear();
            }
            while (selection != 4); // case 4: ExitGame
        }
        static byte Start() // title screen
        {
            // music ( it's placed in Start instead of Main, this way the music changes without the game reopening if the player changes it in the settings)
            if (music != 0)
            {
                background.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + $"sounds\\Music{music}.wav";
                background.Load();
                background2.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + $"sounds\\Fast{music}.wav";
                background2.Load();
                ending.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "sounds\\Ending.wav";
                ending.Load();
                highScore.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "sounds\\HighScore.wav";
                highScore.Load();
                gameOver.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "sounds\\Hit.wav";
                gameOver.Load();

                background.PlayLooping();
            }
            else
            {
                // if the game starts with music, but the player goes to settings and turns it off, the music will stop
                background.Stop();
            }

            // menu
            const string CURSOR = ">"; // the CURSOR is placed in front of options
            string CLEAR_CURSOR = ""; // use space to clear the CURSOR
            for (byte i = 0; i < CURSOR.Length; i++) // increase the length of CLEAR_CURSOR until it reaches that of the CURSOR
            {
                CLEAR_CURSOR += " ";
            }

            Console.Clear();
            Console.WriteLine(@"

          _____ _____ _____ ____  ___ ____  
         |_   _| ____|_   _|  _ \|_ _/ ___| 
           | | |  _|   | | | |_) || |\___ \ 
           | | | |___  | | |  _ < | | ___) |
           |_| |_____| |_| |_| \_\___|____/ 



            ");
            Console.WriteLine(@"
                New Game
                Settings
                Scoreboard
                Tutorial
                Exit Game");
            Console.ReadKey(true); // press any key to continue

            const byte X = 14; // CURSOR horizontal position
            const byte Y_BASE = 12; // original CURSOR vertical position
            byte y = Y_BASE; // CURSOR vertical position (initial is 12: New Game)

            // CURSOR vertical position for each option
            const byte Y_NEWGAME = 12, ySettings = 13, yScoreboard = 14, yTutorial = 15, yExitGame = 16;

            while (true)
            {
                Console.SetCursorPosition(X, y);
                Console.Write(CURSOR);
                while (Console.KeyAvailable)
                {
                    ConsoleKey input = Console.ReadKey(true).Key;
                    Console.SetCursorPosition(X, y);
                    Console.Write(CLEAR_CURSOR); // remove the previous CURSOR
                    if (downInput.Contains(input)) y++;
                    else if (upInput.Contains(input)) y--;
                    else if (confirmInput.Contains(input))
                    {
                        return (byte)(y - Y_BASE);
                    }

                    // prevent the CURSOR from going out of range
                    if (y == Y_NEWGAME - 1) y++;
                    else if (y == yExitGame + 1) y--;
                }
                Thread.Sleep(REFRESH_RATE);
            }
        }
        static void NewGame(byte level = 0)
        {
            // by default, level 28 is the most difficult, after that is the infinite bonus level
            // to change how leveling up works, look for 5124 (default: ctrl + F, 5124)
            // h = horizontal, v = vertical, this will appear later in the code

            // Easter Egg
            const ushort SCORE_INCREMENT = 10000;

            // if the player enters either of these keybinds in game, the score increases
            List<ConsoleKey> konami = new() { ConsoleKey.W, ConsoleKey.W, ConsoleKey.S, ConsoleKey.S, ConsoleKey.A, ConsoleKey.D, ConsoleKey.A, ConsoleKey.D, ConsoleKey.Z, ConsoleKey.X };
            List<ConsoleKey> konami2 = new() { ConsoleKey.UpArrow, ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.DownArrow,
            ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.Z, ConsoleKey.X };


            // statistics
            score = 0;
            // changing level won't result in an actual change in the speed, level is dependent on lines, which controls the speed
            ushort line = 0; // line cleared
            ushort lineShowed = 0; // what will actually be displayed

            // detection
            bool hardDrop = false; // true if the hard drop key is pressed
            bool drawGhost = false; // ghost pieces won't be printed if it overlaps with the actual piece
            bool dropKeyPress = false; // true if the soft drop key is pressed
            bool ifPause = false; // true if the pause key is pressed
            bool intense = false; // intense = close to losing, play intense music

            // tick
            const byte MAX_TICK = 31; // the greater the MAX_TICK, the slower the game, default is 31
            byte maxTickLevel = MAX_TICK; // maxTickLevel changes depending on the level
            byte maxTickDrop = maxTickLevel; // maxTickDrop changes if the soft drop key is pressed
            sbyte tick = 0; // tick loops between 0 and maxTickDrop, it increases by 1 on each update
            byte taxiTick = 0; // calculates when a tetromino can slide while on top of blocks
            float taxiTickMultiplier = 2; // how many times of maxTickLevel needs to be reached to generate a new tetromino

            if (level != 0)
            {
                switch (level)
                {
                    case 1: maxTickLevel = (byte)(MAX_TICK * 0.9); line = 10; break;
                    case 2: maxTickLevel = (byte)(MAX_TICK * 0.8); line = 20; break;
                    case 3: maxTickLevel = (byte)(MAX_TICK * 0.7); line = 30; break;
                    case 4: maxTickLevel = (byte)(MAX_TICK * 0.58); line = 40; break;
                    case 5: maxTickLevel = (byte)(MAX_TICK * 0.48); line = 50; break;
                    case 6: maxTickLevel = (byte)(MAX_TICK * 0.4); line = 110; break;
                    case 7: maxTickLevel = (byte)(MAX_TICK * 0.27); line = 180; break;
                    case 8: maxTickLevel = (byte)(MAX_TICK * 0.17); line = 260; break;
                    case 9: maxTickLevel = (byte)(MAX_TICK * 0.13); line = 350; break;
                }
            }

            // miscellaneous
            byte[] x = new byte[BLOC]; // x position for each block in the tetromino
            byte[] y = new byte[BLOC]; // y position for each block in the tetromino
            sbyte direction = 0;
            bool generateNew = true; // if true then generate new tetromino
            byte pose = 0; // pose equal 0 when tetromino is generated
            byte highScorePosX; // x position for printing high score (it varies)
            const string NO_HIGHSCORE_MSG = "N/A"; // what it will print if there's no high score yet
            const byte INTENSE_Y = 8 + UP_SPARE; // if the player reaches this line, the intense music plays
            bool canMove = false; // if the player can move left or right
            const byte MAX_POSE = 4; // number of poses allowed
            const byte MAX_LINE_CLEAR = 4; // max number of lines that can be cleared at a time 

            // x and y base position for printing high score, score and lines
            const byte VALUE_BASE_X = 36; // x position for printing score, lines and high score
            const byte HIGHSCORE_BASE_Y = 26;
            const byte SCORE_BASE_Y = 20;
            const byte LINES_BASE_Y = 23;
            const byte LEVEL_X = 18;
            const byte LEVEL_Y = 0;

            // score ( how much score to give for each scenario )
            const ushort SCORE_1 = 40; // 1 line
            const ushort SCORE_2 = 100; // 2 lines
            const ushort SCORE_3 = 300; // 3 lines
            const ushort SCORE_4 = 1200; // 4 lines
            const ushort SCORE_T = 2400; // T-spin (2 lines)
            const ushort SCORE_SOFT = 1; // soft drop
            const ushort SCORE_HARD = 2; // hard drop

            // store the coordinates of occupied slots (excluding player-controlled tetromino)
            bool[,] coords = new bool[WIDTH + LEFT_SPARE + RIGHT_SPARE, HEIGHT + UP_SPARE + DOWN_SPARE]; // normally there should be 26 y values
            // the last y value is reserved for knowing vertical hit in advance (if you don't declare the 27th, the program will crash)
            ConsoleColor[,] colours = new ConsoleColor[WIDTH + 4, HEIGHT + 4]; // stores the colours of each block 

            List<byte> tetros = new(); // list of types of tetrominoes to generate
            // generating the list of types of tetrominoes
            tetros.Add(Dice()); // so that tetros[i - 1] has a value
            for (byte i = 0; i < BLOC; i++)
            {
                tetros.Add(Dice(tetros[i])); // picking a number between types (0 - 6)
            }

            // keybinds
            ConsoleKey[] rotate = { ConsoleKey.W, ConsoleKey.UpArrow, ConsoleKey.Z, ConsoleKey.X }; // rotate clockwise
            ConsoleKey[] drop = { ConsoleKey.S, ConsoleKey.DownArrow }; // soft drop
            ConsoleKey[] fall = { ConsoleKey.Spacebar }; // hard drop
            ConsoleKey[] pause = { ConsoleKey.P, ConsoleKey.Escape };

            List<ConsoleKey> keyList = new(); // records the recent keys pressed
            const byte KEY_LIST_LENGTH = 11; // max count of keyList

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(@"            LEVEL 
     #════════════════════#   +------------+
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   +------------+
     ║                    ║
     ║                    ║       SCORE
     ║                    ║         0
     ║                    ║
     ║                    ║       LINES
     ║                    ║         0
     ║                    ║
     ║                    ║     HIGH SCORE
     ║                    ║         0
     ║                    ║
     #════════════════════#         >       ");

            if (scores.Count != 0) // if the scoreboard isn't empty
            {
                // print the highest score
                string strHighScore = Convert.ToString(scores[0]);
                if (strHighScore.Length == 1) highScorePosX = (byte)(VALUE_BASE_X + 1); // centre text if string length = 1
                else highScorePosX = (byte)(VALUE_BASE_X + 1 - Math.Ceiling((float)(strHighScore.Length / 2))); // centre text if string length > 1
                Console.SetCursorPosition(highScorePosX - 1, HIGHSCORE_BASE_Y);
                Console.Write("         ");
                Console.SetCursorPosition(highScorePosX - 1, HIGHSCORE_BASE_Y);
                Console.Write(scores[0]);
            }
            else // if the scoreboard is empty
            {
                Console.SetCursorPosition(VALUE_BASE_X - 1, HIGHSCORE_BASE_Y);
                Console.Write("         ");
                Console.SetCursorPosition(VALUE_BASE_X - 1, HIGHSCORE_BASE_Y);
                Console.Write(NO_HIGHSCORE_MSG);
            }

            for (byte v = 0; v < HEIGHT + 1 + UP_SPARE; v++)
            {
                for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                {
                    coords[h, v] = false; // emptying the game window
                    colours[h, v] = ConsoleColor.Black; // resetting colours
                    coords[h, HEIGHT + 1 + UP_SPARE] = false;
                }

                // filling the corners
                for (byte h2 = 0; h2 < LEFT_SPARE; h2++)
                {
                    coords[h2, v] = true;
                    coords[h2, HEIGHT + 1 + UP_SPARE] = false;
                    for (byte h3 = WIDTH + LEFT_SPARE; h3 < WIDTH + LEFT_SPARE + RIGHT_SPARE; h3++)
                    {
                        coords[h3, v] = true;
                        coords[h3, HEIGHT + 1 + UP_SPARE] = false;
                    }
                }
            }

            while (true)
            {
                long t1 = DateTime.Now.Millisecond; // for delta time ( decides how long to pause )

                // Easter Egg
                if (keyList.SequenceEqual(konami) || keyList.SequenceEqual(konami2))
                {
                    score += SCORE_INCREMENT;
                    byte scorePosX = (byte)(VALUE_BASE_X - Math.Ceiling((float)(score.ToString().Length / 2)));
                    Console.SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Console.Write("         ");
                    Console.SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Console.Write(score);
                }

                if (generateNew) // if previous tetromino hit then make new one
                {
                    generateNew = false;
                    tick = 0;
                    maxTickDrop = maxTickLevel;
                    byte streak = 0; // how many line are cleared at a time

                    // check if game over
                    for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                    {
                        if (coords[h, 2 + UP_SPARE]) // if block is found on top
                        {
                            GameOver();
                            return;
                        }
                    }

                    for (byte i = 0; i < MAX_LINE_CLEAR; i++) // 4 is the maximum number of line that can be cleared at a time
                    {
                        byte blockNum; // number of occupied slots each line, if it's 20, the line will be cleared
                        for (byte v = HEIGHT - 1 + UP_SPARE; v > UP_SPARE - 1; v--) // from bottom to top
                        {
                            blockNum = 0;
                            for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                            {
                                if (coords[h, v + UP_SPARE] == true) // if the slot is occupied
                                {
                                    blockNum++;
                                }
                            }
                            if (blockNum == WIDTH) // if the line is filled
                            {
                                streak++;
                                line++;
                                lineShowed++;
                                byte yFilledLine = v; // rename "v" to y level of the filled line
                                for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                                {
                                    coords[h, yFilledLine + UP_SPARE] = false; // clear the filled line (value)
                                    colours[h, yFilledLine + UP_SPARE] = ConsoleColor.Black; // clear the filled line (colour)
                                    for (byte v2 = yFilledLine; v2 > 0; v2--) // for the filled line and each line above it
                                    {
                                        coords[h, v2 + UP_SPARE] = coords[h, v2 + UP_SPARE - 1]; // line below = line above (copying value)
                                        colours[h, v2 + UP_SPARE] = colours[h, v2 + UP_SPARE - 1]; // line below = line above (copying colour)
                                    }
                                }
                                DrawLines(yFilledLine, coords, colours, WIDTH);
                            }
                        }

                        // print line and level
                        Console.ForegroundColor = ConsoleColor.White;
                        string strLine = lineShowed.ToString();
                        byte linePosX = (byte)(VALUE_BASE_X - Math.Ceiling((float)(strLine.Length / 2)));
                        Console.SetCursorPosition(linePosX, LINES_BASE_Y);
                        Console.Write(lineShowed);
                        Console.SetCursorPosition(LEVEL_X, LEVEL_Y);
                        Console.Write(level);

                        // 5124 level changing mechanic
                        if (line < 60)
                        {
                            if (line < 10) maxTickLevel = MAX_TICK; // level 0
                            else if (line < 20) { maxTickLevel = (byte)(MAX_TICK * 0.9); level = 1; } // level 1
                            else if (line < 30) { maxTickLevel = (byte)(MAX_TICK * 0.8); level = 2; } // level 2
                            else if (line < 40) { maxTickLevel = (byte)(MAX_TICK * 0.7); level = 3; } // level 3
                            else if (line < 50) { maxTickLevel = (byte)(MAX_TICK * 0.58); level = 4; } // level 4
                            else { maxTickLevel = (byte)(MAX_TICK * 0.48); level = 5; } // level 5
                        }
                        else if (line < 690)
                        {
                            if (line < 120) { maxTickLevel = (byte)(MAX_TICK * 0.4); level = 6; } // level 6
                            else if (line < 190) { maxTickLevel = (byte)(MAX_TICK * 0.27); level = 7; } // level 7
                            else if (line < 270) { maxTickLevel = (byte)(MAX_TICK * 0.17); level = 8; } // level 8
                            else if (line < 360) { maxTickLevel = (byte)(MAX_TICK * 0.13); level = 9; } // level 9
                            else // level 10 - 12
                            {
                                maxTickLevel = (byte)(MAX_TICK * 0.1);
                                if (line < 460) level = 10; // level 10
                                else if (line < 570) level = 11; // level 11
                                else level = 12; // level 12
                            }
                        }
                        else if (line < 1290)
                        {
                            if (line < 1150) // level 13 - 15
                            {
                                maxTickLevel = (byte)(MAX_TICK * 0.08);
                                if (line < 790) level = 13; // level 13
                                else if (line < 900) level = 14; // level 14
                                else if (line < 1020) level = 15; // level 15
                            }
                            else { maxTickLevel = (byte)(MAX_TICK * 0.06); level = 16; } // level 16
                        }
                        else if (line < 3400)
                        {
                            if (line < 1520) // level 17 - 18
                            {
                                maxTickLevel = (byte)(MAX_TICK * 0.06);
                                if (line < 1400) level = 17; // level 17
                                else level = 18; // level 18
                            }
                            else // level 19 - 28
                            {
                                maxTickLevel = (byte)(MAX_TICK * 0.04);
                                if (line < 1650) level = 19; // level 19
                                else if (line < 1790) level = 20; // level 20
                                else if (line < 1940) level = 21; // level 21
                                else if (line < 2100) level = 22; // level 22
                                else if (line < 2270) level = 23; // level 23
                                else if (line < 2450) level = 24; // level 24
                                else if (line < 2640) level = 25; // level 25
                                else if (line < 2840) level = 26; // level 26
                                else if (line < 3050) level = 27; // level 27
                                else if (line < 3270) level = 28; // level 28
                                else { maxTickLevel = (byte)(MAX_TICK * 0.02); level = 29; } // level 29+
                            }
                        }
                        else { maxTickLevel = (byte)(MAX_TICK * 0.02); level = 29; } // level 29+
                    }

                    // add score depending on number of lines cleared
                    switch (streak)
                    {
                        case 1:
                            score += SCORE_1 * (level + 1);
                            break;
                        case 2:
                            if (tetros[0] == 3) // if it's a "T" shape (condition for T-spin)
                            {
                                for (byte i = 0; i < BLOC; i++) // if there's a block above the tetromino
                                {
                                    if (coords[x[i], y[i] + UP_SPARE - 1])
                                    {
                                        score += (SCORE_T - SCORE_2) * (level + 1);
                                        break;
                                    }
                                }
                                score += SCORE_2 * (level + 1);
                            }
                            else score += SCORE_2 * (level + 1);
                            break;
                        case 3:
                            score += SCORE_3 * (level + 1);
                            break;
                        case 4:
                            score += SCORE_4 * (level + 1);
                            break;
                    }

                    // print score
                    string strScore = Convert.ToString(score);
                    byte scorePosX = (byte)(36 - Math.Ceiling((float)(strScore.Length / 2)));
                    Console.SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Console.Write("         ");
                    Console.SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Console.Write(score);

                    pose = 0;
                    tetros.Add(Dice(tetros[BLOC - 1])); // which type of tetro to generate
                    tetros.RemoveAt(0); // remove the one generated already
                    DrawPreview(tetros);
                    switch (tetros[0]) // initialise tetromino blocks' position & colour
                    {
                        case 0: // I                                        ████████
                            x[0] = LEFT_SPARE + 3; y[0] = UP_SPARE; //  
                            x[1] = LEFT_SPARE + 4; y[1] = UP_SPARE;
                            x[2] = LEFT_SPARE + 5; y[2] = UP_SPARE;
                            x[3] = LEFT_SPARE + 6; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            iNum++;
                            break;
                        case 1: // L                                            ██
                            x[0] = LEFT_SPARE + 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = LEFT_SPARE + 4; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 5; y[2] = UP_SPARE + 1;
                            x[3] = LEFT_SPARE + 5; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            lNum++;
                            break;
                        case 2: // J                                        ██
                            x[0] = LEFT_SPARE + 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = LEFT_SPARE + 4; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 5; y[2] = UP_SPARE + 1;
                            x[3] = LEFT_SPARE + 3; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            jNum++;
                            break;
                        case 3: // T                                          ██
                            x[0] = LEFT_SPARE + 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = LEFT_SPARE + 4; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 5; y[2] = UP_SPARE + 1;
                            x[3] = LEFT_SPARE + 4; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            tNum++;
                            break;
                        case 4: // O                                        ████
                            x[0] = LEFT_SPARE + 4; y[0] = UP_SPARE + 1; //  ████
                            x[1] = LEFT_SPARE + 5; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 4; y[2] = UP_SPARE;
                            x[3] = LEFT_SPARE + 5; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            oNum++;
                            break;
                        case 5: // S                                          ████
                            x[0] = LEFT_SPARE + 3; y[0] = UP_SPARE + 1; //  ████
                            x[1] = LEFT_SPARE + 4; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 4; y[2] = UP_SPARE;
                            x[3] = LEFT_SPARE + 5; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Green;
                            sNum++;
                            break;
                        case 6: // Z                                        ████
                            x[0] = LEFT_SPARE + 4; y[0] = UP_SPARE + 1; //    ████
                            x[1] = LEFT_SPARE + 5; y[1] = UP_SPARE + 1;
                            x[2] = LEFT_SPARE + 3; y[2] = UP_SPARE;
                            x[3] = LEFT_SPARE + 4; y[3] = UP_SPARE;
                            Console.ForegroundColor = ConsoleColor.Red;
                            zNum++;
                            break;
                    }

                    if (colour == 0) // for dicromatic mode
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    if (music != 0)
                    {
                        if (!intense) // if not intense
                        {
                            for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                            {
                                if (coords[h, INTENSE_Y]) // if a block is detected on that line
                                {
                                    intense = true;
                                    background.Stop();
                                    background2.PlayLooping();
                                    break;
                                }
                            }
                        }
                        else // if intense
                        {
                            byte blockNum = 0;
                            for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                            {
                                if (coords[h, INTENSE_Y])
                                {
                                    blockNum++; // if there's no block, record it
                                }
                            }
                            if (blockNum == 0) // if none of the blocks is detected on that line
                            {
                                background2.Stop();
                                background.PlayLooping();
                                intense = false;
                            }
                        }
                    }
                }

                while (Console.KeyAvailable) // if a key is pressed
                {
                    ConsoleKey input = Console.ReadKey(true).Key;

                    // records key sequence and maintain its length
                    keyList.Add(input);
                    if (keyList.Count == KEY_LIST_LENGTH) keyList.RemoveAt(0);

                    if (rotate.Contains(input))
                    {
                        // change of coordinates of tetromino blocks after rotating
                        sbyte[] deltaX = new sbyte[BLOC];
                        sbyte[] deltaY = new sbyte[BLOC];

                        // setting deltaX and deltaY values depending on the type and pose of tetrominoes
                        switch (tetros[0])
                        {
                            case 0:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 2; deltaY[0] = -1;
                                        deltaX[1] = 1; deltaY[1] = 0;
                                        deltaX[2] = 0; deltaY[2] = 1;
                                        deltaX[3] = -1; deltaY[3] = 2;
                                        break;
                                    case 1:
                                        deltaX[0] = 1; deltaY[0] = 2;
                                        deltaX[1] = 0; deltaY[1] = 1;
                                        deltaX[2] = -1; deltaY[2] = 0;
                                        deltaX[3] = -2; deltaY[3] = -1;
                                        break;
                                    case 2:
                                        deltaX[0] = -2; deltaY[0] = 1;
                                        deltaX[1] = -1; deltaY[1] = 0;
                                        deltaX[2] = 0; deltaY[2] = -1;
                                        deltaX[3] = 1; deltaY[3] = -2;
                                        break;
                                    case 3:
                                        deltaX[0] = -1; deltaY[0] = -2;
                                        deltaX[1] = 0; deltaY[1] = -1;
                                        deltaX[2] = 1; deltaY[2] = 0;
                                        deltaX[3] = 2; deltaY[3] = 1;
                                        break;
                                }
                                break;
                            case 1:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = 1;
                                        deltaX[3] = 0; deltaY[3] = 2;
                                        break;
                                    case 1:
                                        deltaX[0] = 1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = -1;
                                        deltaX[3] = -2; deltaY[3] = 0;
                                        break;
                                    case 2:
                                        deltaX[0] = -1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = -1;
                                        deltaX[3] = 0; deltaY[3] = -2;
                                        break;
                                    case 3:
                                        deltaX[0] = -1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = 1;
                                        deltaX[3] = 2; deltaY[3] = 0;
                                        break;
                                }
                                break;
                            case 2:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = 1;
                                        deltaX[3] = 2; deltaY[3] = 0;
                                        break;
                                    case 1:
                                        deltaX[0] = 1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = -1;
                                        deltaX[3] = 0; deltaY[3] = 2;
                                        break;
                                    case 2:
                                        deltaX[0] = -1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = -1;
                                        deltaX[3] = -2; deltaY[3] = 0;
                                        break;
                                    case 3:
                                        deltaX[0] = -1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = 1;
                                        deltaX[3] = 0; deltaY[3] = -2;
                                        break;
                                }
                                break;
                            case 3:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = 1;
                                        deltaX[3] = 1; deltaY[3] = 1;
                                        break;
                                    case 1:
                                        deltaX[0] = 1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = -1;
                                        deltaX[3] = -1; deltaY[3] = 1;
                                        break;
                                    case 2:
                                        deltaX[0] = -1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = -1;
                                        deltaX[3] = -1; deltaY[3] = -1;
                                        break;
                                    case 3:
                                        deltaX[0] = -1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = 1;
                                        deltaX[3] = 1; deltaY[3] = -1;
                                        break;
                                }
                                break;
                            case 4:
                                deltaX[0] = 0; deltaY[0] = 0;
                                deltaX[1] = 0; deltaY[1] = 0;
                                deltaX[2] = 0; deltaY[2] = 0;
                                deltaX[3] = 0; deltaY[3] = 0;
                                break;
                            case 5:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = 1;
                                        deltaX[3] = 0; deltaY[3] = 2;
                                        break;
                                    case 1:
                                        deltaX[0] = 1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = 1;
                                        deltaX[3] = -2; deltaY[3] = 0;
                                        break;
                                    case 2:
                                        deltaX[0] = -1; deltaY[0] = 1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = -1; deltaY[2] = -1;
                                        deltaX[3] = 0; deltaY[3] = -2;
                                        break;
                                    case 3:
                                        deltaX[0] = -1; deltaY[0] = -1;
                                        deltaX[1] = 0; deltaY[1] = 0;
                                        deltaX[2] = 1; deltaY[2] = -1;
                                        deltaX[3] = 2; deltaY[3] = 0;
                                        break;
                                }
                                break;
                            case 6:
                                switch (pose)
                                {
                                    case 0:
                                        deltaX[0] = 0; deltaY[0] = 0;
                                        deltaX[1] = -1; deltaY[1] = 1;
                                        deltaX[2] = 2; deltaY[2] = 0;
                                        deltaX[3] = 1; deltaY[3] = 1;
                                        break;
                                    case 1:
                                        deltaX[0] = 0; deltaY[0] = 0;
                                        deltaX[1] = -1; deltaY[1] = -1;
                                        deltaX[2] = 0; deltaY[2] = 2;
                                        deltaX[3] = -1; deltaY[3] = 1;
                                        break;
                                    case 2:
                                        deltaX[0] = 0; deltaY[0] = 0;
                                        deltaX[1] = 1; deltaY[1] = -1;
                                        deltaX[2] = -2; deltaY[2] = 0;
                                        deltaX[3] = -1; deltaY[3] = -1;
                                        break;
                                    case 3:
                                        deltaX[0] = 0; deltaY[0] = 0;
                                        deltaX[1] = 1; deltaY[1] = 1;
                                        deltaX[2] = 0; deltaY[2] = -2;
                                        deltaX[3] = 1; deltaY[3] = -1;
                                        break;
                                }
                                break;
                        }

                        bool doRotate = CanRotate(x, y, coords, deltaX, deltaY);

                        if (doRotate) // if can rotate
                        {
                            taxiTick = 0;
                            for (byte i = 0; i < BLOC; i++)
                            {
                                x[i] += (byte)deltaX[i];
                                y[i] += (byte)deltaY[i];
                            }
                            pose++;
                            if (pose == MAX_POSE) pose = 0;
                        }
                    }
                    else if (drop.Contains(input)) // soft drop
                    {
                        dropKeyPress = true;

                        // make the tetromino fall at the next tick instead of waiting for several ticks
                        tick = 0;
                        maxTickDrop = 1;
                    }
                    else if (fall.Contains(input)) // hard drop
                    {
                        hardDrop = true;
                        break;
                    }
                    else if (moveLeft.Contains(input))
                    {
                        direction = -1;
                        canMove = GetAxisRaw(coords, input, x, y, WIDTH, -1);
                    }
                    else if (moveRight.Contains(input))
                    {
                        direction = 1;
                        canMove = GetAxisRaw(coords, input, x, y, WIDTH, 1);
                    }
                    else if (pause.Contains(input))
                    {
                        ifPause = true;
                    }
                    else if (input == ConsoleKey.Backspace) // force game over (quit game)
                    {
                        GameOver();
                        return;
                    }
                }
                if (!hardDrop) drawGhost = GetLowestPos(x, y, coords, true);
                else drawGhost = GetLowestPos(x, y, coords);

                if (drawGhost && ghost == 1)
                {
                    ConsoleColor previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Draw(x, lowY, true);
                    Console.ForegroundColor = previousColor;
                }
                if (hardDrop)
                {
                    score += SCORE_HARD * (lowY.Min() - y.Min());
                    for (byte i = 0; i < BLOC; i++)
                    {
                        y[i] = lowY[i];
                    }
                    generateNew = true;
                }
                Draw(x, y, true);
                if (ifPause)
                {
                    ConsoleKey input;
                    ifPause = false;
                    ConsoleColor previousColour = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(0, 0);
                    Console.Write("PAUSED");
                    Console.SetCursorPosition(35, 28);
                    Console.Write(" ║ ");
                    do
                    {
                        input = Console.ReadKey(true).Key;
                    }
                    while (!pause.Contains(input) && input != ConsoleKey.Backspace);
                    if (input == ConsoleKey.Backspace)
                    {
                        GameOver();
                        return;
                    }
                    else
                    {
                        Console.SetCursorPosition(35, 28);
                        Console.Write(" > ");
                        Console.SetCursorPosition(0, 0);
                        Console.Write("      ");
                        Console.ForegroundColor = previousColour;
                    }
                }
                long t2 = DateTime.Now.Millisecond; // for delta time
                int pauseTime;
                if (t2 < t1)
                {
                    pauseTime = REFRESH_RATE - (int)(1000 - t1 + t2);
                }
                else
                {
                    pauseTime = REFRESH_RATE - (int)(t2 - t1);
                }
                if (pauseTime >= 0 && pauseTime <= REFRESH_RATE)
                {
                    Thread.Sleep(pauseTime);
                }
                if (ghost == 1 && drawGhost && !hardDrop)
                {
                    Draw(x, lowY, false);
                }
                if (dropKeyPress) t1 = DateTime.Now.Millisecond;
                tick++;
                if (!hardDrop)
                {
                    bool hit = !CanFall(x, y, coords, canMove, direction); // check if the tetromino the player controls hit any occupied slot (vertically)
                    if (!hit)
                    {
                        if (dropKeyPress)
                        {
                            score += SCORE_SOFT;
                        }
                        taxiTick = 0;
                    }
                    if (hit)
                    {
                        if (dropKeyPress) // if soft drop key is pressed
                        {
                            t2 = DateTime.Now.Millisecond;
                            taxiTick = (byte)((maxTickLevel * taxiTickMultiplier) - 1);
                            if (t2 < t1)
                            {
                                pauseTime = REFRESH_RATE - (int)(1000 - t1 + t2);
                            }
                            else
                            {
                                pauseTime = REFRESH_RATE - (int)(t2 - t1);
                            }
                            if (pauseTime >= 0 && pauseTime <= REFRESH_RATE)
                            {
                                Thread.Sleep(pauseTime);
                            }
                        }
                        taxiTick++;
                        if (taxiTick == taxiTickMultiplier * maxTickLevel)
                        {
                            generateNew = true;
                            for (byte i = 0; i < BLOC; i++) // recording occupied slots
                            {
                                coords[x[i], y[i] + UP_SPARE] = true;
                                colours[x[i], y[i] + UP_SPARE] = Console.ForegroundColor;
                            }
                            taxiTick = 0;
                            hardDrop = false;
                        }
                        else
                        {
                            Draw(x, y, false);
                        }
                    }
                    else
                    {
                        Draw(x, y, false);
                    }
                    if (canMove)
                    {
                        switch (direction)
                        {
                            case -1: // left
                                for (byte i = 0; i < BLOC; i++) x[i]--;
                                break;
                            case 1: // right
                                for (byte i = 0; i < BLOC; i++) x[i]++;
                                break;
                        }
                    }
                    direction = 0;
                    if (tick == maxTickDrop && taxiTick == 0)
                    {
                        for (byte i = 0; i < BLOC; i++) y[i]++;
                        tick = 0;
                        maxTickDrop = maxTickLevel;
                    }
                }
                else // if hardDrop
                {
                    hardDrop = false;
                    for (byte i = 0; i < BLOC; i++) // recording occupied slots
                    {
                        coords[x[i], y[i] + UP_SPARE] = true;
                        colours[x[i], y[i] + UP_SPARE] = Console.ForegroundColor;
                    }
                }
                if (tick > maxTickDrop) // prevent some edge cases
                {
                    tick = 0;
                }
                dropKeyPress = false;
            }
        }
        static byte LevelSelection()
        {
            ConsoleKey input;
            byte x = 1, y = 1;
            byte[,] map = {
                {1,1,1,1,1},
                {1,0,0,0,1},
                {1,0,0,0,1},
                {1,0,0,0,1},
                {1,1,1,1,1},
                {1,1,0,1,1},
                {1,1,1,1,1},
                };

            Console.Clear();
            Console.WriteLine(@"
                    Select a Level

                *---*---*---*
                | 0 | 1 | 2 |
                *---*---*---*   *---*
                | 3 | 4 | 5 |   | 9 |
                *---*---*---*   *---*
                | 6 | 7 | 8 |
                *---*---*---*");
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Red;
            else Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.SetCursorPosition(16, 3);
            Console.Write("*---*");
            Console.SetCursorPosition(16, 4);
            Console.Write("|");
            Console.SetCursorPosition(20, 4);
            Console.Write("|");
            Console.SetCursorPosition(16, 5);
            Console.Write("*---*");
            do
            {
                sbyte deltaX = 0, deltaY = 0;
                input = Console.ReadKey(true).Key;
                if (moveLeft.Contains(input)) deltaX = -1;
                else if (moveRight.Contains(input)) deltaX = 1;
                else if (upInput.Contains(input)) deltaY = -1;
                else if (downInput.Contains(input)) deltaY = 1;
                Console.SetCursorPosition(16 + x, 4 + y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(16 + ((x - 1) * 4), 3 + (y - 1) * 2);
                Console.Write("*---*");
                Console.SetCursorPosition(16 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Console.Write("|");
                Console.SetCursorPosition(20 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Console.Write("|");
                Console.SetCursorPosition(16 + ((x - 1) * 4), 5 + (y - 1) * 2);
                Console.Write("*---*");
                if (map[x + deltaX, y + deltaY] == 0)
                {
                    x += (byte)deltaX;
                    y += (byte)deltaY;
                }
                else if (x == 3 && deltaX == 1)
                {
                    x = 5;
                    y = 2;
                }
                else if (x == 5 && deltaX == -1)
                {
                    x = 3;
                    y = 2;
                }
                ConsoleColor selectionColour = ConsoleColor.White;
                if (colour != 0)
                {
                    byte colourNum = Dice();
                    switch (colourNum)
                    {
                        case 0: selectionColour = ConsoleColor.Red; break;
                        case 1: selectionColour = ConsoleColor.DarkYellow; break;
                        case 2: selectionColour = ConsoleColor.Green; break;
                        case 3: selectionColour = ConsoleColor.Blue; break;
                        case 4: selectionColour = ConsoleColor.Magenta; break;
                        case 5: selectionColour = ConsoleColor.DarkMagenta; break;
                        case 6: selectionColour = ConsoleColor.DarkBlue; break;
                    }
                }
                else selectionColour = ConsoleColor.DarkGray;
                Console.ForegroundColor = selectionColour;
                Console.SetCursorPosition(16 + ((x - 1) * 4), 3 + (y - 1) * 2);
                Console.Write("*---*");
                Console.SetCursorPosition(16 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Console.Write("|");
                Console.SetCursorPosition(20 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Console.Write("|");
                Console.SetCursorPosition(16 + ((x - 1) * 4), 5 + (y - 1) * 2);
                Console.Write("*---*");
            }
            while (!confirmInput.Contains(input));
            switch (x)
            {
                case 1:
                    switch (y)
                    {
                        case 1: return 0;
                        case 2: return 3;
                        case 3: return 6;
                    }
                    break;
                case 2:
                    switch (y)
                    {
                        case 1: return 1;
                        case 2: return 4;
                        case 3: return 7;
                    }
                    break;
                case 3:
                    switch (y)
                    {
                        case 1: return 2;
                        case 2: return 5;
                        case 3: return 8;
                    }
                    break;
                case 5: return 9;
            }
            return 0;
        }
        static byte Dice(byte lastType = 0) // pick a type of tetromino
        {
            byte type = 0;
            byte typeIndex;
            switch (bag)
            {
                case 0:
                    if (bag0.Count == 0) bag0 = new() { 0, 1, 2, 3, 4, 5, 6 };
                    typeIndex = (byte)rnd.Next(bag0.Count);
                    type = bag0[typeIndex];
                    bag0.RemoveAt(typeIndex);
                    break;
                case 1:
                    if (bag1.Count == 0) bag1 = new() { 0, 1, 2, 3, 4, 5, 6 };
                    typeIndex = (byte)rnd.Next(bag1.Count);
                    type = bag1[typeIndex];
                    bag1.RemoveAt(typeIndex);
                    break;
                case 2:
                    if (bag2.Count == 0) bag2 = new() { 0, 1, 2, 3, 4, 5, 6 };
                    typeIndex = (byte)rnd.Next(bag2.Count);
                    type = bag2[typeIndex];
                    bag2.RemoveAt(typeIndex);
                    break;
            }
            bag++;
            if (bag > 2) bag = 0;
            return type;
        }
        static bool GetLowestPos(byte[] x, byte[] y, bool[,] coords, bool complex = false) // get coordinates of the tetromino if it dropped
        {
            for (byte i = 0; i < BLOC; i++)
            {
                if (complex)
                {
                    for (byte j = 0; j < BLOC; j++)
                    {
                        if (lowY[i] == y[j])
                        {
                            return false;
                        }
                    }
                }
                lowY[i] = y[i];
            }
            bool doFall;
            do
            {
                doFall = CanFall(x, lowY, coords, false);
                if (doFall)
                {
                    for (byte i = 0; i < BLOC; i++)
                    {
                        lowY[i]++;
                    }
                }
            }
            while (doFall == true);
            return true;
        }
        static bool CanFall(byte[] xByte, byte[] y, bool[,] coords, bool canMove, sbyte direction = 0)
        {
            sbyte[] x = new sbyte[BLOC];
            for (byte i = 0; i < BLOC; i++)
            {
                x[i] = (sbyte)xByte[i];
            }
            if (canMove)
            {
                for (byte j = 0; j < BLOC; j++) x[j] += direction;
            }
            for (byte i = 0; i < BLOC; i++)
            {
                if (y[i] == HEIGHT + UP_SPARE - 1)
                {
                    return false;
                }
                if (coords[x[i], y[i] + UP_SPARE + 1])
                {
                    return false;
                }
            }
            return true;
        }
        static bool GetAxisRaw(bool[,] coords, ConsoleKey input, byte[] x, byte[] y, byte WIDTH, sbyte inputDirection)
        {
            sbyte direction = 0; // -1 means left, 1 means right, 0 means neither
            if (inputDirection == -1 && x[0] >= LEFT_SPARE && x[1] >= LEFT_SPARE && x[2] >= LEFT_SPARE && x[3] >= LEFT_SPARE)
            {
                direction = -1;
            }
            else if (inputDirection == 1 && x[0] <= WIDTH + LEFT_SPARE - 1 && x[1] <= WIDTH + LEFT_SPARE - 1 && x[2] <= WIDTH + LEFT_SPARE - 1 && x[3] <= WIDTH + LEFT_SPARE - 1)
            {
                direction = 1;
            }
            for (byte i = 0; i < BLOC; i++)
            {
                if (coords[x[i] + direction, y[i] + UP_SPARE] == true)
                {
                    return false;
                }
            }
            return true;
        }
        static bool CanRotate(byte[] x, byte[] y, bool[,] coords, sbyte[] deltaX, sbyte[] deltaY)
        {
            for (byte i = 0; i < BLOC; i++)
            {
                if (coords[x[i] + deltaX[i], y[i] + deltaY[i] + UP_SPARE] == true) // if hit something
                {
                    return false;
                }
                else if (y[i] + deltaY[i] == HEIGHT + UP_SPARE) // if hit the floor
                {
                    return false;
                }
            }
            return true;
        }
        static void DrawLines(byte yFilledLine, bool[,] coords, ConsoleColor[,] colours, byte WIDTH)
        {
            for (byte v = (byte)(yFilledLine); v > 1; v--) // for the filled line and each line above it
            {
                for (byte h = LEFT_SPARE; h < WIDTH + LEFT_SPARE; h++)
                {
                    Console.SetCursorPosition(6 + ((h - LEFT_SPARE) * 2), v + 2 - UP_SPARE);
                    Console.ForegroundColor = colours[h, v + UP_SPARE];
                    if (coords[h, v + UP_SPARE] == true) Console.Write(BLOCK);
                    else Console.Write(BLANK);
                }
            }
        }
        static void Draw(byte[] x, byte[] y, bool add) // draw tetromino blocks
        {
            string block;
            if (add) // add
            {
                block = BLOCK;
            }
            else // remove
            {
                block = BLANK;
            }
            for (byte i = 0; i < BLOC; i++)
            {
                if (y[i] > 1)
                {
                    Console.SetCursorPosition(6 + ((x[i] - LEFT_SPARE) * 2), y[i] + 2 - UP_SPARE);
                    Console.Write(block);
                }
            }
        }
        static bool TryMove(bool[,] coords, byte[] x, byte[] y, sbyte direction)
        {
            for (byte i = 0; i < BLOC; i++)
            {
                if (coords[x[i] + direction, y[i] + UP_SPARE] == true)
                {
                    return false;
                }
            }
            return true;
        }
        static void DrawPreview(List<byte> tetros)
        {
            for (byte v = 0; v < 12; v += 5)
            {
                // clear previous preview
                Console.SetCursorPosition(33, 4 + v);
                Console.Write("        ");
                Console.SetCursorPosition(33, 5 + v);
                Console.Write("        ");
                Console.SetCursorPosition(33, 4 + v);
                // draw current preview
                switch (tetros[(v / 5) + 1])
                {
                    case 0: // I
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(BLOCK + BLOCK + BLOCK + BLOCK);
                        break;
                    case 1: // L
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(BLANK + BLANK + BLANK + BLOCK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;
                    case 2: // J
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(BLANK + BLOCK + BLANK + BLANK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;
                    case 3: // T
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(BLANK + BLANK + BLOCK + BLANK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;
                    case 4: // O
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(BLANK + BLOCK + BLOCK + BLANK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLOCK + BLOCK + BLANK);
                        break;
                    case 5: // S
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(BLANK + BLANK + BLOCK + BLOCK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLOCK + BLOCK + BLANK);
                        break;
                    case 6: // Z
                        if (colour == 1) Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(BLANK + BLOCK + BLOCK + BLANK);
                        Console.SetCursorPosition(33, 5 + v);
                        Console.Write(BLANK + BLANK + BLOCK + BLOCK);
                        break;
                }
            }
        }
        static void GameOver()
        {
            const byte Y_YES = 6; // y position for yes
            const byte Y_NO = 7; // y position for no
            const byte CURSOR_X = 12;
            const ushort CURTAIN_DROP_PAUSE = 50; // time interval between each row of curtain falls
            if (music != 0)
            {
                background.Stop();
                background2.Stop();
                gameOver.Play();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            for (byte i = 0; i < HEIGHT; i++)
            {
                Console.SetCursorPosition(6, 2 + i);
                Console.Write("████████████████████"); // curtain dropping
                Thread.Sleep(CURTAIN_DROP_PAUSE);
            }
            Thread.Sleep(PAUSE);
            Console.SetCursorPosition(16, 6);
            Console.WriteLine(@"
                                                
   ___   _   __  __ ___    _____   _____ ___    
  / __| /_\ |  \/  | __|  / _ \ \ / / __| _ \   
 | (_ |/ _ \| |\/| | _|  | (_) \ V /| _||   /   
  \___/_/ \_\_|  |_|___|  \___/ \_/ |___|_|_\   
                                                
                                                ");
            Thread.Sleep(1000);
            Console.ReadKey(true);
            Console.Clear();
            Console.Write("Press any key to continue  .\r\n                           T\r\n                          ( )\r\n                          <==>\r\n                           FJ\r\n                           ==\r\n                          J||F\r\n                          F||J\r\n                         /\\/\\/\\\r\n                         F++++J\r\n                        J{}{}{}F         .\r\n                     .  F{}{}{}J         T\r\n          .          T J{}{}{}{}F        ;;\r\n          T         /|\\F \\/ \\/ \\J  .   ,;;;;.\r\n         /:\\      .'/|\\\\:========F T ./;;;;;;\\\r\n       ./:/:/.   ///|||\\\\\\\"\"\"\"\"\"\" /x\\T\\;;;;;;/\r\n      //:/:/:/\\  \\\\\\\\|////..[]...xXXXx.|====|\r\n      \\:/:/:/:T7 :.:.:.:.:||[]|/xXXXXXx\\|||||\r\n      ::.:.:.:A. `;:;:;:;'=====\\XXXXXXX/=====.\r\n      `;\"\"::/xxx\\.|,|,|,| ( )( )| | | |.=..=.|\r\n       :. :`\\xxx/(_)(_)(_) _  _ | | | |'-''-'|\r\n       :T-'-.:\"\":|\"\"\"\"\"\"\"|/ \\/ \\|=====|======|\r\n       .A.\"\"\"||_|| ,. .. || || |/\\/\\/\\/ | | ||\r\n   :;:////\\:::.'.| || || ||-||-|/\\/\\/\\+|+| | |\r\n  ;:;;\\////::::,='======='============/\\/\\=====.\r\n:;:::;\"\"\":::::;:|__..,__|===========/||\\|\\====|\r\n:::::;|=:::;:;::|,;:::::         |========|   |\r\n::l42::::::(}:::::;::::::________|========|___|__");
            // https://www.asciiart.eu/buildings-and-places/monuments/other
            if (music != 0) ending.Play();
            Console.ReadKey(true);
            Console.Clear();
            if (names.Count < MAX_QUANTITY)
            {
                Console.WriteLine(@"



            Would you like to Record your Score?

             Yes
             No
");
                Console.SetCursorPosition(49, 4);
                Console.Write($"({score})");
                if (scores.Count != 0)
                {
                    if (scores[0] <= score)
                    {
                        Console.SetCursorPosition(CURSOR_X, 3);
                        Console.Write("New High Score! Congratulations!");
                        if (music != 0)
                        {
                            ending.Stop();
                            highScore.Play();
                        }
                    }
                }
                ConsoleKey input;
                byte y = Y_YES; // y position of the CURSOR
                do
                {
                    Console.SetCursorPosition(CURSOR_X, y);
                    Console.Write(">");
                    input = Console.ReadKey(true).Key;
                    Console.SetCursorPosition(CURSOR_X, y);
                    Console.Write(" ");
                    if (upInput.Contains(input))
                    {
                        y--;
                    }
                    else if (downInput.Contains(input))
                    {
                        y++;
                    }
                    if (y == Y_YES - 1) y = Y_YES;
                    else if (y == Y_NO + 1) y = Y_NO;
                }
                while (!confirmInput.Contains(input));
                if (confirmInput.Contains(input) && y == Y_YES)
                {
                    Console.Clear();
                    Console.SetCursorPosition(CURSOR_X, 4);
                    Console.Write("Please Enter Your Name: ");
                    char charEntered;
                    string name = "";
                    do
                    {
                        charEntered = Console.ReadKey(true).KeyChar;
                        if (charEntered == '\b')
                        {
                            if (name.Length > 0)
                            {
                                name = name.Remove(name.Length - 1);
                                Console.CursorLeft--;
                                Console.Write(" ");
                                Console.CursorLeft--;
                            }
                        }
                        else if (name.Length <= MAX_LENGTH && charEntered != ' ')
                        {
                            name += Convert.ToString(charEntered);
                            Console.Write(charEntered);
                        }
                    }
                    while (charEntered != '\r');
                    if (name == "\r") name = "Anonymous";
                    else name = name.Remove(name.Length - 1); // the last char detected is "enter", therefore it needs to be removed
                    Console.Clear();
                    Console.WriteLine("Saving . . .");
                    do
                    {
                        if (names.Count != 0)
                        {
                            if (score < scores[scores.Count - 1]) // if smaller than the lowest
                            {
                                names.Add(name);
                                scores.Add(score);
                                break; // I have to create a meaningless loop in order to break
                            }
                            for (byte i = 0; i < scores.Count; i++)
                            {
                                if (score >= scores[i]) // if current score is greater than that
                                {
                                    names.Insert(i, name);
                                    scores.Insert(i, score);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            names.Add(name);
                            scores.Add(score);
                        }
                    }
                    while (false);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", "");
                    for (byte i = 0; i < names.Count; i++)
                    {
                        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", $"{names[i]} {scores[i]}\r");
                    }
                    Console.Clear();
                    Console.WriteLine("Saved");
                    Thread.Sleep(PAUSE);
                }
            }
        }
        static void Settings()
        {
            byte selection;
            Console.Clear();
            Console.WriteLine(@"
      ___      _   _   _              
     / __| ___| |_| |_(_)_ _  __ _ ___
     \__ \/ -_)  _|  _| | ' \/ _` (_-<
     |___/\___|\__|\__|_|_||_\__, /__/
                             |___/    
");
            Console.WriteLine(@"
     Background Music: 
     Invert Movement Keys: 
     Show Ghost Pieces: 
     Colours: 
     [ Set to default ]
     [Clear Scoreboard]

     Press Space or Enter to switch options
     Press ESC or BackSpace to return and save


     Some changes will only be applied after the game restarts
    ");
            for (byte i = 0; i < 4; i++)
            {
                UpdateSettingsWord(i);
            }
            ConsoleKey input;
            Console.ReadKey(true);
            byte y = 8;
            selection = (byte)(y - 8);
            UpdateSettingsWord(selection);
            while (true)
            {
                while (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true).Key;
                    Console.SetCursorPosition(3, y);
                    Console.Write(" ");
                    if (upInput.Contains(input) && y != 8)
                    {
                        y--;
                    }
                    else if (downInput.Contains(input) && y != 13)
                    {
                        y++;
                    }
                    else if (confirmInput.Contains(input))
                    {
                        selection = (byte)(y - 8);
                        switch (selection)
                        {
                            case 0:
                                music++;
                                if (music == 4) music = 0;
                                break;
                            case 1:
                                invert++;
                                if (invert == 2) invert = 0;
                                break;
                            case 2:
                                ghost++;
                                if (ghost == 2) ghost = 0;
                                break;
                            case 3:
                                colour++;
                                if (colour == 2) colour = 0;
                                break;
                            case 4:
                                Console.SetCursorPosition(24, y);
                                Console.Write("Are you sure? (Y/N)");
                                input = Console.ReadKey(true).Key;
                                Console.Write("");
                                if (input == ConsoleKey.Y)
                                {
                                    music = 1;
                                    invert = 0;
                                    ghost = 0;
                                    colour = 1;
                                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt", Convert.ToString(music) + Convert.ToString(invert) + Convert.ToString(ghost) + Convert.ToString(colour));
                                }
                                Console.SetCursorPosition(24, y);
                                Console.Write("                   ");
                                UpdateSettingsWord();
                                break;
                            case 5:
                                Console.SetCursorPosition(24, y);
                                Console.Write("Are you sure? (Y/N)");
                                input = Console.ReadKey(true).Key;
                                if (input == ConsoleKey.Y)
                                {
                                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", "");
                                    Console.Clear();
                                    Console.WriteLine("Exiting to clear the scoreboard");
                                    Thread.Sleep(PAUSE * 2);
                                    Environment.Exit(0);
                                }
                                Console.SetCursorPosition(24, y);
                                Console.Write("                   ");
                                break;
                        }
                        UpdateSettingsWord(selection);
                    }
                    else if (exitInput.Contains(input))
                    {
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt", $"{music}{invert}{ghost}{colour}");
                        Console.Clear();
                        Console.WriteLine("Saved");
                        Thread.Sleep(PAUSE);
                        return;
                    }
                }
                Console.SetCursorPosition(3, y);
                Console.Write(">");
                Thread.Sleep(REFRESH_RATE);
            }
            Console.ReadKey(true);
        }
        static void UpdateSettingsWord(byte selection = 255)
        {
            byte j; // for a loop
            if (selection == 255)
            {
                j = 3;
                selection = 0;
            }
            else j = 1;
            for (byte i = 0; i < j; i++)
            {
                switch (selection)
                {
                    case 0:
                        Console.SetCursorPosition(23, 8);
                        switch (music)
                        {
                            case 0: Console.Write("None   "); break;
                            case 1: Console.Write("Music 1"); break;
                            case 2: Console.Write("Music 2"); break;
                            case 3: Console.Write("Music 3"); break;
                        }
                        break;
                    case 1:
                        Console.SetCursorPosition(27, 9);
                        switch (invert)
                        {
                            case 1: Console.Write("True "); break;
                            case 0: Console.Write("False"); break;
                        }
                        break;
                    case 2:
                        Console.SetCursorPosition(24, 10);
                        switch (ghost)
                        {
                            case 1: Console.Write("True "); break;
                            case 0: Console.Write("False"); break;
                        }
                        break;
                    case 3:
                        Console.SetCursorPosition(14, 11);
                        switch (colour)
                        {
                            case 0: Console.Write("dichromatic  "); break;
                            case 1: Console.Write("multicoloured"); break;
                        }
                        break;
                }
                selection++;
            }
        }
        static void Scoreboard()
        {
            Console.Clear();
            Console.WriteLine(@"
      ___                 _                      _ 
     / __| __ ___ _ _ ___| |__  ___  __ _ _ _ __| |
     \__ \/ _/ _ \ '_/ -_) '_ \/ _ \/ _` | '_/ _` |
     |___/\__\___/_| \___|_.__/\___/\__,_|_| \__,_|
                                               
");
            Console.SetCursorPosition(8, 7);
            Console.Write("Name");
            Console.SetCursorPosition(32, 7);
            Console.Write("Score");

            for (byte i = 0; i < names.Count; i++)
            {
                Console.SetCursorPosition(8, i + 8);
                Console.Write(names[i]);
                Console.SetCursorPosition(32, i + 8);
                Console.Write(scores[i]);
            }
            Console.ReadKey(true);
        }
        static void LoadScoreboard()
        {
            string scoreboardData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt");
            if (scoreboardData != "") // if it's not empty
            {
                try
                {
                    string[] scoreboardLines = scoreboardData.Split("\r"); // divide into each line
                    Array.Resize(ref scoreboardLines, scoreboardLines.Length - 1); // the last line is empty, remove it
                    string[] eachNameAndScore;
                    for (byte i = 0; i < scoreboardLines.Length; i++)
                    {
                        eachNameAndScore = scoreboardLines[i].Split(' '); // divide into mixed names and scores
                        names.Add(eachNameAndScore[0]);
                        scores.Add(Convert.ToInt32(eachNameAndScore[1]));
                    }
                    if (names.Count > 255)
                    {
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", "");
                        Console.Clear();
                        Console.WriteLine(@"
    You just changed data in the scoreboard,
    And now there are too much data to process.
    The scoreboard is cleared now,
    You just need to restart the game to play.

");
                        Thread.Sleep(PAUSE);
                        Console.WriteLine("    Never edit the game file again");
                        for (byte i = 0; i < 255; i++)
                        {
                            Console.ReadKey(true);
                        }
                    }
                }
                catch // if the file corrupted
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", "");
                }
            }
        }
        static void LoadSettings()
        {
            try
            {
                string settingsData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt");
                music = Convert.ToByte(Convert.ToString(settingsData[0]));
                invert = Convert.ToByte(Convert.ToString(settingsData[1]));
                ghost = Convert.ToByte(Convert.ToString(settingsData[2]));
                colour = Convert.ToByte(Convert.ToString(settingsData[3]));
            }
            catch // if the file corrupted
            {
                music = 1;
                invert = 0;
                ghost = 0;
                colour = 1;
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt", Convert.ToString(music) + Convert.ToString(invert) + Convert.ToString(ghost) + Convert.ToString(colour));
            }
        }
        static void Tutorial()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleKey input;
            bool page1 = false; // this will be true if the player has seen page1 already
            byte page = 0;
            Page0();
            while (true)
            {
                while (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true).Key;
                    if (upInput.Contains(input) && page >= 1)
                    {
                        page--;
                    }
                    else if (downInput.Contains(input) || confirmInput.Contains(input))
                    {
                        if (page <= 3)
                        {
                            page++;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (exitInput.Contains(input))
                    {
                        return;
                    }
                    else if (input == ConsoleKey.Y && page == 2)
                    {
                        if (page == 2)
                        {
                            Page2A();
                        }
                    }
                    switch (page)
                    {
                        case 0:
                            Page0();
                            break;
                        case 1:
                            page1 = Page1(page1);
                            break;
                        case 2:
                            Page2();
                            break;
                        case 3:
                            Page3();
                            break;
                        case 4:
                            Page4();
                            break;
                    }
                }
                Thread.Sleep(REFRESH_RATE);
            }
        }
        static void Page0()
        {
            Console.Clear();
            Console.Write(@"0
     Press <- and -> to flip pages   Press Esc to exit at any time
     #════════════════════#   +------------+
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   +------------+
     ║                    ║
     ║                    ║       SCORE
     ║                    ║         0
     ║                    ║
     ║                    ║       LINES
     ║                    ║         0
     ║                    ║
     ║                  ██║     HIGH SCORE
     ║  ████    ████    ██║         0
     ║████      ████  ████║
     #════════════════════#         >       ");
        }
        static bool Page1(bool alreadyPlayed)
        {
            ushort pause;
            if (alreadyPlayed == true)
            {
                pause = 0;
            }
            else
            {
                pause = PAUSE / 2;
            }
            byte x = 4, y = 10; // x and y level for showing tetrominoes, used for setting CURSOR location
            byte xIncrement = 12;
            Console.Clear();
            Console.Write(@"1
    The goal of this game is to earn as much score as possible. To earn score,
    you need to control the ");
            HighlightWord(" tetrominoes ");
            Console.Write(@" so that they fill a complete line.
    Once a line is filled, it will be cleared automatically, and you 
    will earn score depending on how many line you cleared at a time.
    If your");
            HighlightWord(" tetromino ");
            Console.Write(@" lands and touches the ceiling, the game ends.


    Here are the seven types of ");
            HighlightWord(" tetrominoes ");
            Console.WriteLine(" , each consisting of 4 blocks");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(x, y + 2);
            Console.Write("████████");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.DarkYellow;
            x += xIncrement;
            Console.SetCursorPosition(x, y);
            Console.Write("██");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("██");
            Console.SetCursorPosition(x, y + 2);
            Console.Write("████");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Green;
            x += xIncrement;
            Console.SetCursorPosition(x, y + 2);
            Console.Write("████");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("  ████");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Magenta;
            x += xIncrement;
            Console.SetCursorPosition(x, y + 2);
            Console.Write("██████");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("  ██  ");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Red;
            x += xIncrement;
            Console.SetCursorPosition(x, y + 2);
            Console.Write("  ████");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("████");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Blue;
            x += xIncrement;
            Console.SetCursorPosition(x, y);
            Console.Write("  ██");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("  ██");
            Console.SetCursorPosition(x, y + 2);
            Console.Write("████");

            Thread.Sleep(pause);
            if (colour != 0) Console.ForegroundColor = ConsoleColor.Yellow;
            x += xIncrement;
            Console.SetCursorPosition(x, y + 2);
            Console.Write("████");
            Console.SetCursorPosition(x, y + 1);
            Console.Write("████");

            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        static void Page2()
        {
            Console.Clear();
            Console.Write(@"2
    Here's the list of controls:
    A, D or <, > to move left and right
    W or ^ or Z or X to rotate clockwise
    S or v to increasing falling speed
    Spacebar to instantly drop
    Esc or P to pause the game
    Backspace to exit the game");
            Console.WriteLine(@"
    . -------------------------------------------------------------------.        
    | [Esc] [F1][F2][F3][F4][F5][F6][F7][F8][F9][F0][F10][F11][F12] o o o|        
    |                                                                    |        
    | [`][1][2][3][4][5][6][7][8][9][0][-][=][_<_] [I][H][U] [N][/][*][-]|        
    | [|-][Q][W][E][R][T][Y][U][I][O][P][{][}] | | [D][E][D] [7][8][9]|+||        
    | [CAP][A][S][D][F][G][H][J][K][L][;]['][#]|_|           [4][5][6]|_||        
    | [^][\][Z][X][C][V][B][N][M][,][.][/] [__^__]    [^]    [1][2][3]| ||        
    | [c]   [a][________________________][a]   [c] [<][V][>] [ 0  ][.]|_||        
    `--------------------------------------------------------------------'        

    Press 'Y' to try it for yourself
    ");

        }
        static void Page2A()
        {
            byte tick = 0;
            byte x = 4, y = 8; // x and y of the upper-left keyboard corner
            Console.SetCursorPosition(10, y + 10);
            Console.Write("'N' to finish             ");
            ConsoleKey input;
            while (true)
            {
                while (Console.KeyAvailable)
                {
                    input = Console.ReadKey(true).Key;
                    if (input == ConsoleKey.Z)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 8, y + 6);
                        Console.Write("[Z]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.X)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 11, y + 6);
                        Console.Write("[X]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.A)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 7, y + 5);
                        Console.Write("[A]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Move Left                    ");
                    }
                    else if (input == ConsoleKey.D)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 13, y + 5);
                        Console.Write("[D]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Move Right                   ");
                    }
                    else if (input == ConsoleKey.W)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 9, y + 4);
                        Console.Write("[W]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.S)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 10, y + 5);
                        Console.Write("[S]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Drop Soft                    ");
                    }
                    else if (input == ConsoleKey.Spacebar)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 11, y + 7);
                        Console.Write("[________________________]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Drop Hard                    ");
                    }
                    else if (input == ConsoleKey.LeftArrow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 47, y + 7);
                        Console.Write("[<]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Move Left                    ");
                    }
                    else if (input == ConsoleKey.RightArrow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 53, y + 7);
                        Console.Write("[>]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Move Right                   ");
                    }
                    else if (input == ConsoleKey.UpArrow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 50, y + 6);
                        Console.Write("[^]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.DownArrow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 50, y + 7);
                        Console.Write("[v]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Drop Soft                    ");
                    }
                    else if (input == ConsoleKey.Escape)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 2, y + 1);
                        Console.Write("[Esc]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Pause                        ");
                    }
                    else if (input == ConsoleKey.P)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 33, y + 4);
                        Console.Write("[P]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Pause                        ");
                    }
                    else if (input == ConsoleKey.Backspace)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(x + 41, y + 3);
                        Console.Write("[_<_]");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.SetCursorPosition(x, y + 9);
                        Console.WriteLine("Exit                         ");

                    }
                    else if (input == ConsoleKey.N)
                    {
                        return;
                    }
                }
                Thread.Sleep(REFRESH_RATE);
                tick++;
                if (tick == 28)
                {
                    Console.SetCursorPosition(0, y - 1);
                    Console.WriteLine(@"
    . -------------------------------------------------------------------.        
    | [Esc] [F1][F2][F3][F4][F5][F6][F7][F8][F9][F0][F10][F11][F12] o o o|        
    |                                                                    |        
    | [`][1][2][3][4][5][6][7][8][9][0][-][=][_<_] [I][H][U] [N][/][*][-]|        
    | [|-][Q][W][E][R][T][Y][U][I][O][P][{][}] | | [D][E][D] [7][8][9]|+||        
    | [CAP][A][S][D][F][G][H][J][K][L][;]['][#]|_|           [4][5][6]|_||        
    | [^][\][Z][X][C][V][B][N][M][,][.][/] [__^__]    [^]    [1][2][3]| ||        
    | [c]   [a][________________________][a]   [c] [<][V][>] [ 0  ][.]|_||        
    `--------------------------------------------------------------------'
                                                                          ");
                    tick = 0;
                }
            }
        }
        static void Page3()
        {
            Console.Clear();
            Console.Write(@"3           LEVEL 0 <- Current level
     #════════════════════#   +------------+
     ║     GAME PANEL     ║   |  PREVIEW   |
     ║                    ║   |            |
     ║  This is the main  ║   | This panel |
     ║  panel, where you  ║   | shows the  |
     ║  need to pay most  ║   | next type  |
     ║  attention to      ║   |of tetromino|
     ║  each round        ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   |            |
     ║                    ║   +------------+
     ║                    ║
     ║                    ║       SCORE <----- Your current score
     ║                    ║         0
     ║                    ║
     ║                    ║       LINES <----- Number of line cleared
     ║                    ║         0
     ║                    ║
     ║                  ██║     HIGH SCORE <-- Highest score recorded
     ║  ████    ████    ██║         0
     ║████      ████  ████║
     #════════════════════#         >      <-- Pause Indicator > or ║ ");
        }
        static void Page4()
        {
            Console.Clear();
            Console.Write("4                         <==>\r\n                           FJ\r\n                           ==\r\n                          J||F\r\n                          F||J\r\n                         /\\/\\/\\\r\n                         F++++J\r\n                        J{}{}{}F         .\r\n                     .  F{}{}{}J         T\r\n          .          T J{}{}{}{}F        ;;\r\n          T         /|\\F \\/ \\/ \\J  .   ,;;;;.\r\n         /:\\      .'/|\\\\:========F T ./;;;;;;\\\r\n       ./:/:/.   ///|||\\\\\\\"\"\"\"\"\"\" /x\\T\\;;;;;;/\r\n      //:/:/:/\\  \\\\\\\\|////..[]...xXXXx.|====|\r\n      \\:/:/:/:T7 :.:.:.:.:||[]|/xXXXXXx\\|||||\r\n      ::.:.:.:A. `;:;:;:;'=====\\XXXXXXX/=====.\r\n      `;\"\"::/xxx\\.|,|,|,| ( )( )| | | |.=..=.|\r\n       :. :`\\xxx/(_)(_)(_) _  _ | | | |'-''-'|\r\n       :T-'-.:\"\":|\"\"\"\"\"\"\"|/ \\/ \\|=====|======|\r\n       .A.\"\"\"||_|| ,. .. || || |/\\/\\/\\/ | | ||\r\n   :;:////\\:::.'.| || || ||-||-|/\\/\\/\\+|+| | |\r\n  ;:;;\\////::::,='======='============/\\/\\=====.\r\n:;:::;\"\"\":::::;:|__..,__|===========/||\\|\\====|\r\n:::::;|=:::;:;::|,;:::::         |========|   |\r\n::l42::::::(}:::::;::::::________|========|___|__");
            // https://www.asciiart.eu/buildings-and-places/monuments/other
            Console.WriteLine();
            Console.WriteLine(@"
    Don't forget to record your score after playing!

    Good Luck Have Fun!");
            // Alexey Leonidovich Pajitnov
            // 田中 宏和
            // Пётр Ильи́ч Чайко́вский
            // Alexandre César Léopold Bizet
        }
        static void HighlightWord(string word)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(word); // highlighting the word
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}