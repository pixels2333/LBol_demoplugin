using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x0200014B RID: 331
	public class SeijaExhibit : Exhibit
	{
		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000484 RID: 1156 RVA: 0x0000BD90 File Offset: 0x00009F90
		protected virtual Type SeType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x0000BD93 File Offset: 0x00009F93
		protected StatusEffect Effect
		{
			get
			{
				if (this._effect != null)
				{
					return this._effect;
				}
				if (this.SeType != null)
				{
					this.GenerateEffect();
					return this._effect;
				}
				return null;
			}
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x0000BDC0 File Offset: 0x00009FC0
		protected virtual void GenerateEffect()
		{
			this._effect = Library.CreateStatusEffect(this.SeType);
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000487 RID: 1159 RVA: 0x0000BDD3 File Offset: 0x00009FD3
		public override string Description
		{
			get
			{
				StatusEffect effect = this.Effect;
				if (effect == null)
				{
					return null;
				}
				return effect.Description;
			}
		}

		// Token: 0x0400003D RID: 61
		protected StatusEffect _effect;
	}
}
