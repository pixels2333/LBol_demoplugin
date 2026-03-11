using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UiObjectQueryPlugin.Models;

namespace UiObjectQueryPlugin.Http;

public sealed class UiQueryHttpServer : IDisposable
{
    private readonly Func<ObjectQueryRequest, Task<ObjectQueryResponse>> _queryHandler;
    private readonly Action<string> _infoLogger;
    private readonly Action<string> _warningLogger;
    private readonly Action<string> _errorLogger;
    private readonly HttpListener _listener = new HttpListener();
    private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();
    private Task _serverTask;

    public UiQueryHttpServer(
        string host,
        int port,
        Func<ObjectQueryRequest, Task<ObjectQueryResponse>> queryHandler,
        Action<string> infoLogger,
        Action<string> warningLogger,
        Action<string> errorLogger)
    {
        Host = host;
        Port = port;
        Prefix = $"http://{host}:{port}/";
        _queryHandler = queryHandler;
        _infoLogger = infoLogger;
        _warningLogger = warningLogger;
        _errorLogger = errorLogger;
        _listener.Prefixes.Add(Prefix);
    }

    public string Host { get; }

    public int Port { get; }

    public string Prefix { get; }

    public void Start()
    {
        _listener.Start();
        _serverTask = Task.Run(ListenLoopAsync);
        _infoLogger($"HTTP 查询服务已启动: {Prefix}");
    }

    public void Dispose()
    {
        _shutdownCts.Cancel();
        if (_listener.IsListening)
        {
            _listener.Stop();
        }

        _listener.Close();
        _shutdownCts.Dispose();
    }

    private async Task ListenLoopAsync()
    {
        while (!_shutdownCts.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _errorLogger($"HTTP 监听循环异常: {ex}");
                break;
            }

            _ = Task.Run(() => HandleContextAsync(context));
        }
    }

    private async Task HandleContextAsync(HttpListenerContext context)
    {
        try
        {
            string path = context.Request.Url?.AbsolutePath ?? "/";
            if (string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJsonAsync(context.Response, HttpStatusCode.OK, new
                {
                    ok = true,
                    plugin = PluginInfo.PLUGIN_NAME,
                    version = PluginInfo.PLUGIN_VERSION,
                    host = Host,
                    port = Port,
                }).ConfigureAwait(false);
                return;
            }

            if (!string.Equals(path, "/query", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJsonAsync(context.Response, HttpStatusCode.NotFound, new
                {
                    success = false,
                    error = "仅支持 `/health` 和 `/query`。",
                }).ConfigureAwait(false);
                return;
            }

            if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJsonAsync(context.Response, HttpStatusCode.MethodNotAllowed, new
                {
                    success = false,
                    error = "`/query` 仅支持 POST。",
                }).ConfigureAwait(false);
                return;
            }

            string body;
            using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            ObjectQueryRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<ObjectQueryRequest>(body) ?? new ObjectQueryRequest();
            }
            catch (JsonException ex)
            {
                await WriteJsonAsync(context.Response, HttpStatusCode.BadRequest, new
                {
                    success = false,
                    error = $"JSON 解析失败: {ex.Message}",
                }).ConfigureAwait(false);
                return;
            }

            ObjectQueryResponse response = await _queryHandler(request).ConfigureAwait(false);
            HttpStatusCode statusCode = response.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            await WriteJsonAsync(context.Response, statusCode, response).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _warningLogger($"处理 HTTP 请求失败: {ex.Message}");
            if (!context.Response.OutputStream.CanWrite)
            {
                return;
            }

            await WriteJsonAsync(context.Response, HttpStatusCode.InternalServerError, new
            {
                success = false,
                error = ex.Message,
            }).ConfigureAwait(false);
        }
    }

    private static async Task WriteJsonAsync(HttpListenerResponse response, HttpStatusCode statusCode, object payload)
    {
        string json = JsonConvert.SerializeObject(payload, Formatting.Indented);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        response.StatusCode = (int)statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentEncoding = Encoding.UTF8;
        response.ContentLength64 = bytes.Length;

        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        response.OutputStream.Close();
    }
}
