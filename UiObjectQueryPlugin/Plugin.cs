using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UiObjectQueryPlugin.Http;
using UiObjectQueryPlugin.Models;
using UiObjectQueryPlugin.Services;
using UnityEngine;

namespace UiObjectQueryPlugin;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("LBoL.exe")]
public sealed class Plugin : BaseUnityPlugin
{
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();

    private ConfigEntry<string> _hostConfig;
    private ConfigEntry<int> _portConfig;
    private ConfigEntry<bool> _includeInactiveConfig;
    private ConfigEntry<int> _maxResultsConfig;
    private UiQueryHttpServer _httpServer;
    private UnityObjectQueryService _queryService;

    internal static ManualLogSource LoggerInstance { get; private set; }

    private void Awake()
    {
        LoggerInstance = base.Logger;
        DontDestroyOnLoad(gameObject);

        _hostConfig = Config.Bind("Http", "Host", "127.0.0.1", "HTTP 监听地址，建议保持 127.0.0.1 仅本机访问。");
        _portConfig = Config.Bind("Http", "Port", 37654, "HTTP 查询端口。");
        _includeInactiveConfig = Config.Bind("Query", "IncludeInactiveByDefault", true, "默认是否包含未激活对象。");
        _maxResultsConfig = Config.Bind("Query", "MaxResults", 64, "单次查询最多返回多少个对象。");

        _queryService = new UnityObjectQueryService(_maxResultsConfig.Value, _includeInactiveConfig.Value);
        _httpServer = new UiQueryHttpServer(
            _hostConfig.Value,
            _portConfig.Value,
            HandleQueryAsync,
            message => LoggerInstance.LogInfo(message),
            message => LoggerInstance.LogWarning(message),
            message => LoggerInstance.LogError(message));

        try
        {
            _httpServer.Start();
            LoggerInstance.LogInfo($"{PluginInfo.PLUGIN_NAME} 已加载。POST {_httpServer.Prefix}query 即可按对象名查询。默认 IncludeInactive={_includeInactiveConfig.Value}。");
        }
        catch (Exception ex)
        {
            LoggerInstance.LogError($"启动 HTTP 查询服务失败: {ex}");
        }
    }

    private void Update()
    {
        while (_mainThreadQueue.TryDequeue(out Action action))
        {
            action();
        }
    }

    private void OnDestroy()
    {
        _httpServer?.Dispose();
        LoggerInstance?.LogInfo($"{PluginInfo.PLUGIN_NAME} 已卸载。");
    }

    private Task<ObjectQueryResponse> HandleQueryAsync(ObjectQueryRequest request)
    {
        return InvokeOnMainThreadAsync(() => _queryService.Query(request ?? new ObjectQueryRequest()));
    }

    private Task<T> InvokeOnMainThreadAsync<T>(Func<T> action)
    {
        TaskCompletionSource<T> completion = new TaskCompletionSource<T>();
        _mainThreadQueue.Enqueue(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        });

        return completion.Task;
    }
}
