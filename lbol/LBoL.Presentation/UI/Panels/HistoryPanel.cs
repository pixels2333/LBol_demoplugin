using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using LBoL.Core.SaveData;
using LBoL.EntityLib.Cards.Others;
using LBoL.EntityLib.Exhibits;
using LBoL.Presentation.I10N;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200009E RID: 158
	[DisallowMultipleComponent]
	public sealed class HistoryPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x06000830 RID: 2096 RVA: 0x00026B10 File Offset: 0x00024D10
		private void Awake()
		{
			this.recordRowTemplate.gameObject.SetActive(false);
			this.cardCellTemplate.gameObject.SetActive(false);
			this.seedButton.onClick.AddListener(new UnityAction(this.CopySeed));
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x00026B50 File Offset: 0x00024D50
		protected override void OnShowing()
		{
			this._records = GameMaster.GetGameRunHistory();
			this.listContent.DestroyChildren();
			float num = 10f;
			float num2 = 10f;
			this._prevSelected = null;
			using (List<GameRunRecordSaveData>.Enumerator enumerator = this._records.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GameRunRecordSaveData record = enumerator.Current;
					RecordRow row = Object.Instantiate<RecordRow>(this.recordRowTemplate, this.listContent);
					row.gameObject.SetActive(true);
					RectTransform rectTransform = (RectTransform)row.transform;
					rectTransform.anchoredPosition = new Vector2(0f, -num);
					HistoryPanel.AvatarGroup valueOrDefault = CollectionExtensions.GetValueOrDefault<string, HistoryPanel.AvatarGroup>(this.avatarTable, record.Player, null);
					Sprite sprite2;
					if (valueOrDefault != null)
					{
						Sprite sprite;
						switch (record.ResultType)
						{
						case GameResultType.Failure:
							sprite = valueOrDefault.Failure;
							break;
						case GameResultType.NormalEnd:
							sprite = valueOrDefault.Normal;
							break;
						case GameResultType.TrueEndFail:
							sprite = valueOrDefault.Normal;
							break;
						case GameResultType.TrueEnd:
							sprite = valueOrDefault.TrueEnd;
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						sprite2 = sprite;
					}
					else
					{
						sprite2 = null;
					}
					Sprite sprite3 = sprite2;
					row.Set(sprite3, record);
					row.SetSelected(false, false);
					num += rectTransform.sizeDelta.y + num2;
					row.Click += delegate
					{
						this.OnRecordRowClick(row, record);
					};
					row.GetComponent<GamepadButtonCursor>().OnClick.AddListener(delegate
					{
						this.OnRecordRowClick(row, record);
					});
					this._prevSelected = row;
				}
			}
			this.listContent.sizeDelta = this.listContent.sizeDelta.WithY(num);
			this.listScrollRect.ScrollToBottom();
			if (this._prevSelected)
			{
				this._prevSelected.SetSelected(true, false);
				this.SetRecord(this._prevSelected.Record);
				this._prevSelected.gameObject.AddComponent<GamepadNavigationOrigin>();
			}
			else
			{
				this.ClearRecord();
			}
			this.rootCanvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x00026DAC File Offset: 0x00024FAC
		public override void OnLocaleChanged()
		{
			if (this._prevSelected != null)
			{
				this.SetRecord(this._prevSelected.Record);
			}
			foreach (ValueTuple<int, PuzzleFlag> valueTuple in PuzzleFlags.AllPuzzleFlags.WithIndices<PuzzleFlag>())
			{
				int item = valueTuple.Item1;
				PuzzleFlagDisplayWord displayWord = PuzzleFlags.GetDisplayWord(valueTuple.Item2);
				SimpleTooltipSource.CreateDirect(this.puzzleIcons[item], displayWord.Name, displayWord.Description);
			}
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x00026E40 File Offset: 0x00025040
		protected override void OnHiding()
		{
			this.rootCanvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x00026E54 File Offset: 0x00025054
		protected override void OnHided()
		{
			this.listContent.DestroyChildren();
			this.ClearRecord();
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x00026E67 File Offset: 0x00025067
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x00026E70 File Offset: 0x00025070
		public void OnRecordRowClick(RecordRow row, GameRunRecordSaveData record)
		{
			if (this._prevSelected)
			{
				if (this._prevSelected == row)
				{
					return;
				}
				this._prevSelected.SetSelected(false, true);
			}
			this._prevSelected = row;
			row.SetSelected(true, true);
			this.SetRecord(record);
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x00026EBC File Offset: 0x000250BC
		private unsafe void SetRecord(GameRunRecordSaveData record)
		{
			try
			{
				this.detailPageGroup.gameObject.SetActive(true);
				this.detailPageGroup.DOKill(false);
				this.detailPageGroup.DOFade(1f, 0.5f).From(0f, true, false).SetLink(this.detailPageGroup.gameObject)
					.SetTarget(this.detailPageGroup);
				StringBuilder stringBuilder = new StringBuilder();
				DateTime dateTime;
				if (Utils.TryParseIso8601Timestamp(record.SaveTimestamp, out dateTime))
				{
					stringBuilder.AppendLine(dateTime.ToString(L10nManager.Info.Culture));
				}
				else
				{
					stringBuilder.AppendLine("？时？刻");
				}
				stringBuilder.AppendLine(string.Format("Tooltip.Difficulty{0}.Name", record.Difficulty).Localize(true));
				if (record.ResultType == GameResultType.Failure)
				{
					string failingEnemyGroup = record.FailingEnemyGroup;
					if (failingEnemyGroup != null)
					{
						stringBuilder.Append("<color=#ff0000>").Append("GameResult.FailReason".Localize(true)).Append(failingEnemyGroup)
							.Append("</color>")
							.AppendLine();
					}
					else
					{
						string failingAdventure = record.FailingAdventure;
						if (failingAdventure != null)
						{
							Adventure adventure = Library.TryCreateAdventure(failingAdventure);
							if (adventure != null)
							{
								stringBuilder.Append("<color=#ff0000>").Append("GameResult.FailReason".Localize(true)).Append(adventure.Title)
									.Append("</color>")
									.AppendLine();
							}
							else
							{
								stringBuilder.Append("<color=#ff0000>").Append("GameResult.FailReason".Localize(true)).Append("GameResult.Deprecated".Localize(true))
									.Append("</color>")
									.AppendLine();
							}
						}
						else
						{
							stringBuilder.AppendLine("<color=#ffff00>Abandon</color>");
						}
					}
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					string text;
					switch (record.ResultType)
					{
					case GameResultType.NormalEnd:
						text = "GameResult.NormalEnd".Localize(true);
						break;
					case GameResultType.TrueEndFail:
						text = "GameResult.TrueEndFail".Localize(true);
						break;
					case GameResultType.TrueEnd:
						text = "GameResult.TrueEnd".Localize(true);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					stringBuilder2.AppendLine(text);
				}
				this.difficultyIcon.sprite = CollectionExtensions.GetValueOrDefault<GameDifficulty, Sprite>(this.difficultyIcons, record.Difficulty, null);
				if (record.Puzzles != PuzzleFlag.None)
				{
					this.puzzlesParent.SetActive(true);
					string text2 = "UI.Numbers".LocalizeStrings(true)[Enumerable.Count<PuzzleFlag>(PuzzleFlags.EnumerateComponents(record.Puzzles))];
					this.puzzleText.text = "History.Puzzles".LocalizeFormat(new object[] { text2 });
					using (IEnumerator<ValueTuple<int, PuzzleFlag>> enumerator = PuzzleFlags.AllPuzzleFlags.WithIndices<PuzzleFlag>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ValueTuple<int, PuzzleFlag> valueTuple = enumerator.Current;
							int item = valueTuple.Item1;
							PuzzleFlag item2 = valueTuple.Item2;
							this.puzzleIcons[item].SetActive(record.Puzzles.HasFlag(item2));
						}
						goto IL_02EF;
					}
				}
				this.puzzlesParent.SetActive(false);
				IL_02EF:
				string[] array = record.JadeBoxes;
				if (array != null && array.Length > 0)
				{
					this.jadeBoxParent.SetActive(true);
					List<string> list = new List<string>();
					foreach (string text3 in record.JadeBoxes)
					{
						JadeBox jadeBox = Library.TryCreateJadeBox(text3);
						if (jadeBox != null)
						{
							list.Add(jadeBox.Name);
						}
						else
						{
							Debug.LogError("JadeBox " + text3 + " not found");
						}
					}
					this.jadeBoxNamesText.text = string.Join(" ", list);
				}
				else
				{
					this.jadeBoxParent.SetActive(false);
				}
				this.manaText.text = "History.Mana".Localize(true);
				this.moneyText.text = string.Concat(new string[]
				{
					"<sprite=\"Point\" name=\"Heart\"> ",
					record.MaxHp.ToString(),
					"  <sprite=\"Point\" name=\"Gold\"> ",
					record.TotalMoney.ToString(),
					"  <sprite=\"Point\" name=\"Point\"> ",
					record.BluePoint.ToString()
				});
				this.baseManaParent.DestroyChildren();
				ManaGroup manaGroup;
				if (ManaGroup.TryParse(record.BaseMana, out manaGroup))
				{
					foreach (ManaColor manaColor in manaGroup.EnumerateComponents())
					{
						Object.Instantiate<BaseManaWidget>(this.baseManaTemplate, this.baseManaParent).SetBaseMana(manaColor);
					}
				}
				Dictionary<Rarity, int> dictionary = new Dictionary<Rarity, int>();
				this.exhibitsParent.DestroyChildren();
				Vector2 sizeDelta = this.exhibitsParent.sizeDelta;
				int num = 100;
				int num2 = 20;
				int num3 = 20;
				int num4 = ((record.Exhibits.Length != 0) ? ((record.Exhibits.Length - 1) / num3 + 1) : 0);
				foreach (ValueTuple<int, string> valueTuple2 in record.Exhibits.WithIndices<string>())
				{
					int item3 = valueTuple2.Item1;
					string item4 = valueTuple2.Item2;
					Exhibit exhibit = Library.TryCreateExhibit(item4);
					if (exhibit == null)
					{
						Debug.LogError("[HistoryPanel] Missing " + item4 + " in current version");
						exhibit = Library.CreateExhibit<KongZhanpinhe>();
					}
					ExhibitWidget exhibitWidget = Object.Instantiate<ExhibitWidget>(this.exhibitTemplate, this.exhibitsParent);
					exhibitWidget.Exhibit = exhibit;
					exhibitWidget.ShowCounter = false;
					RectTransform rectTransform = exhibitWidget.RectTransform;
					rectTransform.anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
					int i = item3 % num3;
					int num5 = item3 / num3;
					int num6 = i;
					int num7 = num5;
					rectTransform.anchoredPosition = new Vector2((float)((num + num2) * num6 + num / 2), (float)(-(float)(num + num2) * num7 - num / 2));
					Rarity rarity = exhibit.Config.Rarity;
					dictionary[rarity] = CollectionExtensions.GetValueOrDefault<Rarity, int>(dictionary, rarity, 0) + 1;
				}
				this.exhibitsParent.sizeDelta = new Vector2(sizeDelta.x, (float)((num4 > 0) ? (num4 * (num + num2)) : 0));
				this.exhibitText.text = "History.Exhibit".LocalizeFormat(new object[] { record.Exhibits.Length });
				List<string> list2 = new List<string>();
				IntPtr intPtr = stackalloc byte[(UIntPtr)20];
				*intPtr = 4;
				*(intPtr + (IntPtr)sizeof(Rarity)) = 3;
				*(intPtr + (IntPtr)2 * (IntPtr)sizeof(Rarity)) = 2;
				*(intPtr + (IntPtr)3 * (IntPtr)sizeof(Rarity)) = 1;
				*(intPtr + (IntPtr)4 * (IntPtr)sizeof(Rarity)) = 0;
				Span<Rarity> span = new Span<Rarity>(intPtr, 5);
				for (int i = 0; i < span.Length; i++)
				{
					Rarity rarity2 = *span[i];
					int num8;
					if (dictionary.TryGetValue(rarity2, ref num8))
					{
						Color valueOrDefault = CollectionExtensions.GetValueOrDefault<Rarity, Color>(this.rarityColors, rarity2, Color.white);
						string text4 = "History.StatsSegment".LocalizeFormat(new object[]
						{
							("Rarity." + rarity2.ToString()).Localize(true),
							num8
						});
						list2.Add(UiUtils.WrapByColor(text4, valueOrDefault));
					}
				}
				this.exhibitStatsText.text = string.Join("   ", list2);
				Dictionary<Rarity, int> dictionary2 = new Dictionary<Rarity, int>();
				int num9 = 0;
				this.cardsParent.DestroyChildren();
				var list3 = Enumerable.ToList(Enumerable.Select(Enumerable.GroupBy<CardRecordSaveData, ValueTuple<string, bool, int?>>(record.Cards, (CardRecordSaveData c) => new ValueTuple<string, bool, int?>(c.Id, c.Upgraded, c.UpgradeCounter)), ([TupleElementNames(new string[] { "Id", "Upgraded", "UpgradeCounter" })] IGrouping<ValueTuple<string, bool, int?>, CardRecordSaveData> g) => new
				{
					Card = Enumerable.First<CardRecordSaveData>(g),
					Count = Enumerable.Count<CardRecordSaveData>(g),
					Upgraded = g.Key.Item2,
					UpgradeCounter = g.Key.Item3
				}));
				foreach (var valueTuple3 in list3.WithIndices())
				{
					int item5 = valueTuple3.Item1;
					var item6 = valueTuple3.Item2;
					Card card = Library.TryCreateCard(item6.Card.Id, item6.Upgraded, item6.UpgradeCounter);
					if (card == null)
					{
						HistoryCard historyCard = Library.CreateCard<HistoryCard>(item6.Upgraded);
						historyCard.CardId = item6.Card.Id;
						card = historyCard;
					}
					RecordCardCell recordCardCell = Object.Instantiate<RecordCardCell>(this.cardCellTemplate, this.cardsParent);
					recordCardCell.Card = card;
					recordCardCell.SetNum(item6.Count);
					recordCardCell.gameObject.SetActive(true);
					if (card.CardType == CardType.Misfortune)
					{
						recordCardCell.SetBorderColor(this.misfortuneColor);
						num9 += item6.Count;
					}
					else
					{
						Rarity rarity3 = card.Config.Rarity;
						Color valueOrDefault2 = CollectionExtensions.GetValueOrDefault<Rarity, Color>(this.rarityColors, rarity3, Color.white);
						recordCardCell.SetBorderColor(valueOrDefault2);
						dictionary2[rarity3] = CollectionExtensions.GetValueOrDefault<Rarity, int>(dictionary2, rarity3, 0) + item6.Count;
					}
					int i = item5 / 4;
					int num10 = item5 % 4;
					int num11 = i;
					int num12 = num10;
					recordCardCell.RectTransform.anchoredPosition = new Vector2((float)(656 * num12), (float)(-104 * num11));
				}
				int num13 = (list3.Count + 1) / 4 * 104;
				this.cardsParent.sizeDelta = this.cardsParent.sizeDelta.WithY((float)num13);
				this.cardText.text = "History.Card".LocalizeFormat(new object[] { record.Cards.Length });
				List<string> list4 = new List<string>();
				IntPtr intPtr2 = stackalloc byte[(UIntPtr)12];
				*intPtr2 = 2;
				*(intPtr2 + (IntPtr)sizeof(Rarity)) = 1;
				*(intPtr2 + (IntPtr)2 * (IntPtr)sizeof(Rarity)) = 0;
				span = new Span<Rarity>(intPtr2, 3);
				for (int i = 0; i < span.Length; i++)
				{
					Rarity rarity4 = *span[i];
					int num14;
					if (dictionary2.TryGetValue(rarity4, ref num14))
					{
						Color valueOrDefault3 = CollectionExtensions.GetValueOrDefault<Rarity, Color>(this.rarityColors, rarity4, Color.white);
						string text5 = "History.StatsSegment".LocalizeFormat(new object[]
						{
							("Rarity." + rarity4.ToString()).Localize(true),
							num14
						});
						list4.Add(UiUtils.WrapByColor(text5, valueOrDefault3));
					}
				}
				if (num9 > 0)
				{
					string text6 = "History.StatsSegment".LocalizeFormat(new object[]
					{
						"CardType.Misfortune".Localize(true),
						num9
					});
					list4.Add(UiUtils.WrapByColor(text6, this.misfortuneColor));
				}
				this.cardStatsText.text = string.Join("   ", list4);
				Vector2 zero = Vector2.zero;
				this.manaMoneyTitle.anchoredPosition = zero;
				zero.y -= this.manaMoneyTitle.sizeDelta.y;
				this.exhibitsTitle.anchoredPosition = zero;
				zero.y -= this.exhibitsTitle.sizeDelta.y;
				this.exhibitsParent.anchoredPosition = zero;
				zero.y -= this.exhibitsParent.sizeDelta.y;
				this.cardsTitle.anchoredPosition = zero;
				zero.y -= this.cardsTitle.sizeDelta.y;
				this.cardsParent.anchoredPosition = zero;
				zero.y -= this.cardsParent.sizeDelta.y;
				this.entityArea.sizeDelta = this.entityArea.sizeDelta.WithY(-zero.y);
				this.seedString = RandomGen.SeedToString(record.Seed);
				if (record.GameVersion.IsNullOrEmpty())
				{
					this.seedText.text = this.seedString;
				}
				else if (record.IsAutoSeed)
				{
					this.seedText.text = this.seedString + "    " + "History.AutoSeed".Localize(true);
				}
				else
				{
					this.seedText.text = this.seedString + "    " + "History.NotAutoSeed".Localize(true);
				}
				string text7 = "";
				if (!record.GameVersion.IsNullOrEmpty())
				{
					text7 = text7 + record.GameVersion + "    ";
					if (record.ShowRandomResult)
					{
						text7 += UiUtils.WrapByColor("StartGame.ShowRandomResult".Localize(true), GlobalConfig.UiRed);
					}
					else
					{
						text7 += UiUtils.WrapByColor("StartGame.NotShowRandomResult".Localize(true), GlobalConfig.UiRed);
					}
					text7 = text7 + "    " + UiUtils.WrapByColor("History.ReloadTimes".LocalizeFormat(new object[] { record.ReloadTimes }), GlobalConfig.UiBlue) + "    ";
				}
				text7 += "History.PlayedTime".LocalizeFormat(new object[] { Utils.SecondsToHHMMSS(record.TotalSeconds) });
				this.playedTimeText.text = text7;
			}
			catch (Exception ex)
			{
				Debug.LogError("[RecordPanel] Failed to show record on " + record.SaveTimestamp);
				Debug.LogException(ex);
				this.ClearRecord();
			}
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x00027BF8 File Offset: 0x00025DF8
		private void ClearRecord()
		{
			this.detailPageGroup.gameObject.SetActive(false);
			this.detailPageGroup.DOKill(false);
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x00027C18 File Offset: 0x00025E18
		private void CopySeed()
		{
			GUIUtility.systemCopyBuffer = this.seedString;
			base.StartCoroutine(this.CopySeedRunner());
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00027C32 File Offset: 0x00025E32
		private IEnumerator CopySeedRunner()
		{
			this.seedCopyHint.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			this.seedCopyHint.gameObject.SetActive(false);
			yield break;
		}

		// Token: 0x040005BB RID: 1467
		[SerializeField]
		private CanvasGroup rootCanvasGroup;

		// Token: 0x040005BC RID: 1468
		[SerializeField]
		private ScrollRect listScrollRect;

		// Token: 0x040005BD RID: 1469
		[SerializeField]
		private RectTransform listContent;

		// Token: 0x040005BE RID: 1470
		[SerializeField]
		private RecordRow recordRowTemplate;

		// Token: 0x040005BF RID: 1471
		[Header("Details Page")]
		[SerializeField]
		private CanvasGroup detailPageGroup;

		// Token: 0x040005C0 RID: 1472
		[SerializeField]
		private Image difficultyIcon;

		// Token: 0x040005C1 RID: 1473
		[SerializeField]
		private GameObject puzzlesParent;

		// Token: 0x040005C2 RID: 1474
		[SerializeField]
		private TextMeshProUGUI puzzleText;

		// Token: 0x040005C3 RID: 1475
		[SerializeField]
		private TextMeshProUGUI oldPuzzleText;

		// Token: 0x040005C4 RID: 1476
		[SerializeField]
		private GameObject[] puzzleIcons;

		// Token: 0x040005C5 RID: 1477
		[SerializeField]
		private GameObject jadeBoxParent;

		// Token: 0x040005C6 RID: 1478
		[SerializeField]
		private TextMeshProUGUI jadeBoxNamesText;

		// Token: 0x040005C7 RID: 1479
		[SerializeField]
		private TextMeshProUGUI manaText;

		// Token: 0x040005C8 RID: 1480
		[SerializeField]
		private RectTransform baseManaParent;

		// Token: 0x040005C9 RID: 1481
		[SerializeField]
		private TextMeshProUGUI moneyText;

		// Token: 0x040005CA RID: 1482
		[SerializeField]
		private RectTransform entityArea;

		// Token: 0x040005CB RID: 1483
		[SerializeField]
		private RectTransform manaMoneyTitle;

		// Token: 0x040005CC RID: 1484
		[SerializeField]
		private RectTransform exhibitsTitle;

		// Token: 0x040005CD RID: 1485
		[SerializeField]
		private TextMeshProUGUI exhibitText;

		// Token: 0x040005CE RID: 1486
		[SerializeField]
		private TextMeshProUGUI exhibitStatsText;

		// Token: 0x040005CF RID: 1487
		[SerializeField]
		private RectTransform exhibitsParent;

		// Token: 0x040005D0 RID: 1488
		[SerializeField]
		private RectTransform cardsTitle;

		// Token: 0x040005D1 RID: 1489
		[SerializeField]
		private TextMeshProUGUI cardText;

		// Token: 0x040005D2 RID: 1490
		[SerializeField]
		private TextMeshProUGUI cardStatsText;

		// Token: 0x040005D3 RID: 1491
		[SerializeField]
		private RectTransform cardsParent;

		// Token: 0x040005D4 RID: 1492
		[SerializeField]
		private RecordCardCell cardCellTemplate;

		// Token: 0x040005D5 RID: 1493
		[SerializeField]
		private TextMeshProUGUI seedText;

		// Token: 0x040005D6 RID: 1494
		[SerializeField]
		private TextMeshProUGUI seedCopyHint;

		// Token: 0x040005D7 RID: 1495
		[SerializeField]
		private Button seedButton;

		// Token: 0x040005D8 RID: 1496
		[SerializeField]
		private TextMeshProUGUI playedTimeText;

		// Token: 0x040005D9 RID: 1497
		[Header("Resources")]
		[SerializeField]
		private BaseManaWidget baseManaTemplate;

		// Token: 0x040005DA RID: 1498
		[SerializeField]
		private ExhibitWidget exhibitTemplate;

		// Token: 0x040005DB RID: 1499
		[SerializeField]
		private AssociationList<GameDifficulty, Sprite> difficultyIcons;

		// Token: 0x040005DC RID: 1500
		[SerializeField]
		private AssociationList<string, HistoryPanel.AvatarGroup> avatarTable;

		// Token: 0x040005DD RID: 1501
		[Header("Text Colors")]
		[SerializeField]
		private AssociationList<Rarity, Color> rarityColors;

		// Token: 0x040005DE RID: 1502
		[SerializeField]
		private Color misfortuneColor;

		// Token: 0x040005DF RID: 1503
		private List<GameRunRecordSaveData> _records;

		// Token: 0x040005E0 RID: 1504
		private RecordRow _prevSelected;

		// Token: 0x040005E1 RID: 1505
		private string seedString;

		// Token: 0x02000266 RID: 614
		[Serializable]
		public sealed class AvatarGroup
		{
			// Token: 0x040010D9 RID: 4313
			public Sprite Normal;

			// Token: 0x040010DA RID: 4314
			public Sprite TrueEnd;

			// Token: 0x040010DB RID: 4315
			public Sprite Failure;
		}
	}
}
