using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace LBoL.Presentation.Animations
{
	[RequireComponent(typeof(Animator))]
	public class SingleAnimationClipPlayer : MonoBehaviour, IAnimationClipSource
	{
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
		private void Awake()
		{
			this.Initialize();
		}
		private void OnDestroy()
		{
			if (this._graph.IsValid())
			{
				this._graph.Destroy();
			}
		}
		public void Play()
		{
			this.Initialize();
			this._graph.Play();
		}
		public void Rewind()
		{
			this._playable.SetTime(0.0);
			this._playable.SetTime(0.0);
		}
		public void Stop()
		{
			this._playable.SetDone(false);
		}
		public void GetAnimationClips(List<AnimationClip> results)
		{
			results.Add(this.clip);
		}
		[SerializeField]
		private AnimationClip clip;
		private bool _initialized;
		private Animator _animator;
		private PlayableGraph _graph;
		private AnimationClipPlayable _playable;
	}
}
