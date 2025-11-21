using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits;

namespace LBoL.EntityLib.Stages
{
	// Token: 0x020000FB RID: 251
	public sealed class BossOnlyStage : Stage
	{
		// Token: 0x06000388 RID: 904 RVA: 0x00009264 File Offset: 0x00007464
		public BossOnlyStage()
		{
			base.SentinelExhibitType = typeof(KongZhanpinhe);
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0000927C File Offset: 0x0000747C
		public BossOnlyStage WithLevel(int level)
		{
			base.Level = level;
			return this;
		}

		// Token: 0x0600038A RID: 906 RVA: 0x00009288 File Offset: 0x00007488
		public override void InitBoss(RandomGen rng)
		{
			string text;
			switch (base.Level)
			{
			case 1:
				text = "Koishi";
				break;
			case 2:
				text = "Tianzi";
				break;
			case 3:
				text = "Junko";
				break;
			case 4:
				text = "Sanae";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			base.Boss = Library.GetEnemyGroupEntry(text);
		}

		// Token: 0x0600038B RID: 907 RVA: 0x000092E8 File Offset: 0x000074E8
		public override GameMap CreateMap()
		{
			return GameMap.CreateSingleRoute(base.Boss.Id, new StationType[] { StationType.Boss });
		}

		// Token: 0x04000033 RID: 51
		private string[] BossIds;
	}
}
