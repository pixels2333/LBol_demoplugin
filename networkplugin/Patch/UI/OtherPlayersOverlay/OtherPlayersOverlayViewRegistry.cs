using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using NetworkPlugin.Network.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    #region 远端视图注册与地图图标

    private static void EnsureRemoteCharacters()
    {
        if (Singleton<GameDirector>.Instance == null || Singleton<GameDirector>.Instance.PlayerUnitView == null)
        {
            HideRemoteCharacters();
            return;
        }

        Transform unitRoot = TryGetGameDirectorTransform("unitRoot");
        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        GameObject unitPrefab = TryGetGameDirectorUnitPrefab();
        if (unitRoot == null || playerRoot == null || unitPrefab == null)
        {
            return;
        }

        if (_remoteCharactersRoot == null)
        {
            GameObject rootGo = new("NetworkPlugin_RemoteCharacters");
            rootGo.transform.SetParent(unitRoot, false);
            _remoteCharactersRoot = rootGo.transform;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (remotePlayers.Count == 0)
        {
            HideRemoteCharacters();
            return;
        }

        _remoteCharactersRoot.gameObject.SetActive(true);

        HashSet<string> alive = new HashSet<string>(remotePlayers.Select(p => p.PlayerId));
        foreach (string existingId in _remoteCharacters.Keys.ToList())
        {
            if (!alive.Contains(existingId))
            {
                RemoveRemoteCharacter(existingId);
            }
        }

        foreach (PlayerSummary p in remotePlayers)
        {
            EnsureRemoteCharacterView(unitPrefab, p);
        }
    }

    private static void UpdateRemoteCharactersLayout()
    {
        if (_remoteCharactersRoot == null || !_remoteCharactersRoot.gameObject.activeInHierarchy)
        {
            return;
        }

        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        if (playerRoot == null)
        {
            return;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        Vector3 basePos = playerRoot.localPosition;
        const float xStep = 1.4f;
        const float yStep = 0.5f;
        const float scale = 0.85f;

        for (int index = 0; index < remotePlayers.Count; index++)
        {
            PlayerSummary p = remotePlayers[index];
            if (!_remoteCharacters.TryGetValue(p.PlayerId, out RemoteCharacterView view) || view?.Root == null)
            {
                continue;
            }

            int row = index / 2;
            int col = index % 2;
            float x = (row + 1) * xStep;
            float y = col == 0 ? yStep : -yStep;

            view.Root.transform.localPosition = basePos + new Vector3(x, y, 0f);
            view.Root.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private static void TickRemoteCharacters()
    {
        if (_remoteCharacters.Count == 0)
        {
            return;
        }

        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
            {
                continue;
            }

            rc.View.Tick();
        }
    }

    private static void EnsureRemoteCharacterView(GameObject unitPrefab, PlayerSummary player)
    {
        if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
        {
            return;
        }

        string desiredCharacter = player.CharacterId;
        if (string.IsNullOrWhiteSpace(desiredCharacter))
        {
            desiredCharacter = GetFallbackCharacterId();
        }

        if (_remoteCharacters.TryGetValue(player.PlayerId, out RemoteCharacterView existing))
        {
            if (existing != null && existing.Root != null && string.Equals(existing.CharacterId, desiredCharacter, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RemoveRemoteCharacter(player.PlayerId);
        }

        UnitStatusHud hud = UiManager.GetPanel<UnitStatusHud>();
        if (hud == null)
        {
            return;
        }

        PlayerUnit unit = TryCreatePlayerUnit(desiredCharacter);
        if (unit == null)
        {
            return;
        }

        unit.Initialize();

        GameObject container = new($"RemotePlayer_{player.PlayerId}");
        container.transform.SetParent(_remoteCharactersRoot, false);

        GameObject go = UnityEngine.Object.Instantiate(unitPrefab, container.transform);
        UnitView view = go.GetComponent<UnitView>();
        if (view == null)
        {
            UnityEngine.Object.Destroy(container);
            return;
        }

        view.Unit = unit;
        view.SetStatusWidget(hud.CreateStatusWidget(unit), 0f);
        view.SetInfoWidget(hud.CreateInfoWidget(unit), 0f);

        unit.SetView(view);
        view.IsHidden = false;
        DisableRemoteCharacterInteractions(view);

        _remoteCharacters[player.PlayerId] = new RemoteCharacterView
        {
            PlayerId = player.PlayerId,
            CharacterId = desiredCharacter,
            Root = container,
            View = view,
        };

        try
        {
            Singleton<GameDirector>.Instance.StartCoroutine(view.LoadUnitModelAsync(unit.ModelName, true, default(float?)).ToCoroutine());
        }
        catch
        {
        }
    }

    private static void DisableRemoteCharacterInteractions(UnitView view)
    {
        if (view == null)
        {
            return;
        }

        view.BoxCollider?.enabled = false;

        try
        {
            Collider2D circle = Traverse.Create(view).Field("_circleCollider").GetValue<Collider2D>();
            circle?.enabled = false;
        }
        catch
        {
        }

        Collider selector = view.SelectorCollider;
        if (selector != null)
        {
            selector.enabled = false;
            selector.gameObject.SetActive(false);
        }
    }

    internal static IEnumerable<UnitView> SnapshotRemoteCharacterUnitViews()
    {
        if (_remoteCharacters.Count == 0)
        {
            return Array.Empty<UnitView>();
        }

        List<UnitView> list = new List<UnitView>(_remoteCharacters.Count);
        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
            {
                continue;
            }

            list.Add(rc.View);
        }
        return list;
    }

    internal static void SetRemoteCharacterTargetingEnabled(bool enabled)
    {
        if (_remoteCharacters.Count == 0)
        {
            return;
        }

        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null)
            {
                continue;
            }

            SetSelectorColliderEnabled(rc.View, enabled);
        }
    }

    internal static bool TryGetPointedRemotePlayer(Vector2 screenPosition, out string playerId, out string playerName)
    {
        playerId = null;
        playerName = null;

        try
        {
            if (_remoteCharacters.Count == 0)
            {
                return false;
            }

            Ray ray = CameraController.MainCamera.ScreenPointToRay(screenPosition);
            foreach (RemoteCharacterView rc in _remoteCharacters.Values)
            {
                if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
                {
                    continue;
                }

                Collider selector = rc.View.SelectorCollider;
                if (selector == null)
                {
                    continue;
                }

                if (!selector.Raycast(ray, out _, float.PositiveInfinity))
                {
                    continue;
                }

                playerId = rc.PlayerId;
                lock (_syncLock)
                {
                    if (!string.IsNullOrWhiteSpace(playerId) && _players.TryGetValue(playerId, out PlayerSummary summary))
                    {
                        playerName = ResolveDisplayName(playerId, summary?.PlayerName);
                    }
                }

                return !string.IsNullOrWhiteSpace(playerId);
            }
        }
        catch
        {
        }

        playerId = null;
        playerName = null;
        return false;
    }

    internal static bool TryGetRemoteCharacterUnitView(string playerId, out UnitView view)
    {
        view = null;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        if (_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView rc) && rc?.View != null && rc.Root != null && rc.Root.activeInHierarchy)
        {
            view = rc.View;
            return true;
        }

        view = null;
        return false;
    }

    private static void SetSelectorColliderEnabled(UnitView view, bool enabled)
    {
        if (view == null)
        {
            return;
        }

        Collider selector = view.SelectorCollider;
        if (selector == null)
        {
            return;
        }

        selector.enabled = enabled;
        selector.gameObject.SetActive(enabled);
    }

    private static void HideRemoteCharacters()
    {
        _remoteCharactersRoot?.gameObject.SetActive(false);
    }

    private static void ClearRemoteCharacters()
    {
        foreach (string playerId in _remoteCharacters.Keys.ToList())
        {
            RemoveRemoteCharacter(playerId);
        }

        if (_remoteCharactersRoot != null)
        {
            UnityEngine.Object.Destroy(_remoteCharactersRoot.gameObject);
            _remoteCharactersRoot = null;
        }
    }

    private static void RemoveRemoteCharacter(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView rc))
        {
            if (rc?.Root != null)
            {
                UnityEngine.Object.Destroy(rc.Root);
            }
            _remoteCharacters.Remove(playerId);
        }
    }

    private static Transform TryGetGameDirectorTransform(string fieldName)
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field(fieldName).GetValue<Transform>();
        }
        catch
        {
            return null;
        }
    }

    private static GameObject TryGetGameDirectorUnitPrefab()
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field("unitPrefab").GetValue<GameObject>();
        }
        catch
        {
            return null;
        }
    }

    private static string GetFallbackCharacterId()
    {
        try
        {
            PlayerUnit local = Singleton<GameDirector>.Instance.PlayerUnitView?.Unit as PlayerUnit;
            return local?.Id ?? local?.ModelName ?? "Koishi";
        }
        catch
        {
            return "Koishi";
        }
    }

    private static PlayerUnit TryCreatePlayerUnit(string characterId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(characterId))
            {
                PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
                if (unit != null)
                {
                    return unit;
                }
            }
        }
        catch
        {
        }

        try
        {
            return Library.TryCreatePlayerUnit("Koishi") ?? Library.CreatePlayerUnit("Koishi");
        }
        catch
        {
            return null;
        }
    }

    private static void UpdateMapIcons(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        INetworkClient client = TryGetNetworkClient();
        EnsureVirtualAiDefaultPlayer_NoThrow();
        if (client == null || !client.IsConnected)
        {
            if (!IsVirtualAiDefaultEnabled())
            {
                HideAllMapIcons();
                return;
            }
        }

        MapNodeWidget[,] widgets;
        try
        {
            widgets = Traverse.Create(mapPanel).Field("_mapNodeWidgets").GetValue<MapNodeWidget[,]>();
        }
        catch
        {
            return;
        }

        if (widgets == null)
        {
            return;
        }

        CleanupMapIconRoots();
        _defaultFont ??= FindDefaultFont(mapPanel.transform);

        List<PlayerSummary> players;
        lock (_syncLock)
        {
            players = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .Where(p => p.LocationX >= 0 && p.LocationY >= 0)
                .ToList();
        }

        if (players.Count == 0)
        {
            HideAllMapIcons();
            return;
        }

        HashSet<string> alive = new HashSet<string>();
        foreach (var group in players.GroupBy(p => (X: p.LocationX, Y: p.LocationY)))
        {
            int x = group.Key.X;
            int y = group.Key.Y;

            if (x < widgets.GetLowerBound(0) || x > widgets.GetUpperBound(0) || y < widgets.GetLowerBound(1) || y > widgets.GetUpperBound(1))
            {
                continue;
            }

            MapNodeWidget widget = widgets[x, y];
            if (widget == null)
            {
                continue;
            }

            RectTransform widgetIconsRoot = EnsureMapIconsRoot(widget);
            if (widgetIconsRoot == null)
            {
                continue;
            }

            widgetIconsRoot.gameObject.SetActive(true);
            widgetIconsRoot.SetAsLastSibling();

            int i = 0;
            foreach (PlayerSummary p in group.OrderByDescending(p => p.IsHost).ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase))
            {
                alive.Add(p.PlayerId);

                MapIconUi icon = EnsureMapIcon(p);
                icon.Root.SetActive(true);
                if (icon.Root.transform.parent != widgetIconsRoot)
                {
                    icon.Root.transform.SetParent(widgetIconsRoot, false);
                }

                icon.Root.transform.SetAsLastSibling();

                icon.RootRect.localPosition = new Vector3(0f, 70f + i * 18f, 0f);
                icon.Label.text = ResolveDisplayName(p.PlayerId, p.PlayerName);
                icon.Image.color = p.IsHost ? new Color(1f, 0.95f, 0.4f, 1f) : Color.white;

                i++;
            }
        }

        foreach (var kv in _mapIcons)
        {
            if (!alive.Contains(kv.Key))
            {
                kv.Value.Root.SetActive(false);
            }
        }

        foreach (var kv in _mapNodeIconsRoots)
        {
            RectTransform root = kv.Value;
            if (root == null)
            {
                continue;
            }

            bool hasActiveChild = false;
            for (int i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i)?.gameObject.activeSelf == true)
                {
                    hasActiveChild = true;
                    break;
                }
            }

            root.gameObject.SetActive(hasActiveChild);
        }
    }

    private static RectTransform EnsureMapIconsRoot(MapNodeWidget widget)
    {
        if (widget == null)
        {
            return null;
        }

        if (_mapNodeIconsRoots.TryGetValue(widget, out RectTransform existing) && existing != null && existing.transform.parent == widget.transform)
        {
            return existing;
        }

        if (existing != null)
        {
            UnityEngine.Object.Destroy(existing.gameObject);
        }

        GameObject root = new("NetworkPlugin_RemotePlayerIcons");
        root.transform.SetParent(widget.transform, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(140f, 140f);
        rt.SetAsLastSibling();
        _mapNodeIconsRoots[widget] = rt;
        return rt;
    }

    private static void CleanupMapIconRoots()
    {
        if (_mapNodeIconsRoots.Count == 0)
        {
            return;
        }

        List<MapNodeWidget> toRemove = null;
        foreach (var kv in _mapNodeIconsRoots)
        {
            MapNodeWidget widget = kv.Key;
            RectTransform root = kv.Value;
            if (widget != null && root != null && root.transform.parent == widget.transform)
            {
                continue;
            }

            toRemove ??= new List<MapNodeWidget>();
            toRemove.Add(widget);
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
            }
        }

        if (toRemove == null)
        {
            return;
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            _mapNodeIconsRoots.Remove(toRemove[i]);
        }
    }

    private static MapIconUi EnsureMapIcon(PlayerSummary player)
    {
        if (_mapIcons.TryGetValue(player.PlayerId, out MapIconUi ui) && ui?.Root != null)
        {
            if (!string.Equals(ui.CharacterId, player.CharacterId, StringComparison.OrdinalIgnoreCase))
            {
                SetMapIconSprite(ui, player.CharacterId);
            }
            return ui;
        }

        GameObject root = new($"RemoteIcon_{player.PlayerId}");
    root.transform.SetParent(null, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(48f, 64f);

        GameObject avatarGo = new("Avatar");
        avatarGo.transform.SetParent(root.transform, false);

        RectTransform avatarRect = avatarGo.AddComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 1f);
        avatarRect.anchorMax = new Vector2(1f, 1f);
        avatarRect.pivot = new Vector2(0.5f, 1f);
        avatarRect.anchoredPosition = Vector2.zero;
        avatarRect.sizeDelta = new Vector2(0f, 48f);

        Image avatar = avatarGo.AddComponent<Image>();
        avatar.raycastTarget = false;
        avatar.preserveAspect = true;

        TextMeshProUGUI label = CreateTmpText(root.transform, "Name", player.PlayerName ?? player.PlayerId, 14f);
        label.text = ResolveDisplayName(player.PlayerId, player.PlayerName);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(0f, 16f);

        MapIconUi icon = new MapIconUi
        {
            Root = root,
            RootRect = rootRect,
            Image = avatar,
            Label = label,
            CharacterId = null,
        };

        _mapIcons[player.PlayerId] = icon;
        SetMapIconSprite(icon, player.CharacterId);
        return icon;
    }

    private static void SetMapIconSprite(MapIconUi icon, string characterId)
    {
        icon.CharacterId = characterId;
        icon.Image.sprite = TryGetAvatarSprite(characterId) ?? GetWhiteSprite();
    }

    private static Sprite TryGetAvatarSprite(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        if (_avatarCache.TryGetValue(characterId, out Sprite cached) && cached != null)
        {
            return cached;
        }

        try
        {
            Sprite sprite = ResourcesHelper.LoadCharacterAvatarSprite(characterId);
            if (sprite != null)
            {
                _avatarCache[characterId] = sprite;
            }
            return sprite;
        }
        catch
        {
            return null;
        }
    }

    private static void HideMapIcon(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_mapIcons.TryGetValue(playerId, out MapIconUi icon) && icon?.Root != null)
        {
            icon.Root.SetActive(false);
        }
    }

    private static void HideAllMapIcons()
    {
        foreach (MapIconUi icon in _mapIcons.Values)
        {
            if (icon?.Root != null)
            {
                icon.Root.SetActive(false);
            }
        }

        foreach (RectTransform root in _mapNodeIconsRoots.Values)
        {
            if (root != null)
            {
                root.gameObject.SetActive(false);
            }
        }

        _mapIconsRoot?.gameObject.SetActive(false);
    }

    private static void ClearMapIcons()
    {
        foreach (MapIconUi icon in _mapIcons.Values)
        {
            if (icon?.Root != null)
            {
                UnityEngine.Object.Destroy(icon.Root);
            }
        }
        _mapIcons.Clear();

        if (_mapIconsRoot != null)
        {
            UnityEngine.Object.Destroy(_mapIconsRoot.gameObject);
            _mapIconsRoot = null;
        }

        foreach (RectTransform root in _mapNodeIconsRoots.Values)
        {
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
            }
        }

        _mapNodeIconsRoots.Clear();
    }

    private sealed class RemoteCharacterView
    {
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public GameObject Root { get; set; }
        public UnitView View { get; set; }
    }

    private sealed class MapIconUi
    {
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public Image Image { get; set; }
        public TextMeshProUGUI Label { get; set; }
        public string CharacterId { get; set; }
    }

    #endregion
}