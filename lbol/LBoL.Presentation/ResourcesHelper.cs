using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Effect;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LBoL.Presentation
{
	// Token: 0x02000011 RID: 17
	public static class ResourcesHelper
	{
		// Token: 0x06000170 RID: 368 RVA: 0x00007A48 File Offset: 0x00005C48
		public static async UniTask InitializeAsync()
		{
			if (!ResourcesHelper._loaded)
			{
				await Addressables.InitializeAsync();
				await ResourcesHelper.LoadUiSprites<Exhibit>();
				await ResourcesHelper.LoadUiSprites<StatusEffect>();
				await ResourcesHelper.LoadUiSprites<UltimateSkill>();
				await ResourcesHelper.LoadAllByLabel<Texture>("CardImages", ResourcesHelper.CardImages);
				await ResourcesHelper.LoadAllByLabel<Sprite>("UI/BossIcons", ResourcesHelper.BossIcons);
				await ResourcesHelper.LoadAllByLabel<Sprite>("UI/Intention", ResourcesHelper.IntentionSprites);
				await ResourcesHelper.LoadAllByLabel<Sprite>("UI/Achievement", ResourcesHelper.AchievementSprites);
				ResourcesHelper._loaded = true;
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00007A84 File Offset: 0x00005C84
		internal static async UniTask<AsyncOperationHandle<IList<T>>?> LoadAllByLabel<T>(string label, IDictionary<string, T> dict) where T : Object
		{
			AsyncOperationHandle<IList<T>>? asyncOperationHandle;
			try
			{
				AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, delegate(T _)
				{
				});
				foreach (T t in Enumerable.Distinct<T>(await handle))
				{
					if (dict.ContainsKey(t.name))
					{
						Debug.LogWarning("Loading folder " + label + ": duplicated asset name " + t.name);
					}
					else
					{
						dict.Add(t.name, t);
					}
				}
				asyncOperationHandle = new AsyncOperationHandle<IList<T>>?(handle);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("Error loading folder {0}: {1}", label, ex));
				asyncOperationHandle = default(AsyncOperationHandle<IList<T>>?);
			}
			return asyncOperationHandle;
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00007AD0 File Offset: 0x00005CD0
		private static async UniTask LoadUiSprites<T>()
		{
			Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
			try
			{
				await ResourcesHelper.LoadAllByLabel<Sprite>("UI/" + typeof(T).Name + "s", dict);
			}
			finally
			{
				ResourcesHelper.Sprites.Add(typeof(T), dict);
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00007B0C File Offset: 0x00005D0C
		public static Texture TryGetCardImage(string id)
		{
			Texture texture;
			if (!ResourcesHelper.CardImages.TryGetValue(id, ref texture))
			{
				return null;
			}
			return texture;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00007B2C File Offset: 0x00005D2C
		public static Sprite TryGetSprite<T>(string id)
		{
			Dictionary<string, Sprite> dictionary;
			if (!ResourcesHelper.Sprites.TryGetValue(typeof(T), ref dictionary))
			{
				return null;
			}
			Sprite sprite;
			if (!dictionary.TryGetValue(id, ref sprite))
			{
				return null;
			}
			return sprite;
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00007B64 File Offset: 0x00005D64
		public static Sprite TryGetBossIcon(string id)
		{
			Sprite sprite;
			if (!ResourcesHelper.BossIcons.TryGetValue(id, ref sprite))
			{
				return null;
			}
			return sprite;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00007B84 File Offset: 0x00005D84
		public static Sprite TryGetAchievementSprite(string id)
		{
			Sprite sprite;
			if (!ResourcesHelper.AchievementSprites.TryGetValue(id, ref sprite))
			{
				return null;
			}
			return sprite;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00007BA4 File Offset: 0x00005DA4
		public static Sprite TryGetIntention(string id, string suffix = null)
		{
			if (id.EndsWith("Intention"))
			{
				id = id.RemoveEnd("Intention");
			}
			Sprite sprite;
			if (!ResourcesHelper.IntentionSprites.TryGetValue(id + suffix, ref sprite))
			{
				return null;
			}
			return sprite;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00007BE3 File Offset: 0x00005DE3
		public static void Release(AsyncOperationHandle handle)
		{
			Addressables.Release(handle);
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00007BEC File Offset: 0x00005DEC
		private static T SyncLoadAddressable<T>(object key) where T : Object
		{
			AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
			asyncOperationHandle.WaitForCompletion();
			if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				return asyncOperationHandle.Result;
			}
			throw new ArgumentException(string.Format("Cannot load addressable {0} of type {1}", key, typeof(T)));
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00007C34 File Offset: 0x00005E34
		public static Sprite LoadCharacterAvatarSprite(string characterName)
		{
			return ResourcesHelper.SyncLoadAddressable<Sprite>(string.Concat(new string[] { "Assets/AA/StandPicture/", characterName, ".png[", characterName, "_Avatar]" }));
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00007C68 File Offset: 0x00005E68
		public static async UniTask<Sprite> LoadSpellPortraitAsync(string characterName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Units/SpellPortraits/" + characterName + ".png");
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00007CAC File Offset: 0x00005EAC
		public static async UniTask<Sprite> LoadSimpleUnitSpriteAsync(string characterName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Units/Sprite/" + characterName + ".png");
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00007CF0 File Offset: 0x00005EF0
		public static async UniTask<SkeletonDataAsset> LoadSpineUnitAsync(string characterName)
		{
			string text = characterName.ToLowerInvariant();
			return await Addressables.LoadAssetAsync<SkeletonDataAsset>(string.Concat(new string[] { "Assets/AA/Units/Spine/", text, "/", text, "_SkeletonData.asset" }));
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00007D34 File Offset: 0x00005F34
		public static async UniTask<Sprite> LoadSimpleDollSpriteAsync(string dollName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Doll/Sprite/" + dollName + ".png");
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00007D78 File Offset: 0x00005F78
		public static async UniTask<AudioClip> LoadBgmAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/Bgm/" + path);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00007DBC File Offset: 0x00005FBC
		public static async UniTask<AudioClip> LoadSfxAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/Sfx/" + path);
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00007DFF File Offset: 0x00005FFF
		public static AudioClip LoadSfx(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<AudioClip>("Assets/AA/Audio/Sfx/" + path);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00007E14 File Offset: 0x00006014
		public static async UniTask<AudioClip> LoadUiSoundAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/UiSound/" + path);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00007E57 File Offset: 0x00006057
		public static AudioClip LoadUiSound(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<AudioClip>("Assets/AA/Audio/UiSound/" + path);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00007E69 File Offset: 0x00006069
		public static Texture2D LoadAdventureImage(string name)
		{
			return ResourcesHelper.SyncLoadAddressable<Texture2D>("Assets/AA/AdventureImage/" + name + ".png");
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00007E80 File Offset: 0x00006080
		public static Sprite LoadUiBackground(string bgName)
		{
			return ResourcesHelper.SyncLoadAddressable<Sprite>("Assets/AA/UI/Background/" + bgName + ".png");
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00007E98 File Offset: 0x00006098
		public static async UniTask<GameObject> LoadUiPanelAsync([MaybeNull] string folder, string name)
		{
			string path = ((folder == null) ? name : (folder + "/" + name));
			UnityAsyncExtensions.ResourceRequestAwaiter resourceRequestAwaiter = Resources.LoadAsync<GameObject>("UI/Panels/" + path).GetAwaiter();
			if (!resourceRequestAwaiter.IsCompleted)
			{
				await resourceRequestAwaiter;
				UnityAsyncExtensions.ResourceRequestAwaiter resourceRequestAwaiter2;
				resourceRequestAwaiter = resourceRequestAwaiter2;
				resourceRequestAwaiter2 = default(UnityAsyncExtensions.ResourceRequestAwaiter);
			}
			GameObject gameObject = (GameObject)resourceRequestAwaiter.GetResult();
			if (!gameObject)
			{
				throw new ArgumentException("Cannot load UiPanel at '" + path + "'");
			}
			return gameObject;
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00007EE4 File Offset: 0x000060E4
		public static async UniTask<GameObject> LoadUiDialogAsync([MaybeNull] string folder, string name)
		{
			string path = ((folder == null) ? name : (folder + "/" + name));
			UnityAsyncExtensions.ResourceRequestAwaiter resourceRequestAwaiter = Resources.LoadAsync<GameObject>("UI/Dialogs/" + path).GetAwaiter();
			if (!resourceRequestAwaiter.IsCompleted)
			{
				await resourceRequestAwaiter;
				UnityAsyncExtensions.ResourceRequestAwaiter resourceRequestAwaiter2;
				resourceRequestAwaiter = resourceRequestAwaiter2;
				resourceRequestAwaiter2 = default(UnityAsyncExtensions.ResourceRequestAwaiter);
			}
			GameObject gameObject = (GameObject)resourceRequestAwaiter.GetResult();
			if (!gameObject)
			{
				throw new ArgumentException("Cannot load UiDialog at '" + path + "'");
			}
			return gameObject;
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00007F30 File Offset: 0x00006130
		public static async UniTask<EffectWidget> LoadEffectAsync(string path)
		{
			return (await Addressables.LoadAssetAsync<GameObject>("Assets/AA/Effect/" + path + ".prefab")).GetComponent<EffectWidget>();
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00007F73 File Offset: 0x00006173
		public static EffectWidget LoadEffect(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<GameObject>("Assets/AA/Effect/" + path + ".prefab").GetComponent<EffectWidget>();
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00007F90 File Offset: 0x00006190
		public static async UniTask<TMP_FontAsset> LoadFontAsync(string path)
		{
			TMP_FontAsset tmp_FontAsset = Resources.Load<TMP_FontAsset>("Fonts/" + path);
			if (!tmp_FontAsset)
			{
				throw new ArgumentException("Cannot load font at " + path);
			}
			return await UniTask.FromResult<TMP_FontAsset>(tmp_FontAsset);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x00007FD3 File Offset: 0x000061D3
		public static void Release(Object @object)
		{
			Addressables.Release<Object>(@object);
		}

		// Token: 0x04000068 RID: 104
		private static readonly Dictionary<Type, Dictionary<string, Sprite>> Sprites = new Dictionary<Type, Dictionary<string, Sprite>>();

		// Token: 0x04000069 RID: 105
		private static readonly Dictionary<string, Texture> CardImages = new Dictionary<string, Texture>();

		// Token: 0x0400006A RID: 106
		private static readonly Dictionary<string, Sprite> BossIcons = new Dictionary<string, Sprite>();

		// Token: 0x0400006B RID: 107
		private static readonly Dictionary<string, Sprite> IntentionSprites = new Dictionary<string, Sprite>();

		// Token: 0x0400006C RID: 108
		private static readonly Dictionary<string, Sprite> AchievementSprites = new Dictionary<string, Sprite>();

		// Token: 0x0400006D RID: 109
		private static bool _loaded;
	}
}
