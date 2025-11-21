using System;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200004E RID: 78
	public class ReflectDamage
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000FC RID: 252 RVA: 0x00003C6F File Offset: 0x00001E6F
		public Unit Target { get; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000FD RID: 253 RVA: 0x00003C77 File Offset: 0x00001E77
		public int Damage { get; }

		// Token: 0x060000FE RID: 254 RVA: 0x00003C7F File Offset: 0x00001E7F
		public ReflectDamage(Unit target, int damage)
		{
			this.Target = target;
			this.Damage = damage;
		}
	}
}
