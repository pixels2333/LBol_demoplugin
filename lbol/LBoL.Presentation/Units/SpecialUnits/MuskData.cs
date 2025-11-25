using System;
using LBoL.Presentation.Effect;
using Spine.Unity;
using UnityEngine;
namespace LBoL.Presentation.Units.SpecialUnits
{
	public class MuskData
	{
		public MuskData(SkeletonAnimation skeleton, Transform point, int index)
		{
			this.Index = index;
			this.Skeleton = skeleton;
			this.EffectPoint = point;
			this.Front = index < 2;
		}
		public int Index { get; set; }
		public SkeletonAnimation Skeleton { get; set; }
		public Transform EffectPoint { get; set; }
		public EffectWidget EffectWidget { get; set; }
		public bool Front { get; set; }
	}
}
