using System;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x02000408 RID: 1032
	public abstract class YinyangCardBase : Card
	{
		// Token: 0x06000E43 RID: 3651 RVA: 0x0001A4F6 File Offset: 0x000186F6
		public override void Initialize()
		{
			base.Initialize();
			base.SetKeyword(Keyword.Yinyang, true);
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000E44 RID: 3652 RVA: 0x0001A507 File Offset: 0x00018707
		protected override int AdditionalDamage
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigDamage;
			}
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000E45 RID: 3653 RVA: 0x0001A516 File Offset: 0x00018716
		protected override int AdditionalBlock
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigBlock;
			}
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000E46 RID: 3654 RVA: 0x0001A525 File Offset: 0x00018725
		protected override int AdditionalShield
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigShield;
			}
		}
	}
}
