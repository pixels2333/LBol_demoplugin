using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Exhibits.Adventure;
using Yarn;

namespace LBoL.EntityLib.Adventures.Shared12
{
	// Token: 0x0200051A RID: 1306
	[AdventureInfo(WeighterType = typeof(KeineSales.KeineSalesWeighter))]
	public sealed class KeineSales : Adventure
	{
		// Token: 0x0600111F RID: 4383 RVA: 0x0001F0DC File Offset: 0x0001D2DC
		protected override void InitVariables(IVariableStorage storage)
		{
			int num;
			switch (base.GameRun.CurrentStage.Level)
			{
			case 1:
				num = 1;
				break;
			case 2:
				num = 2;
				break;
			case 3:
				num = 3;
				break;
			default:
				num = 1;
				break;
			}
			int num2 = num;
			this._threeQuestions = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f }
			}.SampleMany(base.GameRun.AdventureRng, 3, true);
			storage.SetValue("$question1No", (float)this._threeQuestions[0]);
			storage.SetValue("$question2No", (float)this._threeQuestions[1]);
			storage.SetValue("$question3No", (float)this._threeQuestions[2]);
			this._threeArtifacts = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f }
			}.SampleMany(base.GameRun.AdventureRng, 3, true);
			Exhibit exhibit;
			switch (this._threeArtifacts[0])
			{
			case 0:
				exhibit = LBoL.Core.Library.CreateExhibit<Dianshiji>();
				break;
			case 1:
				exhibit = LBoL.Core.Library.CreateExhibit<Xiyiji>();
				break;
			case 2:
				exhibit = LBoL.Core.Library.CreateExhibit<Bingxiang>();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit2 = exhibit;
			switch (this._threeArtifacts[1])
			{
			case 0:
				exhibit = LBoL.Core.Library.CreateExhibit<Dianshiji>();
				break;
			case 1:
				exhibit = LBoL.Core.Library.CreateExhibit<Xiyiji>();
				break;
			case 2:
				exhibit = LBoL.Core.Library.CreateExhibit<Bingxiang>();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit3 = exhibit;
			switch (this._threeArtifacts[2])
			{
			case 0:
				exhibit = LBoL.Core.Library.CreateExhibit<Dianshiji>();
				break;
			case 1:
				exhibit = LBoL.Core.Library.CreateExhibit<Xiyiji>();
				break;
			case 2:
				exhibit = LBoL.Core.Library.CreateExhibit<Bingxiang>();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit4 = exhibit;
			storage.SetValue("$stageNo", (float)num2);
			storage.SetValue("$artifactA", exhibit2.Id);
			storage.SetValue("$artifactAName", exhibit2.Name);
			storage.SetValue("$artifactANo", (float)this._threeArtifacts[0]);
			storage.SetValue("$artifactB", exhibit3.Id);
			storage.SetValue("$artifactBName", exhibit3.Name);
			storage.SetValue("$artifactBNo", (float)this._threeArtifacts[1]);
			storage.SetValue("$artifactC", exhibit4.Id);
			storage.SetValue("$artifactCName", exhibit4.Name);
			storage.SetValue("$artifactCNo", (float)this._threeArtifacts[2]);
			switch (this._threeQuestions[0])
			{
			case 0:
				exhibit = exhibit2;
				break;
			case 1:
				exhibit = exhibit3;
				break;
			case 2:
				exhibit = exhibit4;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit5 = exhibit;
			switch (this._threeQuestions[1])
			{
			case 0:
				exhibit = exhibit2;
				break;
			case 1:
				exhibit = exhibit3;
				break;
			case 2:
				exhibit = exhibit4;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit6 = exhibit;
			switch (this._threeQuestions[2])
			{
			case 0:
				exhibit = exhibit2;
				break;
			case 1:
				exhibit = exhibit3;
				break;
			case 2:
				exhibit = exhibit4;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Exhibit exhibit7 = exhibit;
			storage.SetValue("$position1Artifact", exhibit5.Id);
			storage.SetValue("$position2Artifact", exhibit6.Id);
			storage.SetValue("$position3Artifact", exhibit7.Id);
			storage.SetValue("$teacherCheck", this.teacherCheck);
			this.singleDigit = base.GameRun.Player.Hp % 10;
			storage.SetValue("$singleDigit", (float)this.singleDigit);
			List<Type> adventureHistory = base.GameRun.AdventureHistory;
			int count = adventureHistory.Count;
			this._charaTitle[0] = LBoL.Core.Library.CreateEnemyUnit("Eirin").Title;
			this._charaName[0] = LBoL.Core.Library.CreateEnemyUnit("Eirin").Name;
			this._charaTitle[1] = LBoL.Core.Library.CreateEnemyUnit("Kaguya").Title;
			this._charaName[1] = LBoL.Core.Library.CreateEnemyUnit("Kaguya").Name;
			this._charaTitle[2] = base.GameRun.Player.Title;
			this._charaName[2] = base.GameRun.Player.Name;
			if (count >= 1)
			{
				string name = adventureHistory[count - 1].Name;
				this.lastAdvHost = AdventureConfig.FromId(name);
				if (this.lastAdvHost.Id == "JunkoColorless")
				{
					this.junkoNo = 1;
				}
				this._charaTitle[0] = LBoL.Core.Library.CreateEnemyUnit(this.lastAdvHost.HostId).Title;
				this._charaName[0] = LBoL.Core.Library.CreateEnemyUnit(this.lastAdvHost.HostId).Name;
			}
			if (count >= 2)
			{
				string name2 = adventureHistory[count - 2].Name;
				this.last2AdvHost = AdventureConfig.FromId(name2);
				if (this.last2AdvHost.Id == "JunkoColorless")
				{
					this.junkoNo = 2;
				}
				this._charaTitle[1] = LBoL.Core.Library.CreateEnemyUnit(this.last2AdvHost.HostId).Title;
				this._charaName[1] = LBoL.Core.Library.CreateEnemyUnit(this.last2AdvHost.HostId).Name;
			}
			if (count >= 3)
			{
				string name3 = adventureHistory[count - 3].Name;
				this.last3AdvHost = AdventureConfig.FromId(name3);
				if (this.last3AdvHost.Id == "JunkoColorless")
				{
					this.junkoNo = 3;
				}
				this._charaTitle[2] = LBoL.Core.Library.CreateEnemyUnit(this.last3AdvHost.HostId).Title;
				this._charaName[2] = LBoL.Core.Library.CreateEnemyUnit(this.last3AdvHost.HostId).Name;
			}
			this._threeCharacters = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f }
			}.SampleMany(base.GameRun.AdventureRng, 3, true);
			string text;
			switch (this._threeCharacters[0])
			{
			case 0:
				text = this._charaTitle[0];
				break;
			case 1:
				text = this._charaTitle[1];
				break;
			case 2:
				text = this._charaTitle[2];
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text2 = text;
			switch (this._threeCharacters[1])
			{
			case 0:
				text = this._charaTitle[0];
				break;
			case 1:
				text = this._charaTitle[1];
				break;
			case 2:
				text = this._charaTitle[2];
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text3 = text;
			switch (this._threeCharacters[2])
			{
			case 0:
				text = this._charaTitle[0];
				break;
			case 1:
				text = this._charaTitle[1];
				break;
			case 2:
				text = this._charaTitle[2];
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text4 = text;
			storage.SetValue("$characterA", text2);
			storage.SetValue("$characterB", text3);
			storage.SetValue("$characterC", text4);
			this.whoToAsk = base.GameRun.AdventureRng.NextInt(0, 2);
			switch (this.whoToAsk)
			{
			case 0:
				text = this._charaName[0];
				break;
			case 1:
				text = this._charaName[1];
				break;
			case 2:
				text = this._charaName[2];
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text5 = text;
			storage.SetValue("$charToAsk", text5);
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x0001F850 File Offset: 0x0001DA50
		[RuntimeCommand("setJunko", "")]
		[UsedImplicitly]
		public void SetJunko(string title)
		{
			this.junkoTitle = title;
			switch (this.junkoNo)
			{
			case 0:
				return;
			case 1:
				this.FindJunko(0);
				return;
			case 2:
				this.FindJunko(1);
				return;
			case 3:
				this.FindJunko(2);
				return;
			default:
				throw new ArgumentException();
			}
		}

		// Token: 0x06001121 RID: 4385 RVA: 0x0001F8A4 File Offset: 0x0001DAA4
		private void FindJunko(int index)
		{
			switch (this._threeCharacters.IndexOf(index))
			{
			case 0:
				base.Storage.SetValue("$characterA", this.junkoTitle);
				return;
			case 1:
				base.Storage.SetValue("$characterB", this.junkoTitle);
				return;
			case 2:
				base.Storage.SetValue("$characterC", this.junkoTitle);
				return;
			default:
				throw new ArgumentException();
			}
		}

		// Token: 0x06001122 RID: 4386 RVA: 0x0001F91C File Offset: 0x0001DB1C
		[RuntimeCommand("rollQuestions", "")]
		[UsedImplicitly]
		public void RollQuestions()
		{
			for (int i = 0; i < 3; i++)
			{
				this._questionTitles[i] = base.Storage.GetValue(string.Format("$question{0}Source", i + 1));
			}
			base.Storage.SetValue("$question1", this._questionTitles[this._threeQuestions[0]]);
			base.Storage.SetValue("$question2", this._questionTitles[this._threeQuestions[1]]);
			base.Storage.SetValue("$question3", this._questionTitles[this._threeQuestions[2]]);
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x0001F9B8 File Offset: 0x0001DBB8
		[RuntimeCommand("mathCheck", "")]
		[UsedImplicitly]
		public void MathCheck(int answer)
		{
			int num = answer + this.singleDigit;
			this.teacherCheck = num == 2 || num == 3 || num == 5 || num == 7 || num == 11 || num == 13;
			base.Storage.SetValue("$teacherCheck", this.teacherCheck);
		}

		// Token: 0x06001124 RID: 4388 RVA: 0x0001FA07 File Offset: 0x0001DC07
		[RuntimeCommand("historyCheck", "")]
		[UsedImplicitly]
		public void HistoryCheck(int answer)
		{
			if (this._threeCharacters[answer] == this.whoToAsk)
			{
				this.teacherCheck = true;
			}
			base.Storage.SetValue("$teacherCheck", this.teacherCheck);
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x0001FA36 File Offset: 0x0001DC36
		[RuntimeCommand("applianceCheck", "")]
		[UsedImplicitly]
		public void ApplianceCheck(int answer)
		{
			if (answer == this._threeArtifacts[2])
			{
				this.teacherCheck = true;
			}
			base.Storage.SetValue("$teacherCheck", this.teacherCheck);
		}

		// Token: 0x04000133 RID: 307
		private int junkoNo;

		// Token: 0x04000134 RID: 308
		private int[] _threeQuestions;

		// Token: 0x04000135 RID: 309
		private int[] _threeArtifacts;

		// Token: 0x04000136 RID: 310
		private int[] _threeCharacters;

		// Token: 0x04000137 RID: 311
		private string junkoTitle;

		// Token: 0x04000138 RID: 312
		private readonly string[] _charaTitle = new string[3];

		// Token: 0x04000139 RID: 313
		private readonly string[] _charaName = new string[3];

		// Token: 0x0400013A RID: 314
		private readonly string[] _questionTitles = new string[3];

		// Token: 0x0400013B RID: 315
		private int singleDigit;

		// Token: 0x0400013C RID: 316
		private int whoToAsk;

		// Token: 0x0400013D RID: 317
		private bool teacherCheck;

		// Token: 0x0400013E RID: 318
		private AdventureConfig lastAdvHost;

		// Token: 0x0400013F RID: 319
		private AdventureConfig last2AdvHost;

		// Token: 0x04000140 RID: 320
		private AdventureConfig last3AdvHost;

		// Token: 0x02000A73 RID: 2675
		private class KeineSalesWeighter : IAdventureWeighter
		{
			// Token: 0x0600376D RID: 14189 RVA: 0x000869BF File Offset: 0x00084BBF
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Money >= 100 && gameRun.AdventureHistory.Count > 2) ? 1 : 0);
			}
		}
	}
}
