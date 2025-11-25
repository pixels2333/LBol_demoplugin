using System;
using LBoL.Core.Stations;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class StationRecord
	{
		public StationRecord Clone()
		{
			return new StationRecord
			{
				Type = this.Type,
				EnemyGroup = this.EnemyGroup,
				Adventure = this.Adventure
			};
		}
		public StationType Type;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string EnemyGroup;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string Adventure;
		public int Hp;
		public int MaxHp;
		public int Money;
		public int EnemyDamage;
		public int EnemyTurn;
		public string[] CardGettings;
		public string[] CardAbandons;
		public string[] CardUpgraded;
		public string[] CardRemoving;
		public string[] ExhibitGettings;
		public string[] ExhibitAbandons;
		public string gapOptionId;
	}
}
