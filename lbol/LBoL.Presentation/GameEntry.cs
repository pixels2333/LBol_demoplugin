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
	public class GameEntry : MonoBehaviour
	{
		public bool IsIntroEnd { get; private set; }
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
		private Sequence FadeIn()
		{
			return DOTween.Sequence().Append(this.aliothStudio.DOFade(1f, 0.6f)).AppendInterval(1.2f)
				.Append(this.aliothStudio.DOFade(0f, 0.6f))
				.Append(this.shanghaiAlice.DOFade(1f, 0.6f))
				.AppendInterval(1.2f);
		}
		private Sequence FadeOut()
		{
			return DOTween.Sequence().Append(this.shanghaiAlice.DOFade(0f, 0.6f));
		}
		private Task InitializeRestAsync()
		{
			GameEntry.<InitializeRestAsync>d__17 <InitializeRestAsync>d__;
			<InitializeRestAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<InitializeRestAsync>d__.<>4__this = this;
			<InitializeRestAsync>d__.<>1__state = -1;
			<InitializeRestAsync>d__.<>t__builder.Start<GameEntry.<InitializeRestAsync>d__17>(ref <InitializeRestAsync>d__);
			return <InitializeRestAsync>d__.<>t__builder.Task;
		}
		private static PlatformHandler _platformHandler;
		[SerializeField]
		private RawImage aliothStudio;
		[SerializeField]
		private RawImage shanghaiAlice;
		[SerializeField]
		private AudioMixer audioMixer;
		[SerializeField]
		private Texture2D cursorTexture;
		[SerializeField]
		private bool showIntro;
		private const float FadeTime = 0.6f;
		private const float ShowTime = 1.2f;
	}
}
