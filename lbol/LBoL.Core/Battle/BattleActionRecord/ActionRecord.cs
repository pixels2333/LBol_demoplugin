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
	public class ActionRecord
	{
		public static List<ActionRecord> ActionRecords { get; } = new List<ActionRecord>();
		public static Action<ActionRecord> ActionResolvedHandler { get; set; }
		internal static void TriggerResolved(ActionRecord record)
		{
			Action<ActionRecord> actionResolvedHandler = ActionRecord.ActionResolvedHandler;
			if (actionResolvedHandler == null)
			{
				return;
			}
			actionResolvedHandler.Invoke(record);
		}
		public List<PhaseRecord> Phases { get; } = new List<PhaseRecord>();
		public string Name { get; }
		public string Details { get; }
		[CanBeNull]
		public string Source { get; }
		public ActionRecord(BattleAction action, [CanBeNull] string name = null)
		{
			this.Name = name ?? action.Name;
			this.Details = action.ExportDebugDetails();
			this.Source = ((action.Source != null) ? (action.Source.Name ?? action.Source.Id) : null);
		}
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
		public static string Dump(ActionRecord root)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(ActionRecord.ActionToSingleLine(root));
			ActionRecord.DumpPhases(root.Phases, stringBuilder, "     ");
			return stringBuilder.ToString();
		}
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
		private static readonly Color Green = new Color(0.65f, 0.89f, 0.18f);
		private static readonly string GreenText = "#A5E22D";
		private static readonly Color Blue = new Color(0.4f, 0.85f, 0.94f);
		private static readonly string BlueText = "#66D8EF";
		private static readonly Color Orange = new Color(0.99f, 0.59f, 0.12f);
		private static readonly string OrangeText = "#FD971F";
		private static readonly Color Red = new Color(0.98f, 0.14f, 0.45f);
		private static readonly string RedText = "#F92472";
	}
}
