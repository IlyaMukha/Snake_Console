using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
    public static class Colors
    {
        public static Color Red { get { return new Color(ConsoleColor.Red, ConsoleColor.DarkRed); } }
        public static Color Blue { get { return new Color(ConsoleColor.Blue, ConsoleColor.DarkBlue); } }
        public static Color Green { get { return new Color(ConsoleColor.Green, ConsoleColor.DarkGreen); } }
        public static Color Yellow { get { return new Color(ConsoleColor.Yellow, ConsoleColor.DarkYellow); } }
        public static ConsoleColor Wall { get { return ConsoleColor.White; } }
        public static ConsoleColor Food { get { return ConsoleColor.Cyan; } }

    }
    public static class Controls
    {
        public static List<ConsoleKey> Arrows
        {
            get { return new List<ConsoleKey>() { ConsoleKey.RightArrow, ConsoleKey.LeftArrow, ConsoleKey.UpArrow, ConsoleKey.DownArrow }; }
        }
        public static List<ConsoleKey> WASD
        {
            get { return new List<ConsoleKey>() { ConsoleKey.D, ConsoleKey.A, ConsoleKey.W, ConsoleKey.S }; }
        }
    }
    public static class StartPoint
    {
        public static (int i, int j) LeftTop = (1, 1);
        public static (int i, int j) LeftBottom = (Settings.Width, 1);
        public static (int i, int j) RightTop = (1, Settings.Height);
        public static (int i, int j) RightBottom = (Settings.Height, Settings.Width);
    }
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }
    public class Color
    {
        public ConsoleColor MainColor {  get; set; }
        public ConsoleColor ThecondColor { get; set; }

        public Color(ConsoleColor main, ConsoleColor thecond)
        {
            MainColor = main;
            ThecondColor = thecond;
        }
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
            var snakesCoords = snakes.Select(q => new {q.snake, q.Color });
            var heads = snakes.Select(q => q.Head);
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var point = matrix[i, j];
                    var temp = snakesCoords.FirstOrDefault(q => q.snake.Contains((i,j)));
                    if (temp != null)
                    {
                        if(heads.Contains((i,j)))
                            Console.ForegroundColor = temp.Color.MainColor;
                        else
                            Console.ForegroundColor = temp.Color.ThecondColor;
                        Console.Write(point);
                    }
                    else if ((i, j) == Food)
                    {
                        Console.ForegroundColor = Colors.Food;
                        Console.Write(point);
                    }
                    else if (walls.Contains((i, j)))
                    {
                        Console.ForegroundColor = Colors.Wall;
                        Console.Write(point);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(point);
                    }
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            for (int i = 0; i < snakes.Count; i++)
            {
                Console.SetCursorPosition(0, Rows + i);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Score{snakes[i].PlayerId}: {snakes[i].Score}   ");
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
        public Color Color;
        public ConsoleKey R;
        public ConsoleKey L;
        public ConsoleKey U;
        public ConsoleKey D;
        public static int TotalPlayers = 0;
        public int PlayerId;
        public bool isGameOver = false;
        public int HeightsScore = Settings.Width;
        public (int i, int j) Head = (0, 0);
        public List<(int i, int j)> snake = new List<(int, int)>();
        public int Score { get; set; }
        public Direction direction = Direction.Right;
        public Snake((int i, int j) startPoint, Direction direction, List<ConsoleKey> keys, Color color)
        {
            R = keys[0];
            L = keys[1];
            U = keys[2];
            D = keys[3];
            Color = color;
            this.direction = direction;
            Score = 1;
            snake.Add(startPoint);
            TotalPlayers++;
            PlayerId = TotalPlayers;
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
            Console.CursorVisible = false;
            Map map = new Map();
            List<Snake> snakes = new List<Snake>()
            {
                //new Snake(StartPoint.RightTop, Direction.Down, Controls.Arrows, Colors.Red),
                //new Snake(StartPoint.LeftBottom, Direction.Up, Controls.Arrows, Colors.Green),
                new Snake(StartPoint.LeftTop, Direction.Right, Controls.Arrows, Colors.Yellow),
                new Snake(StartPoint.RightBottom, Direction.Left, Controls.WASD, Colors.Blue)
            };

            map.GenerateFood();
            List<Task> tasks = new List<Task>()
            {
                Task.Run(() => Move(snakes, map)),
                Task.Run(() => Render(snakes, map)),
                Task.Run(() => Control(snakes))
            };
            await Task.WhenAll(tasks);
        }
        static async Task Control(List<Snake> snakes)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var button = Console.ReadKey(true).Key;
                    foreach (var snake in snakes)
                    {
                        if (snake.R == button && snake.direction != Direction.Left)
                        {
                            snake.direction = Direction.Right;
                        }
                        else if (snake.L == button && snake.direction != Direction.Right)
                        {
                            snake.direction = Direction.Left;
                        }
                        else if (snake.U == button && snake.direction != Direction.Down)
                        {
                            snake.direction = Direction.Up;
                        }
                        else if (snake.D == button && snake.direction != Direction.Up)
                        {
                            snake.direction = Direction.Down;
                        }
                    }
                    
                }
                await Task.Delay(10);
            }
        }
        static async Task Move(List<Snake> snakes, Map map)
        {
            while (true)
            {
                for (int i = 0; i < snakes.Count; i++)
                    snakes[i].Move(map);

                await Task.Delay(Settings.Speed);
            }
        }
        static async Task Render(List<Snake> snakes, Map map)
        {
            while (true)
            {
                map.Print(snakes);
                await Task.Delay(10);
            }
        }
    }
}
