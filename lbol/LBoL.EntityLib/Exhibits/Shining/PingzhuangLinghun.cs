using System;
using JetBrains.Annotations;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000135 RID: 309
	[UsedImplicitly]
	public sealed class PingzhuangLinghun : ShiningExhibit
	{
		// Token: 0x0600043B RID: 1083 RVA: 0x0000B5E0 File Offset: 0x000097E0
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DieEventArgs>(base.Battle.EnemyPointGenerating, delegate(DieEventArgs args)
			{
				base.NotifyActivating();
				args.Money += base.Value1 * args.Power;
				args.Power = 0;
			});
		}
	}
}
