using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Presentation.Units;
using Spine;
using UnityEngine;

namespace LBoL.Presentation.Environments
{
	// Token: 0x020000F6 RID: 246
	public class Environment : MonoBehaviour
	{
		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000DF4 RID: 3572 RVA: 0x000430D7 File Offset: 0x000412D7
		// (set) Token: 0x06000DF5 RID: 3573 RVA: 0x000430DE File Offset: 0x000412DE
		public static Environment Instance { get; private set; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000DF6 RID: 3574 RVA: 0x000430E6 File Offset: 0x000412E6
		// (set) Token: 0x06000DF7 RID: 3575 RVA: 0x000430ED File Offset: 0x000412ED
		private static string StageName { get; set; }

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000DF8 RID: 3576 RVA: 0x000430F5 File Offset: 0x000412F5
		// (set) Token: 0x06000DF9 RID: 3577 RVA: 0x000430FC File Offset: 0x000412FC
		private static int StationLevel { get; set; }

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x00043104 File Offset: 0x00041304
		// (set) Token: 0x06000DFB RID: 3579 RVA: 0x0004310B File Offset: 0x0004130B
		private static string CurrentEnvironmentName { get; set; }

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000DFC RID: 3580 RVA: 0x00043113 File Offset: 0x00041313
		// (set) Token: 0x06000DFD RID: 3581 RVA: 0x0004311A File Offset: 0x0004131A
		public static EnvironmentObject CurrentEnvironment { get; set; }

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000DFE RID: 3582 RVA: 0x00043122 File Offset: 0x00041322
		private static global::Spine.AnimationState State
		{
			get
			{
				return Environment.CurrentEnvironment.skeletonAnimation.state;
			}
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x00043134 File Offset: 0x00041334
		private void Awake()
		{
			Environment.Instance = this;
			this.stageRoot.gameObject.SetActive(true);
			this.gapRoot.SetActive(false);
			this.yukariRoom.gameObject.SetActive(false);
			this.yukariRoomSimple.SetActive(false);
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x00043181 File Offset: 0x00041381
		public IEnumerator LoadEnvironment(string stageId, int stationLevel)
		{
			Environment.StageName = stageId;
			Environment.StationLevel = stationLevel;
			StageConfig stageConfig = StageConfig.FromId(Environment.StageName);
			if (stageConfig == null)
			{
				throw new InvalidOperationException("有一个Stage的表格没配：" + Environment.StageName);
			}
			string text;
			if (!stageConfig.Obj4.IsNullOrEmpty() && Environment.StationLevel >= stageConfig.Level4)
			{
				text = stageConfig.Obj4;
			}
			else if (!stageConfig.Obj3.IsNullOrEmpty() && Environment.StationLevel >= stageConfig.Level3)
			{
				text = stageConfig.Obj3;
			}
			else if (!stageConfig.Obj2.IsNullOrEmpty() && Environment.StationLevel >= stageConfig.Level2)
			{
				text = stageConfig.Obj2;
			}
			else if (!stageConfig.Obj1.IsNullOrEmpty() && Environment.StationLevel >= stageConfig.Level1)
			{
				text = stageConfig.Obj1;
			}
			else
			{
				text = stageConfig.Obj0;
			}
			yield return this.LoadEnvironment(text);
			yield break;
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x0004319E File Offset: 0x0004139E
		public IEnumerator LoadEnvironment(string environmentId)
		{
			if (environmentId == Environment.CurrentEnvironmentName && Environment.CurrentEnvironment)
			{
				this.stageRoot.gameObject.SetActive(true);
				yield break;
			}
			EnvironmentObject environmentObject = (GameMaster.IsAnimatingEnvironmentEnabled ? Enumerable.FirstOrDefault<EnvironmentObject>(this.templates, (EnvironmentObject eo) => eo.name == environmentId) : Enumerable.FirstOrDefault<EnvironmentObject>(this.simpleTemplates, (EnvironmentObject eo) => eo.name == environmentId));
			if (environmentObject)
			{
				if (Environment.CurrentEnvironment)
				{
					this.ClearEnvironment();
				}
				Environment.CurrentEnvironmentName = environmentId;
				Environment.CurrentEnvironment = Object.Instantiate<EnvironmentObject>(environmentObject, this.stageRoot);
				if (Environment.CurrentEnvironment.skeletonAnimation)
				{
					Environment.SetSpineAnimationTask().Forget();
				}
				this.stageRoot.gameObject.SetActive(true);
			}
			yield break;
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x000431B4 File Offset: 0x000413B4
		private static async UniTask SetSpineAnimationTask()
		{
			await UniTask.WaitUntil(() => Environment.State != null, PlayerLoopTiming.Update, default(CancellationToken), false);
			IEnumerable<global::Spine.Animation> animations = Environment.State.Data.SkeletonData.Animations;
			int num = 0;
			foreach (global::Spine.Animation animation in Enumerable.Where<global::Spine.Animation>(animations, (global::Spine.Animation anime) => anime.Name.Contains("idle")))
			{
				Environment.State.SetAnimation(num, animation, true);
				num++;
			}
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x000431EF File Offset: 0x000413EF
		public void ClearEnvironment()
		{
			this.stageRoot.gameObject.SetActive(false);
			Object.Destroy(Environment.CurrentEnvironment.gameObject);
			Environment.StageName = null;
			Environment.CurrentEnvironment = null;
			if (Environment._inGapRoom)
			{
				Environment.LeaveGapRoom();
			}
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x0004322C File Offset: 0x0004142C
		public static void EnterGapRoom()
		{
			if (Environment._inGapRoom)
			{
				Debug.LogError("[Environment] Reenter gap room");
				return;
			}
			Environment._inGapRoom = true;
			Environment.Instance.stageRoot.gameObject.SetActive(false);
			Environment.Instance.gapRoot.SetActive(true);
			GameDirector.HideAll();
			if (GameMaster.IsAnimatingEnvironmentEnabled)
			{
				Environment.Instance.yukariRoom.Enter();
			}
			Environment.Instance.yukariRoom.gameObject.SetActive(GameMaster.IsAnimatingEnvironmentEnabled);
			Environment.Instance.yukariRoomSimple.SetActive(!GameMaster.IsAnimatingEnvironmentEnabled);
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x000432C4 File Offset: 0x000414C4
		public static void LeaveGapRoom()
		{
			if (!Environment._inGapRoom)
			{
				Debug.LogError("[Environment] LeaveGapRoom while not in gap room");
				return;
			}
			Environment._inGapRoom = false;
			Environment.Instance.stageRoot.gameObject.SetActive(true);
			Environment.Instance.gapRoot.SetActive(false);
			GameDirector.RevealAll(true);
			if (GameMaster.IsAnimatingEnvironmentEnabled)
			{
				Environment.Instance.yukariRoom.Leave();
			}
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x0004332A File Offset: 0x0004152A
		public static void PlayFinalStageEffect()
		{
			Environment.CurrentEnvironment != null;
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x00043338 File Offset: 0x00041538
		public async UniTask IntoFinalTask()
		{
			IntoFinalEffect effect = Object.Instantiate<IntoFinalEffect>(this.intoFinalEffect);
			await effect.PlayTask();
			Object.Destroy(effect.gameObject, 5f);
		}

		// Token: 0x04000A74 RID: 2676
		[SerializeField]
		private Transform stageRoot;

		// Token: 0x04000A75 RID: 2677
		[SerializeField]
		private GameObject gapRoot;

		// Token: 0x04000A76 RID: 2678
		[SerializeField]
		private YukariRoom yukariRoom;

		// Token: 0x04000A77 RID: 2679
		[SerializeField]
		private GameObject yukariRoomSimple;

		// Token: 0x04000A78 RID: 2680
		[SerializeField]
		private IntoFinalEffect intoFinalEffect;

		// Token: 0x04000A79 RID: 2681
		[SerializeField]
		private List<EnvironmentObject> templates;

		// Token: 0x04000A7A RID: 2682
		[SerializeField]
		private List<EnvironmentObject> simpleTemplates;

		// Token: 0x04000A80 RID: 2688
		private static bool _inGapRoom;
	}
}
