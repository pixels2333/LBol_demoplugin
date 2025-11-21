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
	// Token: 0x02000007 RID: 7
	public class AudioManager : Singleton<AudioManager>
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001A RID: 26 RVA: 0x0000257E File Offset: 0x0000077E
		public static AudioMixerGroup BgmGroup
		{
			get
			{
				return AudioManager.GuardedGetInstance()._bgmGroup;
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000258A File Offset: 0x0000078A
		private void EnsureInitialized()
		{
			if (!this._initialized)
			{
				throw new InvalidOperationException("AudioManager.InitializeAsync must be called before use.");
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000259F File Offset: 0x0000079F
		private static AudioManager GuardedGetInstance()
		{
			AudioManager instance = Singleton<AudioManager>.Instance;
			instance.EnsureInitialized();
			return instance;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000025AC File Offset: 0x000007AC
		public static UniTask InitializeAsync(AudioMixer mixer)
		{
			return Singleton<AudioManager>.Instance.InternalInitializeAsync(mixer);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000025B9 File Offset: 0x000007B9
		private void InitVolume(string volumeName)
		{
			if (PlayerPrefs.HasKey(volumeName))
			{
				this.SetVolume(volumeName, PlayerPrefs.GetFloat(volumeName), false);
				return;
			}
			PlayerPrefs.SetFloat(volumeName, this.GetVolume(volumeName));
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000025E0 File Offset: 0x000007E0
		private float GetVolume(string volumeName)
		{
			float num;
			if (!this._mixer.GetFloat(volumeName, out num))
			{
				throw new ArgumentException("Cannot get '" + volumeName + "' of AudioMixer", "volumeName");
			}
			return Mathf.Clamp01(Mathf.Pow(10f, num / 20f));
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000262E File Offset: 0x0000082E
		private float GetMasterVolume()
		{
			return this.GetVolume("MasterVolume");
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000263C File Offset: 0x0000083C
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

		// Token: 0x06000022 RID: 34 RVA: 0x0000269C File Offset: 0x0000089C
		private void InternalSetMasterVolume(float value)
		{
			value = (this._isBackgroundMute ? (this._isApplicationFocused ? value : 0f) : value);
			float num = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
			if (!this._mixer.SetFloat("MasterVolume", num))
			{
				throw new ArgumentException("Cannot set 'MasterVolume' of AudioMixer", "MasterVolume");
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002705 File Offset: 0x00000905
		private void SetMasterVolume(float value, bool setPrefs = false)
		{
			this.InternalSetMasterVolume(value);
			this._masterVolume = value;
			if (setPrefs)
			{
				PlayerPrefs.SetFloat("MasterVolume", value);
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002723 File Offset: 0x00000923
		private bool GetIsBackgroundMute()
		{
			return this._isBackgroundMute;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000272B File Offset: 0x0000092B
		private void SetIsBackgroundMute(bool mute)
		{
			if (mute != this._isBackgroundMute)
			{
				this._isBackgroundMute = mute;
				this.InternalSetMasterVolume(this._masterVolume);
				PlayerPrefs.SetInt("IsBackgroundMute", mute ? 1 : 0);
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x0000275C File Offset: 0x0000095C
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

		// Token: 0x06000027 RID: 39 RVA: 0x000027A7 File Offset: 0x000009A7
		private static void CancelBgmCoroutine()
		{
			Singleton<AudioManager>.Instance._bgmSource.Volume = 1f;
			if (AudioManager._bgmCoroutine != null)
			{
				Singleton<AudioManager>.Instance.StopCoroutine(AudioManager._bgmCoroutine);
				AudioManager._bgmCoroutine = null;
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000027D9 File Offset: 0x000009D9
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

		// Token: 0x06000029 RID: 41 RVA: 0x0000280D File Offset: 0x00000A0D
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

		// Token: 0x0600002A RID: 42 RVA: 0x00002823 File Offset: 0x00000A23
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

		// Token: 0x0600002B RID: 43 RVA: 0x00002839 File Offset: 0x00000A39
		public static void FadeOutBgm(float interval)
		{
			AudioManager.GuardedGetInstance().InternalFadeOutBgm(interval);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002846 File Offset: 0x00000A46
		private void InternalFadeOutBgm(float interval)
		{
			AudioManager.CancelBgmCoroutine();
			AudioManager._bgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(this.CoFadeOutBgm(interval));
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002863 File Offset: 0x00000A63
		public static void StopBgm()
		{
			AudioManager.GuardedGetInstance().InternalStopBgm();
		}

		// Token: 0x0600002E RID: 46 RVA: 0x0000286F File Offset: 0x00000A6F
		private void InternalStopBgm()
		{
			AudioManager.CancelBgmCoroutine();
			if (this._bgmSource.Clip)
			{
				ResourcesHelper.Release(this._bgmSource.Clip);
				this._bgmSource.ClearClip();
			}
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000028A3 File Offset: 0x00000AA3
		public static void PlayBgm(string id, float startTime = 0f, bool layer0 = false)
		{
			AudioManager.FadeOutAndPlayBgm(id, 0f, 0f, startTime, layer0);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000028B7 File Offset: 0x00000AB7
		public static void FadeOutAndPlayBgm(string id, float fadeOutInterval = 1f, float fadeInInterval = 0f, float startTime = 0f, bool layer0 = false)
		{
			AudioManager.GuardedGetInstance().InternalFadeOutAndPlayBgm(id, fadeOutInterval, fadeInInterval, startTime, layer0);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000028CC File Offset: 0x00000ACC
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

		// Token: 0x06000032 RID: 50 RVA: 0x0000296D File Offset: 0x00000B6D
		public static void PlaySfx(string sfxName, float volume = -1f)
		{
			AudioManager.GuardedGetInstance().PlaySfxHandler(sfxName, volume);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x0000297B File Offset: 0x00000B7B
		public static void PlaySfxDelay(string sfxName, float delay)
		{
			AudioManager.GuardedGetInstance().StartCoroutine(AudioManager.PlaySfxDelayRunner(sfxName, delay));
		}

		// Token: 0x06000034 RID: 52 RVA: 0x0000298F File Offset: 0x00000B8F
		private static IEnumerator PlaySfxDelayRunner(string sfxName, float delay)
		{
			yield return new WaitForSeconds(delay);
			AudioManager.PlaySfx(sfxName, -1f);
			yield break;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000029A8 File Offset: 0x00000BA8
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

		// Token: 0x06000036 RID: 54 RVA: 0x00002A34 File Offset: 0x00000C34
		private void InternalPlaySfx(AudioManager.SfxEntry entry, float volume)
		{
			AudioClip sfxClip = this.GetSfxClip(entry);
			this.InternalPlaySfxImmediately(sfxClip, (volume > 0f) ? (volume * entry.Volume) : entry.Volume);
			entry.PreviousPlayTime = AudioSettings.dspTime;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002A74 File Offset: 0x00000C74
		private void InternalPlaySfx(AudioManager.SfxEntry entry, float volume, double scheduledTime)
		{
			AudioClip sfxClip = this.GetSfxClip(entry);
			this.InternalPlaySfxScheduled(sfxClip, (volume > 0f) ? volume : entry.Volume, scheduledTime);
			entry.PreviousPlayTime = scheduledTime;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002AAC File Offset: 0x00000CAC
		private void InternalPlaySfxOneMore(AudioManager.SfxEntry entry, float volume)
		{
			double num = entry.PreviousPlayTime + entry.ReplayLimit;
			this.InternalPlaySfx(entry, volume, num);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002AD0 File Offset: 0x00000CD0
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

		// Token: 0x0600003A RID: 58 RVA: 0x00002B28 File Offset: 0x00000D28
		private void InternalPlaySfxImmediately(AudioClip clip, float volume)
		{
			AudioSource audioSource = this._sourceHolder.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = this._sfxGroup;
			audioSource.volume = volume;
			audioSource.Play();
			Object.Destroy(audioSource, audioSource.clip.length + 1f);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002B78 File Offset: 0x00000D78
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

		// Token: 0x0600003C RID: 60 RVA: 0x00002BEB File Offset: 0x00000DEB
		public static void PlayUi(string key, bool bgmMixer = false)
		{
			AudioManager.GuardedGetInstance().InternalPlayUi(key, bgmMixer);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002BFC File Offset: 0x00000DFC
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

		// Token: 0x0600003E RID: 62 RVA: 0x00002C64 File Offset: 0x00000E64
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

		// Token: 0x0600003F RID: 63 RVA: 0x00002CBA File Offset: 0x00000EBA
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

		// Token: 0x06000040 RID: 64 RVA: 0x00002CEF File Offset: 0x00000EEF
		private void Update()
		{
			this._bgmSource.OnUpdate();
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00002CFC File Offset: 0x00000EFC
		private void OnApplicationFocus(bool focus)
		{
			if (this._isApplicationFocused != focus)
			{
				this._isApplicationFocused = focus;
				this.InternalSetMasterVolume(this._masterVolume);
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00002D1A File Offset: 0x00000F1A
		// (set) Token: 0x06000043 RID: 67 RVA: 0x00002D26 File Offset: 0x00000F26
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

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00002D34 File Offset: 0x00000F34
		// (set) Token: 0x06000045 RID: 69 RVA: 0x00002D45 File Offset: 0x00000F45
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

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00002D58 File Offset: 0x00000F58
		// (set) Token: 0x06000047 RID: 71 RVA: 0x00002D69 File Offset: 0x00000F69
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

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00002D7C File Offset: 0x00000F7C
		// (set) Token: 0x06000049 RID: 73 RVA: 0x00002D8D File Offset: 0x00000F8D
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

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00002DA0 File Offset: 0x00000FA0
		// (set) Token: 0x0600004B RID: 75 RVA: 0x00002DAC File Offset: 0x00000FAC
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

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600004C RID: 76 RVA: 0x00002DBC File Offset: 0x00000FBC
		// (remove) Token: 0x0600004D RID: 77 RVA: 0x00002DF0 File Offset: 0x00000FF0
		public static event Action<BgmConfig> BgmChanged;

		// Token: 0x0600004E RID: 78 RVA: 0x00002E24 File Offset: 0x00001024
		public static void LeaveLayer0()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			if (Singleton<AudioManager>.Instance._bgmSource.Layer0)
			{
				Singleton<AudioManager>.Instance.layer0Time = Singleton<AudioManager>.Instance._bgmSource.Time;
			}
			Singleton<AudioManager>.Instance.inLayer0 = false;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002E70 File Offset: 0x00001070
		public static void EnterLayer0()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			AudioManager.FadeOutAndPlayBgm(Singleton<AudioManager>.Instance.layer0Id, 2f, 2f, Singleton<AudioManager>.Instance.layer0Time, true);
			Singleton<AudioManager>.Instance.inLayer0 = true;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002EAB File Offset: 0x000010AB
		public static void EnterMainMenu()
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			Singleton<AudioManager>.Instance.layer0Id = "MainMenu";
			Singleton<AudioManager>.Instance.layer0Time = 0f;
			AudioManager.PlayBgm(Singleton<AudioManager>.Instance.layer0Id, 0f, true);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00002EEC File Offset: 0x000010EC
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

		// Token: 0x06000052 RID: 82 RVA: 0x00002FA4 File Offset: 0x000011A4
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

		// Token: 0x06000053 RID: 83 RVA: 0x00002FFA File Offset: 0x000011FA
		public static void PlayBossBgm(string enemyId)
		{
			AudioManager.PlayInLayer1((BgmConfig.FromID(enemyId) != null) ? enemyId : "Elite1");
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003011 File Offset: 0x00001211
		public static void PlayShopBgm()
		{
			AudioManager.PlayInLayer1("Shop");
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000301D File Offset: 0x0000121D
		public static void PlayGapBgm()
		{
			AudioManager.PlayInLayer1("Gap");
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003029 File Offset: 0x00001229
		public static void PlayAdventureBgm(int index = 1)
		{
			AudioManager.PlayInLayer1("Adventure" + index.ToString());
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003044 File Offset: 0x00001244
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

		// Token: 0x06000058 RID: 88 RVA: 0x00003090 File Offset: 0x00001290
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

		// Token: 0x06000059 RID: 89 RVA: 0x00003108 File Offset: 0x00001308
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

		// Token: 0x0600005A RID: 90 RVA: 0x00003174 File Offset: 0x00001374
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

		// Token: 0x0600005B RID: 91 RVA: 0x000031DF File Offset: 0x000013DF
		private static IEnumerator WinEliteRunner()
		{
			yield return new WaitForSecondsRealtime(5f);
			AudioManager.EnterLayer0();
			AudioManager._uiBgmCoroutine = null;
			yield break;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000031E7 File Offset: 0x000013E7
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

		// Token: 0x0600005D RID: 93 RVA: 0x0000320B File Offset: 0x0000140B
		public static void WinBoss()
		{
			AudioManager.FadeOutBgm(0.4f);
			AudioManager.PlayUi("WinBoss", true);
			AudioManager._uiBgmCoroutine = Singleton<AudioManager>.Instance.StartCoroutine(AudioManager.WinBossRunner());
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003236 File Offset: 0x00001436
		private static IEnumerator WinBossRunner()
		{
			yield return new WaitForSecondsRealtime(22f);
			AudioManager.PlayBgm("Adventure2", 0f, false);
			AudioManager._uiBgmCoroutine = null;
			yield break;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x0000323E File Offset: 0x0000143E
		public static void Fail(bool layer1)
		{
			if (layer1)
			{
				AudioManager.PlayInLayer1("Fail");
				return;
			}
			AudioManager.PlayBgm("Fail", 0f, false);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000325E File Offset: 0x0000145E
		public static void Victory(bool longVersion)
		{
			AudioManager.GuardedGetInstance().StopUiBgm();
			AudioManager.PlayBgm(longVersion ? "VictoryLong" : "Victory", 0f, false);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003284 File Offset: 0x00001484
		public static void EnterMusicRoomFadeOutBgm()
		{
			AudioManager.GuardedGetInstance().InternalEnterMusicRoomFadeOutBgm();
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003290 File Offset: 0x00001490
		private void InternalEnterMusicRoomFadeOutBgm()
		{
			DOTween.To(() => this._bgmSource.Volume, delegate(float x)
			{
				this._bgmSource.Volume = x;
			}, 0f, 1f);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000032BA File Offset: 0x000014BA
		public static void LeaveMusicRoomFadeInBgm()
		{
			AudioManager.GuardedGetInstance().InternalLeaveMusicRoomFadeInBgm();
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000032C6 File Offset: 0x000014C6
		private void InternalLeaveMusicRoomFadeInBgm()
		{
			DOTween.To(() => this._bgmSource.Volume, delegate(float x)
			{
				this._bgmSource.Volume = x;
			}, this._bgmSource.DefaultVolume, 1f);
		}

		// Token: 0x0400000B RID: 11
		private const string MasterVolumeName = "MasterVolume";

		// Token: 0x0400000C RID: 12
		private const string BgmVolumeName = "BgmVolume";

		// Token: 0x0400000D RID: 13
		private const string UiVolumeName = "UiVolume";

		// Token: 0x0400000E RID: 14
		private const string SfxVolumeName = "SfxVolume";

		// Token: 0x0400000F RID: 15
		private const string IsBackgroundMuteName = "IsBackgroundMute";

		// Token: 0x04000010 RID: 16
		private readonly Dictionary<string, AudioManager.SfxEntry> _sfxTable = new Dictionary<string, AudioManager.SfxEntry>();

		// Token: 0x04000011 RID: 17
		private static Coroutine _bgmCoroutine;

		// Token: 0x04000012 RID: 18
		private static Coroutine _uiBgmCoroutine;

		// Token: 0x04000013 RID: 19
		private bool _initialized;

		// Token: 0x04000014 RID: 20
		private AudioMixer _mixer;

		// Token: 0x04000015 RID: 21
		private float _masterVolume;

		// Token: 0x04000016 RID: 22
		private AudioMixerGroup _bgmGroup;

		// Token: 0x04000017 RID: 23
		private AudioMixerGroup _sfxGroup;

		// Token: 0x04000018 RID: 24
		private AudioMixerGroup _uiGroup;

		// Token: 0x04000019 RID: 25
		private AudioManager.LoopAudioSource _bgmSource;

		// Token: 0x0400001A RID: 26
		private readonly Dictionary<string, AudioManager.UiEntry> _uiTable = new Dictionary<string, AudioManager.UiEntry>();

		// Token: 0x0400001B RID: 27
		private AudioSource _uiSource;

		// Token: 0x0400001C RID: 28
		private AudioSource _uiSourceInBgmMixer;

		// Token: 0x0400001D RID: 29
		private bool _isBackgroundMute;

		// Token: 0x0400001E RID: 30
		private bool _isApplicationFocused;

		// Token: 0x0400001F RID: 31
		private GameObject _sourceHolder;

		// Token: 0x04000020 RID: 32
		private const bool AllRandom = false;

		// Token: 0x04000021 RID: 33
		private const bool NoReplayLimit = false;

		// Token: 0x04000022 RID: 34
		private const double RePlayRatio = 0.6;

		// Token: 0x04000024 RID: 36
		public string layer0Id;

		// Token: 0x04000025 RID: 37
		public float layer0Time;

		// Token: 0x04000026 RID: 38
		public bool inLayer0 = true;

		// Token: 0x04000027 RID: 39
		public bool playingUiBgm;

		// Token: 0x0200011B RID: 283
		private class SfxEntry
		{
			// Token: 0x06000FE8 RID: 4072 RVA: 0x0004A999 File Offset: 0x00048B99
			public SfxEntry(double replayLimit, float volume, AudioClip clip)
			{
				this.ReplayLimit = replayLimit;
				this.Volume = Mathf.Clamp(volume, 0f, 1f);
				this.Clip = clip;
			}

			// Token: 0x06000FE9 RID: 4073 RVA: 0x0004A9C5 File Offset: 0x00048BC5
			public SfxEntry(double replayLimit, float volume)
			{
				this.ReplayLimit = replayLimit;
				this.Volume = Mathf.Clamp(volume, 0f, 1f);
				this.Clips = new List<AudioClip>();
				this.Randomized = true;
			}

			// Token: 0x1700030C RID: 780
			// (get) Token: 0x06000FEA RID: 4074 RVA: 0x0004A9FC File Offset: 0x00048BFC
			public double ReplayLimit { get; }

			// Token: 0x1700030D RID: 781
			// (get) Token: 0x06000FEB RID: 4075 RVA: 0x0004AA04 File Offset: 0x00048C04
			public float Volume { get; }

			// Token: 0x1700030E RID: 782
			// (get) Token: 0x06000FEC RID: 4076 RVA: 0x0004AA0C File Offset: 0x00048C0C
			public AudioClip Clip { get; }

			// Token: 0x1700030F RID: 783
			// (get) Token: 0x06000FED RID: 4077 RVA: 0x0004AA14 File Offset: 0x00048C14
			// (set) Token: 0x06000FEE RID: 4078 RVA: 0x0004AA1C File Offset: 0x00048C1C
			public AudioSource Source { get; set; }

			// Token: 0x17000310 RID: 784
			// (get) Token: 0x06000FEF RID: 4079 RVA: 0x0004AA25 File Offset: 0x00048C25
			public List<AudioClip> Clips { get; }

			// Token: 0x17000311 RID: 785
			// (get) Token: 0x06000FF0 RID: 4080 RVA: 0x0004AA2D File Offset: 0x00048C2D
			// (set) Token: 0x06000FF1 RID: 4081 RVA: 0x0004AA35 File Offset: 0x00048C35
			public double PreviousPlayTime { get; set; }

			// Token: 0x17000312 RID: 786
			// (get) Token: 0x06000FF2 RID: 4082 RVA: 0x0004AA3E File Offset: 0x00048C3E
			public bool Randomized { get; }

			// Token: 0x17000313 RID: 787
			// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x0004AA46 File Offset: 0x00048C46
			// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x0004AA4E File Offset: 0x00048C4E
			public int PreviousIndex { get; set; }
		}

		// Token: 0x0200011C RID: 284
		private class UiEntry
		{
			// Token: 0x17000314 RID: 788
			// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x0004AA57 File Offset: 0x00048C57
			public float Volume { get; }

			// Token: 0x17000315 RID: 789
			// (get) Token: 0x06000FF6 RID: 4086 RVA: 0x0004AA5F File Offset: 0x00048C5F
			public AudioClip Clip { get; }

			// Token: 0x17000316 RID: 790
			// (get) Token: 0x06000FF7 RID: 4087 RVA: 0x0004AA67 File Offset: 0x00048C67
			public List<AudioClip> Clips { get; }

			// Token: 0x17000317 RID: 791
			// (get) Token: 0x06000FF8 RID: 4088 RVA: 0x0004AA6F File Offset: 0x00048C6F
			public bool Randomized { get; }

			// Token: 0x17000318 RID: 792
			// (get) Token: 0x06000FF9 RID: 4089 RVA: 0x0004AA77 File Offset: 0x00048C77
			// (set) Token: 0x06000FFA RID: 4090 RVA: 0x0004AA7F File Offset: 0x00048C7F
			public int PreviousIndex { get; set; }

			// Token: 0x06000FFB RID: 4091 RVA: 0x0004AA88 File Offset: 0x00048C88
			public UiEntry(AudioClip clip, float volume)
			{
				this.Clip = clip;
				this.Volume = volume;
			}

			// Token: 0x06000FFC RID: 4092 RVA: 0x0004AAAD File Offset: 0x00048CAD
			public UiEntry(float volume)
			{
				this.Volume = volume;
				this.Clips = new List<AudioClip>();
				this.Randomized = true;
			}
		}

		// Token: 0x0200011D RID: 285
		private sealed class LoopAudioSource
		{
			// Token: 0x06000FFD RID: 4093 RVA: 0x0004AAD0 File Offset: 0x00048CD0
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

			// Token: 0x06000FFE RID: 4094 RVA: 0x0004AB48 File Offset: 0x00048D48
			private void PrepareLoop()
			{
				int num = 1 - this._sourceIndex;
				this._sources[num].time = this._loopStart;
				this._sources[num].PlayScheduled((double)this._nextLoop);
				this._sources[num].SetScheduledEndTime((double)(this._nextLoop + (this._loopEnd - this._loopStart)));
				this._nextLoop += this._loopEnd - this._loopStart + this._extraDelay;
				this._sourceIndex = num;
			}

			// Token: 0x06000FFF RID: 4095 RVA: 0x0004ABD0 File Offset: 0x00048DD0
			public LoopAudioSource(GameObject go, AudioMixerGroup group)
			{
				this._sources[0] = go.AddComponent<AudioSource>();
				this._sources[0].outputAudioMixerGroup = group;
				this._sources[1] = go.AddComponent<AudioSource>();
				this._sources[1].outputAudioMixerGroup = group;
			}

			// Token: 0x06001000 RID: 4096 RVA: 0x0004AC28 File Offset: 0x00048E28
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

			// Token: 0x17000319 RID: 793
			// (get) Token: 0x06001001 RID: 4097 RVA: 0x0004ACA1 File Offset: 0x00048EA1
			public AudioClip Clip
			{
				get
				{
					return this._sources[0].clip;
				}
			}

			// Token: 0x06001002 RID: 4098 RVA: 0x0004ACB0 File Offset: 0x00048EB0
			public void ClearClip()
			{
				this._sources[0].clip = (this._sources[1].clip = null);
			}

			// Token: 0x1700031A RID: 794
			// (get) Token: 0x06001003 RID: 4099 RVA: 0x0004ACDB File Offset: 0x00048EDB
			// (set) Token: 0x06001004 RID: 4100 RVA: 0x0004ACEC File Offset: 0x00048EEC
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

			// Token: 0x1700031B RID: 795
			// (get) Token: 0x06001005 RID: 4101 RVA: 0x0004AD17 File Offset: 0x00048F17
			// (set) Token: 0x06001006 RID: 4102 RVA: 0x0004AD28 File Offset: 0x00048F28
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

			// Token: 0x1700031C RID: 796
			// (get) Token: 0x06001007 RID: 4103 RVA: 0x0004AD53 File Offset: 0x00048F53
			public float Time
			{
				get
				{
					return this._sources[this._currentIndex].time;
				}
			}

			// Token: 0x1700031D RID: 797
			// (get) Token: 0x06001008 RID: 4104 RVA: 0x0004AD67 File Offset: 0x00048F67
			// (set) Token: 0x06001009 RID: 4105 RVA: 0x0004AD6F File Offset: 0x00048F6F
			public bool Layer0 { get; set; }

			// Token: 0x0600100A RID: 4106 RVA: 0x0004AD78 File Offset: 0x00048F78
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

			// Token: 0x0600100B RID: 4107 RVA: 0x0004AE15 File Offset: 0x00049015
			public void Stop()
			{
				this._sources[0].Stop();
				this._sources[1].Stop();
			}

			// Token: 0x04000BE2 RID: 3042
			private readonly AudioSource[] _sources = new AudioSource[2];

			// Token: 0x04000BE3 RID: 3043
			private int _sourceIndex;

			// Token: 0x04000BE4 RID: 3044
			public float DefaultVolume;

			// Token: 0x04000BE5 RID: 3045
			private float _loopStart;

			// Token: 0x04000BE6 RID: 3046
			private float _loopEnd;

			// Token: 0x04000BE7 RID: 3047
			private float _extraDelay;

			// Token: 0x04000BE8 RID: 3048
			private float _nextLoop;

			// Token: 0x04000BE9 RID: 3049
			private bool _looping;

			// Token: 0x04000BEA RID: 3050
			private int _currentIndex;

			// Token: 0x04000BEB RID: 3051
			private float _currentEnd;
		}
	}
}
