using System.Net.WebSockets;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/*app.MapGet("/", () => "Hello World!");*/
var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(1) };
app.UseWebSockets(wsOptions);
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/send")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                if (result != null)
                {
                    while (!result.CloseStatus.HasValue)
                    {
                        string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
                        Console.WriteLine($"Server receive message: {msg}");
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Server: {DateTime.UtcNow} ")), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                    }
                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
            }
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
        }
    }
    else
    {
        await next();
    }
});

app.Run();
