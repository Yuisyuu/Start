using Start.Utils;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
Server server = new("bedrock_server.exe");
_ = Task.Factory.StartNew(() =>
{
    while (true)
    {
        server.Update().Wait();
        Thread.Sleep(600000);
    }
}, TaskCreationOptions.LongRunning);
server.Start();
while (true)
{
    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    await server.WriteLineAsync(input);
    if (input is "stop")
    {
        await server.WaitForExitAsync();
        break;
    }
}