using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LBoL.Presentation
{
	// Token: 0x02000008 RID: 8
	public class CameraController : MonoBehaviour
	{
		// Token: 0x0600006A RID: 106 RVA: 0x00003354 File Offset: 0x00001554
		private void Awake()
		{
			if (CameraController._instance)
			{
				Debug.LogError("Multiple CameraController exists");
				Object.Destroy(base.gameObject);
				return;
			}
			CameraController._instance = this;
			Object.DontDestroyOnLoad(this);
			Object.DontDestroyOnLoad(this.mainCamera.gameObject);
			Object.DontDestroyOnLoad(this.uiCamera.gameObject);
			Object.DontDestroyOnLoad(this.mainVolume.gameObject);
		}

		// Token: 0x0600006B RID: 107 RVA: 0x000033BF File Offset: 0x000015BF
		private static CameraController GetInstance()
		{
			if (!CameraController._instance)
			{
				throw new InvalidOperationException("CameraController is not initialized");
			}
			return CameraController._instance;
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600006C RID: 108 RVA: 0x000033DD File Offset: 0x000015DD
		public static Camera MainCamera
		{
			get
			{
				return CameraController.GetInstance().mainCamera;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600006D RID: 109 RVA: 0x000033E9 File Offset: 0x000015E9
		public static Camera UiCamera
		{
			get
			{
				return CameraController.GetInstance().uiCamera;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000033F5 File Offset: 0x000015F5
		public static Canvas UiCanvas
		{
			get
			{
				return CameraController.GetInstance().uiCanvas;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003401 File Offset: 0x00001601
		public static RectTransform UiRoot
		{
			get
			{
				return CameraController.GetInstance().root;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000070 RID: 112 RVA: 0x0000340D File Offset: 0x0000160D
		public static Volume MainVolume
		{
			get
			{
				return CameraController.GetInstance().mainVolume;
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x0000341C File Offset: 0x0000161C
		public static Camera DuplicateSceneCamera(string name)
		{
			CameraController instance = CameraController.GetInstance();
			Camera camera = Utils.CreateGameObject(instance.transform, name).AddComponent<Camera>();
			camera.CopyFrom(instance.mainCamera);
			return camera;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x0000344C File Offset: 0x0000164C
		public static Camera SplitSceneCameraByLayer(string name, int splitedMask)
		{
			CameraController instance = CameraController.GetInstance();
			Camera camera = instance.mainCamera;
			Camera camera2 = Utils.CreateGameObject(instance.transform, "Additional Scene Camera").AddComponent<Camera>();
			camera2.CopyFrom(camera);
			int num = camera2.cullingMask & ~splitedMask;
			camera.cullingMask = num;
			camera2.cullingMask = splitedMask;
			UniversalAdditionalCameraData universalAdditionalCameraData = instance.mainCamera.GetUniversalAdditionalCameraData();
			camera2.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
			int num2 = universalAdditionalCameraData.cameraStack.IndexOf(CameraController.UiCamera);
			universalAdditionalCameraData.cameraStack.Insert(num2, camera2);
			return camera2;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x000034D0 File Offset: 0x000016D0
		public static void DestroySceneSubCamera(Camera subCamera)
		{
			CameraController instance = CameraController.GetInstance();
			if (subCamera == instance.mainCamera || subCamera == instance.uiCamera)
			{
				Debug.LogError("Cannot destroy main camera or UI camera");
				return;
			}
			UniversalAdditionalCameraData universalAdditionalCameraData = instance.mainCamera.GetUniversalAdditionalCameraData();
			int num = universalAdditionalCameraData.cameraStack.IndexOf(subCamera);
			if (num >= 0)
			{
				universalAdditionalCameraData.cameraStack.RemoveAt(num);
				instance.mainCamera.cullingMask |= subCamera.cullingMask;
				Object.Destroy(subCamera.gameObject);
				return;
			}
			Debug.LogError("Destroy scene camera " + subCamera.name + " is not in camera stack");
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003571 File Offset: 0x00001771
		public static Vector3 ScenePositionToLocalPositionInRectTransform(Vector3 worldPosition, RectTransform parent)
		{
			return parent.InverseTransformPoint(CameraController.ScenePositionToWorldPositionInUI(worldPosition));
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003580 File Offset: 0x00001780
		public static Vector3 ScenePositionToWorldPositionInUI(Vector3 worldPosition)
		{
			CameraController instance = CameraController.GetInstance();
			Vector2 vector = instance.mainCamera.WorldToScreenPoint(worldPosition);
			Ray ray = RectTransformUtility.ScreenPointToRay(instance.uiCamera, vector);
			RectTransform rectTransform = (RectTransform)instance.uiCanvas.transform;
			Plane plane = new Plane(rectTransform.rotation * Vector3.back, rectTransform.position);
			float num;
			if (plane.Raycast(ray, out num))
			{
				return ray.GetPoint(num);
			}
			Debug.Log(string.Format("[CameraController] Cannot get UI position from screen position {0}", vector));
			return Vector3.zero;
		}

		// Token: 0x04000028 RID: 40
		[SerializeField]
		private Camera mainCamera;

		// Token: 0x04000029 RID: 41
		[SerializeField]
		private Camera uiCamera;

		// Token: 0x0400002A RID: 42
		[SerializeField]
		private Canvas uiCanvas;

		// Token: 0x0400002B RID: 43
		[SerializeField]
		private RectTransform root;

		// Token: 0x0400002C RID: 44
		[SerializeField]
		private Volume mainVolume;

		// Token: 0x0400002D RID: 45
		private static CameraController _instance;

		// Token: 0x0400002E RID: 46
		private Camera _currentCamera;
	}
}
