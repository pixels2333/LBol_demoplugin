using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace LBoL.Presentation.Environments
{
	public class IntoFinalEffect : MonoBehaviour
	{
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
		[SerializeField]
		private ParticleSystem intro;
	}
}
