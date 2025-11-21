using System;
using System.Text;
using LBoL.Base;
using LBoL.Base.Extensions;
using TMPro;
using UnityEngine;

namespace LBoL.Core.Helpers
{
	// Token: 0x02000113 RID: 275
	public static class UiUtils
	{
		// Token: 0x060009EA RID: 2538 RVA: 0x0001C4C0 File Offset: 0x0001A6C0
		public static string ManaGroupToText(ManaGroup mana, bool loopOrder)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (mana.Total == 0)
			{
				stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(0).Append("\">");
			}
			else
			{
				if (mana.Any > 0)
				{
					if (mana.Any > 9)
					{
						stringBuilder.Append(string.Format("{0}x<sprite=\"ManaSprite\" name=\"1\">", mana.Any));
					}
					else
					{
						stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(mana.Any).Append("\">");
					}
				}
				bool flag = loopOrder && mana.TrivialColorCount == 2;
				if (flag)
				{
					if (mana.White > 0)
					{
						if (mana.Red > 0)
						{
							goto IL_00C4;
						}
					}
					else if (mana.Blue <= 0)
					{
						goto IL_00C8;
					}
					if (mana.Green <= 0)
					{
						goto IL_00C8;
					}
					IL_00C4:
					bool flag2 = true;
					goto IL_00CA;
					IL_00C8:
					flag2 = false;
					IL_00CA:
					flag = flag2;
				}
				foreach (ManaColor manaColor in (flag ? ManaColors.ColorsTrivialReversed : ManaColors.Colors))
				{
					int value = mana.GetValue(manaColor);
					if (value > 9)
					{
						stringBuilder.Append(string.Format("{0}x", value));
						stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(manaColor.ToShortName()).Append("\">");
					}
					else
					{
						for (int i = 0; i < value; i++)
						{
							stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(manaColor.ToShortName()).Append("\">");
						}
					}
				}
				if (mana.Hybrid > 0)
				{
					int hybrid = mana.Hybrid;
					string text = (loopOrder ? ManaGroup.HybridColorShortNamesLoopOrder[mana.HybridColor] : ManaGroup.HybridColorShortNames[mana.HybridColor]);
					if (hybrid > 9)
					{
						stringBuilder.Append(string.Format("{0}x", hybrid));
						stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(text).Append("\">");
					}
					else
					{
						for (int j = 0; j < hybrid; j++)
						{
							stringBuilder.Append("<sprite=\"ManaSprite\" name=\"").Append(text).Append("\">");
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x0001C714 File Offset: 0x0001A914
		public static string BaseManaGroupToText(BaseManaGroup mana)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ManaColor manaColor in ManaColors.Colors)
			{
				for (int i = 0; i < mana.Value.GetValue(manaColor); i++)
				{
					stringBuilder.Append("<sprite=\"BaseManaSprite\" name=\"").Append(manaColor.ToShortName()).Append("\">");
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x0001C7A4 File Offset: 0x0001A9A4
		public static string WrapByColor(string text, Color color)
		{
			return string.Concat(new string[]
			{
				"<color=#",
				ColorUtility.ToHtmlStringRGB(color),
				">",
				text,
				"</color>"
			});
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x0001C7D6 File Offset: 0x0001A9D6
		public static string WrapNormalValue(int value)
		{
			return UiUtils.WrapByColor(value.ToString(), GlobalConfig.NormalColor);
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x0001C7EC File Offset: 0x0001A9EC
		public static Vector2 GetPreferredSize(TMP_Text text, float width, float height)
		{
			float num;
			float num2;
			float num3;
			float num4;
			text.margin.Deconstruct(out num, out num2, out num3, out num4);
			float num5 = num;
			float num6 = num2;
			float num7 = num3;
			float num8 = num4;
			text.GetPreferredValues(width - num5 - num7, height - num6 - num8).Deconstruct(out num4, out num3);
			float num9 = num4;
			float num10 = num3;
			return new Vector2(num9, num10);
		}

		// Token: 0x04000506 RID: 1286
		public static readonly string XCostText = "<sprite=\"ManaSprite\" name=\"X\">";
	}
}
