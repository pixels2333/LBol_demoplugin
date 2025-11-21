using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace LBoL.Presentation.UI.Transitions
{
	// Token: 0x0200007D RID: 125
	[RequireComponent(typeof(Animator))]
	public class AnimationTransition : UiTransition, IAnimationClipSource
	{
		// Token: 0x06000680 RID: 1664 RVA: 0x0001BF90 File Offset: 0x0001A190
		public override void Animate(Transform target, bool isOut, Action onComplete)
		{
			if (isOut)
			{
				throw new InvalidOperationException("Cannot play animation backwards");
			}
			base.StopAllCoroutines();
			if (this.clip)
			{
				if (this._graph.IsValid())
				{
					this._graph.Destroy();
				}
				AnimationClipPlayable animationClipPlayable = AnimationPlayableUtilities.PlayClip(base.GetComponent<Animator>(), this.clip, out this._graph);
				animationClipPlayable.SetApplyFootIK(false);
				animationClipPlayable.SetApplyPlayableIK(false);
				base.StartCoroutine(this.WaitForPlayable(animationClipPlayable, this.clip.length, onComplete));
				return;
			}
			onComplete.Invoke();
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x0001C01F File Offset: 0x0001A21F
		private IEnumerator WaitForPlayable(AnimationClipPlayable playable, float length, Action onComplete)
		{
			while (playable.GetTime<AnimationClipPlayable>() < (double)length)
			{
				yield return null;
			}
			onComplete.Invoke();
			this._graph.Destroy();
			yield break;
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x0001C043 File Offset: 0x0001A243
		public override void Kill(Transform target)
		{
			base.StopAllCoroutines();
			if (this._graph.IsValid())
			{
				this._graph.Stop();
				this._graph.Destroy();
			}
		}

		// Token: 0x06000683 RID: 1667 RVA: 0x0001C06E File Offset: 0x0001A26E
		public void GetAnimationClips(List<AnimationClip> results)
		{
			results.Add(this.clip);
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0001C07C File Offset: 0x0001A27C
		private void OnDestroy()
		{
			base.StopAllCoroutines();
			if (this._graph.IsValid())
			{
				this._graph.Destroy();
			}
		}

		// Token: 0x04000408 RID: 1032
		[SerializeField]
		private AnimationClip clip;

		// Token: 0x04000409 RID: 1033
		private PlayableGraph _graph;
	}
}
