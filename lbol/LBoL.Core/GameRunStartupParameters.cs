using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000045 RID: 69
	public sealed class GameRunStartupParameters
	{
		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000327 RID: 807 RVA: 0x0000B252 File Offset: 0x00009452
		// (set) Token: 0x06000328 RID: 808 RVA: 0x0000B25A File Offset: 0x0000945A
		public GameMode Mode { get; set; }

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x06000329 RID: 809 RVA: 0x0000B263 File Offset: 0x00009463
		// (set) Token: 0x0600032A RID: 810 RVA: 0x0000B26B File Offset: 0x0000946B
		public bool ShowRandomResult { get; set; }

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600032B RID: 811 RVA: 0x0000B274 File Offset: 0x00009474
		// (set) Token: 0x0600032C RID: 812 RVA: 0x0000B27C File Offset: 0x0000947C
		public ulong? Seed { get; set; }

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x0600032D RID: 813 RVA: 0x0000B285 File Offset: 0x00009485
		// (set) Token: 0x0600032E RID: 814 RVA: 0x0000B28D File Offset: 0x0000948D
		public GameDifficulty Difficulty { get; set; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x0600032F RID: 815 RVA: 0x0000B296 File Offset: 0x00009496
		// (set) Token: 0x06000330 RID: 816 RVA: 0x0000B29E File Offset: 0x0000949E
		public PuzzleFlag Puzzles { get; set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0000B2A7 File Offset: 0x000094A7
		// (set) Token: 0x06000332 RID: 818 RVA: 0x0000B2AF File Offset: 0x000094AF
		public PlayerUnit Player { get; set; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000333 RID: 819 RVA: 0x0000B2B8 File Offset: 0x000094B8
		// (set) Token: 0x06000334 RID: 820 RVA: 0x0000B2C0 File Offset: 0x000094C0
		public PlayerType PlayerType { get; set; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0000B2C9 File Offset: 0x000094C9
		// (set) Token: 0x06000336 RID: 822 RVA: 0x0000B2D1 File Offset: 0x000094D1
		public Exhibit InitExhibit { get; set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000337 RID: 823 RVA: 0x0000B2DA File Offset: 0x000094DA
		// (set) Token: 0x06000338 RID: 824 RVA: 0x0000B2E2 File Offset: 0x000094E2
		public int? InitMoneyOverride { get; set; }

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000339 RID: 825 RVA: 0x0000B2EB File Offset: 0x000094EB
		// (set) Token: 0x0600033A RID: 826 RVA: 0x0000B2F3 File Offset: 0x000094F3
		public IEnumerable<Card> Deck { get; set; }

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600033B RID: 827 RVA: 0x0000B2FC File Offset: 0x000094FC
		// (set) Token: 0x0600033C RID: 828 RVA: 0x0000B304 File Offset: 0x00009504
		public IEnumerable<Stage> Stages { get; set; }

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x0600033D RID: 829 RVA: 0x0000B30D File Offset: 0x0000950D
		// (set) Token: 0x0600033E RID: 830 RVA: 0x0000B315 File Offset: 0x00009515
		public Type DebutAdventureType
		{
			[return: MaybeNull]
			get;
			set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x0600033F RID: 831 RVA: 0x0000B31E File Offset: 0x0000951E
		// (set) Token: 0x06000340 RID: 832 RVA: 0x0000B326 File Offset: 0x00009526
		public IEnumerable<JadeBox> JadeBoxes { get; set; }

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000341 RID: 833 RVA: 0x0000B32F File Offset: 0x0000952F
		// (set) Token: 0x06000342 RID: 834 RVA: 0x0000B337 File Offset: 0x00009537
		public ProfileSaveData UserProfile { get; set; }

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000343 RID: 835 RVA: 0x0000B340 File Offset: 0x00009540
		// (set) Token: 0x06000344 RID: 836 RVA: 0x0000B348 File Offset: 0x00009548
		public int UnlockLevel { get; set; }
	}
}
