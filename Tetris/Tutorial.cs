using static System.Console;

namespace Tetris
{
    public class Tutorial
    {
        private List<ConsoleKey> _upInput;
        private List<ConsoleKey> _downInput;
        private List<ConsoleKey> _confirmInput;
        private List<ConsoleKey> _exitInput;
        private static int pause;
        private static int refreshRate;
        private static Settings settings;

        public Tutorial(List<ConsoleKey> upInput_, List<ConsoleKey> downInput_, List<ConsoleKey> confirmInput_, List<ConsoleKey> exitInput_, int pause_, int refreshRate_, Settings settings_)
        {
            _upInput = upInput_;
            _downInput = downInput_;
            _confirmInput = confirmInput_;
            _exitInput = exitInput_;
            pause = pause_;
            refreshRate = refreshRate_;
            settings = settings_;
        }

        public void Display()
        {
            Clear();
            ForegroundColor = ConsoleColor.White;
            ConsoleKey input;
            bool page1 = false; // this will be true if the player has seen page1 already
            byte page = 0;

            Page0();

            while (true)
            {
                while (KeyAvailable)
                {
                    input = ReadKey(true).Key;

                    if (_upInput.Contains(input) && page >= 1)
                    {
                        page--;
                    }
                    else if (_downInput.Contains(input) || _confirmInput.Contains(input))
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
                    else if (_exitInput.Contains(input))
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

                Thread.Sleep(refreshRate);
            }
        }
        private static void Page0()
        {
            Clear();
            Write(@"0
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
        private static bool Page1(bool animationPlayed)
        {
            ushort pauseBetweenAnimation;

            if (animationPlayed == true)
            {
                pauseBetweenAnimation = 0;
            }
            else
            {
                pauseBetweenAnimation = (ushort)(pause / 2);
            }

            byte x = 4, y = 10; // x and y level for showing tetrominoes, used for setting CURSOR location
            byte xIncrement = 12;
            Clear();
            Write(@"1
    The goal of this game is to earn as much score as possible. To earn score,
    you need to control the ");
            HighlightWord(" tetrominoes ");
            Write(@" so that they fill a complete line.
    Once a line is filled, it will be cleared automatically, and you 
    will earn score depending on how many line you cleared at a time.
    If your");
            HighlightWord(" tetromino ");
            Write(@" lands and touches the ceiling, the game ends.


    Here are the seven types of ");
            HighlightWord(" tetrominoes ");
            WriteLine(" , each consisting of 4 blocks");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Cyan;
            SetCursorPosition(x, y + 2);
            Write("████████");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.DarkYellow;
            x += xIncrement;
            SetCursorPosition(x, y);
            Write("██");
            SetCursorPosition(x, y + 1);
            Write("██");
            SetCursorPosition(x, y + 2);
            Write("████");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Green;
            x += xIncrement;
            SetCursorPosition(x, y + 2);
            Write("████");
            SetCursorPosition(x, y + 1);
            Write("  ████");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Magenta;
            x += xIncrement;
            SetCursorPosition(x, y + 2);
            Write("██████");
            SetCursorPosition(x, y + 1);
            Write("  ██  ");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Red;
            x += xIncrement;
            SetCursorPosition(x, y + 2);
            Write("  ████");
            SetCursorPosition(x, y + 1);
            Write("████");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Blue;
            x += xIncrement;
            SetCursorPosition(x, y);
            Write("  ██");
            SetCursorPosition(x, y + 1);
            Write("  ██");
            SetCursorPosition(x, y + 2);
            Write("████");

            Sleep.Uninterrupted(pauseBetweenAnimation);
            if (settings.Colourful) ForegroundColor = ConsoleColor.Yellow;
            x += xIncrement;
            SetCursorPosition(x, y + 2);
            Write("████");
            SetCursorPosition(x, y + 1);
            Write("████");

            ForegroundColor = ConsoleColor.White;
            return true;
        }
        private static void Page2()
        {
            Clear();
            Write(@"2
    Here's the list of controls:
    A, D or <, > to move left and right
    W or ^ or Z or X to rotate clockwise
    S or v to increasing falling speed
    Spacebar to instantly drop
    Esc or P to pause the game
    Backspace to exit the game");
            WriteLine(@"
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
        private static void Page2A()
        {
            byte tick = 0;
            byte x = 4, y = 8; // x and y of the upper-left keyboard corner
            SetCursorPosition(10, y + 10);
            Write("'N' to finish             ");
            ConsoleKey input;
            while (true)
            {
                while (KeyAvailable)
                {
                    input = ReadKey(true).Key;
                    if (input == ConsoleKey.Z)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 8, y + 6);
                        Write("[Z]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.X)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 11, y + 6);
                        Write("[X]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.A)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 7, y + 5);
                        Write("[A]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Move Left                    ");
                    }
                    else if (input == ConsoleKey.D)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 13, y + 5);
                        Write("[D]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Move Right                   ");
                    }
                    else if (input == ConsoleKey.W)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 9, y + 4);
                        Write("[W]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.S)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 10, y + 5);
                        Write("[S]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Drop Soft                    ");
                    }
                    else if (input == ConsoleKey.Spacebar)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 11, y + 7);
                        Write("[________________________]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Drop Hard                    ");
                    }
                    else if (input == ConsoleKey.LeftArrow)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 47, y + 7);
                        Write("[<]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Move Left                    ");
                    }
                    else if (input == ConsoleKey.RightArrow)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 53, y + 7);
                        Write("[>]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Move Right                   ");
                    }
                    else if (input == ConsoleKey.UpArrow)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 50, y + 6);
                        Write("[^]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Rotate Clockwise             ");
                    }
                    else if (input == ConsoleKey.DownArrow)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 50, y + 7);
                        Write("[v]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Drop Soft                    ");
                    }
                    else if (input == ConsoleKey.Escape)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 2, y + 1);
                        Write("[Esc]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Pause                        ");
                    }
                    else if (input == ConsoleKey.P)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 33, y + 4);
                        Write("[P]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Pause                        ");
                    }
                    else if (input == ConsoleKey.Backspace)
                    {
                        BackgroundColor = ConsoleColor.DarkGray;
                        ForegroundColor = ConsoleColor.Black;
                        SetCursorPosition(x + 41, y + 3);
                        Write("[_<_]");
                        BackgroundColor = ConsoleColor.Black;
                        ForegroundColor = ConsoleColor.White;
                        SetCursorPosition(x, y + 9);
                        WriteLine("Exit                         ");

                    }
                    else if (input == ConsoleKey.N)
                    {
                        return;
                    }
                }

                Thread.Sleep(refreshRate);

                tick++;

                if (tick == 28)
                {
                    SetCursorPosition(0, y - 1);
                    WriteLine(@"
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
        private static void Page3()
        {
            Clear();
            Write(@"3           LEVEL 0 <- Current level
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
        private static void Page4()
        {
            Clear();
            Write("4                         <==>\r\n                           FJ\r\n                           ==\r\n                          J||F\r\n                          F||J\r\n                         /\\/\\/\\\r\n                         F++++J\r\n                        J{}{}{}F         .\r\n                     .  F{}{}{}J         T\r\n          .          T J{}{}{}{}F        ;;\r\n          T         /|\\F \\/ \\/ \\J  .   ,;;;;.\r\n         /:\\      .'/|\\\\:========F T ./;;;;;;\\\r\n       ./:/:/.   ///|||\\\\\\\"\"\"\"\"\"\" /x\\T\\;;;;;;/\r\n      //:/:/:/\\  \\\\\\\\|////..[]...xXXXx.|====|\r\n      \\:/:/:/:T7 :.:.:.:.:||[]|/xXXXXXx\\|||||\r\n      ::.:.:.:A. `;:;:;:;'=====\\XXXXXXX/=====.\r\n      `;\"\"::/xxx\\.|,|,|,| ( )( )| | | |.=..=.|\r\n       :. :`\\xxx/(_)(_)(_) _  _ | | | |'-''-'|\r\n       :T-'-.:\"\":|\"\"\"\"\"\"\"|/ \\/ \\|=====|======|\r\n       .A.\"\"\"||_|| ,. .. || || |/\\/\\/\\/ | | ||\r\n   :;:////\\:::.'.| || || ||-||-|/\\/\\/\\+|+| | |\r\n  ;:;;\\////::::,='======='============/\\/\\=====.\r\n:;:::;\"\"\":::::;:|__..,__|===========/||\\|\\====|\r\n:::::;|=:::;:;::|,;:::::         |========|   |\r\n::l42::::::(}:::::;::::::________|========|___|__");
            // https://www.asciiart.eu/buildings-and-places/monuments/other
            WriteLine();
            WriteLine(@"
    Don't forget to record your score after playing!

    Good Luck Have Fun!");
            // Alexey Leonidovich Pajitnov
            // 田中 宏和
            // Пётр Ильи́ч Чайко́вский
            // Alexandre César Léopold Bizet
        }
        private static void HighlightWord(string word)
        {
            BackgroundColor = ConsoleColor.White;
            ForegroundColor = ConsoleColor.Black;
            Write(word); // highlighting the word
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
        }
    }
}
