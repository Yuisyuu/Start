using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace Start.Utils;

internal class Server(string fileName)
{
    private Process? _process;
    private bool _updating;

    public async Task Start()
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
                await Logger.LogAsync("启动失败！将重试！", LogLevel.Error);
                continue;
            }

            _process.BeginOutputReadLine();
            _process.OutputDataReceived += (_, e) => Console.WriteLine(e.Data);
            _process.BeginErrorReadLine();
            _process.ErrorDataReceived += (_, e) => Console.WriteLine(e.Data);
            _process.Exited += async (_, _) =>
            {
                while (_updating)
                {
                    Task.Yield();
                }

                await Logger.LogAsync("检测到服务器关闭，正在启动", LogLevel.Warn);
                await Start();
            };
            break;
        }
    }

    public async Task Update()
    {
        await Logger.LogAsync("开始检测更新");
        CancellationTokenSource tokenSource = new();
        HttpClient httpClient = new();
        string? link = default;
        Task task = Task.Run(async () =>
        {
            string pageData = await httpClient.GetStringAsync("https://www.minecraft.net/download/server/bedrock",
                tokenSource.Token);
            link = Regex.UrlRegex().Match(pageData).Value;
        }, tokenSource.Token);
        await task.WaitAsync(TimeSpan.FromSeconds(10), tokenSource.Token);
        if (!task.IsCompleted)
        {
            await tokenSource.CancelAsync();
        }

        if (string.IsNullOrWhiteSpace(link))
        {
            await Logger.LogAsync("检测失败", LogLevel.Warn);
            return;
        }

        string version = Regex.VersionRegex().Match(link).Value;
        if (string.IsNullOrWhiteSpace(version) || (File.Exists("lv.dat") && File.ReadAllText("lv.dat") == version))
        {
            await Logger.LogAsync("检测完毕，尚无更新版本");
            return;
        }

        await Logger.LogAsync($"检测到新版本{version}，开始更新");
        if (_process is not null && !_process.HasExited)
        {
            await _process.StandardInput.WriteLineAsync(
                Encoding.Default.GetString(Encoding.UTF8.GetBytes($"say 即将关闭服务器并更新至{version}！")));
        }

        Directory.CreateDirectory("cache");
        await File.WriteAllBytesAsync("cache/bds.zip", await httpClient.GetByteArrayAsync(link), tokenSource.Token);
        _updating = true;
        if (_process is not null && !_process.HasExited)
        {
            await _process.StandardInput.WriteLineAsync(Encoding.Default.GetString("stop"u8));
            await _process.WaitForExitAsync(tokenSource.Token);
        }

        await Logger.LogAsync("开始更新");
        File.Copy("server.properties", "cache/server.properties");
        ZipFile.ExtractToDirectory("cache/bds.zip", ".", true);
        File.Copy("cache/server.properties", "server.properties", true);
        _updating = false;
        await File.WriteAllTextAsync("lv.dat", version, tokenSource.Token);
        Directory.Delete("cache", true);
        await Logger.LogAsync("更新完毕");
    }

    public async Task WriteLineAsync(string input)
    {
        if (_process is null)
        {
            throw new NullReferenceException();
        }

        await _process.StandardInput.WriteLineAsync(Encoding.Default.GetString(Encoding.UTF8.GetBytes(input)));
    }

    public async Task WaitForExitAsync()
    {
        if (_process is null)
        {
            throw new NullReferenceException();
        }

        await _process.WaitForExitAsync();
    }
}