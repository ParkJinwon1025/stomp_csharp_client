using System.Net.WebSockets;
using System.Text;

// ========== 설정 ==========
string WS_URL = "ws://localhost:9030/stomp/websocket";
string LOGIN = "guest";
string PASSCODE = "guest";
string SUBSCRIBE_TOPIC = "/topic/ubisam";
string PUBLISH_TOPIC = "/app/ubisam";
// ==========================

var ws = new ClientWebSocket();
await ws.ConnectAsync(new Uri(WS_URL), CancellationToken.None);
Console.WriteLine("[INFO] WebSocket 연결 성공");

// STOMP CONNECT
await SendRaw(ws, $"CONNECT\naccept-version:1.2\nhost:/\nlogin:{LOGIN}\npasscode:{PASSCODE}\n\n\0");

// CONNECTED 대기
string response = await Receive(ws);
Console.WriteLine($"[RECV] {response}");

if (!response.StartsWith("CONNECTED"))
{
    Console.WriteLine("[ERROR] STOMP 연결 실패");
    return;
}

Console.WriteLine("[INFO] STOMP 연결 성공! 메시지를 입력하세요. (exit 종료)");

// 구독
await SendRaw(ws, $"SUBSCRIBE\ndestination:{SUBSCRIBE_TOPIC}\nid:sub-0\n\n\0");

// 수신 백그라운드 태스크
var receiveTask = Task.Run(async () =>
{
    while (ws.State == WebSocketState.Open)
    {
        string msg = await Receive(ws);
        if (string.IsNullOrEmpty(msg)) continue;

        if (msg.StartsWith("MESSAGE"))
        {
            int bodyStart = msg.IndexOf("\n\n");
            string body = bodyStart >= 0 ? msg.Substring(bodyStart + 2).TrimEnd('\0') : msg;
            Console.WriteLine($"[RECV] {body}");
        }
    }
});

// 사용자 입력 루프
while (true)
{
    string? input = Console.ReadLine();
    if (input == "exit") break;

    string body = $"{{\"type\":\"request\",\"message\":\"{input}\"}}";
    await SendRaw(ws, $"SEND\ndestination:{PUBLISH_TOPIC}\ncontent-type:application/json\ncontent-length:{Encoding.UTF8.GetByteCount(body)}\n\n{body}\0");
    Console.WriteLine($"[SEND] {body}");
}

await SendRaw(ws, "DISCONNECT\n\n\0");
await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);

// ========== 헬퍼 함수 ==========
static async Task SendRaw(ClientWebSocket ws, string frame)
{
    byte[] buffer = Encoding.UTF8.GetBytes(frame);
    await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
}

static async Task<string> Receive(ClientWebSocket ws)
{
    byte[] buffer = new byte[4096];
    var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
    return Encoding.UTF8.GetString(buffer, 0, result.Count);
}