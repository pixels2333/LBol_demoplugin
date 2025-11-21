using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Yarn;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000127 RID: 295
	public class DialogRunner
	{
		// Token: 0x06000A6B RID: 2667 RVA: 0x0001D3B0 File Offset: 0x0001B5B0
		public static async UniTask<DialogRunner> LoadAsync(string name, IVariableStorage storage, Library library)
		{
			DialogProgram dialogProgram = await Addressables.LoadAssetAsync<DialogProgram>("Dialogs/" + name + ".yarn");
			DialogProgram data = dialogProgram;
			Dictionary<string, string> dictionary = await Localization.LoadFileAsync<string>("Dialogs/" + name);
			UniTask<DialogRunner> uniTask = new DialogRunner(name, data.bytes, dictionary, storage, library);
			Addressables.Release<DialogProgram>(data);
			return uniTask;
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x0001D404 File Offset: 0x0001B604
		public async UniTask ReloadLocalizationAsync()
		{
			Dictionary<string, string> dictionary = await Localization.LoadFileAsync<string>("Dialogs/" + this._name);
			this._stringTable = dictionary;
		}

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06000A6D RID: 2669 RVA: 0x0001D447 File Offset: 0x0001B647
		// (set) Token: 0x06000A6E RID: 2670 RVA: 0x0001D44F File Offset: 0x0001B64F
		public DialogPhase CurrentPhase { get; private set; }

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06000A6F RID: 2671 RVA: 0x0001D458 File Offset: 0x0001B658
		public IVariableStorage VariableStorage { get; }

		// Token: 0x06000A70 RID: 2672 RVA: 0x0001D460 File Offset: 0x0001B660
		private DialogRunner(string name, byte[] byteCode, IDictionary<string, string> stringTable, IVariableStorage variableStorage, Library library)
		{
			this._name = name;
			this._program = Program.Parser.ParseFrom(byteCode);
			this.VariableStorage = variableStorage ?? new DialogStorage();
			Dialogue dialogue = new Dialogue(this.VariableStorage);
			dialogue.LanguageCode = Localization.CurrentLocale.ToAlpha2Name();
			dialogue.LineHandler = delegate(Line line)
			{
				this.CurrentPhase = new DialogLinePhase(line);
			};
			dialogue.CommandHandler = delegate(Command command)
			{
				this.CurrentPhase = new DialogCommandPhase(command.Text);
			};
			dialogue.OptionsHandler = delegate(OptionSet options)
			{
				this.CurrentPhase = new DialogOptionsPhase(Enumerable.Select<OptionSet.Option, DialogOption>(options.Options, (OptionSet.Option o) => new DialogOption(o.Line, o.ID, o.IsAvailable)));
			};
			dialogue.DialogueCompleteHandler = delegate
			{
				this.CurrentPhase = null;
				this._complete = true;
			};
			dialogue.NodeStartHandler = delegate(string _)
			{
			};
			dialogue.NodeCompleteHandler = delegate(string _)
			{
			};
			dialogue.LogDebugMessage = delegate(string _)
			{
			};
			dialogue.LogErrorMessage = delegate(string message)
			{
				Debug.LogError("[Yarn] " + message);
			};
			this._dialogue = dialogue;
			this._dialogue.Library.ImportLibrary(library);
			this._stringTable = stringTable;
			this._complete = false;
			this._dialogue.AddProgram(this._program);
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06000A71 RID: 2673 RVA: 0x0001D5CA File Offset: 0x0001B7CA
		// (set) Token: 0x06000A72 RID: 2674 RVA: 0x0001D5D2 File Offset: 0x0001B7D2
		public LineArgumentHandler LineArgumentHandler { get; set; }

		// Token: 0x06000A73 RID: 2675 RVA: 0x0001D5DC File Offset: 0x0001B7DC
		internal string LocalizeLine(string lineId, string[] args)
		{
			string text;
			if (!this._stringTable.TryGetValue(lineId, ref text))
			{
				return this._program.Name + "." + lineId;
			}
			try
			{
				text = text.RuntimeFormat(delegate(string key, string format)
				{
					int num;
					if (!int.TryParse(key, ref num))
					{
						throw new ArgumentException("Invalid sequence format string");
					}
					if (num < 0 || num >= args.Length)
					{
						string[] array = new string[5];
						array[0] = "Element '";
						array[1] = key;
						array[2] = "' not found when formatting with args [";
						array[3] = string.Join(", ", Enumerable.Select<string, string>(args, (string o) => o.ToString()));
						array[4] = "]'";
						throw new ArgumentException(string.Concat(array));
					}
					LineArgumentHandler lineArgumentHandler = this.LineArgumentHandler;
					return ((lineArgumentHandler != null) ? lineArgumentHandler(args[num], format) : null) ?? args[num];
				});
			}
			catch (Exception ex)
			{
				Debug.LogError("[Localization] failed to format " + text + ": " + ex.Message);
				return "<Error>";
			}
			return StringDecorator.Decorate(text);
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x0001D678 File Offset: 0x0001B878
		public IEnumerable<DialogPhase> Phases([MaybeNull] string startNode = null)
		{
			this._dialogue.SetNode(startNode ?? Enumerable.First<string>(this._dialogue.NodeNames));
			while (!this._complete)
			{
				this._dialogue.Continue();
				if (this.CurrentPhase != null)
				{
					yield return this.CurrentPhase;
				}
			}
			yield break;
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x0001D68F File Offset: 0x0001B88F
		public void SelectOption(int id)
		{
			this._dialogue.SetSelectedOption(id);
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x0001D69D File Offset: 0x0001B89D
		public void Jump(string nodeName)
		{
			this._dialogue.SetNode(nodeName);
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x0001D6AB File Offset: 0x0001B8AB
		public void ImportLibrary(Library library)
		{
			this._dialogue.Library.ImportLibrary(library);
		}

		// Token: 0x04000520 RID: 1312
		private readonly string _name;

		// Token: 0x04000521 RID: 1313
		private readonly Program _program;

		// Token: 0x04000522 RID: 1314
		private readonly Dialogue _dialogue;

		// Token: 0x04000523 RID: 1315
		private IDictionary<string, string> _stringTable;

		// Token: 0x04000524 RID: 1316
		private bool _complete;
	}
}
