using System;
using LBoL.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace LBoL.Presentation
{
	public class CameraController : MonoBehaviour
	{
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
		private static CameraController GetInstance()
		{
			if (!CameraController._instance)
			{
				throw new InvalidOperationException("CameraController is not initialized");
			}
			return CameraController._instance;
		}
		public static Camera MainCamera
		{
			get
			{
				return CameraController.GetInstance().mainCamera;
			}
		}
		public static Camera UiCamera
		{
			get
			{
				return CameraController.GetInstance().uiCamera;
			}
		}
		public static Canvas UiCanvas
		{
			get
			{
				return CameraController.GetInstance().uiCanvas;
			}
		}
		public static RectTransform UiRoot
		{
			get
			{
				return CameraController.GetInstance().root;
			}
		}
		public static Volume MainVolume
		{
			get
			{
				return CameraController.GetInstance().mainVolume;
			}
		}
		public static Camera DuplicateSceneCamera(string name)
		{
			CameraController instance = CameraController.GetInstance();
			Camera camera = Utils.CreateGameObject(instance.transform, name).AddComponent<Camera>();
			camera.CopyFrom(instance.mainCamera);
			return camera;
		}
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
		public static Vector3 ScenePositionToLocalPositionInRectTransform(Vector3 worldPosition, RectTransform parent)
		{
			return parent.InverseTransformPoint(CameraController.ScenePositionToWorldPositionInUI(worldPosition));
		}
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
		[SerializeField]
		private Camera mainCamera;
		[SerializeField]
		private Camera uiCamera;
		[SerializeField]
		private Canvas uiCanvas;
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private Volume mainVolume;
		private static CameraController _instance;
		private Camera _currentCamera;
	}
}
