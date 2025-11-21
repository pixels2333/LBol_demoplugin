using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI
{
	// Token: 0x0200001D RID: 29
	[RequireComponent(typeof(TMP_InputField))]
	public class CharNumTransf : MonoBehaviour
	{
		// Token: 0x060002F6 RID: 758 RVA: 0x0000D47E File Offset: 0x0000B67E
		private void Awake()
		{
			if (this.inputField == null)
			{
				this.inputField = base.GetComponent<TMP_InputField>();
			}
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000D49A File Offset: 0x0000B69A
		private void Start()
		{
			this.inputField.onValidateInput = new TMP_InputField.OnValidateInput(this._OnValidateInput);
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000D4B3 File Offset: 0x0000B6B3
		private char _OnValidateInput(string text, int charIndex, char addedChar)
		{
			if (this.GetTransCharNum(text) + this.GetTransCharNum(addedChar.ToString()) > this.MaxLimit)
			{
				return '\0';
			}
			return addedChar;
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x0000D4D8 File Offset: 0x0000B6D8
		private int GetTransCharNum(string text)
		{
			int num = 0;
			foreach (char c in text.ToCharArray())
			{
				num += this.SingleCharTrans(c);
			}
			return num;
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0000D50C File Offset: 0x0000B70C
		private int SingleCharTrans(char singChar)
		{
			int num = Encoding.UTF8.GetBytes(singChar.ToString()).Length;
			if (num >= 2)
			{
				num = 2;
			}
			return num;
		}

		// Token: 0x0400013B RID: 315
		private TMP_InputField inputField;

		// Token: 0x0400013C RID: 316
		public int MaxLimit;
	}
}
