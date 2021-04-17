using Fleck;
using System;

namespace CCPlayer
{
    static class Program
    {
        static void Main(string[] args)
        {
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
        }

        private static void OnReceive(IWebSocketConnection socket, string msg)
        {
            Console.WriteLine(msg);

            if (msg == "")
                socket.Close();

            String[] split = msg.Split(',');
            switch (split[0])
            {
                case "video":
                    new VideoConvert(split, socket);
                    break;
                case "screenshot":
                    new ScreenshotConvert(socket);
                    break;
                case "image":
                    new ImageConvert(socket, split[1]);
                    break;
            }
        }

        private static void OnDisconnect(IWebSocketConnection socket)
        {
            Console.WriteLine($"< {socket.GetHashCode()}");
        }

        static void OnConnected(IWebSocketConnection socket)
        {
            Console.WriteLine($"> {socket.GetHashCode()}");
        }
	}
}
