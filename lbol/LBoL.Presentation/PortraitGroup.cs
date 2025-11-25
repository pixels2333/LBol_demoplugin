using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace LBoL.Presentation
{
	public class PortraitGroup
	{
		public static PortraitGroup Load(string charName)
		{
			PortraitGroup portraitGroup = new PortraitGroup();
			foreach (Sprite sprite in Resources.LoadAll<Sprite>("Portraits/" + charName))
			{
				portraitGroup._spriteTable.Add(sprite.name, sprite);
			}
			return portraitGroup;
		}
		public void Release()
		{
			if (this._handle != null)
			{
				ResourcesHelper.Release(this._handle.Value);
			}
			this._spriteTable.Clear();
		}
		public Sprite Get(string name)
		{
			Sprite sprite;
			if (!this._spriteTable.TryGetValue(name, ref sprite))
			{
				return null;
			}
			return sprite;
		}
		private readonly Dictionary<string, Sprite> _spriteTable = new Dictionary<string, Sprite>();
		private AsyncOperationHandle? _handle;
	}
}
