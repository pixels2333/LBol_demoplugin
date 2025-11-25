using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace LBoL.Presentation.UI.Transitions
{
	[RequireComponent(typeof(Animator))]
	public class AnimationTransition : UiTransition, IAnimationClipSource
	{
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
		public override void Kill(Transform target)
		{
			base.StopAllCoroutines();
			if (this._graph.IsValid())
			{
				this._graph.Stop();
				this._graph.Destroy();
			}
		}
		public void GetAnimationClips(List<AnimationClip> results)
		{
			results.Add(this.clip);
		}
		private void OnDestroy()
		{
			base.StopAllCoroutines();
			if (this._graph.IsValid())
			{
				this._graph.Destroy();
			}
		}
		[SerializeField]
		private AnimationClip clip;
		private PlayableGraph _graph;
	}
}
