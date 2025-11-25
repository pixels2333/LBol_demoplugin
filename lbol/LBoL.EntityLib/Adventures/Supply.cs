using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.JadeBoxes;
using Yarn;
namespace LBoL.EntityLib.Adventures
{
	public sealed class Supply : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			int num;
			switch (base.GameRun.CurrentStage.Level)
			{
			case 1:
				num = 1;
				break;
			case 2:
				num = 2;
				break;
			case 3:
				num = 3;
				break;
			default:
				num = 1;
				break;
			}
			int num2 = num;
			Exhibit exhibit = ((num2 == 1 && !base.GameRun.HasJadeBox<StartWithJingjie>()) ? LBoL.Core.Library.CreateExhibit<WaijieYanjing>() : base.Stage.GetSupplyExhibit());
			Exhibit supplyExhibit = base.Stage.GetSupplyExhibit();
			if (exhibit.Config.Keywords.HasFlag(Keyword.Power) || supplyExhibit.Config.Keywords.HasFlag(Keyword.Power))
			{
				storage.SetValue("$hasPowerExhibit", true);
			}
			storage.SetValue("$FlagA", !exhibit.Config.IsSentinel);
			storage.SetValue("$FlagB", !supplyExhibit.Config.IsSentinel);
			storage.SetValue("$stageNo", (float)num2);
			storage.SetValue("$exhibitA", exhibit.Id);
			storage.SetValue("$exhibitB", supplyExhibit.Id);
			bool flag = base.GameRun.Player.HasExhibit<GaodangShoubao>();
			if (exhibit.Config.IsSentinel || supplyExhibit.Config.IsSentinel)
			{
				flag = false;
			}
			storage.SetValue("$bothFlag", flag);
		}
		[RuntimeCommand("getDialogues", "")]
		[UsedImplicitly]
		public void GetDialogues()
		{
			List<int> list = new List<int>();
			for (int i = 1; i <= 5; i++)
			{
				if (!base.GameRun.ExtraFlags.Contains("SupplyYes" + i.ToString()))
				{
					list.Add(i);
				}
			}
			int num = ((list.Count > 0) ? list.Sample(base.GameRun.AdventureRng) : Enumerable.Range(1, 5).Sample(base.GameRun.AdventureRng));
			List<int> list2 = new List<int>();
			for (int j = 1; j <= 5; j++)
			{
				if (!base.GameRun.ExtraFlags.Contains("SupplyNo" + j.ToString()))
				{
					list2.Add(j);
				}
			}
			int num2 = ((list2.Count > 0) ? list2.Sample(base.GameRun.AdventureRng) : Enumerable.Range(1, 5).Sample(base.GameRun.AdventureRng));
			base.GameRun.ExtraFlags.Add("SupplyYes" + num.ToString());
			base.GameRun.ExtraFlags.Add("SupplyNo" + num2.ToString());
			string value = base.Storage.GetValue(string.Format("$Yes{0}Dialogue", num));
			string value2 = base.Storage.GetValue(string.Format("$No{0}Dialogue", num2));
			base.Storage.SetValue("$YesDialogue", value);
			base.Storage.SetValue("$NoDialogue", value2);
		}
	}
}
