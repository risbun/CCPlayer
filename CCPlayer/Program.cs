using Fleck;
using System;

namespace CCPlayer
{
    static class Program
    {
        static void Main(string[] args)
        {
            new AudioConvert();
            /*
            var server = new WebSocketServer("ws://0.0.0.0:6969");
            server.Start(socket =>
            {
                socket.OnOpen = () => OnConnected(socket);
                socket.OnClose = () => OnDisconnect(socket);
                socket.OnMessage = message => OnReceive(socket, message);
            });
            var command = string.Empty;
            while (command != "exit")
            {
                command = Console.ReadLine();
            }
            */
        }

        private static void OnReceive(IWebSocketConnection socket, string msg)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(">");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"] {socket.GetHashCode()} \n");
            Console.WriteLine(msg);

            String[] split = msg.Split(',');
            if (split[0] == "video")
            {
                new VideoConvert(split, socket);
            }
            /*else if (split[0] == "audio")
            {
                new AudioConvert(split, context);
            }*/
            else if (split[0] == "screenshot")
            {
                new ScreenshotConvert(split, socket);
            }
        }

        private static void OnDisconnect(IWebSocketConnection socket)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("-");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"] {socket.GetHashCode()} \n");
        }

        static void OnConnected(IWebSocketConnection socket)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"] {socket.GetHashCode()} \n");
        }
	}
}
