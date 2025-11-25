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
	public static class ResourcesHelper
	{
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
		public static Texture TryGetCardImage(string id)
		{
			Texture texture;
			if (!ResourcesHelper.CardImages.TryGetValue(id, ref texture))
			{
				return null;
			}
			return texture;
		}
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
		public static Sprite TryGetBossIcon(string id)
		{
			Sprite sprite;
			if (!ResourcesHelper.BossIcons.TryGetValue(id, ref sprite))
			{
				return null;
			}
			return sprite;
		}
		public static Sprite TryGetAchievementSprite(string id)
		{
			Sprite sprite;
			if (!ResourcesHelper.AchievementSprites.TryGetValue(id, ref sprite))
			{
				return null;
			}
			return sprite;
		}
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
		public static void Release(AsyncOperationHandle handle)
		{
			Addressables.Release(handle);
		}
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
		public static Sprite LoadCharacterAvatarSprite(string characterName)
		{
			return ResourcesHelper.SyncLoadAddressable<Sprite>(string.Concat(new string[] { "Assets/AA/StandPicture/", characterName, ".png[", characterName, "_Avatar]" }));
		}
		public static async UniTask<Sprite> LoadSpellPortraitAsync(string characterName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Units/SpellPortraits/" + characterName + ".png");
		}
		public static async UniTask<Sprite> LoadSimpleUnitSpriteAsync(string characterName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Units/Sprite/" + characterName + ".png");
		}
		public static async UniTask<SkeletonDataAsset> LoadSpineUnitAsync(string characterName)
		{
			string text = characterName.ToLowerInvariant();
			return await Addressables.LoadAssetAsync<SkeletonDataAsset>(string.Concat(new string[] { "Assets/AA/Units/Spine/", text, "/", text, "_SkeletonData.asset" }));
		}
		public static async UniTask<Sprite> LoadSimpleDollSpriteAsync(string dollName)
		{
			return await Addressables.LoadAssetAsync<Sprite>("Assets/AA/Doll/Sprite/" + dollName + ".png");
		}
		public static async UniTask<AudioClip> LoadBgmAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/Bgm/" + path);
		}
		public static async UniTask<AudioClip> LoadSfxAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/Sfx/" + path);
		}
		public static AudioClip LoadSfx(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<AudioClip>("Assets/AA/Audio/Sfx/" + path);
		}
		public static async UniTask<AudioClip> LoadUiSoundAsync(string path)
		{
			return await Addressables.LoadAssetAsync<AudioClip>("Assets/AA/Audio/UiSound/" + path);
		}
		public static AudioClip LoadUiSound(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<AudioClip>("Assets/AA/Audio/UiSound/" + path);
		}
		public static Texture2D LoadAdventureImage(string name)
		{
			return ResourcesHelper.SyncLoadAddressable<Texture2D>("Assets/AA/AdventureImage/" + name + ".png");
		}
		public static Sprite LoadUiBackground(string bgName)
		{
			return ResourcesHelper.SyncLoadAddressable<Sprite>("Assets/AA/UI/Background/" + bgName + ".png");
		}
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
		public static async UniTask<EffectWidget> LoadEffectAsync(string path)
		{
			return (await Addressables.LoadAssetAsync<GameObject>("Assets/AA/Effect/" + path + ".prefab")).GetComponent<EffectWidget>();
		}
		public static EffectWidget LoadEffect(string path)
		{
			return ResourcesHelper.SyncLoadAddressable<GameObject>("Assets/AA/Effect/" + path + ".prefab").GetComponent<EffectWidget>();
		}
		public static async UniTask<TMP_FontAsset> LoadFontAsync(string path)
		{
			TMP_FontAsset tmp_FontAsset = Resources.Load<TMP_FontAsset>("Fonts/" + path);
			if (!tmp_FontAsset)
			{
				throw new ArgumentException("Cannot load font at " + path);
			}
			return await UniTask.FromResult<TMP_FontAsset>(tmp_FontAsset);
		}
		public static void Release(Object @object)
		{
			Addressables.Release<Object>(@object);
		}
		private static readonly Dictionary<Type, Dictionary<string, Sprite>> Sprites = new Dictionary<Type, Dictionary<string, Sprite>>();
		private static readonly Dictionary<string, Texture> CardImages = new Dictionary<string, Texture>();
		private static readonly Dictionary<string, Sprite> BossIcons = new Dictionary<string, Sprite>();
		private static readonly Dictionary<string, Sprite> IntentionSprites = new Dictionary<string, Sprite>();
		private static readonly Dictionary<string, Sprite> AchievementSprites = new Dictionary<string, Sprite>();
		private static bool _loaded;
	}
}
