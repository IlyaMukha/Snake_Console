using System;
using System.Drawing;
namespace ConsoleApp
{
    public static class Settings
    {
        public const string Element = "██";
        public const string Empty = "  ";
        public static int Speed = 500;
        public const int Height = 11;
        public const int Width = 11;
    }
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    public class Snake
    {
        public static int Rows 
        {
            get { return Settings.Height + 2; }
        }
        public static int Columns
        {
            get { return Settings.Width + 2; }
        }
        public bool isGameOver = false;
        public int HeightsScore = Settings.Width;
        (int i, int j) Head = (0, 0);
        (int i, int j) Food = (0, 0);
        public static string[,] matrix = new string[0,0];
        private List<(int i, int j)> snake = new List<(int, int)>();
        private List<(int i ,int j)> walls = new List<(int, int)>();
        private static readonly Random random = new Random();
        public static int Score { get; set; }
        public Direction direction = Direction.Right;
        public Snake(int x, int y, Direction direction)
        {
            matrix = new string[Rows, Columns];
            this.direction = direction;
            Score = 1;
            snake.Add((x, y));
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if(i == 0 || i == Rows - 1 || j == 0 || j == Columns -1)
                    {
                        matrix[i, j] = Settings.Element;
                        walls.Add((i, j));
                    }
                    else
                        matrix[i,j] = Settings.Empty;
                }
            }
            matrix[x,y] = Settings.Element;
        }
        public void Print()
        {
            int iq = 0;
            int jq = 0;
            Console.Clear();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var point = matrix[i,j];
                    if (snake.Contains((i,j)))
                    {
                        if((i,j) == Head)
                            Console.ForegroundColor = ConsoleColor.Green;
                        else
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(point);
                    }
                    else if ((i,j) == Food)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(point);
                    }
                    else if (walls.Contains((i,j)))
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
            Console.WriteLine($"Score:{Score}");
        }
        public void Move()
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
            var tempPoint = matrix[newH.i, newH.j];
            if (snake.Contains((newH.i, newH.j)) || walls.Contains((newH.i, newH.j)))
            {
                isGameOver = true;
                return;
            }
            Head = newH;

            bool isGrow = false;
            if ((newH.i, newH.j) == Food)
            {
                isGrow = true;
                GenerateFood();
                Score++;
            }

            var oldH = snake[snake.Count-1];
            snake.Insert(0,newH);
            if (!isGrow)
            {
                snake.Remove(oldH);
                matrix[oldH.i, oldH.j] = Settings.Empty;
            }

            matrix[newH.i, newH.j] = Settings.Element;
        }
        public void GenerateFood()
        {
            bool isReady = false;
            do
            {
                var x = random.Next(1, Rows - 2);
                var y = random.Next(1, Columns - 2);
                if (matrix[x,y] == Settings.Empty)
                {
                    isReady = true;
                    matrix[x,y] = Settings.Element;
                    Food = (x, y);
                }
            }
            while (!isReady && Score != HeightsScore);
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
            Snake snake = new Snake(11, 11, Direction.Left);
            snake.GenerateFood();
            var task1 = Task.Run(() => Game(snake));
            var task2 = Task.Run(() => Select(snake));
            var task3 = Task.Run(() => Paint(snake));
            await Task.WhenAll(task1, task2, task3);
            Console.ReadKey();
        }
        static void Select(Snake snake)
        {
            while (!snake.isGameOver)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.RightArrow:
                        if (snake.direction != Direction.Left)
                            snake.direction = Direction.Right;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (snake.direction != Direction.Right)
                            snake.direction = Direction.Left;
                        break;
                    case ConsoleKey.UpArrow:
                        if (snake.direction != Direction.Down)
                            snake.direction = Direction.Up;
                        break;
                    case ConsoleKey.DownArrow:
                        if (snake.direction != Direction.Up)
                            snake.direction = Direction.Down;
                        break;
                }
                Thread.Sleep(1);
            }
           
        }
        static void Game(Snake snake)
        {
            while (!snake.isGameOver)
            {                
                snake.Move();
                //Settings.Speed = Settings.Speed - (int)Math.Pow(Snake.Score,2);
                Thread.Sleep(Settings.Speed);
            }
        }
        static void Paint(Snake snake)
        {
            while (!snake.isGameOver)
            {
                snake.Print();
                Thread.Sleep(500);
            }
        }
    }
}
