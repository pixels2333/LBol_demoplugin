using System;
using UnityEngine;

namespace LBoL.Presentation.Bullet
{
	// Token: 0x02000115 RID: 277
	public class ParticalRotator : MonoBehaviour
	{
		// Token: 0x06000F69 RID: 3945 RVA: 0x000488E3 File Offset: 0x00046AE3
		private void Awake()
		{
			this._lastLocate = base.transform.position;
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x000488F8 File Offset: 0x00046AF8
		private void Update()
		{
			this.speed += this.speedIncre;
			if (this.speedByMove == 0f)
			{
				base.transform.Rotate(new Vector3(0f, 0f, this.speed * Time.deltaTime));
				return;
			}
			Vector3 position = base.transform.position;
			float num = Vector3.Distance(position, this._lastLocate);
			this._lastLocate = position;
			base.transform.Rotate(new Vector3(0f, 0f, this.speed * Time.deltaTime + num * this.speedByMove));
		}

		// Token: 0x04000B78 RID: 2936
		public float speed = 500f;

		// Token: 0x04000B79 RID: 2937
		public float speedIncre;

		// Token: 0x04000B7A RID: 2938
		public float speedByMove;

		// Token: 0x04000B7B RID: 2939
		private Vector3 _lastLocate;
	}
}
