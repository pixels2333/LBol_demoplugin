using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace LBoL.Presentation.Animations
{
	// Token: 0x02000118 RID: 280
	[RequireComponent(typeof(Animator))]
	public class SingleAnimationClipPlayer : MonoBehaviour, IAnimationClipSource
	{
		// Token: 0x06000FE0 RID: 4064 RVA: 0x0004A88C File Offset: 0x00048A8C
		private void Initialize()
		{
			if (this._initialized)
			{
				return;
			}
			this._animator = base.GetComponent<Animator>();
			if (!this._animator)
			{
				this._animator = base.gameObject.AddComponent<Animator>();
			}
			this._graph = PlayableGraph.Create();
			AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(this._graph, "AnimationClip", this._animator);
			this._playable = AnimationClipPlayable.Create(this._graph, this.clip);
			animationPlayableOutput.SetSourcePlayable(this._playable);
			this._initialized = true;
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x0004A916 File Offset: 0x00048B16
		private void Awake()
		{
			this.Initialize();
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x0004A91E File Offset: 0x00048B1E
		private void OnDestroy()
		{
			if (this._graph.IsValid())
			{
				this._graph.Destroy();
			}
		}

		// Token: 0x06000FE3 RID: 4067 RVA: 0x0004A938 File Offset: 0x00048B38
		public void Play()
		{
			this.Initialize();
			this._graph.Play();
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x0004A94B File Offset: 0x00048B4B
		public void Rewind()
		{
			this._playable.SetTime(0.0);
			this._playable.SetTime(0.0);
		}

		// Token: 0x06000FE5 RID: 4069 RVA: 0x0004A975 File Offset: 0x00048B75
		public void Stop()
		{
			this._playable.SetDone(false);
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x0004A983 File Offset: 0x00048B83
		public void GetAnimationClips(List<AnimationClip> results)
		{
			results.Add(this.clip);
		}

		// Token: 0x04000BC3 RID: 3011
		[SerializeField]
		private AnimationClip clip;

		// Token: 0x04000BC4 RID: 3012
		private bool _initialized;

		// Token: 0x04000BC5 RID: 3013
		private Animator _animator;

		// Token: 0x04000BC6 RID: 3014
		private PlayableGraph _graph;

		// Token: 0x04000BC7 RID: 3015
		private AnimationClipPlayable _playable;
	}
}
