// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.WebSockets;
using System.Text;

Console.WriteLine("Press enter to start!..... ");
Console.Read();

using (ClientWebSocket client = new ClientWebSocket())
{
    Uri serverUri = new Uri("ws://localhost:5178/send");
    var cTs = new CancellationTokenSource();
    cTs.CancelAfter(TimeSpan.FromSeconds(120));
    try
    {
        await client.ConnectAsync(serverUri, cTs.Token);
        var n = 0;
        while (client.State == WebSocketState.Open)
        {
            Console.WriteLine("Client send to message: ");
            string message = Console.ReadLine();
            if (!string.IsNullOrEmpty(message))
            {
                ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cTs.Token);
                var responseBuffer = new byte[1024];
                var offSet = 0;
                var packet = 1024;
                while (true)
                {
                    ArraySegment<byte> byteReceived = new ArraySegment<byte>(responseBuffer, offSet, packet);
                    WebSocketReceiveResult response = await client.ReceiveAsync(responseBuffer, cTs.Token);
                    var responseMessage = Encoding.UTF8.GetString(responseBuffer, offSet, response.Count);
                    Console.WriteLine($"{responseMessage}");
                    if (response.EndOfMessage)
                    {
                        break;
                    }
                }
            }
        }
    } catch (Exception ex) { 
        Console.WriteLine(ex.Message);
    }
}
Console.ReadLine();