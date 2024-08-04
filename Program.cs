using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Tetris
{
    class Program
    {
        const int M = 20;
        const int N = 10;

        static int[,] field = new int[M, N];

        struct Point
        {
            public int x, y;
        }

        static Point[] a = new Point[4];
        static Point[] b = new Point[4];

        static int[,] figures = new int[7, 4]
        {
            { 1, 3, 5, 7 }, // I
            { 2, 4, 5, 7 }, // Z
            { 3, 5, 4, 6 }, // S
            { 3, 5, 4, 7 }, // T
            { 2, 3, 5, 7 }, // L
            { 3, 5, 7, 6 }, // J
            { 2, 3, 4, 5 }  // O
        };

        static bool escapeButtonStatus = false;
        static bool pause = false;
        static uint score = 0;

        static bool Check()
        {
            for (int i = 0; i < 4; i++)
            {
                if (a[i].x < 0 || a[i].x >= N || a[i].y >= M)
                {
                    return false;
                }
                if (field[a[i].y, a[i].x] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        static void Main(string[] args)
        {
            Random rand = new Random();
            RenderWindow window = new RenderWindow(new VideoMode(320, 480), "The Game!");

            Texture t1 = new Texture("images/tiles.png");
            Texture t2 = new Texture("images/background.png");
            Texture t3 = new Texture("images/frame.png");

            Sprite s = new Sprite(t1);
            Sprite background = new Sprite(t2);
            Sprite frame = new Sprite(t3);

            int dx = 0;
            bool rotate = false;
            int colorNum = 1;
            float timer = 0, delay = 0.3f;

            Clock clock = new Clock();

            {
                int firstFigure = rand.Next(7);
                for (int i = 0; i < 4; i++)
                {
                    a[i].x = figures[firstFigure, i] % 2;
                    a[i].y = figures[firstFigure, i] / 2;
                }
            }

            while (window.IsOpen)
            {
                if (!pause || !escapeButtonStatus)
                    timer += clock.Restart().AsSeconds();

                window.DispatchEvents();

                window.Closed += (sender, e) => window.Close();
                window.LostFocus += (sender, e) => pause = true;
                window.GainedFocus += (sender, e) => pause = false;
                window.KeyPressed += (sender, e) =>
                {
                    if (e.Code == Keyboard.Key.Escape) escapeButtonStatus = !escapeButtonStatus;
                    else if (e.Code == Keyboard.Key.Up) rotate = true;
                    else if (e.Code == Keyboard.Key.Left) dx = -1;
                    else if (e.Code == Keyboard.Key.Right) dx = 1;
                };

                if (pause || escapeButtonStatus)
                {
                    System.Threading.Thread.Sleep(250);
                    continue;
                }

                if (Keyboard.IsKeyPressed(Keyboard.Key.Down)) delay = 0.05f;

                // Move
                for (int i = 0; i < 4; i++) { b[i] = a[i]; a[i].x += dx; }
                if (!Check()) for (int i = 0; i < 4; i++) a[i] = b[i];

                // Rotate
                if (rotate)
                {
                    Point p = a[1]; // center of rotation
                    for (int i = 0; i < 4; i++)
                    {
                        int x = a[i].y - p.y;
                        int y = a[i].x - p.x;
                        a[i].x = p.x - x;
                        a[i].y = p.y + y;
                    }
                    if (!Check()) for (int i = 0; i < 4; i++) a[i] = b[i];
                }

                // Tick
                if (timer >= delay)
                {
                    for (int i = 0; i < 4; i++) { b[i] = a[i]; a[i].y += 1; }

                    if (!Check())
                    {
                        for (int i = 0; i < 4; i++) field[b[i].y, b[i].x] = colorNum;

                        colorNum = 1 + rand.Next(7);
                        int n = rand.Next(7);
                        for (int i = 0; i < 4; i++)
                        {
                            a[i].x = figures[n, i] % 2;
                            a[i].y = figures[n, i] / 2;
                        }
                        if (!Check())
                        {
                            window.Close();
                            break;
                        }
                    }

                    timer = 0;
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                }

                // Check lines
                int k = M - 1;
                for (int i = M - 1; i > 0; i--)
                {
                    int count = 0;
                    for (int j = 0; j < N; j++)
                    {
                        if (field[i, j] != 0)
                        {
                            count++;
                        }
                        field[k, j] = field[i, j];
                    }
                    if (count < N)
                    {
                        k--;
                    }
                    else
                    {
                        // increase user's score
                        score++;
                    }
                }
                if (k != 0)
                {
                    Array.Clear(field, 0, N * (k + 1));
                }

                dx = 0; rotate = false; delay = 0.3f;

                // Draw
                window.Clear(Color.White);
                window.Draw(background);

                for (int i = 0; i < M; i++)
                    for (int j = 0; j < N; j++)
                    {
                        if (field[i, j] == 0) continue;
                        s.TextureRect = new IntRect(field[i, j] * 18, 0, 18, 18);
                        s.Position = new Vector2f(j * 18, i * 18);
                        s.Position += new Vector2f(28, 31); // offset
                        window.Draw(s);
                    }

                for (int i = 0; i < 4; i++)
                {
                    s.TextureRect = new IntRect(colorNum * 18, 0, 18, 18);
                    s.Position = new Vector2f(a[i].x * 18, a[i].y * 18);
                    s.Position += new Vector2f(28, 31); // offset
                    window.Draw(s);
                }

                window.Draw(frame);
                window.Display();
            }

            Console.WriteLine("Game Over");
            Console.WriteLine("Your score: " + score);
        }
    }
}
