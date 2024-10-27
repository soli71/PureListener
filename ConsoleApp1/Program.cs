using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

int port = 5000;
var listener = new TcpListener(IPAddress.Any, port);
listener.Start();
while (true)
{
    var client = await listener.AcceptTcpClientAsync();

    _ = HandleClient(client);
}

static async Task HandleClient(TcpClient client)
{
    using (client)
    {
        NetworkStream stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string requestLine = await reader.ReadLineAsync();
        Console.WriteLine($"Received request: {requestLine}");

        var requestParts = requestLine.Split(' ');
        if (requestParts.Length < 2)
        {
            await SendResponse(stream, "400 Bad Request", "Invalid request format");
            return;
        }

        string method = requestParts[0];
        string path = requestParts[1];

        if (method == "GET" && path == "/api/data")
        {
            var data = new { id = 1, name = "Sample Data", description = "This is sample data." };
            string jsonResponse = JsonSerializer.Serialize(data);
            await SendResponse(stream, "200 OK", jsonResponse, "application/json");
        }
        else if (method == "GET" && path == "/api/message")
        {
            var message = new { message = "Hello, this is a custom message!" };
            string jsonResponse = JsonSerializer.Serialize(message);
            await SendResponse(stream, "200 OK", jsonResponse, "application/json");
        }
        else
        {
            await SendResponse(stream, "404 Not Found", "The requested resource was not found.");
        }
    }
}

static async Task SendResponse(NetworkStream stream, string status, string body, string contentType = "text/plain")
{
    string response =
        $"HTTP/1.1 {status}\r\n" +
        $"Content-Type: {contentType}; charset=UTF-8\r\n" +
        $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
        "Connection: close\r\n\r\n" +
        body;

    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    await stream.FlushAsync();

    Console.WriteLine("Response sent.");
}