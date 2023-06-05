using static System.Console;

namespace Tetris
{
    public class Settings
    {
        private byte _music; // whether play music and sounds or not / which piece to play
        private bool _invertMovement; // whether invert left and right movement keys
        private bool _showGhostPiece; // whether show ghost pieces
        private bool _colourful; // whether black and white or colourful

        private List<ConsoleKey> _upInput;
        private List<ConsoleKey> _downInput;
        private List<ConsoleKey> _confirmInput;
        private List<ConsoleKey> _exitInput;
        private int _pause;
        private int _refreshRate;

        private string _settingsPath;
        private string _scoreboardPath;

        private static string CURSOR = ">";
        private static string CURSOR_SPACE;
        private static byte CURSOR_X_POS = 3;

        private static string CONFIRM_TEXT = "Are you sure? (Y/N)";
        private static string CONFIRM_TEXT_SPACE;
        private static byte CONFIRM_TEXT_X_POS = 24;

        private static byte MIN_CURSOR_Y = 8;
        private static byte MAX_CURSOR_Y = 13;

        public Settings(List<ConsoleKey> upInput_, List<ConsoleKey> downInput_, List<ConsoleKey> confirmInput_, List<ConsoleKey> exitInput_, int pause_, int refreshRate_, string settingsPath_, string scoreboardPath_)
        {
            _upInput = upInput_;
            _downInput = downInput_;
            _confirmInput = confirmInput_;
            _exitInput = exitInput_;
            _pause = pause_;
            _refreshRate = refreshRate_;
            CONFIRM_TEXT_SPACE = ConvertToSpace(CONFIRM_TEXT);
            CURSOR_SPACE = ConvertToSpace(CURSOR);
            _settingsPath = settingsPath_;
            _scoreboardPath = scoreboardPath_;
        }

        public byte Music
        {
            get { return _music; }
        }

        public bool InvertMovement
        {
            get { return _invertMovement; }
        }

        public bool ShowGhostPiece
        {
            get { return _showGhostPiece; }
        }

        public bool Colourful
        {
            get { return _colourful; }
        }

        public void Init()
        {
            byte cursorY = MIN_CURSOR_Y;
            byte selection = (byte) (cursorY - MIN_CURSOR_Y);
            ConsoleKey input;

            PrintMenu();

            ReadKey(true);

            while (true)
            {
                while (KeyAvailable)
                {
                    SetCursorPosition(CURSOR_X_POS, cursorY);
                    Write(CURSOR_SPACE);

                    input = ReadKey(true).Key;

                    if (_upInput.Contains(input) && cursorY > MIN_CURSOR_Y)
                    {
                        cursorY--;
                    }
                    else if (_downInput.Contains(input) && cursorY < MAX_CURSOR_Y)
                    {
                        cursorY++;
                    }
                    else if (_confirmInput.Contains(input))
                    {
                        selection = (byte)(cursorY - MIN_CURSOR_Y);

                        switch (selection)
                        {
                            case 0:
                                _music++;

                                if (_music == 4)
                                    _music = 0;
                                break;

                            case 1:
                                _invertMovement = !_invertMovement;
                                break;

                            case 2:
                                _showGhostPiece = !_showGhostPiece;
                                break;

                            case 3:
                                _colourful = !_colourful;
                                break;

                            case 4: // default settings
                                if (ConfirmChange(cursorY))
                                {
                                    DefaultSettings();
                                    Update();
                                }

                                UpdateVisual();

                                break;

                            case 5: // clear scoreboard
                                if (ConfirmChange(cursorY))
                                {
                                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath, String.Empty);

                                    Clear();
                                    WriteLine("Exiting to clear the scoreboard");
                                    Thread.Sleep(_pause * 2);

                                    Environment.Exit(0);
                                }

                                break;
                        }

                        UpdateVisual(selection);
                    }
                    else if (_exitInput.Contains(input))
                    {
                        Update();

                        Clear();
                        WriteLine("Saved");
                        Thread.Sleep(_pause);

                        return;
                    }
                }

                SetCursorPosition(CURSOR_X_POS, cursorY);
                Write(CURSOR);

                Thread.Sleep(_refreshRate);
            }
        }

