using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000C7 RID: 199
	public class DamagePopup : MonoBehaviour
	{
		// Token: 0x06000C18 RID: 3096 RVA: 0x0003E6F0 File Offset: 0x0003C8F0
		public void Show()
		{
			base.gameObject.SetActive(true);
			this.rb.velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(4f, 6f));
			this.rb.gravityScale = 2f;
			base.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 1f).From(Vector3.one, true, false).SetEase(Ease.InSine);
			Object.Destroy(base.gameObject, 3f);
		}

		// Token: 0x04000960 RID: 2400
		public TextMeshProUGUI tmp;

		// Token: 0x04000961 RID: 2401
		public Rigidbody2D rb;
	}
}
