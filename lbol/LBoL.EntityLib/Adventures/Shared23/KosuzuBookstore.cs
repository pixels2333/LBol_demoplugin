using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using Yarn;

namespace LBoL.EntityLib.Adventures.Shared23
{
	// Token: 0x02000516 RID: 1302
	[AdventureInfo(WeighterType = typeof(KosuzuBookstore.KosuzuBookShopWeighter))]
	public sealed class KosuzuBookstore : Adventure
	{
		// Token: 0x06001112 RID: 4370 RVA: 0x0001ED80 File Offset: 0x0001CF80
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$money", 120f);
			this._books.Shuffle(base.GameRun.AdventureRng);
			if (base.GameRun.CurrentStage.Level >= 3)
			{
				storage.SetValue("$thirdBook", true);
			}
			for (int i = 0; i < 3; i++)
			{
				Exhibit exhibit = LBoL.Core.Library.CreateExhibit(this._books[i]);
				storage.SetValue("$book" + i.ToString(), exhibit.Id);
			}
			int num = 0;
			foreach (Type type in this._returnBooks)
			{
				if (base.GameRun.Player.HasExhibit(type))
				{
					storage.SetValue("$returnBook" + num.ToString(), base.GameRun.Player.GetExhibit(type).Id);
					num++;
				}
			}
			storage.SetValue("$returnBookCount", (float)num);
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x0001EEA4 File Offset: 0x0001D0A4
		public KosuzuBookstore()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(PulpFiction));
			list.Add(typeof(FixBook));
			list.Add(typeof(TimeBook));
			list.Add(typeof(FirstAidBook));
			this._books = list;
			List<Type> list2 = new List<Type>();
			list2.Add(typeof(HuanxiangxiangYuanqi));
			list2.Add(typeof(Mengriji));
			list2.Add(typeof(Modaoshu));
			list2.Add(typeof(HeiseBijiben));
			this._returnBooks = list2;
			base..ctor();
		}

		// Token: 0x0400012C RID: 300
		private readonly List<Type> _books;

		// Token: 0x0400012D RID: 301
		private readonly List<Type> _returnBooks;

		// Token: 0x0400012E RID: 302
		private const int Money = 120;

		// Token: 0x02000A70 RID: 2672
		private class KosuzuBookShopWeighter : IAdventureWeighter
		{
			// Token: 0x06003763 RID: 14179 RVA: 0x000867F6 File Offset: 0x000849F6
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.Money < 120)
				{
					return 0f;
				}
				return 1f;
			}
		}
	}
}
