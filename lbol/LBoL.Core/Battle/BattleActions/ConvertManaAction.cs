using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000169 RID: 361
	public class ConvertManaAction : SimpleEventBattleAction<ManaConvertingEventArgs>
	{
		// Token: 0x06000DF2 RID: 3570 RVA: 0x0002657D File Offset: 0x0002477D
		public ConvertManaAction(ManaGroup input, ManaGroup output, bool allowPartial)
		{
			base.Args = new ManaConvertingEventArgs
			{
				Input = input,
				Output = output,
				AllowPartial = allowPartial
			};
		}

		// Token: 0x06000DF3 RID: 3571 RVA: 0x000265A5 File Offset: 0x000247A5
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.ManaConverting);
		}

		// Token: 0x06000DF4 RID: 3572 RVA: 0x000265B8 File Offset: 0x000247B8
		protected override void MainPhase()
		{
			ManaGroup manaGroup;
			ManaGroup manaGroup2;
			if (!base.Battle.ConvertMana(base.Args.Input, base.Args.Output, base.Args.AllowPartial, out manaGroup, out manaGroup2))
			{
				base.Args.ForceCancelBecause(CancelCause.Failure);
				return;
			}
			if (base.Args.Input != manaGroup)
			{
				base.Args.Input = manaGroup;
				base.Args.IsModified = true;
			}
			if (base.Args.Output != manaGroup2)
			{
				base.Args.Output = manaGroup2;
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x0002665A File Offset: 0x0002485A
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.ManaConverted);
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x00026670 File Offset: 0x00024870
		public static ConvertManaAction Purify(ManaGroup mana, int count)
		{
			ManaGroup empty = ManaGroup.Empty;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				ManaColor maxTrivialColor = mana.MaxTrivialColor;
				if (mana[maxTrivialColor] <= 0)
				{
					break;
				}
				num++;
				ManaColor manaColor = maxTrivialColor;
				int num2 = empty[manaColor] + 1;
				empty[manaColor] = num2;
				manaColor = maxTrivialColor;
				num2 = mana[manaColor] - 1;
				mana[manaColor] = num2;
			}
			if (num <= 0)
			{
				return null;
			}
			return new ConvertManaAction(empty, ManaGroup.Colorlesses(num), true);
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x000266F0 File Offset: 0x000248F0
		public static ConvertManaAction PhilosophyRandomMana(ManaGroup mana, int count, RandomGen rng)
		{
			List<ManaColor> list = Enumerable.ToList<ManaColor>(mana.EnumerateComponents());
			list.RemoveAll((ManaColor m) => m == ManaColor.Philosophy);
			ManaColor[] array = list.SampleManyOrAll(count, rng);
			if (array.Length != 0)
			{
				return new ConvertManaAction(ManaGroup.FromComponents(array), ManaGroup.Philosophies(array.Length), true);
			}
			return null;
		}
	}
}