        public void UpdateVisual(byte? selection = null)
        {
            byte j; // for a loop

            if (selection == null)
            {
                j = 3;
                selection = 0;
            }
            else
                j = 1;

            for (byte i = 0; i < j; i++)
            {
                switch (selection)
                {
                    case 0:
                        SetCursorPosition(23, 8);
                        switch (_music)
                        {
                            case 0: Write("None   "); break;
                            default: Write($"Music {_music}"); break;
                        }
                        break;

                    case 1:
                        SetCursorPosition(27, 9);
                        switch (_invertMovement)
                        {
                            case true: Write("True "); break;
                            case false: Write("False"); break;
                        }
                        break;

                    case 2:
                        SetCursorPosition(24, 10);
                        switch (_showGhostPiece)
                        {
                            case true: Write("True "); break;
                            case false: Write("False"); break;
                        }
                        break;

                    case 3:
                        SetCursorPosition(14, 11);
                        switch (_colourful)
                        {
                            case true: Write("multicoloured"); break;
                            case false: Write("dichromatic  "); break;
                        }
                        break;
                }

                selection++;
            }
        }

        public void Load()
        {
            try
            {
                string settingsData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + _settingsPath);
                _music = Convert.ToByte(Convert.ToString(settingsData[0]));
                _invertMovement = Convert.ToBoolean(Convert.ToByte(settingsData[1]));
                _showGhostPiece = Convert.ToBoolean(Convert.ToByte(settingsData[2]));
                _colourful = Convert.ToBoolean(Convert.ToByte(settingsData[3]));
            }
            catch (Exception e) // if the file corrupted
            {
                DefaultSettings();
                WriteLine(@$"
    Unable to load settings, reverted to default settings
    Error message: {e.ToString()}

    Press any Key to continue . . .");

                ReadKey(true);

                Update();
            }
        }

        private void Update()
        {
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + _settingsPath, $"{_music}{Convert.ToByte(_invertMovement)}{Convert.ToByte(_showGhostPiece)}{Convert.ToByte(_colourful)}");
        }

        private void DefaultSettings()
        {
            _music = 1; // music 1
            _invertMovement = false; // no input inverse
            _showGhostPiece = false; // no ghost block display
            _colourful = true; // multicoloured
        }

        private string ConvertToSpace(string str)
        {
            string returnStr = String.Empty;

            for (int i = 0; i < str.Length; i++)
                returnStr += " ";

            return returnStr;
        }

        private bool ConfirmChange(byte cursorY)
        {
            const ConsoleKey CONFIRM_KEY = ConsoleKey.Y;

            SetCursorPosition(CONFIRM_TEXT_X_POS, cursorY);
            Write(CONFIRM_TEXT);

            bool returnValue = (ReadKey(true).Key == CONFIRM_KEY);

            SetCursorPosition(CONFIRM_TEXT_X_POS, cursorY);
            Write(CONFIRM_TEXT_SPACE);

            return returnValue;
        }

        private void PrintMenu()
        {
            Clear();
            WriteLine(@"
      ___      _   _   _              
     / __| ___| |_| |_(_)_ _  __ _ ___
     \__ \/ -_)  _|  _| | ' \/ _` (_-<
     |___/\___|\__|\__|_|_||_\__, /__/
                             |___/    
");
            WriteLine(@"
     Background Music: 
     Invert Movement Keys: 
     Show Ghost Pieces: 
     Colours: 
     [ Set to default ]
     [Clear Scoreboard]

     Press Space or Enter to change options
     Press ESC or BackSpace to return and save


     Some changes will only be applied after the game restarts
    ");

            UpdateVisual();
        }
    }
}
