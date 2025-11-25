using System;
using UnityEngine;
namespace LBoL.Presentation.Bullet
{
	public class ParticalRotator : MonoBehaviour
	{
		private void Awake()
		{
			this._lastLocate = base.transform.position;
		}
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
		public float speed = 500f;
		public float speedIncre;
		public float speedByMove;
		private Vector3 _lastLocate;
	}
}
