using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.Presentation.Effect;
using Spine;
using Spine.Unity;
using UnityEngine;
namespace LBoL.Presentation.Units.SpecialUnits
{
	public class KokoroUnitController : MonoBehaviour
	{
		public global::Spine.AnimationState TopMuskState
		{
			get
			{
				return this.topMusk.state;
			}
		}
		private void ClearEffect()
		{
			foreach (MuskData muskData in this._muskData)
			{
				if (muskData.EffectWidget != null)
				{
					muskData.EffectWidget.DieOut();
					muskData.EffectWidget = null;
				}
			}
			if (this._topMuskData.EffectWidget != null)
			{
				this._topMuskData.EffectWidget.DieOut();
				this._topMuskData.EffectWidget = null;
			}
		}
		public void SwitchToColor(SkirtColor skirtColor)
		{
			this.ClearEffect();
			foreach (MuskData muskData in this._muskData)
			{
				muskData.EffectWidget = EffectManager.CreateEffect("Qin" + skirtColor.ToString(), muskData.EffectPoint, true);
				muskData.EffectWidget.SetSortingOrder(muskData.Front ? 1 : (-1));
			}
			this._topMuskData.EffectWidget = EffectManager.CreateEffect("Qin" + skirtColor.ToString(), this.topEffectPoint, true);
			this._topMuskData.EffectWidget.SetSortingOrder(1);
		}
		private void Start()
		{
			foreach (SkeletonAnimation skeletonAnimation in this.musks.Keys)
			{
				skeletonAnimation.state.Event += this.OnEvent;
			}
			foreach (ValueTuple<int, KeyValuePair<SkeletonAnimation, Transform>> valueTuple in this.musks.WithIndices<KeyValuePair<SkeletonAnimation, Transform>>())
			{
				KeyValuePair<SkeletonAnimation, Transform> item = valueTuple.Item2;
				SkeletonAnimation skeletonAnimation2;
				Transform transform;
				item.Deconstruct(ref skeletonAnimation2, ref transform);
				int item2 = valueTuple.Item1;
				SkeletonAnimation skeletonAnimation3 = skeletonAnimation2;
				Transform transform2 = transform;
				this._muskData.Add(new MuskData(skeletonAnimation3, transform2, item2));
			}
			foreach (MuskData muskData in this._muskData)
			{
				muskData.Skeleton.GetComponent<Renderer>().sortingOrder = (muskData.Front ? 2 : (-2));
			}
			this._topMuskData = new MuskData(this.topMusk, this.topEffectPoint, 0);
			this._topMuskData.Skeleton.GetComponent<Renderer>().sortingOrder = 2;
			foreach (MuskData muskData2 in this._muskData)
			{
				this.AllMuskStates.Add(muskData2.Skeleton.state);
			}
		}
		private void OnEvent(TrackEntry trackEntry, global::Spine.Event e)
		{
			string name = e.Data.Name;
			if (!(name == "Front"))
			{
				if (!(name == "Back"))
				{
					return;
				}
			}
			else
			{
				using (List<MuskData>.Enumerator enumerator = this._muskData.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						MuskData muskData = enumerator.Current;
						if (muskData.Skeleton.state.GetCurrent(0) == trackEntry)
						{
							muskData.Skeleton.GetComponent<Renderer>().sortingOrder = 2;
							if (muskData.EffectWidget != null)
							{
								muskData.EffectWidget.SetSortingOrder(1);
								break;
							}
							break;
						}
					}
					return;
				}
			}
			foreach (MuskData muskData2 in this._muskData)
			{
				if (muskData2.Skeleton.state.GetCurrent(0) == trackEntry)
				{
					muskData2.Skeleton.GetComponent<Renderer>().sortingOrder = -2;
					if (muskData2.EffectWidget != null)
					{
						muskData2.EffectWidget.SetSortingOrder(-1);
						break;
					}
					break;
				}
			}
		}
		[SerializeField]
		private AssociationList<SkeletonAnimation, Transform> musks;
		[SerializeField]
		private SkeletonAnimation topMusk;
		[SerializeField]
		private Transform topEffectPoint;
		private readonly List<MuskData> _muskData = new List<MuskData>();
		private MuskData _topMuskData;
		public List<global::Spine.AnimationState> AllMuskStates = new List<global::Spine.AnimationState>();
	}
}
