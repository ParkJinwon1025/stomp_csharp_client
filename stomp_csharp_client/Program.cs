using System.Text;
using WebSocketSharp;

var ws = new WebSocket("ws://localhost:9030/stomp/websocket");

ws.OnOpen += (s, e) =>
{
    ws.Send("CONNECT\naccept-version:1.2\nhost:/\nlogin:guest\npasscode:guest\n\n\0");
};

ws.OnMessage += (s, e) =>
{
    if (e.Data.StartsWith("CONNECTED"))
    {
        Console.WriteLine("[INFO] STOMP 연결 성공! 메시지를 입력하세요.");
        ws.Send("SUBSCRIBE\ndestination:/topic/ubisam\nid:sub-0\n\n\0");
    }
    else if (e.Data.StartsWith("MESSAGE"))
    {
        int bodyStart = e.Data.IndexOf("\n\n");
        string body = bodyStart >= 0 ? e.Data.Substring(bodyStart + 2).TrimEnd('\0') : e.Data;
        Console.WriteLine($"[RECV] {body}");
    }
};

ws.OnError += (s, e) => Console.WriteLine($"[ERROR] {e.Message}");
ws.OnClose += (s, e) => Console.WriteLine($"[CLOSE] {e.Code}");

ws.Connect();

// 사용자 입력 루프
while (true)
{
    string input = Console.ReadLine();
    if (input == "exit") break;

    string body = $"{{\"type\":\"request\",\"message\":\"{input}\"}}";
    ws.Send($"SEND\ndestination:/app/ubisam\ncontent-type:application/json\ncontent-length:{Encoding.UTF8.GetByteCount(body)}\n\n{body}\0");
    Console.WriteLine($"[SEND] {body}");
}

ws.Close();