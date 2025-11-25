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
	public class Environment : MonoBehaviour
	{
		public static Environment Instance { get; private set; }
		private static string StageName { get; set; }
		private static int StationLevel { get; set; }
		private static string CurrentEnvironmentName { get; set; }
		public static EnvironmentObject CurrentEnvironment { get; set; }
		private static global::Spine.AnimationState State
		{
			get
			{
				return Environment.CurrentEnvironment.skeletonAnimation.state;
			}
		}
		private void Awake()
		{
			Environment.Instance = this;
			this.stageRoot.gameObject.SetActive(true);
			this.gapRoot.SetActive(false);
			this.yukariRoom.gameObject.SetActive(false);
			this.yukariRoomSimple.SetActive(false);
		}
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
		public static void PlayFinalStageEffect()
		{
			Environment.CurrentEnvironment != null;
		}
		public async UniTask IntoFinalTask()
		{
			IntoFinalEffect effect = Object.Instantiate<IntoFinalEffect>(this.intoFinalEffect);
			await effect.PlayTask();
			Object.Destroy(effect.gameObject, 5f);
		}
		[SerializeField]
		private Transform stageRoot;
		[SerializeField]
		private GameObject gapRoot;
		[SerializeField]
		private YukariRoom yukariRoom;
		[SerializeField]
		private GameObject yukariRoomSimple;
		[SerializeField]
		private IntoFinalEffect intoFinalEffect;
		[SerializeField]
		private List<EnvironmentObject> templates;
		[SerializeField]
		private List<EnvironmentObject> simpleTemplates;
		private static bool _inGapRoom;
	}
}
