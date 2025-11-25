using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using UnityEngine;
namespace LBoL.Presentation.Environments
{
	public class FinalStageEnvironment : MonoBehaviour
	{
		private void Awake()
		{
			this.introVFX.gameObject.SetActive(false);
			this.loopVFX.SetActive(false);
		}
		public void PlayEffect()
		{
			this.introVFX.gameObject.SetActive(true);
			this.introVFX.Play();
			this.DelayPlayLoop();
		}
		private async UniTask DelayPlayLoop()
		{
			await UniTask.WaitForSeconds(3, true, PlayerLoopTiming.Update, default(CancellationToken), false);
			this.loopVFX.SetActive(true);
			EnvironmentObject component = base.GetComponent<EnvironmentObject>();
			SkeletonAnimation skeletonAnimation = ((component != null) ? component.skeletonAnimation : null);
			if (skeletonAnimation != null)
			{
				global::Spine.AnimationState state = skeletonAnimation.state;
				IEnumerable<global::Spine.Animation> animations = state.Data.SkeletonData.Animations;
				int num = 1;
				foreach (global::Spine.Animation animation in Enumerable.Where<global::Spine.Animation>(animations, (global::Spine.Animation anime) => anime.Name.Contains("vfxloop")))
				{
					state.SetAnimation(num, animation, true);
					num++;
				}
			}
		}
		[SerializeField]
		private ParticleSystem introVFX;
		[SerializeField]
		private GameObject loopVFX;
	}
}
