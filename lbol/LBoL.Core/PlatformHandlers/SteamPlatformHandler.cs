using System;
using System.IO;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

namespace LBoL.Core.PlatformHandlers
{
	// Token: 0x020000F7 RID: 247
	public sealed class SteamPlatformHandler : PlatformHandler
	{
		// Token: 0x0600097E RID: 2430 RVA: 0x0001B948 File Offset: 0x00019B48
		[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x0001B950 File Offset: 0x00019B50
		public override bool Init()
		{
			if (!Packsize.Test())
			{
				Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}
			if (!DllCheck.Test())
			{
				Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}
			try
			{
				if (SteamAPI.RestartAppIfNecessary(SteamPlatformHandler.AppId))
				{
					return false;
				}
			}
			catch (DllNotFoundException)
			{
				Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.");
				return false;
			}
			if (!SteamAPI.Init())
			{
				Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
				return false;
			}
			SteamClient.SetWarningMessageHook(new SteamAPIWarningMessageHook_t(SteamPlatformHandler.SteamAPIDebugTextHook));
			CSteamID steamID = SteamUser.GetSteamID();
			this._steamId = steamID.m_SteamID;
			Callback<UserStatsReceived_t>.Create(new Callback<UserStatsReceived_t>.DispatchDelegate(this.OnUserStatsReceived));
			SteamUserStats.RequestCurrentStats();
			return true;
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x0001BA04 File Offset: 0x00019C04
		private void OnUserStatsReceived(UserStatsReceived_t param)
		{
			if ((ulong)SteamPlatformHandler.AppId.m_AppId != param.m_nGameID)
			{
				Debug.LogError("");
				return;
			}
			if (param.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogError(string.Format("Request user stats failed: {0}", param.m_eResult));
				return;
			}
			this._userStatsReceived = true;
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0001BA5A File Offset: 0x00019C5A
		public override void Update()
		{
			SteamAPI.RunCallbacks();
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x0001BA61 File Offset: 0x00019C61
		public override void Shutdown()
		{
			SteamAPI.Shutdown();
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x0001BA68 File Offset: 0x00019C68
		public override Locale GetDefaultLocale()
		{
			string steamUILanguage = SteamUtils.GetSteamUILanguage();
			uint num = <PrivateImplementationDetails>.ComputeStringHash(steamUILanguage);
			if (num <= 2471602315U)
			{
				if (num <= 683056061U)
				{
					if (num <= 505713757U)
					{
						if (num != 380651494U)
						{
							if (num == 505713757U)
							{
								if (steamUILanguage == "brazilian")
								{
									return Locale.Pt;
								}
							}
						}
						else if (steamUILanguage == "russian")
						{
							return Locale.Ru;
						}
					}
					else if (num != 599131013U)
					{
						if (num == 683056061U)
						{
							if (steamUILanguage == "ukrainian")
							{
								return Locale.Uk;
							}
						}
					}
					else if (steamUILanguage == "french")
					{
						return Locale.Fr;
					}
				}
				else if (num <= 1580935484U)
				{
					if (num != 1544226106U)
					{
						if (num == 1580935484U)
						{
							if (steamUILanguage == "portuguese")
							{
								return Locale.Pt;
							}
						}
					}
					else if (steamUILanguage == "hungarian")
					{
						return Locale.Hu;
					}
				}
				else if (num != 1901528810U)
				{
					if (num == 2471602315U)
					{
						if (steamUILanguage == "italian")
						{
							return Locale.It;
						}
					}
				}
				else if (steamUILanguage == "japanese")
				{
					return Locale.Ja;
				}
			}
			else if (num <= 3180870988U)
			{
				if (num <= 2805355685U)
				{
					if (num != 2499415067U)
					{
						if (num == 2805355685U)
						{
							if (steamUILanguage == "schinese")
							{
								return Locale.ZhHans;
							}
						}
					}
					else if (steamUILanguage == "english")
					{
						return Locale.En;
					}
				}
				else if (num != 3088622664U)
				{
					if (num == 3180870988U)
					{
						if (steamUILanguage == "polish")
						{
							return Locale.Pl;
						}
					}
				}
				else if (steamUILanguage == "tchinses")
				{
					return Locale.ZhHant;
				}
			}
			else if (num <= 3405445907U)
			{
				if (num != 3210859552U)
				{
					if (num == 3405445907U)
					{
						if (steamUILanguage == "german")
						{
							return Locale.De;
						}
					}
				}
				else if (steamUILanguage == "koreana")
				{
					return Locale.Ko;
				}
			}
			else if (num != 3426057626U)
			{
				if (num != 3719199419U)
				{
					if (num == 3739448251U)
					{
						if (steamUILanguage == "turkish")
						{
							return Locale.Tr;
						}
					}
				}
				else if (steamUILanguage == "spanish")
				{
					return Locale.Es;
				}
			}
			else if (steamUILanguage == "vietnamese")
			{
				return Locale.Vi;
			}
			return Locale.En;
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x0001BD32 File Offset: 0x00019F32
		public override string GetSaveDataFolder()
		{
			return Path.Combine(Application.persistentDataPath, this._steamId.ToString());
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x0001BD49 File Offset: 0x00019F49
		public override void SetMainMenuInfo(MainMenuStatus status)
		{
			SteamFriends.SetRichPresence("status", status.ToString());
			SteamFriends.SetRichPresence("steam_display", "#StatusMainMenu");
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x0001BD74 File Offset: 0x00019F74
		public override void SetGameRunInfo(GameRunController gameRun)
		{
			SteamFriends.SetRichPresence("player", gameRun.Player.Id);
			SteamFriends.SetRichPresence("stage", gameRun.CurrentStage.isNormalStage ? gameRun.CurrentStage.Id : "Unknown");
			SteamFriends.SetRichPresence("level", gameRun.CurrentStation.Level.ToString());
			string text = gameRun.Difficulty.ToString();
			int puzzleLevel = PuzzleFlags.GetPuzzleLevel(gameRun.Puzzles);
			if (puzzleLevel > 0)
			{
				text += string.Format("({0})", puzzleLevel);
			}
			SteamFriends.SetRichPresence("difficulty", text);
			SteamFriends.SetRichPresence("steam_display", "#StatusGameRun");
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x0001BE37 File Offset: 0x0001A037
		public override void SetAchievement(string key)
		{
			if (!this._userStatsReceived)
			{
				Debug.LogError("[Steam] Cannot set achievement before user stats received");
				return;
			}
			if (!SteamUserStats.SetAchievement(key))
			{
				Debug.LogError("[Steam] Cannot set achievement " + key);
				return;
			}
			if (!SteamUserStats.StoreStats())
			{
				Debug.LogError("[Steam] Failed to store stats");
				return;
			}
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x0001BE77 File Offset: 0x0001A077
		public override void ClearAchievement(string key)
		{
			if (!this._userStatsReceived)
			{
				Debug.LogError("[Steam] Cannot clear achievement before user stats received");
				return;
			}
			if (!SteamUserStats.ClearAchievement(key))
			{
				Debug.LogError("[Steam] Cannot clear achievement " + key);
				return;
			}
			if (!SteamUserStats.StoreStats())
			{
				Debug.LogError("[Steam] Failed to store stats");
				return;
			}
		}

		// Token: 0x040004F8 RID: 1272
		private static readonly AppId_t AppId = (AppId_t)1140150U;

		// Token: 0x040004F9 RID: 1273
		private ulong _steamId;

		// Token: 0x040004FA RID: 1274
		private bool _userStatsReceived;
	}
}
