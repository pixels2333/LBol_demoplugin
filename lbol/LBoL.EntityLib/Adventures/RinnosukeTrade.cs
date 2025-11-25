using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using UnityEngine;
using Yarn;
namespace LBoL.EntityLib.Adventures
{
	public sealed class RinnosukeTrade : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			List<Exhibit> list = Enumerable.ToList<Exhibit>(Enumerable.Where<Exhibit>(base.GameRun.Player.Exhibits, (Exhibit e) => e.LosableType == ExhibitLosableType.Losable).SampleManyOrAll(2, base.GameRun.AdventureRng));
			if (list.Count >= 2)
			{
				storage.SetValue("$canSell1", true);
				storage.SetValue("$exhibit1", list[0].Id);
				storage.SetValue("$exhibit1Price", (float)this.GetExhibitPrice(list[0]));
				storage.SetValue("$canSell2", true);
				storage.SetValue("$exhibit2", list[1].Id);
				storage.SetValue("$exhibit2Price", (float)this.GetExhibitPrice(list[1]));
				return;
			}
			if (list.Count >= 1)
			{
				storage.SetValue("$canSell1", true);
				storage.SetValue("$exhibit1", list[0].Id);
				storage.SetValue("$exhibit1Price", (float)this.GetExhibitPrice(list[0]));
				storage.SetValue("$canSell2", false);
				return;
			}
			storage.SetValue("$canSell1", false);
			storage.SetValue("$canSell2", false);
		}
		private int GetExhibitPrice(Exhibit exhibit)
		{
			int num;
			switch (exhibit.Config.Rarity)
			{
			case Rarity.Common:
				num = GlobalConfig.ExhibitPrices[0];
				break;
			case Rarity.Uncommon:
				num = GlobalConfig.ExhibitPrices[1];
				break;
			case Rarity.Rare:
				num = GlobalConfig.ExhibitPrices[2];
				break;
			default:
				throw new InvalidOperationException("exhibit rarity out of range.");
			}
			float num2 = (float)num;
			float num3 = base.GameRun.ShopRng.NextFloat(-0.08f, 0f) + 1f;
			return Mathf.RoundToInt((float)Mathf.RoundToInt(num2 * num3) * 60f / 100f);
		}
		[RuntimeCommand("sellExhibit", "")]
		[UsedImplicitly]
		public void SellExhibit(string id)
		{
			Exhibit exhibit = base.GameRun.Player.GetExhibit(id);
			if (exhibit != null)
			{
				base.GameRun.LoseExhibit(exhibit, true, true);
			}
			if ((exhibit is ZhangchibangQiu || exhibit is Zijing || exhibit is Taozi) && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.RinnosukeAdventure);
			}
		}
		[RuntimeCommand("trade", "")]
		[UsedImplicitly]
		public IEnumerator Trade(int index)
		{
			base.LoseExhibit("WaijieYanjing");
			yield return base.GameRun.GainExhibitRunner(LBoL.Core.Library.CreateExhibit<WaijieYouxiji>(), true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = index
			});
			yield break;
		}
		private const float SellPricePercentage = 60f;
	}
}
