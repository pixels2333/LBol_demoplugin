using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class StartupSelectLocalePanel : UiPanel<Locale>
	{
		private void Awake()
		{
			this.dropDown.AddOptions(Enumerable.ToList<string>(Enumerable.Select<LocaleSettingItem, string>(LocaleSettingItem.AllNoLocale, (LocaleSettingItem l) => l.Name)));
		}
		private void Start()
		{
			this.dropDown.onValueChanged.AddListener(delegate(int value)
			{
				this._locale = LocaleSettingItem.AllNoLocale[value].Locale;
				this.title.text = StartupSelectLocalePanel.TitleTable[this._locale];
				this.continueText.text = StartupSelectLocalePanel.ConfirmTable[this._locale];
			});
			this.continueButton.onClick.AddListener(new UnityAction(base.Hide));
		}
		protected override void OnShowing(Locale payload)
		{
			int num = LocaleSettingItem.AllNoLocale.FindIndexOf((LocaleSettingItem i) => i.Locale == payload);
			if (num >= 0)
			{
				this._locale = payload;
				this.dropDown.SetValueWithoutNotify(num);
				this.title.text = StartupSelectLocalePanel.TitleTable[payload];
				this.continueText.text = StartupSelectLocalePanel.ConfirmTable[payload];
				return;
			}
			this._locale = Locale.En;
			this.title.text = StartupSelectLocalePanel.TitleTable[this._locale];
			this.continueText.text = StartupSelectLocalePanel.ConfirmTable[this._locale];
		}
		public async UniTask<Locale> ShowAsync(Locale locale)
		{
			base.Show(locale);
			await UniTask.WaitWhile(() => base.IsVisible, PlayerLoopTiming.Update, default(CancellationToken), false);
			return this._locale;
		}
		// Note: this type is marked as 'beforefieldinit'.
		static StartupSelectLocalePanel()
		{
			Dictionary<Locale, string> dictionary = new Dictionary<Locale, string>();
			dictionary.Add(Locale.En, "Please select game language:");
			dictionary.Add(Locale.ZhHans, "请选择游戏语言：");
			dictionary.Add(Locale.Ja, "言語を選択してください:");
			dictionary.Add(Locale.Ru, "Пожалуйста, выберите язык игры:");
			dictionary.Add(Locale.Es, "Seleccione el idioma del juego:");
			dictionary.Add(Locale.Pl, "Proszę wybrać język gry:");
			dictionary.Add(Locale.Pt, "Selecione o idioma do jogo:");
			dictionary.Add(Locale.Fr, "Veuillez sélectionner la langue du jeu:");
			dictionary.Add(Locale.Tr, "Lütfen oyun dilini seçiniz:");
			dictionary.Add(Locale.Ko, "게임 언어를 선택해 주세요.");
			dictionary.Add(Locale.Vi, "Hãy chọn ngôn ngữ của trò chơi:");
			dictionary.Add(Locale.It, "Please select game language:");
			dictionary.Add(Locale.De, "Please select game language:");
			dictionary.Add(Locale.Uk, "Please select game language:");
			dictionary.Add(Locale.Hu, "Please select game language:");
			StartupSelectLocalePanel.TitleTable = dictionary;
			Dictionary<Locale, string> dictionary2 = new Dictionary<Locale, string>();
			dictionary2.Add(Locale.En, "Confirm");
			dictionary2.Add(Locale.ZhHans, "确认");
			dictionary2.Add(Locale.Ja, "はい");
			dictionary2.Add(Locale.Ru, "Принять");
			dictionary2.Add(Locale.Es, "Confirmar");
			dictionary2.Add(Locale.Pl, "Akceptuj");
			dictionary2.Add(Locale.Pt, "Confirmar");
			dictionary2.Add(Locale.Fr, "Confirmer");
			dictionary2.Add(Locale.Tr, "Onayla");
			dictionary2.Add(Locale.Ko, "확인");
			dictionary2.Add(Locale.Vi, "Xác nhận");
			dictionary2.Add(Locale.It, "Confirm");
			dictionary2.Add(Locale.De, "Confirm");
			dictionary2.Add(Locale.Uk, "Confirm");
			dictionary2.Add(Locale.Hu, "Confirm");
			StartupSelectLocalePanel.ConfirmTable = dictionary2;
		}
		private static readonly Dictionary<Locale, string> TitleTable;
		private static readonly Dictionary<Locale, string> ConfirmTable;
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TMP_Dropdown dropDown;
		[SerializeField]
		private Button continueButton;
		[SerializeField]
		private TextMeshProUGUI continueText;
		private Locale _locale;
	}
}
