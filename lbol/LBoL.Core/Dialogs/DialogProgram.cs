using System;
using UnityEngine;
namespace LBoL.Core.Dialogs
{
	public class DialogProgram : ScriptableObject
	{
		[SerializeField]
		[HideInInspector]
		public byte[] bytes;
	}
}
