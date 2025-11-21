using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.PlatformHandlers;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace LBoL.Presentation
{
	// Token: 0x02000009 RID: 9
	public class GameEntry : MonoBehaviour
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000077 RID: 119 RVA: 0x00003616 File Offset: 0x00001816
		// (set) Token: 0x06000078 RID: 120 RVA: 0x0000361E File Offset: 0x0000181E
		public bool IsIntroEnd { get; private set; }

		// Token: 0x06000079 RID: 121 RVA: 0x00003628 File Offset: 0x00001828
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void InitBeforeLogo()
		{
			PlatformHandler platformHandler = new SteamPlatformHandler();
			if (!platformHandler.Init())
			{
				Application.Quit();
				return;
			}
			GameEntry._platformHandler = platformHandler;
			Vector2Int vector2Int = new Vector2Int(Screen.width, Screen.height);
			if (!ResolutionHelper.IsValidAspect(vector2Int))
			{
				IReadOnlyList<Vector2Int> availableResolutions = ResolutionHelper.GetAvailableResolutions();
				if (availableResolutions.Count == 0)
				{
					Debug.LogError(string.Format("No avaliable resolutions found for screen size {0} x {1}", vector2Int.x, vector2Int.y));
					return;
				}
				Vector2Int vector2Int2 = Enumerable.Last<Vector2Int>(availableResolutions);
				Debug.LogWarning(string.Format("Window size is invalid for this game, trying to resize to {0} x {1}", vector2Int2.x, vector2Int2.y));
				Screen.SetResolution(vector2Int2.x, vector2Int2.y, Screen.fullScreen);
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x000036E8 File Offset: 0x000018E8
		private void Awake()
		{
			GameObject gameObject = new GameObject("[Platform Runner]");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<PlatformHandlerRunner>().PlatformHandler = GameEntry._platformHandler;
			DOTween.Init(default(bool?), default(bool?), default(LogBehaviour?)).SetCapacity(300, 30);
			this.aliothStudio.gameObject.SetActive(true);
			this.shanghaiAlice.gameObject.SetActive(true);
			this.aliothStudio.color = this.aliothStudio.color.WithA(0f);
			this.shanghaiAlice.color = this.shanghaiAlice.color.WithA(0f);
			this.StartAsync();
		}

		// Token: 0x0600007B RID: 123 RVA: 0x000037AC File Offset: 0x000019AC
		private async UniTask StartAsync()
		{
			Task initializeCoroutine = this.InitializeRestAsync();
			if (!Application.isEditor || this.showIntro)
			{
				await this.FadeIn();
			}
			if (!Application.isEditor || this.showIntro)
			{
				await this.FadeOut();
			}
			this.IsIntroEnd = true;
			await initializeCoroutine;
		}

		// Token: 0x0600007C RID: 124 RVA: 0x000037F0 File Offset: 0x000019F0
		private Sequence FadeIn()
		{
			return DOTween.Sequence().Append(this.aliothStudio.DOFade(1f, 0.6f)).AppendInterval(1.2f)
				.Append(this.aliothStudio.DOFade(0f, 0.6f))
				.Append(this.shanghaiAlice.DOFade(1f, 0.6f))
				.AppendInterval(1.2f);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00003864 File Offset: 0x00001A64
		private Sequence FadeOut()
		{
			return DOTween.Sequence().Append(this.shanghaiAlice.DOFade(0f, 0.6f));
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003888 File Offset: 0x00001A88
		private Task InitializeRestAsync()
		{
			GameEntry.<InitializeRestAsync>d__17 <InitializeRestAsync>d__;
			<InitializeRestAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<InitializeRestAsync>d__.<>4__this = this;
			<InitializeRestAsync>d__.<>1__state = -1;
			<InitializeRestAsync>d__.<>t__builder.Start<GameEntry.<InitializeRestAsync>d__17>(ref <InitializeRestAsync>d__);
			return <InitializeRestAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0400002F RID: 47
		private static PlatformHandler _platformHandler;

		// Token: 0x04000030 RID: 48
		[SerializeField]
		private RawImage aliothStudio;

		// Token: 0x04000031 RID: 49
		[SerializeField]
		private RawImage shanghaiAlice;

		// Token: 0x04000032 RID: 50
		[SerializeField]
		private AudioMixer audioMixer;

		// Token: 0x04000033 RID: 51
		[SerializeField]
		private Texture2D cursorTexture;

		// Token: 0x04000034 RID: 52
		[SerializeField]
		private bool showIntro;

		// Token: 0x04000036 RID: 54
		private const float FadeTime = 0.6f;

		// Token: 0x04000037 RID: 55
		private const float ShowTime = 1.2f;
	}
}
