using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace Start.Utils;

internal class Server(string fileName)
{
    private readonly ManualResetEventSlim _manualResetEvent = new(true);
    private Process? _process;

    public void Start()
    {
        while (true)
        {
            _process = new()
            {
                StartInfo = new()
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            if (!_process.Start())
            {
                continue;
            }

            _process.BeginOutputReadLine();
            _process.OutputDataReceived += (_, e) => Console.WriteLine(e.Data);
            _process.BeginErrorReadLine();
            _process.ErrorDataReceived += (_, e) => Console.WriteLine(e.Data);
            _process.Exited += (_, _) =>
            {
                _manualResetEvent.Wait();
                Start();
            };
            break;
        }
    }

    public async Task Update()
    {
        using HttpClient httpClient = new();
        string link;
        {
            try
            {
                string pageData = await httpClient.GetStringAsync("https://www.minecraft.net/download/server/bedrock");
                link = Regex.UrlRegex().Match(pageData).Value;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return;
            }
        }
        string version = Regex.VersionRegex().Match(link).Value;
        if (string.IsNullOrWhiteSpace(version) ||
            (File.Exists("lv.dat") && (await File.ReadAllTextAsync("lv.dat") == version)))
        {
            return;
        }

        if (_process is not null && !_process.HasExited)
        {
            await WriteLineAsync($"say 服务器即将关闭并更新至{version}！");
        }

        Directory.CreateDirectory("cache");
        await using (FileStream fileSteam = File.OpenWrite("cache/bds.zip"))
        {
            try
            {
                await using Stream stream = await httpClient.GetStreamAsync(link);
                await stream.CopyToAsync(fileSteam);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return;
            }
            finally
            {
                fileSteam.Close();
            }
        }

        _manualResetEvent.Reset();
        if (_process is not null && !_process.HasExited)
        {
            await WriteLineAsync("stop");
            await _process.WaitForExitAsync();
        }

        File.Copy("server.properties", "cache/server.properties");
        ZipFile.ExtractToDirectory("cache/bds.zip", ".", true);
        File.Copy("cache/server.properties", "server.properties", true);
        _manualResetEvent.Set();
        await File.WriteAllTextAsync("lv.dat", version);
        Directory.Delete("cache", true);
    }

    public async Task WriteLineAsync(string input)
    {
        await _process!.StandardInput.WriteLineAsync(Encoding.Default.GetString(Encoding.UTF8.GetBytes(input)));
    }

    public async Task WaitForExitAsync()
    {
        await _process!.WaitForExitAsync();
    }
}