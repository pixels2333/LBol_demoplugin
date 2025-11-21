using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace LBoL.Core.Battle.BattleActionRecord
{
	// Token: 0x020001B7 RID: 439
	public class ActionRecord
	{
		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x06000F91 RID: 3985 RVA: 0x000299C6 File Offset: 0x00027BC6
		public static List<ActionRecord> ActionRecords { get; } = new List<ActionRecord>();

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x06000F92 RID: 3986 RVA: 0x000299CD File Offset: 0x00027BCD
		// (set) Token: 0x06000F93 RID: 3987 RVA: 0x000299D4 File Offset: 0x00027BD4
		public static Action<ActionRecord> ActionResolvedHandler { get; set; }

		// Token: 0x06000F94 RID: 3988 RVA: 0x000299DC File Offset: 0x00027BDC
		internal static void TriggerResolved(ActionRecord record)
		{
			Action<ActionRecord> actionResolvedHandler = ActionRecord.ActionResolvedHandler;
			if (actionResolvedHandler == null)
			{
				return;
			}
			actionResolvedHandler.Invoke(record);
		}

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x06000F95 RID: 3989 RVA: 0x000299EE File Offset: 0x00027BEE
		public List<PhaseRecord> Phases { get; } = new List<PhaseRecord>();

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x000299F6 File Offset: 0x00027BF6
		public string Name { get; }

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x000299FE File Offset: 0x00027BFE
		public string Details { get; }

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x06000F98 RID: 3992 RVA: 0x00029A06 File Offset: 0x00027C06
		[CanBeNull]
		public string Source { get; }

		// Token: 0x06000F99 RID: 3993 RVA: 0x00029A10 File Offset: 0x00027C10
		public ActionRecord(BattleAction action, [CanBeNull] string name = null)
		{
			this.Name = name ?? action.Name;
			this.Details = action.ExportDebugDetails();
			this.Source = ((action.Source != null) ? (action.Source.Name ?? action.Source.Id) : null);
		}

		// Token: 0x06000F9A RID: 3994 RVA: 0x00029A78 File Offset: 0x00027C78
		private static void DumpActions(IReadOnlyCollection<ActionRecord> actions, StringBuilder builder, string prefix)
		{
			foreach (ValueTuple<int, ActionRecord> valueTuple in actions.WithIndices<ActionRecord>())
			{
				int item = valueTuple.Item1;
				ActionRecord item2 = valueTuple.Item2;
				if (item == actions.Count - 1)
				{
					builder.Append(prefix).Append("     └── ").Append(ActionRecord.ActionToSingleLine(item2))
						.AppendLine();
					ActionRecord.DumpPhases(item2.Phases, builder, prefix + "         ");
				}
				else
				{
					builder.Append(prefix).Append("     ├── ").Append(ActionRecord.ActionToSingleLine(item2))
						.AppendLine();
					ActionRecord.DumpPhases(item2.Phases, builder, prefix + "     │   ");
				}
			}
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x00029B50 File Offset: 0x00027D50
		private static void DumpPhases(IReadOnlyCollection<PhaseRecord> phases, StringBuilder builder, string prefix)
		{
			List<ValueTuple<string, PhaseRecord>> list = new List<ValueTuple<string, PhaseRecord>>();
			foreach (PhaseRecord phaseRecord in phases)
			{
				string text = ActionRecord.PhaseArgsToString(phaseRecord);
				if (text != null || !phaseRecord.Reactions.Empty<ActionRecord>())
				{
					string text2 = string.Concat(new string[]
					{
						"<color=",
						ActionRecord.BlueText,
						">",
						phaseRecord.Name,
						"</color>"
					});
					if (text != null)
					{
						list.Add(new ValueTuple<string, PhaseRecord>(text2 + ": " + text, phaseRecord));
					}
					else
					{
						list.Add(new ValueTuple<string, PhaseRecord>(text2, phaseRecord));
					}
				}
			}
			foreach (ValueTuple<int, ValueTuple<string, PhaseRecord>> valueTuple in list.WithIndices<ValueTuple<string, PhaseRecord>>())
			{
				ValueTuple<string, PhaseRecord> item = valueTuple.Item2;
				int item2 = valueTuple.Item1;
				string item3 = item.Item1;
				PhaseRecord item4 = item.Item2;
				if (item2 == list.Count - 1)
				{
					builder.Append(prefix).Append("     └── ").Append(item3)
						.AppendLine();
					ActionRecord.DumpActions(item4.Reactions, builder, prefix + "         ");
				}
				else
				{
					builder.Append(prefix).Append("     ├── ").Append(item3)
						.AppendLine();
					ActionRecord.DumpActions(item4.Reactions, builder, prefix + "     │   ");
				}
			}
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x00029CEC File Offset: 0x00027EEC
		private static string ActionToSingleLine(ActionRecord actionRecord)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (actionRecord.Source != null)
			{
				stringBuilder.AppendWithColor(ActionRecord.OrangeText, new string[] { actionRecord.Source }).Append(": ");
			}
			stringBuilder.Append('[').AppendWithColor(ActionRecord.GreenText, new string[] { actionRecord.Name });
			if (!actionRecord.Details.IsNullOrWhiteSpace())
			{
				stringBuilder.Append(": ").Append(actionRecord.Details);
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x00029D84 File Offset: 0x00027F84
		private static string PhaseArgsToString(PhaseRecord phaseRecord)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (phaseRecord.IsModified)
			{
				if (!phaseRecord.Details.IsNullOrWhiteSpace())
				{
					stringBuilder.Append(phaseRecord.Details);
				}
				if (!phaseRecord.Modifiers.Empty<string>())
				{
					bool flag = true;
					stringBuilder.Append(" (Modified by ");
					foreach (string text in phaseRecord.Modifiers)
					{
						if (flag)
						{
							flag = false;
						}
						else
						{
							stringBuilder.Append(", ");
						}
						stringBuilder.AppendWithColor(ActionRecord.OrangeText, new string[] { text });
					}
					stringBuilder.Append(")");
				}
			}
			if (phaseRecord.IsCanceled)
			{
				if (phaseRecord.CancelCause == CancelCause.Reaction)
				{
					stringBuilder.AppendWithColor(ActionRecord.RedText, new string[] { " (Canceled by: <color=" }).Append(ActionRecord.OrangeText).Append(">")
						.Append(phaseRecord.CancelSource ?? "missing-ref")
						.Append("</color>)");
				}
				else
				{
					stringBuilder.AppendWithColor(ActionRecord.RedText, new string[]
					{
						" (Canceled because: ",
						phaseRecord.CancelCause.ToString(),
						")"
					});
				}
			}
			if (phaseRecord.ExceptionString != null)
			{
				stringBuilder.AppendWithColor(ActionRecord.RedText, new string[] { "Error (see log for details): ", phaseRecord.ExceptionString });
			}
			if (stringBuilder.Length <= 0)
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000F9E RID: 3998 RVA: 0x00029F04 File Offset: 0x00028104
		public static string Dump(ActionRecord root)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ActionRecord.ActionToSingleLine(root));
			ActionRecord.DumpPhases(root.Phases, stringBuilder, "     ");
			return stringBuilder.ToString();
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x00029F3C File Offset: 0x0002813C
		public static string ToYaml(ActionRecord root)
		{
			string text;
			using (StringWriter stringWriter = new StringWriter
			{
				NewLine = "\n"
			})
			{
				new Serializer().Serialize(stringWriter, root);
				text = stringWriter.ToString();
			}
			return text;
		}

		// Token: 0x040006AE RID: 1710
		private static readonly Color Green = new Color(0.65f, 0.89f, 0.18f);

		// Token: 0x040006AF RID: 1711
		private static readonly string GreenText = "#A5E22D";

		// Token: 0x040006B0 RID: 1712
		private static readonly Color Blue = new Color(0.4f, 0.85f, 0.94f);

		// Token: 0x040006B1 RID: 1713
		private static readonly string BlueText = "#66D8EF";

		// Token: 0x040006B2 RID: 1714
		private static readonly Color Orange = new Color(0.99f, 0.59f, 0.12f);

		// Token: 0x040006B3 RID: 1715
		private static readonly string OrangeText = "#FD971F";

		// Token: 0x040006B4 RID: 1716
		private static readonly Color Red = new Color(0.98f, 0.14f, 0.45f);

		// Token: 0x040006B5 RID: 1717
		private static readonly string RedText = "#F92472";
	}
}
