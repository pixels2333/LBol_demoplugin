using System;
using LBoL.Core;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Seija
{
	public class SeijaExhibit : Exhibit
	{
		protected virtual Type SeType
		{
			get
			{
				return null;
			}
		}
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
		protected virtual void GenerateEffect()
		{
			this._effect = Library.CreateStatusEffect(this.SeType);
		}
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
		protected StatusEffect _effect;
	}
}
