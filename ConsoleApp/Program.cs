using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;
namespace ConsoleApp
{
    public static class Settings
    {
        public const string Element = "██";
        public const string Empty = "  ";
        public static int Speed = 500;
        public const int Height = 20;
        public const int Width = 20;
    }
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public class Map
    {
        public static int Rows
        {
            get { return Settings.Height + 2; }
        }
        public static int Columns
        {
            get { return Settings.Width + 2; }
        }
        public (int i, int j) Food = (0, 0);
        public int HeightsScore = Settings.Width;
        public string[,] matrix = new string[0, 0];
        public List<(int i, int j)> walls = new List<(int, int)>();
        private static readonly Random random = new Random();
        public Map()
        {
            matrix = new string[Rows, Columns];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (i == 0 || i == Rows - 1 || j == 0 || j == Columns - 1)
                    {
                        matrix[i, j] = Settings.Element;
                        walls.Add((i, j));
                    }
                    else
                        matrix[i, j] = Settings.Empty;
                }
            }
        }
        public void Print(List<Snake> snakes)
        {
            int iq = 0;
            int jq = 0;
            var snakesCoords = snakes.Select(q => new { q.Player, q.snake });
            var heads = snakes.Select(q => q.Head);
            Console.Clear();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var point = matrix[i, j];
                    var temp = snakesCoords.FirstOrDefault(q => q.snake.Contains((i,j)));
                    if (temp != null)
                    {
                        if(heads.Contains((i,j)))
                        {
                            if (temp.Player == 1)
                                Console.ForegroundColor = ConsoleColor.Green;
                            else
                                Console.ForegroundColor = ConsoleColor.Blue;
                        }
                        else
                        {
                            if (temp.Player == 2)
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                            else
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                        }
                        Console.Write(point);
                    }
                    else if ((i, j) == Food)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(point);
                    }
                    else if (walls.Contains((i, j)))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(point);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(point);
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            for (int i = 0; i < snakes.Count; i++)
            {
                Console.WriteLine($"Score{i + 1}:{snakes[i].Score}");
            }
        }
        public void GenerateFood()
        {
            bool isReady = false;
            do
            {
                var x = random.Next(1, Rows - 2);
                var y = random.Next(1, Columns - 2);
                if (matrix[x, y] == Settings.Empty)
                {
                    isReady = true;
                    matrix[x, y] = Settings.Element;
                    Food = (x, y);
                }
            }
            while (!isReady);
        }
    }
    public class Snake
    {
        public ConsoleKey R;
        public ConsoleKey L;
        public ConsoleKey U;
        public ConsoleKey D;
        public int Player;
        public bool isGameOver = false;
        public int HeightsScore = Settings.Width;
        public (int i, int j) Head = (0, 0);
        public List<(int i, int j)> snake = new List<(int, int)>();
        public int Score { get; set; }
        public Direction direction = Direction.Right;
        public Snake(int x, int y, Direction direction, ConsoleKey r, ConsoleKey l, ConsoleKey u, ConsoleKey d, int player)
        {
            R = r;
            L = l;
            U = u;
            D = d;
            //matrix = new string[Rows, Columns];
            this.direction = direction;
            Score = 1;
            snake.Add((x, y));
            Player = player;
            //for (int i = 0; i < Rows; i++)
            //{
            //    for (int j = 0; j < Columns; j++)
            //    {
            //        if(i == 0 || i == Rows - 1 || j == 0 || j == Columns -1)
            //        {
            //            matrix[i, j] = Settings.Element;
            //            walls.Add((i, j));
            //        }
            //        else
            //            matrix[i,j] = Settings.Empty;
            //    }
            //}
            //matrix[x,y] = Settings.Element;
        }
        public void Move(Map map)
        {
            var currenH = snake[0];
            var newH = currenH;
            switch (direction)
            {
                case Direction.Left:
                    newH = (currenH.i, currenH.j -1);
                    break;
                case Direction.Right:
                    newH = (currenH.i, currenH.j + 1);
                    break;
                case Direction.Up:
                    newH = (currenH.i - 1, currenH.j);
                    break;
                case Direction.Down:
                    newH = (currenH.i + 1, currenH.j);
                    break;
                default:
                    break;
            }
            var tempPoint = map.matrix[newH.i, newH.j];
            if (snake.Contains((newH.i, newH.j)) || map.walls.Contains((newH.i, newH.j)))
            {
                isGameOver = true;
                return;
            }
            Head = newH;

            bool isGrow = false;
            if ((newH.i, newH.j) == map.Food)
            {
                isGrow = true;
                map.GenerateFood();
                Score++;
            }

            var oldH = snake[snake.Count-1];
            snake.Insert(0,newH);
            if (!isGrow)
            {
                snake.Remove(oldH);
                map.matrix[oldH.i, oldH.j] = Settings.Empty;
            }

            map.matrix[newH.i, newH.j] = Settings.Element;
        }
    }
    public class Program
    {
        static async Task Main(string[] args)
        {
            await Start();
        }
        static async Task Start()
        {
            List<Snake> snakes = new List<Snake>();
            snakes.Add(new Snake(1, 1, Direction.Right, ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.UpArrow, ConsoleKey.DownArrow, 1));
            snakes.Add(new Snake(Settings.Width, Settings.Height, Direction.Left, ConsoleKey.D, ConsoleKey.A, ConsoleKey.W, ConsoleKey.S, 2));
            Map map = new Map();
            map.GenerateFood();
            List<Task> tasks = new List<Task>();
            var game = Task.Run(() => Game(snakes, map));
            var paint = Task.Run(() => Paint(map, snakes));
            tasks.Add(game);
            tasks.Add(paint);
            tasks.Add(Task.Run(() => Player(snakes[0])));
            tasks.Add(Task.Run(() => Player(snakes[1])));
            await Task.WhenAll(tasks);
            Console.ReadKey();
        }
        static void Player(Snake snake)
        {
            while (true)
            {
                var button = Console.ReadKey(true).Key;
                if (snake.R == button && snake.direction != Direction.Left)
                {
                    snake.direction = Direction.Right;
                }
                else if (snake.L == button && snake.direction != Direction.Right)
                {
                    snake.direction = Direction.Left;
                }
                else if(snake.U == button && snake.direction != Direction.Down)
                {
                    snake.direction = Direction.Up;
                }
                else if (snake.D == button && snake.direction != Direction.Up)
                {
                    snake.direction = Direction.Down;
                }
                Thread.Sleep(1);
            }
        }
        static void Game(List<Snake> snakes, Map map)
        {
            while (true)
            {
                for (int i = 0; i < snakes.Count; i++)
                {
                    snakes[i].Move(map);
                }
                Thread.Sleep(Settings.Speed);
            }
        }
        static void Paint(Map map, List<Snake> snakes)
        {
            while (true) //(!snake.isGameOver)
            {
                map.Print(snakes);
                Thread.Sleep(500);
            }
        }
    }
}
