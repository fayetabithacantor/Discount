using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DiscountService>();

var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[1024 * 4];
    var service = context.RequestServices.GetRequiredService<DiscountService>();

    while (!socket.CloseStatus.HasValue)
    {
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

        if (json.Contains("Count"))
        {
            var request = JsonSerializer.Deserialize<GenerateRequest>(json);
            var codes = await service.GenerateCodesAsync(request.Count);           
            await socket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new GenerateResponse { Codes = codes })), 
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else if (json.Contains("Code"))
        {
            var request = JsonSerializer.Deserialize<UseCodeRequest>(json);
            var success = await service.UseCodeAsync(request.Code);           
            await socket.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new UseCodeResponse { Result = success })), 
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    await socket.CloseAsync(socket.CloseStatus.Value, socket.CloseStatusDescription, CancellationToken.None);
});

app.Run();
