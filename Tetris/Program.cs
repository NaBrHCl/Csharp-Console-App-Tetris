using System.Media;
using static System.Console;

namespace Tetris
{
    public class Program
    {
        #region Variable Declaration
        // score
        static int score = 0;

        static Settings settings;
        static Scoreboard scoreboard;
        static Tutorial tutorial;

        // reserving ( these aren't the actual dimensions, these are for checking if the tetromino will be out of border )
        const byte UP_SPARE = 2;

        // some keybinds
        static List<ConsoleKey> upInput = new() { ConsoleKey.UpArrow, ConsoleKey.W, ConsoleKey.LeftArrow, ConsoleKey.A };
        static List<ConsoleKey> downInput = new() { ConsoleKey.DownArrow, ConsoleKey.S, ConsoleKey.RightArrow, ConsoleKey.D };
        static List<ConsoleKey> confirmInput = new() { ConsoleKey.Enter, ConsoleKey.Spacebar, ConsoleKey.Z };
        static List<ConsoleKey> exitInput = new() { ConsoleKey.Escape, ConsoleKey.Backspace, ConsoleKey.X };

        // be aware when changing these values, you might need to change the part below "5943"
        static ConsoleKey[] moveLeft = { ConsoleKey.A, ConsoleKey.LeftArrow };
        static ConsoleKey[] moveRight = { ConsoleKey.D, ConsoleKey.RightArrow };

        // _music
        // background is normal game _music, background2 is intense game _music, ending is the game over _music
        // highScore is the game over _music if the player gets a new high score, gameOver is the game over sound effect
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
        const ushort MAX_QUANTITY = 255; // the maximum number of _names / _scores allowed
        const ushort MAX_LENGTH = 15; // name length allowed

        const string SETTINGS_PATH = "saves\\Settings.txt";
        const string SCOREBOARD_PATH = "saves\\Scoreboard.txt";

        #endregion

        #region Game
        static void Main(string[] args)
        {
            // tetris screen 10 x 26 grids
            // tuple for coordinates!
            // x: 0-19; y: 0-25

            settings = new Settings(upInput, downInput, confirmInput, exitInput, PAUSE, REFRESH_RATE, SETTINGS_PATH, SCOREBOARD_PATH);
            scoreboard = new Scoreboard(PAUSE, SCOREBOARD_PATH);
            tutorial = new Tutorial(upInput, downInput, confirmInput, exitInput, PAUSE, REFRESH_RATE, settings);

            if (settings.InvertMovement) // 5943 ( _invertMovement keys )
            {
                moveLeft[0] = ConsoleKey.D;
                moveLeft[1] = ConsoleKey.RightArrow;
                moveRight[0] = ConsoleKey.A;
                moveRight[1] = ConsoleKey.LeftArrow;
            }

            CursorVisible = false;
            ForegroundColor = ConsoleColor.White;

            // loading
            settings.Load();
            scoreboard.Load();

            byte selection;
            do
            {
                // if the player goes to fullscreen, the CURSOR will occur,
                // but if he goes back to the main menu, the CURSOR will disappear once again
                CursorVisible = false;

                selection = Start(); // show menu (options) and get the user's selection

                switch (selection)
                {
                    case 0:
                        sbyte levelSelected = LevelSelection();
                        if (levelSelected != -1) NewGame((byte)levelSelected);
                        break;
                    case 1: settings.Display(); break;
                    case 2: scoreboard.Display(); break;
                    case 3: tutorial.Display(); break;
                }

                Clear();
            }
            while (selection != 4); // case 4: ExitGame
        }

        #endregion

