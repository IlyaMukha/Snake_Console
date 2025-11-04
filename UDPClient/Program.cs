using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPClient
{
    public class Data
    {
        public string[,] Matrix { get; set; }
        public (int i, int j) Head { get; set; }
        public List<(int i, int j)> Snake { get; set; }
        public (int i, int j) Food { get; set; }
    }
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
        public ConsoleColor MainColor { get; set; }
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
        public void Print(Snake snake)
        {
            int iq = 0;
            int jq = 0;
            var heads = snake.Head;
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    var point = matrix[i, j];
                    if (snake.snake.Contains((i, j)))
                    {
                        if (heads == (i, j))
                            Console.ForegroundColor = snake.Color.MainColor;
                        else
                            Console.ForegroundColor = snake.Color.ThecondColor;
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
                //Console.WriteLine();
            }
            Console.SetCursorPosition(0, Rows + 1);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Score{snake.PlayerId}: {snake.Score}   ");
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
        public bool isChange = false;
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
                    newH = (currenH.i, currenH.j - 1);
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

            var oldH = snake[snake.Count - 1];
            snake.Insert(0, newH);
            if (!isGrow)
            {
                snake.Remove(oldH);
                map.matrix[oldH.i, oldH.j] = Settings.Empty;
            }

            map.matrix[newH.i, newH.j] = Settings.Element;
        }
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
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string serverIp = "127.0.0.1";
            const int serverPort = 5000;
            Map map = new Map();
            Snake snake = new Snake(StartPoint.LeftTop, Direction.Right, Controls.Arrows, Colors.Green);
            UdpClient client = new UdpClient(0);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

            List<Task> tasks = new List<Task>()
            {
                Task.Run(() => Control(snake, client, serverEP)),
                Task.Run(() => Receive(client, map, snake))
            };
            await Task.WhenAll(tasks);
        }
        static async Task Send(UdpClient client, IPEndPoint serverEP, Snake snake)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{snake.direction.ToString()}");
            client.Send(data, data.Length, serverEP);
            await Task.Delay(200);
        }
        static async Task Receive(UdpClient client, Map card, Snake snake)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] data = (await client.ReceiveAsync()).Buffer;
                string message = Encoding.UTF8.GetString(data);
                string json = Encoding.UTF8.GetString(data);
                Data map = JsonConvert.DeserializeObject<Data>(json);
                card.matrix = map.Matrix;
                card.Food = map.Food;
                snake.snake = map.Snake;
                snake.Head = map.Head;
                card.Print(snake);
            }
        }
        static async Task Control(Snake snake, UdpClient client, IPEndPoint serverEP)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var button = Console.ReadKey(true).Key;
                    if (snake.R == button && snake.direction != Direction.Left && snake.direction != Direction.Right)
                    {
                        snake.direction = Direction.Right;
                        await Send(client, serverEP, snake);
                    }
                    else if (snake.L == button && snake.direction != Direction.Right && snake.direction != Direction.Left)
                    {
                        snake.direction = Direction.Left;
                        await Send(client, serverEP, snake);
                    }
                    else if (snake.U == button && snake.direction != Direction.Down && snake.direction != Direction.Up)
                    {
                        snake.direction = Direction.Up;
                        await Send(client, serverEP, snake);
                    }
                    else if (snake.D == button && snake.direction != Direction.Up && snake.direction != Direction.Down)
                    {
                        snake.direction = Direction.Down;
                        await Send(client, serverEP, snake);
                    }

                }
                await Task.Delay(10);
            }
        }
    }
}
