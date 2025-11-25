using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage3
{
	[AdventureInfo(WeighterType = typeof(MikoDonation.MikoDonationWeighter))]
	public sealed class MikoDonation : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$money", 200f);
		}
		private const int Money = 200;
		private class MikoDonationWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Money < 200)
				{
					return 0f;
				}
				return 1f;
			}
		}
	}
}
