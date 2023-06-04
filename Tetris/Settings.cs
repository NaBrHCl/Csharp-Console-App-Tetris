using System.Runtime.CompilerServices;
using static System.Console;

namespace Tetris
{
    public class Settings
    {
        private byte _music; // whether play music and sounds or not / which piece to play
        private bool _invert; // whether invert left and right movement keys
        private bool _ghost; // whether show ghost pieces
        private bool _colour; // whether black and white or colourful

        private List<ConsoleKey> _upInput;
        private List<ConsoleKey> _downInput;
        private List<ConsoleKey> _confirmInput;
        private List<ConsoleKey> _exitInput;
        private int _pause;
        private int _refreshRate;

        private static string CURSOR = ">";
        private static string CURSOR_SPACE;
        private static byte CURSOR_X_POS = 3;

        private static string CONFIRM_TEXT = "Are you sure? (Y/N)";
        private static string CONFIRM_TEXT_SPACE;
        private static byte CONFIRM_TEXT_X_POS = 24;

        private static byte MIN_CURSOR_Y = 8;
        private static byte MAX_CURSOR_Y = 13;

        public Settings(List<ConsoleKey> upInput_, List<ConsoleKey> downInput_, List<ConsoleKey> confirmInput_, List<ConsoleKey> exitInput_, int pause_, int refreshRate_)
        {
            _upInput = upInput_;
            _downInput = downInput_;
            _confirmInput = confirmInput_;
            _exitInput = exitInput_;
            _pause = pause_;
            _refreshRate = refreshRate_;
            CONFIRM_TEXT_SPACE = ConvertToSpace(CONFIRM_TEXT);
            CURSOR_SPACE = ConvertToSpace(CURSOR);
        }

        public byte Music
        {
            get { return _music; }
        }

        public bool Invert
        {
            get { return _invert; }
        }

        public bool Ghost
        {
            get { return _ghost; }
        }

        public bool Colour
        {
            get { return _colour; }
        }

        public void Init()
        {
            byte selection;

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

            ConsoleKey input;
            ReadKey(true);
            byte cursorY = MIN_CURSOR_Y;
            selection = (byte)(cursorY - MIN_CURSOR_Y);
            UpdateVisual(selection);

            while (true)
            {
                while (KeyAvailable)
                {
                    input = ReadKey(true).Key;
                    SetCursorPosition(CURSOR_X_POS, cursorY);
                    Write(CURSOR_SPACE);

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
                            case 0: // music
                                _music++;

                                if (_music == 4)
                                    _music = 0;
                                break;

                            case 1: // invert
                                _invert = !_invert;
                                break;

                            case 2: // ghost
                                _ghost = !_ghost;
                                break;

                            case 3: // colour
                                _colour = !_colour;
                                break;

                            case 4: // default settings
                                if (ConfirmChange(cursorY))
                                {
                                    DefaultSettings();
                                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt", Convert.ToString(_music) + Convert.ToString(_invert) + Convert.ToString(_ghost) + Convert.ToString(_colour));
                                }

                                UpdateVisual();
                                break;

                            case 5: // clear scoreboard
                                if (ConfirmChange(cursorY))
                                {
                                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", String.Empty);
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

            ReadKey(true);
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
                        switch (_invert)
                        {
                            case true: Write("True "); break;
                            case false: Write("False"); break;
                        }
                        break;

                    case 2:
                        SetCursorPosition(24, 10);
                        switch (_ghost)
                        {
                            case true: Write("True "); break;
                            case false: Write("False"); break;
                        }
                        break;

                    case 3:
                        SetCursorPosition(14, 11);
                        switch (_colour)
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
                string settingsData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt");
                _music = Convert.ToByte(Convert.ToString(settingsData[0]));
                _invert = Convert.ToBoolean(Convert.ToByte(settingsData[1]));
                _ghost = Convert.ToBoolean(Convert.ToByte(settingsData[2]));
                _colour = Convert.ToBoolean(Convert.ToByte(settingsData[3]));
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
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Settings.txt", $"{_music}{Convert.ToByte(_invert)}{Convert.ToByte(_ghost)}{Convert.ToByte(_colour)}");
        }

        private void DefaultSettings()
        {
            _music = 1; // music 1
            _invert = false; // no input inverse
            _ghost = false; // no ghost block display
            _colour = true; // multicoloured
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
    }
}
