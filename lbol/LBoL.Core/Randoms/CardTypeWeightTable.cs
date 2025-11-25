using System;
using LBoL.Base;
namespace LBoL.Core.Randoms
{
	public sealed class CardTypeWeightTable
	{
		public float Attack { get; }
		public float Defense { get; }
		public float Skill { get; }
		public float Ability { get; }
		public float Status { get; }
		public float Misfortune { get; }
		public float Tool { get; }
		public float Friend { get; }
		public CardTypeWeightTable(float attack, float defense, float skill, float ability, float friend, float status, float misfortune, float tool)
		{
			this.Attack = attack;
			this.Defense = defense;
			this.Skill = skill;
			this.Ability = ability;
			this.Friend = friend;
			this.Status = status;
			this.Misfortune = misfortune;
			this.Tool = tool;
		}
		public float WeightFor(CardType cardInfoType)
		{
			float num;
			switch (cardInfoType)
			{
			case CardType.Unknown:
				throw new InvalidOperationException("Try get weight for unknown typed card");
			case CardType.Attack:
				num = this.Attack;
				break;
			case CardType.Defense:
				num = this.Defense;
				break;
			case CardType.Skill:
				num = this.Skill;
				break;
			case CardType.Ability:
				num = this.Ability;
				break;
			case CardType.Friend:
				num = this.Friend;
				break;
			case CardType.Tool:
				num = this.Tool;
				break;
			case CardType.Status:
				num = this.Status;
				break;
			case CardType.Misfortune:
				num = this.Misfortune;
				break;
			default:
				throw new ArgumentOutOfRangeException("cardInfoType", cardInfoType, null);
			}
			return num;
		}
		public static readonly CardTypeWeightTable AllOnes = new CardTypeWeightTable(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);
		public static readonly CardTypeWeightTable CanBeLoot = new CardTypeWeightTable(1f, 1f, 1f, 1f, 1f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlyAttack = new CardTypeWeightTable(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlyDefense = new CardTypeWeightTable(0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlySkill = new CardTypeWeightTable(0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlyAbility = new CardTypeWeightTable(0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlyFriend = new CardTypeWeightTable(0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable ShopSkillAndFriend = new CardTypeWeightTable(0f, 0f, 1f, 0f, 5f, 0f, 0f, 0f);
		public static readonly CardTypeWeightTable OnlyTool = new CardTypeWeightTable(0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f);
	}
}
