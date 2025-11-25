using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class DamagePopup : MonoBehaviour
	{
		public void Show()
		{
			base.gameObject.SetActive(true);
			this.rb.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(4f, 6f));
			this.rb.gravityScale = 2f;
			base.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 1f).From(Vector3.one, true, false).SetEase(Ease.InSine);
			Object.Destroy(base.gameObject, 3f);
		}
		public TextMeshProUGUI tmp;
		public Rigidbody2D rb;
	}
}
