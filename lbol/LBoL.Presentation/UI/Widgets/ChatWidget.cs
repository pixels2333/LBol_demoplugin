using System;
using System.Collections.Generic;
using DG.Tweening;
using LBoL.Base.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200004B RID: 75
	public class ChatWidget : MonoBehaviour
	{
		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x0600048D RID: 1165 RVA: 0x00012B22 File Offset: 0x00010D22
		// (set) Token: 0x0600048E RID: 1166 RVA: 0x00012B2F File Offset: 0x00010D2F
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

		// Token: 0x0600048F RID: 1167 RVA: 0x00012B6F File Offset: 0x00010D6F
		private void Awake()
		{
			this.SetCloud(ChatWidget.CloudType.LeftThink, 800f, 400f);
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x00012BA0 File Offset: 0x00010DA0
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

		// Token: 0x06000491 RID: 1169 RVA: 0x00012C14 File Offset: 0x00010E14
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

		// Token: 0x06000492 RID: 1170 RVA: 0x00012D34 File Offset: 0x00010F34
		private void SetCloud(ChatWidget.CloudType type, float x, float y)
		{
			this._type = type;
			this.chatCloud.sprite = this.cloudSprites[(int)type];
			this.chatCloud.rectTransform.sizeDelta = new Vector2(x, y);
			this.chatCloud.transform.localPosition = new Vector2(-x * this._anchors[(int)type].x, -y * this._anchors[(int)type].y);
			this.text.margin = new Vector4(this._textMargins[(int)type].x * x, 0f, this._textMargins[(int)type].y * x, 0f);
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x00012DFC File Offset: 0x00010FFC
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

		// Token: 0x0400025E RID: 606
		private ScenePositionTier _scenePositionTier;

		// Token: 0x0400025F RID: 607
		private const float CloudFadeTime = 0.2f;

		// Token: 0x04000260 RID: 608
		private const float TextFadeTime = 0.1f;

		// Token: 0x04000261 RID: 609
		private ChatWidget.CloudType _type;

		// Token: 0x04000262 RID: 610
		[SerializeField]
		private Image chatCloud;

		// Token: 0x04000263 RID: 611
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x04000264 RID: 612
		[SerializeField]
		private List<Sprite> cloudSprites;

		// Token: 0x04000265 RID: 613
		private readonly List<Vector2> _anchors;

		// Token: 0x04000266 RID: 614
		private readonly List<Vector2> _textMargins;

		// Token: 0x020001CF RID: 463
		public enum CloudType
		{
			// Token: 0x04000EFD RID: 3837
			LeftThink,
			// Token: 0x04000EFE RID: 3838
			RightThink,
			// Token: 0x04000EFF RID: 3839
			LeftTalk,
			// Token: 0x04000F00 RID: 3840
			RightTalk
		}
	}
}
