using System;
using LBoL.Core;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.Exhibits
{
	public abstract class ShiningExhibit : Exhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			player.GameRun.GainBaseMana(base.BaseMana, false);
		}
		protected override void OnLose(PlayerUnit player)
		{
			if (!player.GameRun.TryLoseBaseMana(base.BaseMana, false))
			{
				Debug.LogError(string.Format("Failed to lose base mana {0}", base.BaseMana));
			}
		}
	}
}
