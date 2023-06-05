using static System.Console;

namespace Tetris
{
    public class Scoreboard
    {
        private static List<string> names = new();
        private static List<int> scores = new();

        private int _pause;

        public Scoreboard(int pause_)
        {
            _pause = pause_;
        }

        public List<string> Names()
        {
            return names;
        }

        public List<int> Scores()
        {
            return scores;
        }

        public void ScoreboardPlaceholder()
        {
            Clear();
            WriteLine(@"
      ___                 _                      _ 
     / __| __ ___ _ _ ___| |__  ___  __ _ _ _ __| |
     \__ \/ _/ _ \ '_/ -_) '_ \/ _ \/ _` | '_/ _` |
     |___/\__\___/_| \___|_.__/\___/\__,_|_| \__,_|
                                               
");
            SetCursorPosition(8, 7);
            Write("Name");
            SetCursorPosition(32, 7);
            Write("Score");

            for (byte i = 0; i < names.Count; i++)
            {
                SetCursorPosition(8, i + 8);
                Write(names[i]);
                SetCursorPosition(32, i + 8);
                Write(scores[i]);
            }
            ReadKey(true);
        }
        public void Load()
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
                        Clear();
                        WriteLine(@"
    You just changed data in the scoreboard,
    And now there are too much data to process.
    The scoreboard is cleared now,
    You just need to restart the game to play.

");
                        Thread.Sleep(_pause);
                        WriteLine("    Never edit the game file again");
                        for (byte i = 0; i < 255; i++)
                        {
                            ReadKey(true);
                        }
                    }
                }
                catch // if the file corrupted
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "saves\\Scoreboard.txt", "");
                }
            }
        }

    }
}
