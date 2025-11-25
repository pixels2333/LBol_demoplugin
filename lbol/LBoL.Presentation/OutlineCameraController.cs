using System;
using UnityEngine;
namespace LBoL.Presentation
{
	public class OutlineCameraController : MonoBehaviour
	{
		private void Start()
		{
			this.outlineMat = new Material(this.postOutline);
			this.tmpCam.backgroundColor = Color.clear;
			this.tmpCam.clearFlags = CameraClearFlags.Color;
			this.tmpCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");
			RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.R8);
			this.tmpCam.targetTexture = this.renderTexture;
			this.tmpCam.RenderWithShader(this.drawSimple, "");
		}
		public Shader drawSimple;
		public Shader postOutline;
		public RenderTexture renderTexture;
		private Material outlineMat;
		public Camera tmpCam;
	}
}
