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
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

public static partial class OtherPlayersOverlayPatch
{
    #region 远端视图注册与地图图标

    private static ulong _lastMapIconLayoutFingerprint;

    private static void EnsureRemoteCharacters()
    {
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            HideRemoteCharacters();
            return;
        }

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
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

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
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

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

        if (view.BoxCollider != null) view.BoxCollider.enabled = false;

        try
        {
            Collider2D circle = Traverse.Create(view).Field("_circleCollider").GetValue<Collider2D>();
            if (circle != null) circle.enabled = false;
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
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return Array.Empty<UnitView>();
        }

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
        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return;
        }

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

        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
        {
            return false;
        }

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

        if (!ShouldRenderRemoteCharacters(IsMapPanelVisible()))
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

        RectTransform overlayRoot = EnsureMapIconsOverlayRoot(mapPanel);
        if (overlayRoot == null)
        {
            return;
        }

        // 尊重 Runtime Editor 手动 Active 开关：不在每帧强制开启。
        if (!overlayRoot.gameObject.activeSelf)
        {
            return;
        }

        overlayRoot.SetAsLastSibling();
        _defaultFont ??= FindDefaultFont(mapPanel.transform);

        if (_mapNodeIconsRoots.Count > 0)
        {
            foreach (RectTransform root in _mapNodeIconsRoots.Values)
            {
                if (root != null)
                {
                    UnityEngine.Object.Destroy(root.gameObject);
                }
            }

            _mapNodeIconsRoots.Clear();
        }

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

        _lastMapIconLayoutFingerprint = 0;

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

            Vector2 nodeAnchoredPosition = GetMapNodeAnchoredPosition(widget);

            List<PlayerSummary> orderedPlayers = [.. group
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)];

            const float baseY = 0f;
            const float horizontalSpacing = 100f;
            float startX = -((orderedPlayers.Count - 1) * horizontalSpacing * 0.5f);

            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                PlayerSummary p = orderedPlayers[i];
                alive.Add(p.PlayerId);

                MapIconUi icon = EnsureMapIcon(p);
                if (icon.Root.transform.parent != overlayRoot)
                {
                    icon.Root.transform.SetParent(overlayRoot, false);
                }

                icon.Root.transform.SetAsLastSibling();
                icon.Root.hideFlags = HideFlags.None;
                icon.RootRect.anchorMin = new Vector2(0.5f, 0.5f);
                icon.RootRect.anchorMax = new Vector2(0.5f, 0.5f);
                icon.RootRect.pivot = new Vector2(0.5f, 0.5f);

                icon.RootRect.anchoredPosition = nodeAnchoredPosition + new Vector2(startX + i * horizontalSpacing, baseY);
                icon.Label.text = ResolveDisplayName(p.PlayerId, p.PlayerName);
                icon.Image.color = p.IsHost ? new Color(1f, 0.95f, 0.4f, 1f) : Color.white;
            }
        }

        foreach (var kv in _mapIcons)
        {
            if (!alive.Contains(kv.Key))
            {
                kv.Value.Root.SetActive(false);
            }
        }

        overlayRoot.SetAsLastSibling();
    }

    private static Vector2 GetMapNodeAnchoredPosition(MapNodeWidget widget)
    {
        if (widget == null)
        {
            return Vector2.zero;
        }

        RectTransform rect = widget.transform as RectTransform;
        if (rect != null)
        {
            return rect.anchoredPosition;
        }

        Vector3 local = widget.transform.localPosition;
        return new Vector2(local.x, local.y);
    }

    private static RectTransform EnsureMapIconsOverlayRoot(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return null;
        }

        RectTransform nodeHolder = TryGetMapNodeHolder(mapPanel);
        if (nodeHolder == null)
        {
            return null;
        }

        if (_mapIconsRoot != null && _mapIconsRoot.transform.parent == nodeHolder)
        {
            _mapIconsRoot.hideFlags = HideFlags.None;
            _mapIconsRoot.SetAsLastSibling();
            return _mapIconsRoot;
        }

        if (_mapIconsRoot != null)
        {
            UnityEngine.Object.Destroy(_mapIconsRoot.gameObject);
            _mapIconsRoot = null;
        }

        GameObject root = new("NetworkPlugin_RemotePlayerIconsOverlay");
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(nodeHolder, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localEulerAngles = Vector3.zero;
        rt.SetAsLastSibling();

        _mapIconsRoot = rt;
        return rt;
    }

    private static RectTransform TryGetMapNodeHolder(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return null;
        }

        try
        {
            return Traverse.Create(mapPanel).Field("nodeHolder").GetValue<RectTransform>();
        }
        catch
        {
            return null;
        }
    }

    private static RectTransform EnsureMapIconsRoot(MapNodeWidget widget, RectTransform overlayRoot)
    {
        if (widget == null || overlayRoot == null)
        {
            return null;
        }

        if (_mapNodeIconsRoots.TryGetValue(widget, out RectTransform existing) && existing != null && existing.transform.parent == overlayRoot)
        {
            existing.localPosition = widget.transform.localPosition;
            existing.SetAsLastSibling();
            return existing;
        }

        if (existing != null)
        {
            UnityEngine.Object.Destroy(existing.gameObject);
        }

        GameObject root = new("NetworkPlugin_RemotePlayerIcons");
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(overlayRoot, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localPosition = widget.transform.localPosition;
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
            if (widget != null && root != null && _mapIconsRoot != null && root.transform.parent == _mapIconsRoot)
            {
                root.localPosition = widget.transform.localPosition;
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
        root.hideFlags = HideFlags.None;
        root.transform.SetParent(null, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(120f, 160f);

        GameObject avatarGo = new("Avatar");
        avatarGo.transform.SetParent(root.transform, false);

        RectTransform avatarRect = avatarGo.AddComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 1f);
        avatarRect.anchorMax = new Vector2(1f, 1f);
        avatarRect.pivot = new Vector2(0.5f, 1f);
        avatarRect.anchoredPosition = Vector2.zero;
        avatarRect.sizeDelta = new Vector2(0f, 120f);

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
        labelRect.sizeDelta = new Vector2(0f, 18f);

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

        string normalized = characterId.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        List<string> candidates = BuildAvatarCharacterCandidates(normalized);
        foreach (string candidate in candidates)
        {
            if (_avatarCache.TryGetValue(candidate, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Sprite sprite = TryLoadAvatarSpriteNoThrow(candidate);
            if (sprite == null)
            {
                continue;
            }

            foreach (string alias in candidates)
            {
                _avatarCache[alias] = sprite;
            }

            return sprite;
        }

        return null;
    }

    private static List<string> BuildAvatarCharacterCandidates(string characterId)
    {
        List<string> candidates = new List<string>(6);
        AddAvatarCandidate(candidates, characterId);

        int slash = Math.Max(characterId.LastIndexOf('/'), characterId.LastIndexOf('\\'));
        if (slash >= 0 && slash + 1 < characterId.Length)
        {
            AddAvatarCandidate(candidates, characterId.Substring(slash + 1));
        }

        if (characterId.EndsWith("_Avatar", StringComparison.OrdinalIgnoreCase))
        {
            AddAvatarCandidate(candidates, characterId.Substring(0, characterId.Length - "_Avatar".Length));
        }

        if (TryResolveCharacterAliasViaLibrary(characterId, out string modelName, out string unitId))
        {
            AddAvatarCandidate(candidates, modelName);
            AddAvatarCandidate(candidates, unitId);
        }

        return candidates;
    }

    private static bool TryResolveCharacterAliasViaLibrary(string characterId, out string modelName, out string unitId)
    {
        modelName = null;
        unitId = null;

        try
        {
            PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
            if (unit == null)
            {
                return false;
            }

            modelName = unit.ModelName;
            unitId = unit.Id;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Sprite TryLoadAvatarSpriteNoThrow(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        try
        {
            return ResourcesHelper.LoadCharacterAvatarSprite(characterId);
        }
        catch
        {
            return null;
        }
    }

    private static void AddAvatarCandidate(List<string> candidates, string characterId)
    {
        if (candidates == null || string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        string normalized = characterId.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (candidates.Any(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        candidates.Add(normalized);
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

        // 保留总根节点 Active 状态，避免覆盖 Runtime Editor 的手动开关。
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

    private static ulong ComputeMapIconLayoutFingerprint(IReadOnlyList<PlayerSummary> players)
    {
        if (players == null || players.Count == 0)
        {
            return 0;
        }

        ulong xor = 0;
        ulong sum = 0;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerSummary player = players[i];
            string signature = string.Join("|",
                player.PlayerId ?? string.Empty,
                player.PlayerName ?? string.Empty,
                player.IsHost ? "1" : "0",
                player.LocationX.ToString(),
                player.LocationY.ToString(),
                player.CharacterId ?? string.Empty);

            ulong playerFingerprint = NetLogHelper.ComputeFnv1a64(signature);
            xor ^= playerFingerprint;
            sum += playerFingerprint;
        }

        return xor ^ sum ^ (ulong)players.Count;
    }

    private static bool AreMapIconRootsStable(MapNodeWidget[,] widgets, IReadOnlyList<PlayerSummary> players)
    {
        if (widgets == null || players == null || _mapIconsRoot == null)
        {
            return false;
        }

        int maxX = widgets.GetUpperBound(0);
        int maxY = widgets.GetUpperBound(1);

        for (int i = 0; i < players.Count; i++)
        {
            PlayerSummary player = players[i];
            if (string.IsNullOrWhiteSpace(player.PlayerId) || player.LocationX < 0 || player.LocationY < 0)
            {
                return false;
            }

            if (player.LocationX < widgets.GetLowerBound(0) || player.LocationX > maxX || player.LocationY < widgets.GetLowerBound(1) || player.LocationY > maxY)
            {
                return false;
            }

            MapNodeWidget widget = widgets[player.LocationX, player.LocationY];
            if (widget == null || !_mapNodeIconsRoots.TryGetValue(widget, out RectTransform widgetRoot) || widgetRoot == null || widgetRoot.transform.parent != _mapIconsRoot || !widgetRoot.gameObject.activeSelf)
            {
                return false;
            }

            if ((widgetRoot.localPosition - widget.transform.localPosition).sqrMagnitude > 0.01f)
            {
                return false;
            }

            if (!_mapIcons.TryGetValue(player.PlayerId, out MapIconUi icon) || icon?.Root == null || icon.Root.transform.parent != widgetRoot || !icon.Root.activeSelf)
            {
                return false;
            }
        }

        return true;
    }

    private static void ResetMapIconLayoutCache()
    {
        _lastMapIconLayoutFingerprint = 0;
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