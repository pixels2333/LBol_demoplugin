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
	// Token: 0x020000F8 RID: 248
	public class FinalStageEnvironment : MonoBehaviour
	{
		// Token: 0x06000E0A RID: 3594 RVA: 0x0004338B File Offset: 0x0004158B
		private void Awake()
		{
			this.introVFX.gameObject.SetActive(false);
			this.loopVFX.SetActive(false);
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x000433AA File Offset: 0x000415AA
		public void PlayEffect()
		{
			this.introVFX.gameObject.SetActive(true);
			this.introVFX.Play();
			this.DelayPlayLoop();
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x000433D0 File Offset: 0x000415D0
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

		// Token: 0x04000A82 RID: 2690
		[SerializeField]
		private ParticleSystem introVFX;

		// Token: 0x04000A83 RID: 2691
		[SerializeField]
		private GameObject loopVFX;
	}
}
