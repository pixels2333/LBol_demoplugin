using System;
using System.Collections.Generic;
using DG.Tweening;
using LBoL.Base.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class ChatWidget : MonoBehaviour
	{
		public Transform TargetTransform
		{
			get
			{
				return this._scenePositionTier.TargetTransform;
			}
			set
			{
				if (!this._scenePositionTier)
				{
					Debug.LogError("Setting TargetTransform before Awake");
					this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
				}
				this._scenePositionTier.TargetTransform = value;
			}
		}
		private void Awake()
		{
			this.SetCloud(ChatWidget.CloudType.LeftThink, 800f, 400f);
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
		}
		public void Chat(string content, float time, float delay = 0f, ChatWidget.CloudType type = ChatWidget.CloudType.LeftThink)
		{
			float num = 800f;
			float num2 = 400f;
			Vector2 sizeDelta = this.chatCloud.rectTransform.sizeDelta;
			if (type != this._type || Math.Abs(num - sizeDelta.x) > 1f || Math.Abs(num - sizeDelta.y) > 1f)
			{
				this.SetCloud(type, num, num2);
			}
			this.InternalChat(content, time, delay, type);
		}
		private void InternalChat(string content, float time, float delay = 0f, ChatWidget.CloudType type = ChatWidget.CloudType.LeftThink)
		{
			this.text.text = content;
			this.chatCloud.color = this.chatCloud.color.WithA(0f);
			this.text.alpha = 0f;
			this.text.ForceMeshUpdate(false, false);
			int lineCount = this.text.textInfo.lineCount;
			this.text.alignment = ((lineCount > 1) ? TextAlignmentOptions.Left : TextAlignmentOptions.Center);
			this.DOKill(true);
			DOTween.Sequence().Insert(delay, this.chatCloud.DOFade(1f, 0.2f)).Insert(delay, this.text.DOFade(1f, 0.1f))
				.Insert(delay + time, this.chatCloud.DOFade(0f, 0.2f))
				.Insert(delay + time, this.text.DOFade(0f, 0.1f))
				.SetAutoKill(true)
				.SetUpdate(true)
				.SetTarget(this)
				.SetLink(base.gameObject);
		}
		private void SetCloud(ChatWidget.CloudType type, float x, float y)
		{
			this._type = type;
			this.chatCloud.sprite = this.cloudSprites[(int)type];
			this.chatCloud.rectTransform.sizeDelta = new Vector2(x, y);
			this.chatCloud.transform.localPosition = new Vector2(-x * this._anchors[(int)type].x, -y * this._anchors[(int)type].y);
			this.text.margin = new Vector4(this._textMargins[(int)type].x * x, 0f, this._textMargins[(int)type].y * x, 0f);
		}
		public ChatWidget()
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(0.19f, 0.03f));
			list.Add(new Vector2(0.81f, 0.03f));
			list.Add(new Vector2(0.17f, 0.03f));
			list.Add(new Vector2(0.83f, 0.03f));
			this._anchors = list;
			List<Vector2> list2 = new List<Vector2>();
			list2.Add(new Vector2(0.12f, 0.1f));
			list2.Add(new Vector2(0.12f, 0.1f));
			list2.Add(new Vector2(0.12f, 0.1f));
			list2.Add(new Vector2(0.12f, 0.1f));
			this._textMargins = list2;
			base..ctor();
		}
		private ScenePositionTier _scenePositionTier;
		private const float CloudFadeTime = 0.2f;
		private const float TextFadeTime = 0.1f;
		private ChatWidget.CloudType _type;
		[SerializeField]
		private Image chatCloud;
		[SerializeField]
		private TextMeshProUGUI text;
		[SerializeField]
		private List<Sprite> cloudSprites;
		private readonly List<Vector2> _anchors;
		private readonly List<Vector2> _textMargins;
		public enum CloudType
		{
			LeftThink,
			RightThink,
			LeftTalk,
			RightTalk
		}
	}
}
