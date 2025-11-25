using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Audio;
namespace LBoL.Presentation
{
	public class AudioManager : Singleton<AudioManager>
	{
		public static AudioMixerGroup BgmGroup
		{
			get
			{
				return AudioManager.GuardedGetInstance()._bgmGroup;
			}
		}
		private void EnsureInitialized()
		{
			if (!this._initialized)
			{
				throw new InvalidOperationException("AudioManager.InitializeAsync must be called before use.");
			}
		}
		private static AudioManager GuardedGetInstance()
		{
			AudioManager instance = Singleton<AudioManager>.Instance;
			instance.EnsureInitialized();
			return instance;
		}
		public static UniTask InitializeAsync(AudioMixer mixer)
		{
			return Singleton<AudioManager>.Instance.InternalInitializeAsync(mixer);
		}
		private void InitVolume(string volumeName)
		{
			if (PlayerPrefs.HasKey(volumeName))
			{
				this.SetVolume(volumeName, PlayerPrefs.GetFloat(volumeName), false);
				return;
			}
			PlayerPrefs.SetFloat(volumeName, this.GetVolume(volumeName));
		}
		private float GetVolume(string volumeName)
		{
			float num;
			if (!this._mixer.GetFloat(volumeName, out num))
			{
				throw new ArgumentException("Cannot get '" + volumeName + "' of AudioMixer", "volumeName");
			}
			return Mathf.Clamp01(Mathf.Pow(10f, num / 20f));
		}
		private float GetMasterVolume()
		{
			return this.GetVolume("MasterVolume");
		}
		private void SetVolume(string volumeName, float value, bool setPrefs = false)
		{
			float num = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
			if (!this._mixer.SetFloat(volumeName, num))
			{
				throw new ArgumentException("Cannot set '" + volumeName + "' of AudioMixer", "volumeName");
			}
			if (setPrefs)
			{
				PlayerPrefs.SetFloat(volumeName, value);
			}
		}
		private void InternalSetMasterVolume(float value)
		{
			value = (this._isBackgroundMute ? (this._isApplicationFocused ? value : 0f) : value);
			float num = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
			if (!this._mixer.SetFloat("MasterVolume", num))
			{
				throw new ArgumentException("Cannot set 'MasterVolume' of AudioMixer", "MasterVolume");
			}
		}
		private void SetMasterVolume(float value, bool setPrefs = false)
		{
			this.InternalSetMasterVolume(value);
			this._masterVolume = value;
			if (setPrefs)
			{
				PlayerPrefs.SetFloat("MasterVolume", value);
			}
		}
		private bool GetIsBackgroundMute()
		{
			return this._isBackgroundMute;
		}
		private void SetIsBackgroundMute(bool mute)
		{
			if (mute != this._isBackgroundMute)
			{
				this._isBackgroundMute = mute;
				this.InternalSetMasterVolume(this._masterVolume);
				PlayerPrefs.SetInt("IsBackgroundMute", mute ? 1 : 0);
			}
		}
		private async UniTask InternalInitializeAsync(AudioMixer mixer)
		{
			if (!this._initialized)
			{
				GameObject gameObject = base.gameObject;
				this._sourceHolder = new GameObject
				{
					name = "SourceHolder"
				};
				this._sourceHolder.transform.SetParent(base.transform);
				this._mixer = mixer;
				this._bgmGroup = this._mixer.FindMatchingGroups("Bgm")[0];
				this._sfxGroup = this._mixer.FindMatchingGroups("Sfx")[0];
				this._uiGroup = mixer.FindMatchingGroups("Ui")[0];
				this.InitVolume("MasterVolume");
				this._masterVolume = this.GetMasterVolume();
				this.InitVolume("BgmVolume");
				this.InitVolume("UiVolume");
				this.InitVolume("SfxVolume");
				if (PlayerPrefs.HasKey("IsBackgroundMute"))
				{
					this._isBackgroundMute = PlayerPrefs.GetInt("IsBackgroundMute") != 0;
				}
				else
				{
					Debug.Log("Cannot find IsBackgroundMute in PlayerPrefs");
					this._isBackgroundMute = false;
					PlayerPrefs.SetInt("IsBackgroundMute", 0);
				}
				this._isApplicationFocused = Application.isFocused;
				PlayerPrefs.Save();
				this._bgmSource = new AudioManager.LoopAudioSource(gameObject, this._bgmGroup)
				{
					Priority = 10
				};
				foreach (SfxConfig config in SfxConfig.AllConfig())
				{
					string path = config.Folder + "/" + config.Path;
					int num = path.IndexOf('{');
					int num2 = path.IndexOf('}');
					if (num < 0 && num2 < 0)
					{
						try
						{
							AudioClip audioClip = await ResourcesHelper.LoadSfxAsync(path);
							AudioManager.SfxEntry sfxEntry = new AudioManager.SfxEntry(config.Rep, config.Volume, audioClip);
							this._sfxTable.Add(config.Name, sfxEntry);
							goto IL_049B;
						}
						catch (Exception ex)
						{
							Debug.LogError("Cannot load SFX at " + path + ": " + ex.Message);
							goto IL_049B;
						}
					}
					if (num >= 0 && num < num2)
					{
						if (path.IndexOf('{', num + 1) >= 0 || path.IndexOf('}', num2 + 1) > 0)
						{
							Debug.LogError("Invalid SFX path: " + path);
							continue;
						}
						int num3 = path.IndexOf('-', num + 1, num2 - num - 1);
						if (num3 < 0)
						{
							Debug.LogError("Invalid SFX path: " + path);
							continue;
						}
						string text = path;
						int num4 = num + 1;
						string text2 = text.Substring(num4, num3 - num4);
						string text3 = path;
						num4 = num3 + 1;
						string text4 = text3.Substring(num4, num2 - num4);
						int length = text2.Length;
						if (length != text4.Length)
						{
							Debug.LogError("Invalid SFX path: " + path);
							continue;
						}
						int num5;
						int num6;
						if (!int.TryParse(text2, ref num5) || !int.TryParse(text4, ref num6) || num5 >= num6)
						{
							Debug.LogError("Invalid SFX path: " + path);
							continue;
						}
						string text5 = path.Substring(0, num);
						string text6 = path;
						num4 = num2 + 1;
						string text7 = text6.Substring(num4, text6.Length - num4);
						AudioManager.SfxEntry sfxEntry2 = new AudioManager.SfxEntry(config.Rep, config.Volume);
						for (int i = num5; i <= num6; i++)
						{
							string text8 = text5 + i.ToString().PadLeft(length, '0') + text7;
							try
							{
								AudioClip audioClip2 = ResourcesHelper.LoadSfx(text8);
								sfxEntry2.Clips.Add(audioClip2);
							}
							catch (Exception ex2)
							{
								Debug.LogError("Cannot load SFX at " + path + ": " + ex2.Message);
							}
						}
						this._sfxTable.Add(config.Name, sfxEntry2);
					}
					else
					{
						Debug.LogError("Invalid SFX path: " + path);
					}
					IL_049B:
					path = null;
					config = null;
				}
				IEnumerator<SfxConfig> enumerator = null;
				this._uiSource = base.gameObject.AddComponent<AudioSource>();
				this._uiSource.outputAudioMixerGroup = this._uiGroup;
				this._uiSourceInBgmMixer = base.gameObject.AddComponent<AudioSource>();
				this._uiSourceInBgmMixer.outputAudioMixerGroup = this._bgmGroup;
				foreach (UiSoundConfig uiSoundConfig in UiSoundConfig.AllConfig())
				{
					string text9 = uiSoundConfig.Folder + "/" + uiSoundConfig.Path;
					int num7 = text9.IndexOf('{');
					int num8 = text9.IndexOf('}');
					if (num7 < 0 && num8 < 0)
					{
						try
						{
							AudioClip audioClip3 = ResourcesHelper.LoadUiSound(uiSoundConfig.Folder + "/" + uiSoundConfig.Path);
							this._uiTable.Add(uiSoundConfig.Name, new AudioManager.UiEntry(audioClip3, uiSoundConfig.Volume));
							continue;
						}
						catch (Exception ex3)
						{
							Debug.LogError("Cannot load Ui at " + text9 + ": " + ex3.Message);
							continue;
						}
					}
					if (num7 >= 0 && num7 < num8)
					{
						if (text9.IndexOf('{', num7 + 1) >= 0 || text9.IndexOf('}', num8 + 1) > 0)
						{
							Debug.LogError("Invalid Ui path: " + text9);
						}
						else
						{
							int num9 = text9.IndexOf('-', num7 + 1, num8 - num7 - 1);
							if (num9 < 0)
							{
								Debug.LogError("Invalid Ui path: " + text9);
							}
							else
							{
								int num4 = num7 + 1;
								string text10 = text9.Substring(num4, num9 - num4);
								num4 = num9 + 1;
								string text11 = text9.Substring(num4, num8 - num4);
								int length2 = text10.Length;
								int num10;
								int num11;
								if (length2 != text11.Length)
								{
									Debug.LogError("Invalid Ui path: " + text9);
								}
								else if (!int.TryParse(text10, ref num10) || !int.TryParse(text11, ref num11) || num10 >= num11)
								{
									Debug.LogError("Invalid Ui path: " + text9);
								}
								else
								{
									string text12 = text9.Substring(0, num7);
									num4 = num8 + 1;
									string text13 = text9.Substring(num4, text9.Length - num4);
									AudioManager.UiEntry uiEntry = new AudioManager.UiEntry(uiSoundConfig.Volume);
									for (int j = num10; j <= num11; j++)
									{
										string text14 = text12 + j.ToString().PadLeft(length2, '0') + text13;
										try
										{
											AudioClip audioClip4 = ResourcesHelper.LoadUiSound(text14);
											uiEntry.Clips.Add(audioClip4);
										}
										catch (Exception ex4)
										{
											Debug.LogError("Cannot load Ui at " + text9 + ": " + ex4.Message);
										}
									}
									this._uiTable.Add(uiSoundConfig.Name, uiEntry);
								}
							}
						}
					}
					else
					{
						Debug.LogError("Invalid Ui path: " + text9);
					}
				}
				this._initialized = true;
			}
		}
		private static void CancelBgmCoroutine()
		{
			Singleton<AudioManager>.Instance._bgmSource.Volume = 1f;
			if (AudioManager._bgmCoroutine != null)
			{
				Singleton<AudioManager>.Instance.StopCoroutine(AudioManager._bgmCoroutine);
				AudioManager._bgmCoroutine = null;
			}
		}
		private IEnumerator CoFadeOutAndPlayBgm(BgmConfig config, float fadeOutInterval, float fadeInInterval, float start, bool layer0)
		{
			AudioManager.<>c__DisplayClass38_0 CS$<>8__locals1 = new AudioManager.<>c__DisplayClass38_0();
			CS$<>8__locals1.path = config.Folder + "/" + config.Path;
			CS$<>8__locals1.newBgm = null;
			yield return UniTask.ToCoroutine(delegate
			{
				AudioManager.<>c__DisplayClass38_0.<<CoFadeOutAndPlayBgm>b__0>d <<CoFadeOutAndPlayBgm>b__0>d;
				<<CoFadeOutAndPlayBgm>b__0>d.<>t__builder = AsyncUniTaskMethodBuilder.Create();
				<<CoFadeOutAndPlayBgm>b__0>d.<>4__this = CS$<>8__locals1;
				<<CoFadeOutAndPlayBgm>b__0>d.<>1__state = -1;
				<<CoFadeOutAndPlayBgm>b__0>d.<>t__builder.Start<AudioManager.<>c__DisplayClass38_0.<<CoFadeOutAndPlayBgm>b__0>d>(ref <<CoFadeOutAndPlayBgm>b__0>d);
				return <<CoFadeOutAndPlayBgm>b__0>d.<>t__builder.Task;
			});
			if (CS$<>8__locals1.newBgm == null)
			{
				yield break;
			}
			yield return this.CoFadeOutBgm(fadeOutInterval);
			this._bgmSource.SetClip(CS$<>8__locals1.newBgm, config.LoopStart.GetValueOrDefault(), config.LoopEnd ?? CS$<>8__locals1.newBgm.length, config.ExtraDelay.GetValueOrDefault(), config.Volume);
			this._bgmSource.Layer0 = layer0;
			this._bgmSource.Play(start);
			Action<BgmConfig> bgmChanged = AudioManager.BgmChanged;
			if (bgmChanged != null)
			{
				bgmChanged.Invoke(config);
			}
			if (fadeInInterval > 0f)
			{
				yield return this.CoFadeInBgm(fadeInInterval);
			}
			else
			{
				this._bgmSource.Volume = this._bgmSource.DefaultVolume;
			}
			yield break;
		}
		private IEnumerator CoFadeInBgm(float interval)
		{
			if (this._bgmSource.Clip != null && interval > 0f)
			{
				float remain = interval;
				while (remain > 0f)
				{
					remain = Math.Max(remain - Time.deltaTime, 0f);
					this._bgmSource.Volume = this._bgmSource.DefaultVolume - remain / interval;
					yield return null;
				}
			}
			this._bgmSource.Volume = this._bgmSource.DefaultVolume;
			AudioManager._bgmCoroutine = null;
			yield break;
		}
		private IEnumerator CoFadeOutBgm(float interval)
		{
			float startVol = this._bgmSource.Volume;
			if (this._bgmSource.Clip)
			{
				if (interval > 0f)
				{
					float remain = interval;
					while (remain > 0f)
					{
						remain = Math.Max(remain - Time.deltaTime, 0f);
						this._bgmSource.Volume = remain / interval * startVol;
						yield return null;
					}
				}
				this._bgmSource.Stop();
				ResourcesHelper.Release(this._bgmSource.Clip);
				this._bgmSource.ClearClip();
			}
			AudioManager._bgmCoroutine = null;
			yield break;
		}
		public static void FadeOutBgm(float interval)
		{
			AudioManager.GuardedGetInstance().InternalFadeOutBgm(interval);
		}
		private void InternalFadeOutBgm(float interval)
		{
			AudioManager.CancelBgmCoroutine();
			AudioManager._bgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(this.CoFadeOutBgm(interval));
		}
		public static void StopBgm()
		{
			AudioManager.GuardedGetInstance().InternalStopBgm();
		}
		private void InternalStopBgm()
		{
			AudioManager.CancelBgmCoroutine();
			if (this._bgmSource.Clip)
			{
				ResourcesHelper.Release(this._bgmSource.Clip);
				this._bgmSource.ClearClip();
			}
		}
		public static void PlayBgm(string id, float startTime = 0f, bool layer0 = false)
		{
			AudioManager.FadeOutAndPlayBgm(id, 0f, 0f, startTime, layer0);
		}
		public static void FadeOutAndPlayBgm(string id, float fadeOutInterval = 1f, float fadeInInterval = 0f, float startTime = 0f, bool layer0 = false)
		{
			AudioManager.GuardedGetInstance().InternalFadeOutAndPlayBgm(id, fadeOutInterval, fadeInInterval, startTime, layer0);
		}
		private void InternalFadeOutAndPlayBgm(string id, float fadeOutInterval, float fadeInInterval, float startTime, bool layer0)
		{
			BgmConfig bgmConfig = BgmConfig.FromID(id);
			if (bgmConfig == null)
			{
				throw new ArgumentException("Bgm '" + id + "' not found in config");
			}
			float? loopStart = bgmConfig.LoopStart;
			float? loopEnd = bgmConfig.LoopEnd;
			if ((loopStart.GetValueOrDefault() > loopEnd.GetValueOrDefault()) & ((loopStart != null) & (loopEnd != null)))
			{
				throw new ArgumentException(string.Format("Bgm '{0}' loop-start ({1}) > loop-end ({2})", id, bgmConfig.LoopStart, bgmConfig.LoopEnd));
			}
			AudioManager.CancelBgmCoroutine();
			AudioManager._bgmCoroutine = base.StartCoroutine(this.CoFadeOutAndPlayBgm(bgmConfig, fadeOutInterval, fadeInInterval, startTime, layer0));
		}
		public static void PlaySfx(string sfxName, float volume = -1f)
		{
			AudioManager.GuardedGetInstance().PlaySfxHandler(sfxName, volume);
		}
		public static void PlaySfxDelay(string sfxName, float delay)
		{
			AudioManager.GuardedGetInstance().StartCoroutine(AudioManager.PlaySfxDelayRunner(sfxName, delay));
		}
		private static IEnumerator PlaySfxDelayRunner(string sfxName, float delay)
		{
			yield return new WaitForSeconds(delay);
			AudioManager.PlaySfx(sfxName, -1f);
			yield break;
		}
		private void PlaySfxHandler(string sfxName, float volume)
		{
			if (sfxName.IsNullOrEmpty() || sfxName == "Empty")
			{
				return;
			}
			AudioManager.SfxEntry sfxEntry;
			if (!this._sfxTable.TryGetValue(sfxName, ref sfxEntry))
			{
				Debug.LogWarning("Sfx with name " + sfxName + " not found");
				return;
			}
			if (AudioSettings.dspTime > sfxEntry.PreviousPlayTime + sfxEntry.ReplayLimit)
			{
				this.InternalPlaySfx(sfxEntry, volume);
				return;
			}
			if (AudioSettings.dspTime > sfxEntry.PreviousPlayTime + sfxEntry.ReplayLimit * 0.6)
			{
				this.InternalPlaySfxOneMore(sfxEntry, volume);
			}
		}
		private void InternalPlaySfx(AudioManager.SfxEntry entry, float volume)
		{
			AudioClip sfxClip = this.GetSfxClip(entry);
			this.InternalPlaySfxImmediately(sfxClip, (volume > 0f) ? (volume * entry.Volume) : entry.Volume);
			entry.PreviousPlayTime = AudioSettings.dspTime;
		}
		private void InternalPlaySfx(AudioManager.SfxEntry entry, float volume, double scheduledTime)
		{
			AudioClip sfxClip = this.GetSfxClip(entry);
			this.InternalPlaySfxScheduled(sfxClip, (volume > 0f) ? volume : entry.Volume, scheduledTime);
			entry.PreviousPlayTime = scheduledTime;
		}
		private void InternalPlaySfxOneMore(AudioManager.SfxEntry entry, float volume)
		{
			double num = entry.PreviousPlayTime + entry.ReplayLimit;
			this.InternalPlaySfx(entry, volume, num);
		}
		private AudioClip GetSfxClip(AudioManager.SfxEntry entry)
		{
			AudioClip audioClip;
			if (entry.Randomized)
			{
				int count = entry.Clips.Count;
				int num = Random.Range(0, count - 1);
				if (num >= entry.PreviousIndex)
				{
					num++;
				}
				audioClip = entry.Clips[num];
				entry.PreviousIndex = num;
			}
			else
			{
				audioClip = entry.Clip;
			}
			return audioClip;
		}
		private void InternalPlaySfxImmediately(AudioClip clip, float volume)
		{
			AudioSource audioSource = this._sourceHolder.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = this._sfxGroup;
			audioSource.volume = volume;
			audioSource.Play();
			Object.Destroy(audioSource, audioSource.clip.length + 1f);
		}
		private void InternalPlaySfxScheduled(AudioClip clip, float volume, double scheduledTime)
		{
			double num = scheduledTime - AudioSettings.dspTime;
			if (num >= 0.0)
			{
				AudioSource audioSource = this._sourceHolder.AddComponent<AudioSource>();
				audioSource.clip = clip;
				audioSource.outputAudioMixerGroup = this._sfxGroup;
				audioSource.volume = volume;
				audioSource.PlayScheduled(scheduledTime);
				Object.Destroy(audioSource, (float)num + audioSource.clip.length + 1f);
				return;
			}
			Debug.LogWarning("一个音效的预定下次播放时间，早于当前时间，舍弃这次预定。");
		}
		public static void PlayUi(string key, bool bgmMixer = false)
		{
			AudioManager.GuardedGetInstance().InternalPlayUi(key, bgmMixer);
		}
		private void InternalPlayUi(string key, bool bgmMixer)
		{
			AudioManager.UiEntry uiEntry;
			if (!this._uiTable.TryGetValue(key, ref uiEntry))
			{
				Debug.LogWarning("Ui sound '" + key + "' not found");
				return;
			}
			if (bgmMixer)
			{
				this._uiSourceInBgmMixer.PlayOneShot(this.GetUiClip(uiEntry), uiEntry.Volume);
				return;
			}
			this._uiSource.PlayOneShot(this.GetUiClip(uiEntry), uiEntry.Volume);
		}
		private AudioClip GetUiClip(AudioManager.UiEntry entry)
		{
			AudioClip audioClip;
			if (entry.Randomized)
			{
				int count = entry.Clips.Count;
				int num = Random.Range(0, count - 1);
				if (num >= entry.PreviousIndex)
				{
					num++;
				}
				audioClip = entry.Clips[num];
				entry.PreviousIndex = num;
			}
			else
			{
				audioClip = entry.Clip;
			}
			return audioClip;
		}
		private void StopUiBgm()
		{
			this._bgmSource.Volume = 1f;
			this._uiSourceInBgmMixer.Stop();
			if (AudioManager._uiBgmCoroutine != null)
			{
				base.StopCoroutine(AudioManager._uiBgmCoroutine);
				AudioManager._uiBgmCoroutine = null;
			}
		}
		private void Update()
		{
			this._bgmSource.OnUpdate();
		}
		private void OnApplicationFocus(bool focus)
		{
			if (this._isApplicationFocused != focus)
			{
				this._isApplicationFocused = focus;
				this.InternalSetMasterVolume(this._masterVolume);
			}
		}
		public static float MasterVolume
		{
			get
			{
				return AudioManager.GuardedGetInstance().GetMasterVolume();
			}
			set
			{
				AudioManager.GuardedGetInstance().SetMasterVolume(value, true);
			}
		}
		public static float BgmVolume
		{
			get
			{
				return AudioManager.GuardedGetInstance().GetVolume("BgmVolume");
			}
			set
			{
				AudioManager.GuardedGetInstance().SetVolume("BgmVolume", value, true);
			}
		}
		public static float UiVolume
		{
			get
			{
				return AudioManager.GuardedGetInstance().GetVolume("UiVolume");
			}
			set
			{
				AudioManager.GuardedGetInstance().SetVolume("UiVolume", value, true);
			}
		}
		public static float SfxVolume
		{
			get
			{
				return AudioManager.GuardedGetInstance().GetVolume("SfxVolume");
			}
			set
			{
				AudioManager.GuardedGetInstance().SetVolume("SfxVolume", value, true);
			}
		}
		public static bool IsBackgroundMute
		{
			get
			{
				return AudioManager.GuardedGetInstance().GetIsBackgroundMute();
			}
			set
			{
				AudioManager.GuardedGetInstance().SetIsBackgroundMute(value);
			}
		}
		public static event Action<BgmConfig> BgmChanged;
		public static void LeaveLayer0()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			if (Singleton<AudioManager>.Instance._bgmSource.Layer0)
			{
				Singleton<AudioManager>.Instance.layer0Time = Singleton<AudioManager>.Instance._bgmSource.Time;
			}
			Singleton<AudioManager>.Instance.inLayer0 = false;
		}
		public static void EnterLayer0()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			AudioManager.FadeOutAndPlayBgm(Singleton<AudioManager>.Instance.layer0Id, 2f, 2f, Singleton<AudioManager>.Instance.layer0Time, true);
			Singleton<AudioManager>.Instance.inLayer0 = true;
		}
		public static void EnterMainMenu()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			Singleton<AudioManager>.Instance.layer0Id = "MainMenu";
			Singleton<AudioManager>.Instance.layer0Time = 0f;
			AudioManager.PlayBgm(Singleton<AudioManager>.Instance.layer0Id, 0f, true);
		}
		public static void EnterStage(int level, bool intoLayer0 = true)
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			if (level > 4 || level < 1)
			{
				Debug.LogWarning("没有对应的场景音乐：目前场景音乐是绑定在Stage的Level值上的");
				return;
			}
			string text = "Stage" + level.ToString();
			if (intoLayer0)
			{
				if (text != Singleton<AudioManager>.Instance.layer0Id)
				{
					Singleton<AudioManager>.Instance.layer0Id = text;
					Singleton<AudioManager>.Instance.layer0Time = 0f;
					Singleton<AudioManager>.Instance.inLayer0 = true;
					AudioManager.PlayBgm(Singleton<AudioManager>.Instance.layer0Id, 0f, true);
					return;
				}
				if (!Singleton<AudioManager>.Instance.inLayer0)
				{
					AudioManager.EnterLayer0();
					return;
				}
			}
			else
			{
				Singleton<AudioManager>.Instance.layer0Id = text;
				Singleton<AudioManager>.Instance.layer0Time = 0f;
			}
		}
		public static void PlayEliteBgm(string enemyId)
		{
			if (!enemyId.IsNullOrEmpty() && BgmConfig.FromID(enemyId) != null)
			{
				AudioManager.PlayInLayer1(enemyId);
				return;
			}
			int act = Singleton<GameMaster>.Instance.CurrentGameRun.CurrentStation.Act;
			if (act == 1 || act == 2)
			{
				AudioManager.PlayInLayer1("Elite2");
				return;
			}
			AudioManager.PlayInLayer1("Elite1");
		}
		public static void PlayBossBgm(string enemyId)
		{
			AudioManager.PlayInLayer1((BgmConfig.FromID(enemyId) != null) ? enemyId : "Elite1");
		}
		public static void PlayShopBgm()
		{
			AudioManager.PlayInLayer1("Shop");
		}
		public static void PlayGapBgm()
		{
			AudioManager.PlayInLayer1("Gap");
		}
		public static void PlayAdventureBgm(int index = 1)
		{
			AudioManager.PlayInLayer1("Adventure" + index.ToString());
		}
		private static void PlayInLayer1(string bgmName)
		{
			AudioManager.LeaveLayer0();
			if (Singleton<AudioManager>.Instance.playingUiBgm)
			{
				Singleton<AudioManager>.Instance.playingUiBgm = false;
				AudioManager.PlayBgm(bgmName, 0f, false);
				return;
			}
			AudioManager.FadeOutAndPlayBgm(bgmName, 1f, 0f, 0f, false);
		}
		public static void Button(int index = 0)
		{
			string text;
			switch (index)
			{
			case 0:
				text = "ButtonClick0";
				break;
			case 1:
				text = "ButtonClick1";
				break;
			case 2:
				text = "ButtonHover";
				break;
			case 3:
				text = "ButtonClickLight0";
				break;
			case 4:
				text = "ButtonClickLight1";
				break;
			case 5:
				text = "ButtonHoverLight";
				break;
			default:
				throw new ArgumentOutOfRangeException("index", index, null);
			}
			AudioManager.PlayUi(text, false);
		}
		public static void Card(int index = 0)
		{
			string text;
			switch (index)
			{
			case 0:
				text = "CardHover";
				break;
			case 1:
				text = "CardHandUse";
				break;
			case 2:
				text = "CardHandCancel";
				break;
			case 3:
				text = "CardShow";
				break;
			case 4:
				text = "CardHide";
				break;
			default:
				throw new ArgumentOutOfRangeException("index", index, null);
			}
			AudioManager.PlayUi(text, false);
		}
		public static void WinBattle()
		{
			AudioManager.PlayUi("WinBattle", true);
			if (Singleton<AudioManager>.Instance.inLayer0)
			{
				AudioManager.CancelBgmCoroutine();
				AudioManager._uiBgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(Singleton<AudioManager>.Instance.BgmVolumeRunner(0.7f, 4.3f, 1f));
				return;
			}
			AudioManager.StopBgm();
			AudioManager._uiBgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(AudioManager.WinEliteRunner());
		}
		private static IEnumerator WinEliteRunner()
		{
			yield return new WaitForSecondsRealtime(5f);
			AudioManager.EnterLayer0();
			AudioManager._uiBgmCoroutine = null;
			yield break;
		}
		private IEnumerator BgmVolumeRunner(float downTime, float bottomTime, float upTime)
		{
			this.playingUiBgm = true;
			float startVol = this._bgmSource.Volume;
			if (this._bgmSource.Clip != null && downTime > 0f)
			{
				float remain = downTime;
				while (remain > 0f)
				{
					remain = Math.Max(remain - Time.deltaTime, 0f);
					this._bgmSource.Volume = remain / downTime * startVol;
					yield return null;
				}
			}
			yield return new WaitForSecondsRealtime(bottomTime);
			this.playingUiBgm = false;
			if (this._bgmSource.Clip != null && upTime > 0f)
			{
				float remain = upTime;
				while (remain > 0f)
				{
					remain = Math.Max(remain - Time.deltaTime, 0f);
					this._bgmSource.Volume = (this._bgmSource.DefaultVolume - remain / upTime) * startVol;
					yield return null;
				}
			}
			AudioManager._bgmCoroutine = null;
			yield break;
		}
		public static void WinBoss()
		{
			AudioManager.FadeOutBgm(0.4f);
			AudioManager.PlayUi("WinBoss", true);
			AudioManager._uiBgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(AudioManager.WinBossRunner());
		}
		private static IEnumerator WinBossRunner()
		{
			yield return new WaitForSecondsRealtime(22f);
			AudioManager.PlayBgm("Adventure2", 0f, false);
			AudioManager._uiBgmCoroutine = null;
			yield break;
		}
		public static void Fail(bool layer1)
		{
			if (layer1)
			{
				AudioManager.PlayInLayer1("Fail");
				return;
			}
			AudioManager.PlayBgm("Fail", 0f, false);
		}
		public static void Victory(bool longVersion)
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			AudioManager.PlayBgm(longVersion ? "VictoryLong" : "Victory", 0f, false);
		}
		public static void EnterMusicRoomFadeOutBgm()
		{
			AudioManager.GuardedGetInstance().InternalEnterMusicRoomFadeOutBgm();
		}
		private void InternalEnterMusicRoomFadeOutBgm()
		{
			DOTween.To(() => this._bgmSource.Volume, delegate(float x)
			{
				this._bgmSource.Volume = x;
			}, 0f, 1f);
		}
		public static void LeaveMusicRoomFadeInBgm()
		{
			AudioManager.GuardedGetInstance().InternalLeaveMusicRoomFadeInBgm();
		}
		private void InternalLeaveMusicRoomFadeInBgm()
		{
			DOTween.To(() => this._bgmSource.Volume, delegate(float x)
			{
				this._bgmSource.Volume = x;
			}, this._bgmSource.DefaultVolume, 1f);
		}
		private const string MasterVolumeName = "MasterVolume";
		private const string BgmVolumeName = "BgmVolume";
		private const string UiVolumeName = "UiVolume";
		private const string SfxVolumeName = "SfxVolume";
		private const string IsBackgroundMuteName = "IsBackgroundMute";
		private readonly Dictionary<string, AudioManager.SfxEntry> _sfxTable = new Dictionary<string, AudioManager.SfxEntry>();
		private static Coroutine _bgmCoroutine;
		private static Coroutine _uiBgmCoroutine;
		private bool _initialized;
		private AudioMixer _mixer;
		private float _masterVolume;
		private AudioMixerGroup _bgmGroup;
		private AudioMixerGroup _sfxGroup;
		private AudioMixerGroup _uiGroup;
		private AudioManager.LoopAudioSource _bgmSource;
		private readonly Dictionary<string, AudioManager.UiEntry> _uiTable = new Dictionary<string, AudioManager.UiEntry>();
		private AudioSource _uiSource;
		private AudioSource _uiSourceInBgmMixer;
		private bool _isBackgroundMute;
		private bool _isApplicationFocused;
		private GameObject _sourceHolder;
		private const bool AllRandom = false;
		private const bool NoReplayLimit = false;
		private const double RePlayRatio = 0.6;
		public string layer0Id;
		public float layer0Time;
		public bool inLayer0 = true;
		public bool playingUiBgm;
		private class SfxEntry
		{
			public SfxEntry(double replayLimit, float volume, AudioClip clip)
			{
				this.ReplayLimit = replayLimit;
				this.Volume = Mathf.Clamp(volume, 0f, 1f);
				this.Clip = clip;
			}
			public SfxEntry(double replayLimit, float volume)
			{
				this.ReplayLimit = replayLimit;
				this.Volume = Mathf.Clamp(volume, 0f, 1f);
				this.Clips = new List<AudioClip>();
				this.Randomized = true;
			}
			public double ReplayLimit { get; }
			public float Volume { get; }
			public AudioClip Clip { get; }
			public AudioSource Source { get; set; }
			public List<AudioClip> Clips { get; }
			public double PreviousPlayTime { get; set; }
			public bool Randomized { get; }
			public int PreviousIndex { get; set; }
		}
		private class UiEntry
		{
			public float Volume { get; }
			public AudioClip Clip { get; }
			public List<AudioClip> Clips { get; }
			public bool Randomized { get; }
			public int PreviousIndex { get; set; }
			public UiEntry(AudioClip clip, float volume)
			{
				this.Clip = clip;
				this.Volume = volume;
			}
			public UiEntry(float volume)
			{
				this.Volume = volume;
				this.Clips = new List<AudioClip>();
				this.Randomized = true;
			}
		}
		private sealed class LoopAudioSource
		{
			public void OnUpdate()
			{
				if (!this._looping)
				{
					return;
				}
				if (AudioSettings.dspTime + (double)((this._loopEnd - this._loopStart) / 2f) > (double)this._nextLoop)
				{
					this.PrepareLoop();
				}
				if (AudioSettings.dspTime > (double)this._currentEnd)
				{
					this._currentIndex = 1 - this._currentIndex;
					this._currentEnd += this._loopEnd - this._loopStart;
				}
			}
			private void PrepareLoop()
			{
				int num = 1 - this._sourceIndex;
				this._sources[num].time = this._loopStart;
				this._sources[num].PlayScheduled((double)this._nextLoop);
				this._sources[num].SetScheduledEndTime((double)(this._nextLoop + (this._loopEnd - this._loopStart)));
				this._nextLoop += this._loopEnd - this._loopStart + this._extraDelay;
				this._sourceIndex = num;
			}
			public LoopAudioSource(GameObject go, AudioMixerGroup group)
			{
				this._sources[0] = go.AddComponent<AudioSource>();
				this._sources[0].outputAudioMixerGroup = group;
				this._sources[1] = go.AddComponent<AudioSource>();
				this._sources[1].outputAudioMixerGroup = group;
			}
			public void SetClip(AudioClip clip, float loopStart, float loopEnd, float extraDelay, float volume = 1f)
			{
				AudioSource audioSource = this._sources[0];
				this._sources[1].clip = clip;
				audioSource.clip = clip;
				this._sources[0].time = (this._sources[1].time = 0f);
				this._loopStart = loopStart;
				this._loopEnd = loopEnd;
				this._extraDelay = extraDelay;
				this.DefaultVolume = volume;
				this._sourceIndex = 0;
				this._currentIndex = 0;
			}
			public AudioClip Clip
			{
				get
				{
					return this._sources[0].clip;
				}
			}
			public void ClearClip()
			{
				this._sources[0].clip = (this._sources[1].clip = null);
			}
			public float Volume
			{
				get
				{
					return this._sources[0].volume;
				}
				set
				{
					AudioSource audioSource = this._sources[0];
					this._sources[1].volume = value;
					audioSource.volume = value;
				}
			}
			public int Priority
			{
				get
				{
					return this._sources[0].priority;
				}
				set
				{
					AudioSource audioSource = this._sources[0];
					this._sources[1].priority = value;
					audioSource.priority = value;
				}
			}
			public float Time
			{
				get
				{
					return this._sources[this._currentIndex].time;
				}
			}
			public bool Layer0 { get; set; }
			public void Play(float start)
			{
				if (start < 0f || start > this._loopEnd - 2f)
				{
					start = 0f;
				}
				this._sources[0].time = start;
				this.Volume = this.DefaultVolume;
				this._nextLoop = (float)AudioSettings.dspTime + this._loopEnd - start;
				this._currentEnd = this._nextLoop;
				this._sources[0].Play();
				this._sources[0].SetScheduledEndTime(AudioSettings.dspTime + (double)this._loopEnd - (double)start);
				this._looping = true;
				this.PrepareLoop();
			}
			public void Stop()
			{
				this._sources[0].Stop();
				this._sources[1].Stop();
			}
			private readonly AudioSource[] _sources = new AudioSource[2];
			private int _sourceIndex;
			public float DefaultVolume;
			private float _loopStart;
			private float _loopEnd;
			private float _extraDelay;
			private float _nextLoop;
			private bool _looping;
			private int _currentIndex;
			private float _currentEnd;
		}
	}
}
