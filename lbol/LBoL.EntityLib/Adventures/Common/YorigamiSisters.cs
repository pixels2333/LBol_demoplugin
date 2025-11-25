using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using UnityEngine;
namespace LBoL.EntityLib.Adventures.Common
{
	[AdventureInfo(WeighterType = typeof(YorigamiSisters.YorigamiSistersWeighter))]
	public sealed class YorigamiSisters : Adventure
	{
		private class YorigamiSistersWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return Mathf.Clamp01((float)gameRun.Money / 200f - 1f);
			}
		}
	}
}
