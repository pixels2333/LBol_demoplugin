using System;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class ChangeLogPanel : UiPanel
	{
		protected override void OnShown()
		{
		}
		protected override void OnHided()
		{
		}
		private void Awake()
		{
			base.GetComponent<Button>().onClick.AddListener(new UnityAction(base.Hide));
		}
	}
}
