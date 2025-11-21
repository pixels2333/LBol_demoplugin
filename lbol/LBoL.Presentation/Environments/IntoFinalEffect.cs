using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LBoL.Presentation.Environments
{
	// Token: 0x020000F9 RID: 249
	public class IntoFinalEffect : MonoBehaviour
	{
		// Token: 0x06000E0E RID: 3598 RVA: 0x0004341C File Offset: 0x0004161C
		public async UniTask PlayTask()
		{
			this.intro.gameObject.SetActive(true);
			this.intro.Play();
			await UniTask.WaitForSeconds(0.25f, true, PlayerLoopTiming.Update, default(CancellationToken), false);
			AudioManager.PlaySfx("扫描1", -1f);
			await UniTask.WaitForSeconds(1.75f, true, PlayerLoopTiming.Update, default(CancellationToken), false);
			AudioManager.PlaySfx("目标点激活2", -1f);
			await UniTask.WaitForSeconds(1f, true, PlayerLoopTiming.Update, default(CancellationToken), false);
			AudioManager.PlaySfx("目标点激活2", -1f);
			await UniTask.WaitForSeconds(1, true, PlayerLoopTiming.Update, default(CancellationToken), false);
		}

		// Token: 0x04000A84 RID: 2692
		[SerializeField]
		private ParticleSystem intro;
	}
}
