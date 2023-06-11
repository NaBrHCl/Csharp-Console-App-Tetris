using static System.Console;

namespace Tetris
{
    public class Sleep
    {
        public static void Uninterrupted(int pause)
        {
            Thread.Sleep(pause);

            if (KeyAvailable)
                ReadKey(true);
        }
    }
}
