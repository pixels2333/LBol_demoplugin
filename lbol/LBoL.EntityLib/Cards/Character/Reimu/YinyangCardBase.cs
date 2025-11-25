using System;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	public abstract class YinyangCardBase : Card
	{
		public override void Initialize()
		{
			base.Initialize();
			base.SetKeyword(Keyword.Yinyang, true);
		}
		protected override int AdditionalDamage
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigDamage;
			}
		}
		protected override int AdditionalBlock
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigBlock;
			}
		}
		protected override int AdditionalShield
		{
			get
			{
				return base.GetSeLevel<YinyangQueenSe>() * base.ConfigShield;
			}
		}
	}
}
