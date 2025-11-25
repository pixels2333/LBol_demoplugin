using System;
using System.Text;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI
{
	[RequireComponent(typeof(TMP_InputField))]
	public class CharNumTransf : MonoBehaviour
	{
		private void Awake()
		{
			if (this.inputField == null)
			{
				this.inputField = base.GetComponent<TMP_InputField>();
			}
		}
		private void Start()
		{
			this.inputField.onValidateInput = new TMP_InputField.OnValidateInput(this._OnValidateInput);
		}
		private char _OnValidateInput(string text, int charIndex, char addedChar)
		{
			if (this.GetTransCharNum(text) + this.GetTransCharNum(addedChar.ToString()) > this.MaxLimit)
			{
				return '\0';
			}
			return addedChar;
		}
		private int GetTransCharNum(string text)
		{
			int num = 0;
			foreach (char c in text.ToCharArray())
			{
				num += this.SingleCharTrans(c);
			}
			return num;
		}
		private int SingleCharTrans(char singChar)
		{
			int num = Encoding.UTF8.GetBytes(singChar.ToString()).Length;
			if (num >= 2)
			{
				num = 2;
			}
			return num;
		}
		private TMP_InputField inputField;
		public int MaxLimit;
	}
}
