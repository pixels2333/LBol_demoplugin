using System;
using UnityEngine;

namespace LBoL.Presentation
{
	// Token: 0x0200000C RID: 12
	public class OutlineCameraController : MonoBehaviour
	{
		// Token: 0x06000158 RID: 344 RVA: 0x00007520 File Offset: 0x00005720
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

		// Token: 0x0400005B RID: 91
		public Shader drawSimple;

		// Token: 0x0400005C RID: 92
		public Shader postOutline;

		// Token: 0x0400005D RID: 93
		public RenderTexture renderTexture;

		// Token: 0x0400005E RID: 94
		private Material outlineMat;

		// Token: 0x0400005F RID: 95
		public Camera tmpCam;
	}
}
