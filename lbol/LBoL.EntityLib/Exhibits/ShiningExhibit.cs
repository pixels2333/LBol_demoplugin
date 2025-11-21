using System;
using LBoL.Core;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Exhibits
{
	// Token: 0x0200011E RID: 286
	public abstract class ShiningExhibit : Exhibit
	{
		// Token: 0x060003F1 RID: 1009 RVA: 0x0000AE5E File Offset: 0x0000905E
		protected override void OnGain(PlayerUnit player)
		{
			player.GameRun.GainBaseMana(base.BaseMana, false);
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x0000AE77 File Offset: 0x00009077
		protected override void OnLose(PlayerUnit player)
		{
			if (!player.GameRun.TryLoseBaseMana(base.BaseMana, false))
			{
				Debug.LogError(string.Format("Failed to lose base mana {0}", base.BaseMana));
			}
		}
	}
}
