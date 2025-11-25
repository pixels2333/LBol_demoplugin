using System;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation
{
	public class PlatformHandlerRunner : MonoBehaviour
	{
		public PlatformHandler PlatformHandler { get; set; }
		public static PlatformHandlerRunner Instance { get; private set; }
		private void Awake()
		{
			PlatformHandlerRunner.Instance = this;
		}
		private void OnDestroy()
		{
			PlatformHandler platformHandler = this.PlatformHandler;
			if (platformHandler == null)
			{
				return;
			}
			platformHandler.Shutdown();
		}
		private void Update()
		{
			PlatformHandler platformHandler = this.PlatformHandler;
			if (platformHandler == null)
			{
				return;
			}
			platformHandler.Update();
		}
	}
}
