using static System.Console;

namespace Tetris
{
    public class Scoreboard
    {
        private List<string> _names = new();
        private List<int> _scores = new();

        private const int NAME_LEFT = 8;
        private const int SCORE_LEFT = 32;

        private string _scoreboardPath;

        private int _pause;

        public Scoreboard(int pause_, string scoreboardPath_)
        {
            _pause = pause_;
            _scoreboardPath = scoreboardPath_;
        }

        public List<string> Names
        {
            get { return _names; }
        }

        public List<int> Scores
        {
            get { return _scores; }
        }

        public void Display()
        {
            Clear();
            WriteLine(@"
      ___                 _                      _ 
     / __| __ ___ _ _ ___| |__  ___  __ _ _ _ __| |
     \__ \/ _/ _ \ '_/ -_) '_ \/ _ \/ _` | '_/ _` |
     |___/\__\___/_| \___|_.__/\___/\__,_|_| \__,_|
                                               
");

            CursorLeft = NAME_LEFT;
            Write("Name");
            CursorLeft = SCORE_LEFT;
            WriteLine("Score");

            int startTop = CursorTop;

            for (byte i = 0; i < _names.Count; i++)
            {
                SetCursorPosition(NAME_LEFT, i + startTop);
                Write(_names[i]);
                SetCursorPosition(SCORE_LEFT, i + startTop);
                Write(_scores[i]);
            }

            ReadKey(true);
        }
        public void Load()
        {
            string scoreboardData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath);
            if (scoreboardData != String.Empty) // if it's not empty
            {
                try
                {
                    string[] scoreboardLines = scoreboardData.Split("\n"); // divide into each line
                    Array.Resize(ref scoreboardLines, scoreboardLines.Length - 1); // the last line is empty, remove it
                    string[] eachNameAndScore;

                    for (byte i = 0; i < scoreboardLines.Length; i++)
                    {
                        eachNameAndScore = scoreboardLines[i].Split(' '); // divide into mixed _names and _scores
                        _names.Add(eachNameAndScore[0]);
                        _scores.Add(Convert.ToInt32(eachNameAndScore[1]));
                    }

                    if (_names.Count > 255)
                    {
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath, String.Empty);
                        Clear();
                        WriteLine(@"
    external file edit detected in scoreboard.txt
    name & score overflow
    scoreboard cleared
    restart required

");
                        Sleep.Uninterrupted(300000);

                    }
                }
                catch // if the file corrupted
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath, String.Empty);
                }
            }
        }

        public void Save(string name, int score)
        {
            if (Names.Count != 0)
            {
                int i = 0;

                while (score < Scores[i] && i < Names.Count)
                    i++;

                Names.Insert(i, name);
                Scores.Insert(i, score);
            }
            else
            {
                Names.Add(name);
                Scores.Add(score);
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath, String.Empty);

            for (byte i = 0; i < Names.Count; i++)
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + _scoreboardPath, $"{Names[i]} {Scores[i]}\n");
        }
    }
}
