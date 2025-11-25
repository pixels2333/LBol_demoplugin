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
	public class DialogRunner
	{
		public static async UniTask<DialogRunner> LoadAsync(string name, IVariableStorage storage, Library library)
		{
			DialogProgram dialogProgram = await Addressables.LoadAssetAsync<DialogProgram>("Dialogs/" + name + ".yarn");
			DialogProgram data = dialogProgram;
			Dictionary<string, string> dictionary = await Localization.LoadFileAsync<string>("Dialogs/" + name);
			UniTask<DialogRunner> uniTask = new DialogRunner(name, data.bytes, dictionary, storage, library);
			Addressables.Release<DialogProgram>(data);
			return uniTask;
		}
		public async UniTask ReloadLocalizationAsync()
		{
			Dictionary<string, string> dictionary = await Localization.LoadFileAsync<string>("Dialogs/" + this._name);
			this._stringTable = dictionary;
		}
		public DialogPhase CurrentPhase { get; private set; }
		public IVariableStorage VariableStorage { get; }
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
		public LineArgumentHandler LineArgumentHandler { get; set; }
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
		public void SelectOption(int id)
		{
			this._dialogue.SetSelectedOption(id);
		}
		public void Jump(string nodeName)
		{
			this._dialogue.SetNode(nodeName);
		}
		public void ImportLibrary(Library library)
		{
			this._dialogue.Library.ImportLibrary(library);
		}
		private readonly string _name;
		private readonly Program _program;
		private readonly Dialogue _dialogue;
		private IDictionary<string, string> _stringTable;
		private bool _complete;
	}
}
