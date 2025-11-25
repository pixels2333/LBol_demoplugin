using System;
using System.IO;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;
namespace LBoL.Core.PlatformHandlers
{
	public sealed class SteamPlatformHandler : PlatformHandler
	{
		[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}
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
		public override void Update()
		{
			SteamAPI.RunCallbacks();
		}
		public override void Shutdown()
		{
			SteamAPI.Shutdown();
		}
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
		public override string GetSaveDataFolder()
		{
			return Path.Combine(Application.persistentDataPath, this._steamId.ToString());
		}
		public override void SetMainMenuInfo(MainMenuStatus status)
		{
			SteamFriends.SetRichPresence("status", status.ToString());
			SteamFriends.SetRichPresence("steam_display", "#StatusMainMenu");
		}
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
		private static readonly AppId_t AppId = (AppId_t)1140150U;
		private ulong _steamId;
		private bool _userStatsReceived;
	}
}
