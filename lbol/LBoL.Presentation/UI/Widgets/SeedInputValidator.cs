using System;
using LBoL.Base;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000070 RID: 112
	[CreateAssetMenu(fileName = "SeedValidator", menuName = "SeedValidator")]
	public sealed class SeedInputValidator : TMP_InputValidator
	{
		// Token: 0x060005DF RID: 1503 RVA: 0x000196CB File Offset: 0x000178CB
		public override char Validate(ref string text, ref int pos, char ch)
		{
			if (text.Length == RandomGen.MaxSeedSize)
			{
				return '\0';
			}
			if (!RandomGen.IsValidSeedChar(ch))
			{
				return '\0';
			}
			text = text.Insert(pos, ch.ToString());
			pos++;
			return ch;
		}
	}
}