        #region Main Game
        static byte Start() // title screen
        {
            // _music ( it's placed in Start instead of Main, this way the _music changes without the game reopening if the player changes it in the settings)
            if (settings.Music != 0)
            {
                background.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + $"sounds\\Music{settings.Music}.wav";
                background.Load();
                background2.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + $"sounds\\Fast{settings.Music}.wav";
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
                // if the game starts with _music, but the player goes to settings and turns it off, the _music will stop
                background.Stop();
            }

            // menu
            const string CURSOR = ">"; // the CURSOR is placed in front of options
            string CLEAR_CURSOR = ""; // use space to clear the CURSOR
            for (byte i = 0; i < CURSOR.Length; i++) // increase the length of CLEAR_CURSOR until it reaches that of the CURSOR
            {
                CLEAR_CURSOR += " ";
            }

            Clear();
            ForegroundColor = ConsoleColor.White;
            WriteLine(@"

          _____ _____ _____ ____  ___ ____  
         |_   _| ____|_   _|  _ \|_ _/ ___| 
           | | |  _|   | | | |_) || |\___ \ 
           | | | |___  | | |  _ < | | ___) |
           |_| |_____| |_| |_| \_\___|____/ 



            ");
            WriteLine(@"
                New Game
                Settings
                Scoreboard
                Tutorial
                Exit Game");
            ReadKey(true); // press any key to continue

            const byte X = 14; // CURSOR horizontal position
            const byte Y_BASE = 12; // original CURSOR vertical position
            byte y = Y_BASE; // CURSOR vertical position (initial is 12: New Game)

            // CURSOR vertical position for each option
            const byte Y_NEWGAME = 12, ySettings = 13, yScoreboard = 14, yTutorial = 15, yExitGame = 16;

            while (true)
            {
                while (KeyAvailable)
                {
                    ConsoleKey input = ReadKey(true).Key;
                    SetCursorPosition(X, y);
                    Write(CLEAR_CURSOR); // remove the previous CURSOR

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

                SetCursorPosition(X, y);
                Write(CURSOR);

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
            bool drawGhost = false; // _showGhostPiece pieces won't be printed if it overlaps with the actual piece
            bool dropKeyPress = false; // true if the soft drop key is pressed
            bool ifPause = false; // true if the pause key is pressed
            bool intense = false; // intense = close to losing, play intense _music

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
            const byte INTENSE_Y = 8 + UP_SPARE; // if the player reaches this line, the intense _music plays
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
            bool[,] coords = new bool[WIDTH, HEIGHT + UP_SPARE]; // normally there should be 26 y values
            // the last y value is reserved for knowing vertical hit in advance (if you don't declare the 27th, the program will crash)
            ConsoleColor[,] colours = new ConsoleColor[WIDTH, HEIGHT + UP_SPARE]; // stores the colours of each block 

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

            Clear();
            ForegroundColor = ConsoleColor.White;
            Write(@"            LEVEL 
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

            if (scoreboard.Scores.Count != 0) // if the scoreboard isn't empty
            {
                // print the highest score
                string strHighScore = Convert.ToString(scoreboard.Scores[0]);
                if (strHighScore.Length == 1) highScorePosX = (byte)(VALUE_BASE_X + 1); // centre text if string length = 1
                else highScorePosX = (byte)(VALUE_BASE_X + 1 - Math.Ceiling((float)(strHighScore.Length / 2))); // centre text if string length > 1
                SetCursorPosition(highScorePosX - 1, HIGHSCORE_BASE_Y);
                Write("         ");
                SetCursorPosition(highScorePosX - 1, HIGHSCORE_BASE_Y);
                Write(scoreboard.Scores[0]);
            }
            else // if the scoreboard is empty
            {
                SetCursorPosition(VALUE_BASE_X - 1, HIGHSCORE_BASE_Y);
                Write("         ");
                SetCursorPosition(VALUE_BASE_X - 1, HIGHSCORE_BASE_Y);
                Write(NO_HIGHSCORE_MSG);
            }

            for (byte v = 0; v < HEIGHT + UP_SPARE; v++)
            {
                for (byte h = 0; h < WIDTH; h++)
                {
                    coords[h, v] = false; // emptying the game window
                    colours[h, v] = ConsoleColor.Black; // resetting colours
                    coords[h, HEIGHT + UP_SPARE - 1] = false;
                }

                // filling the corners
                for (byte h2 = 0; h2 < 0; h2++)
                {
                    coords[h2, v] = true;
                    coords[h2, HEIGHT + UP_SPARE] = false;
                    //for (byte h3 = WIDTH; h3 < WIDTH; h3++)
                    //{
                    //    coords[h3, v] = true;
                    //    coords[h3, HEIGHT + UP_SPARE] = false;
                    //}
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
                    SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Write("         ");
                    SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Write(score);
                }

                if (generateNew) // if previous tetromino hit then make new one
                {
                    generateNew = false;
                    tick = 0;
                    maxTickDrop = maxTickLevel;
                    byte streak = 0; // how many line are cleared at a time

                    // check if game over
                    for (byte h = 0; h < WIDTH; h++)
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
                            for (byte h = 0; h < WIDTH; h++)
                            {
                                if (coords[h, v]) // if the slot is occupied
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
                                for (byte h = 0; h < WIDTH; h++)
                                {
                                    coords[h, yFilledLine] = false; // clear the filled line (value)
                                    colours[h, yFilledLine] = ConsoleColor.Black; // clear the filled line (_colourful)
                                    for (byte v2 = yFilledLine; v2 > 0; v2--) // for the filled line and each line above it
                                    {
                                        coords[h, v2] = coords[h, v2 - 1]; // line below = line above (copying value)
                                        colours[h, v2] = colours[h, v2 - 1]; // line below = line above (copying _colourful)
                                    }
                                }
                                DrawLines(yFilledLine, coords, colours, WIDTH);
                            }
                        }

                        // print line and level
                        ForegroundColor = ConsoleColor.White;
                        string strLine = lineShowed.ToString();
                        byte linePosX = (byte)(VALUE_BASE_X - Math.Ceiling((float)(strLine.Length / 2)));
                        SetCursorPosition(linePosX, LINES_BASE_Y);
                        Write(lineShowed);
                        SetCursorPosition(LEVEL_X, LEVEL_Y);
                        Write(level);

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
                    SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Write("         ");
                    SetCursorPosition(scorePosX, SCORE_BASE_Y);
                    Write(score);

                    pose = 0;
                    tetros.Add(Dice(tetros[BLOC - 1])); // which type of tetro to generate
                    tetros.RemoveAt(0); // remove the one generated already
                    DrawPreview(tetros);
                    switch (tetros[0]) // initialise tetromino blocks' position & _colourful
                    {
                        case 0: // I               ████████
                            x[0] = 3; y[0] = UP_SPARE; //  
                            x[1] = 4; y[1] = UP_SPARE;
                            x[2] = 5; y[2] = UP_SPARE;
                            x[3] = 6; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Cyan;
                            iNum++;
                            break;
                        case 1: // L                               ██
                            x[0] = 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = 4; y[1] = UP_SPARE + 1;
                            x[2] = 5; y[2] = UP_SPARE + 1;
                            x[3] = 5; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.DarkYellow;
                            lNum++;
                            break;
                        case 2: // J                           ██
                            x[0] = 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = 4; y[1] = UP_SPARE + 1;
                            x[2] = 5; y[2] = UP_SPARE + 1;
                            x[3] = 3; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Blue;
                            jNum++;
                            break;
                        case 3: // T                             ██
                            x[0] = 3; y[0] = UP_SPARE + 1; //  ██████
                            x[1] = 4; y[1] = UP_SPARE + 1;
                            x[2] = 5; y[2] = UP_SPARE + 1;
                            x[3] = 4; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Magenta;
                            tNum++;
                            break;
                        case 4: // O                           ████
                            x[0] = 4; y[0] = UP_SPARE + 1; //  ████
                            x[1] = 5; y[1] = UP_SPARE + 1;
                            x[2] = 4; y[2] = UP_SPARE;
                            x[3] = 5; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Yellow;
                            oNum++;
                            break;
                        case 5: // S                             ████
                            x[0] = 3; y[0] = UP_SPARE + 1; //  ████
                            x[1] = 4; y[1] = UP_SPARE + 1;
                            x[2] = 4; y[2] = UP_SPARE;
                            x[3] = 5; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Green;
                            sNum++;
                            break;
                        case 6: // Z                           ████
                            x[0] = 4; y[0] = UP_SPARE + 1; //    ████
                            x[1] = 5; y[1] = UP_SPARE + 1;
                            x[2] = 3; y[2] = UP_SPARE;
                            x[3] = 4; y[3] = UP_SPARE;
                            ForegroundColor = ConsoleColor.Red;
                            zNum++;
                            break;
                    }

                    if (!settings.Colourful) // for dicromatic mode
                    {
                        ForegroundColor = ConsoleColor.White;
                    }

                    if (settings.Music != 0)
                    {
                        if (!intense) // if not intense
                        {
                            for (byte h = 0; h < WIDTH; h++)
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
                            for (byte h = 0; h < WIDTH; h++)
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

                while (KeyAvailable) // if a key is pressed
                {
                    ConsoleKey input = ReadKey(true).Key;

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

                if (drawGhost && settings.ShowGhostPiece)
                {
                    ConsoleColor previousColor = ForegroundColor;
                    ForegroundColor = ConsoleColor.DarkGray;
                    Draw(x, lowY, true);
                    ForegroundColor = previousColor;
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
                    ConsoleColor previousColour = ForegroundColor;
                    ForegroundColor = ConsoleColor.White;
                    SetCursorPosition(0, 0);
                    Write("PAUSED");
                    SetCursorPosition(35, 28);
                    Write(" ║ ");
                    do
                    {
                        input = ReadKey(true).Key;
                    }
                    while (!pause.Contains(input) && input != ConsoleKey.Backspace);
                    if (input == ConsoleKey.Backspace)
                    {
                        GameOver();
                        return;
                    }
                    else
                    {
                        SetCursorPosition(35, 28);
                        Write(" > ");
                        SetCursorPosition(0, 0);
                        Write("      ");
                        ForegroundColor = previousColour;
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
                if (settings.ShowGhostPiece && drawGhost && !hardDrop)
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
                                coords[x[i], y[i]] = true;
                                colours[x[i], y[i]] = ForegroundColor;
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
                        coords[x[i], y[i]] = true;
                        colours[x[i], y[i]] = ForegroundColor;
                    }
                }
                if (tick > maxTickDrop) // prevent some edge cases
                {
                    tick = 0;
                }
                dropKeyPress = false;
            }
        }
        static sbyte LevelSelection()
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

            Clear();
            WriteLine(@"
                    Select a Level

                *---*---*---*
                | 0 | 1 | 2 |
                *---*---*---*   *---*
                | 3 | 4 | 5 |   | 9 |
                *---*---*---*   *---*
                | 6 | 7 | 8 |
                *---*---*---*");
            if (settings.Colourful) ForegroundColor = ConsoleColor.Red;
            else ForegroundColor = ConsoleColor.DarkGray;
            SetCursorPosition(16, 3);
            Write("*---*");
            SetCursorPosition(16, 4);
            Write("|");
            SetCursorPosition(20, 4);
            Write("|");
            SetCursorPosition(16, 5);
            Write("*---*");
            do
            {
                sbyte deltaX = 0, deltaY = 0;
                input = ReadKey(true).Key;
                if (moveLeft.Contains(input)) deltaX = -1;
                else if (moveRight.Contains(input)) deltaX = 1;
                else if (upInput.Contains(input)) deltaY = -1;
                else if (downInput.Contains(input)) deltaY = 1;
                else if (exitInput.Contains(input)) return -1;
                SetCursorPosition(16 + x, 4 + y);
                ForegroundColor = ConsoleColor.White;
                SetCursorPosition(16 + ((x - 1) * 4), 3 + (y - 1) * 2);
                Write("*---*");
                SetCursorPosition(16 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Write("|");
                SetCursorPosition(20 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Write("|");
                SetCursorPosition(16 + ((x - 1) * 4), 5 + (y - 1) * 2);
                Write("*---*");
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
                if (settings.Colourful)
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
                ForegroundColor = selectionColour;
                SetCursorPosition(16 + ((x - 1) * 4), 3 + (y - 1) * 2);
                Write("*---*");
                SetCursorPosition(16 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Write("|");
                SetCursorPosition(20 + ((x - 1) * 4), 4 + (y - 1) * 2);
                Write("|");
                SetCursorPosition(16 + ((x - 1) * 4), 5 + (y - 1) * 2);
                Write("*---*");
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
                if (complex) // if !hardDrop
                {
                    for (byte j = 0; j < BLOC; j++)
                    {
                        if (lowY[i] == y[j]) // if the _showGhostPiece piece overlaps with the actual piece
                        {
                            return false; // drawGhost = false
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
            while (doFall);

            return true; // drawGhost = true
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
                if (y[i] == HEIGHT + UP_SPARE - 1) // if height reaches the bottom
                {
                    return false;
                }

                if (x[i] >= WIDTH || y[i] + 1 >= HEIGHT + UP_SPARE) // if out of coords' range
                {
                    return false;
                }

                if (coords[x[i], y[i] + 1]) // if there's a placed tetromino below the falling tetromino
                {
                    return false;
                }
            }
            return true;
        }
        static bool GetAxisRaw(bool[,] coords, ConsoleKey input, byte[] x, byte[] y, byte WIDTH, sbyte direction)
        {
            // direction: -1 = left, 0 = none, 1 = right

            // check if left side or right side will be exceeded
            for (byte i = 0; i < BLOC; i++)
            {
                if (x[i] + direction < 0 || x[i] + direction > WIDTH - 1)
                {
                    return false;
                }
                else if (coords[x[i] + direction, y[i]])
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
                if (x[i] + deltaX[i] < 0 || x[i] + deltaX[i] >= WIDTH || y[i] >= HEIGHT) // if out of range
                {
                    return false;
                }
                else if (coords[x[i] + deltaX[i], y[i] + deltaY[i] + UP_SPARE]) // if hit placed tetromino
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
                for (byte h = 0; h < WIDTH; h++)
                {
                    SetCursorPosition(6 + (h * 2), v + 2 - UP_SPARE);
                    ForegroundColor = colours[h, v];
                    if (coords[h, v]) Write(BLOCK);
                    else Write(BLANK);
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
                    SetCursorPosition(6 + (x[i] * 2), y[i] + 2 - UP_SPARE);
                    Write(block);
                }
            }
        }
        static void DrawPreview(List<byte> tetros)
        {
            for (byte v = 0; v < 12; v += 5)
            {
                // clear previous preview
                SetCursorPosition(33, 4 + v);
                Write("        ");
                SetCursorPosition(33, 5 + v);
                Write("        ");
                SetCursorPosition(33, 4 + v);
                // draw current preview
                switch (tetros[(v / 5) + 1])
                {
                    case 0: // I
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Cyan;
                        Write(BLOCK + BLOCK + BLOCK + BLOCK);
                        break;

                    case 1: // L
                        if (settings.Colourful) ForegroundColor = ConsoleColor.DarkYellow;
                        Write(BLANK + BLANK + BLANK + BLOCK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;

                    case 2: // J
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Blue;
                        Write(BLANK + BLOCK + BLANK + BLANK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;

                    case 3: // T
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Magenta;
                        Write(BLANK + BLANK + BLOCK + BLANK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLOCK + BLOCK + BLOCK);
                        break;

                    case 4: // O
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Yellow;
                        Write(BLANK + BLOCK + BLOCK + BLANK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLOCK + BLOCK + BLANK);
                        break;

                    case 5: // S
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Green;
                        Write(BLANK + BLANK + BLOCK + BLOCK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLOCK + BLOCK + BLANK);
                        break;

                    case 6: // Z
                        if (settings.Colourful) ForegroundColor = ConsoleColor.Red;
                        Write(BLANK + BLOCK + BLOCK + BLANK);
                        SetCursorPosition(33, 5 + v);
                        Write(BLANK + BLANK + BLOCK + BLOCK);
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

            if (settings.Music != 0)
            {
                background.Stop();
                background2.Stop();
                gameOver.Play();
            }
            ForegroundColor = ConsoleColor.White;
            BackgroundColor = ConsoleColor.Black;

            for (byte i = 0; i < HEIGHT; i++)
            {
                SetCursorPosition(6, 2 + i);
                Write("████████████████████"); // curtain dropping
                Sleep.Uninterrupted(CURTAIN_DROP_PAUSE);
            }
            Sleep.Uninterrupted(PAUSE);

            SetCursorPosition(16, 6);
            WriteLine(@"
                                                
   ___   _   __  __ ___    _____   _____ ___    
  / __| /_\ |  \/  | __|  / _ \ \ / / __| _ \   
 | (_ |/ _ \| |\/| | _|  | (_) \ V /| _||   /   
  \___/_/ \_\_|  |_|___|  \___/ \_/ |___|_|_\   
                                                
                                                ");
            Sleep.Uninterrupted(PAUSE);

            ReadKey(true);
            Clear();
            Write("Press any key to continue  .\r\n                           T\r\n                          ( )\r\n                          <==>\r\n                           FJ\r\n                           ==\r\n                          J||F\r\n                          F||J\r\n                         /\\/\\/\\\r\n                         F++++J\r\n                        J{}{}{}F         .\r\n                     .  F{}{}{}J         T\r\n          .          T J{}{}{}{}F        ;;\r\n          T         /|\\F \\/ \\/ \\J  .   ,;;;;.\r\n         /:\\      .'/|\\\\:========F T ./;;;;;;\\\r\n       ./:/:/.   ///|||\\\\\\\"\"\"\"\"\"\" /x\\T\\;;;;;;/\r\n      //:/:/:/\\  \\\\\\\\|////..[]...xXXXx.|====|\r\n      \\:/:/:/:T7 :.:.:.:.:||[]|/xXXXXXx\\|||||\r\n      ::.:.:.:A. `;:;:;:;'=====\\XXXXXXX/=====.\r\n      `;\"\"::/xxx\\.|,|,|,| ( )( )| | | |.=..=.|\r\n       :. :`\\xxx/(_)(_)(_) _  _ | | | |'-''-'|\r\n       :T-'-.:\"\":|\"\"\"\"\"\"\"|/ \\/ \\|=====|======|\r\n       .A.\"\"\"||_|| ,. .. || || |/\\/\\/\\/ | | ||\r\n   :;:////\\:::.'.| || || ||-||-|/\\/\\/\\+|+| | |\r\n  ;:;;\\////::::,='======='============/\\/\\=====.\r\n:;:::;\"\"\":::::;:|__..,__|===========/||\\|\\====|\r\n:::::;|=:::;:;::|,;:::::         |========|   |\r\n::l42::::::(}:::::;::::::________|========|___|__");
            // https://www.asciiart.eu/buildings-and-places/monuments/other
            if (settings.Music != 0) ending.Play();

            ReadKey(true);
            Clear();

            if (scoreboard.Names.Count < MAX_QUANTITY)
            {
                WriteLine(@"



            Would you like to Record your Score?

             Yes
             No
");
                SetCursorPosition(49, 4);
                Write($"({score})");
                if (scoreboard.Scores.Count != 0)
                {
                    if (scoreboard.Scores[0] <= score)
                    {
                        SetCursorPosition(CURSOR_X, 3);
                        Write("New High Score! Congratulations!");
                        if (settings.Music != 0)
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
                    SetCursorPosition(CURSOR_X, y);
                    Write(">");
                    input = ReadKey(true).Key;
                    SetCursorPosition(CURSOR_X, y);
                    Write(" ");
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
                    Clear();
                    SetCursorPosition(CURSOR_X, 4);
                    Write("Please Enter Your Name: ");
                    char charEntered;
                    string name = "";
                    do
                    {
                        charEntered = ReadKey(true).KeyChar;
                        if (charEntered == '\b')
                        {
                            if (name.Length > 0)
                            {
                                name = name.Remove(name.Length - 1);
                                CursorLeft--;
                                Write(" ");
                                CursorLeft--;
                            }
                        }
                        else if (name.Length <= MAX_LENGTH && charEntered != ' ')
                        {
                            name += Convert.ToString(charEntered);
                            Write(charEntered);
                        }
                    }
                    while (charEntered != '\r');

                    if (name == "\r") name = "Anonymous";
                    else name = name.Remove(name.Length - 1); // the last char detected is "enter", therefore it needs to be removed

                    Clear();
                    WriteLine("Saving . . .");

                    scoreboard.Save(name, score);

                    Clear();
                    WriteLine("Saved");
                    Thread.Sleep(PAUSE);
                }
            }
        }

        #endregion


    }
}