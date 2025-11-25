using System;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class CardFlyBrief : MonoBehaviour
	{
		private bool Updating { get; set; }
		private Vector2 LastPosition { get; set; }
		private void Start()
		{
			this.LastPosition = base.transform.position;
			this.Updating = true;
		}
		private void Update()
		{
			if (this.Updating)
			{
				Vector2 vector = base.transform.position - this.LastPosition;
				float num = Vector2.SignedAngle(Vector2.up, vector);
				this.card.transform.localRotation = Quaternion.Euler(0f, 0f, num);
			}
		}
		public void CloseCard()
		{
			this.Updating = false;
			this.card.SetActive(false);
		}
		public GameObject card;
	}
}
