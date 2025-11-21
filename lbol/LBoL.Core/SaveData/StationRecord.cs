using System;
using LBoL.Core.Stations;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E7 RID: 231
	public sealed class StationRecord
	{
		// Token: 0x06000900 RID: 2304 RVA: 0x0001A47E File Offset: 0x0001867E
		public StationRecord Clone()
		{
			return new StationRecord
			{
				Type = this.Type,
				EnemyGroup = this.EnemyGroup,
				Adventure = this.Adventure
			};
		}

		// Token: 0x0400049D RID: 1181
		public StationType Type;

		// Token: 0x0400049E RID: 1182
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string EnemyGroup;

		// Token: 0x0400049F RID: 1183
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string Adventure;

		// Token: 0x040004A0 RID: 1184
		public int Hp;

		// Token: 0x040004A1 RID: 1185
		public int MaxHp;

		// Token: 0x040004A2 RID: 1186
		public int Money;

		// Token: 0x040004A3 RID: 1187
		public int EnemyDamage;

		// Token: 0x040004A4 RID: 1188
		public int EnemyTurn;

		// Token: 0x040004A5 RID: 1189
		public string[] CardGettings;

		// Token: 0x040004A6 RID: 1190
		public string[] CardAbandons;

		// Token: 0x040004A7 RID: 1191
		public string[] CardUpgraded;

		// Token: 0x040004A8 RID: 1192
		public string[] CardRemoving;

		// Token: 0x040004A9 RID: 1193
		public string[] ExhibitGettings;

		// Token: 0x040004AA RID: 1194
		public string[] ExhibitAbandons;

		// Token: 0x040004AB RID: 1195
		public string gapOptionId;
	}
}
