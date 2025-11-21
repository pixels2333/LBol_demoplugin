using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LBoL.Presentation
{
	// Token: 0x0200000E RID: 14
	public class PortraitGroup
	{
		// Token: 0x06000162 RID: 354 RVA: 0x0000760C File Offset: 0x0000580C
		public static PortraitGroup Load(string charName)
		{
			PortraitGroup portraitGroup = new PortraitGroup();
			foreach (Sprite sprite in Resources.LoadAll<Sprite>("Portraits/" + charName))
			{
				portraitGroup._spriteTable.Add(sprite.name, sprite);
			}
			return portraitGroup;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00007655 File Offset: 0x00005855
		public void Release()
		{
			if (this._handle != null)
			{
				ResourcesHelper.Release(this._handle.Value);
			}
			this._spriteTable.Clear();
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00007680 File Offset: 0x00005880
		public Sprite Get(string name)
		{
			Sprite sprite;
			if (!this._spriteTable.TryGetValue(name, ref sprite))
			{
				return null;
			}
			return sprite;
		}

		// Token: 0x04000062 RID: 98
		private readonly Dictionary<string, Sprite> _spriteTable = new Dictionary<string, Sprite>();

		// Token: 0x04000063 RID: 99
		private AsyncOperationHandle? _handle;
	}
}
