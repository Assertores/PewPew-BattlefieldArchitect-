using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPBA
{
	public class UIPawnScheduler : MonoBehaviour
	{
		[System.Serializable]
		struct progressBar
		{
			public Button button;
			public Image bar;
			public TextMeshProUGUI Number;
		}
		[SerializeField] progressBar[] progressBars;

		private void Start()
		{
#if !UNITY_SERVER
			TickHandler.s_GatherValues += WriteToInputState;
			TickHandler.s_DoTick += UpdateVisuals;
#endif
		}

		private void OnDestroy()
		{
#if !UNITY_SERVER
			TickHandler.s_GatherValues -= WriteToInputState;
			TickHandler.s_DoTick -= UpdateVisuals;
#endif
		}

		[SerializeField] private TMP_InputField _countField;

		Dictionary<int, int> buffer = new Dictionary<int, int>();
		int[] max = new int[3];

		public void SchedulePawn(int i)
		{
			buffer[i] = int.Parse(_countField.text);
			max[i] = buffer[i];
		}

		void WriteToInputState(int tick)
		{
			int client = GlobalVariables.s_instance._clients[0]._id;
			foreach(var it in buffer)
			{
				TickHandler.s_interfaceInputState._produceUnits.Add(new ISC.ProduceUnit { _client = client, _pawnType = (byte)it.Key, _pawnCount = (byte)it.Value });
			}
			buffer.Clear();
		}

		void UpdateVisuals(int tick)
		{
			var sp = GlobalVariables.s_instance._clients[0]._scheduledPawns;
			for(int i = 0; i < progressBars.Length; i++)
			{
				progressBars[i].bar.gameObject.SetActive(sp[i] > 0);
				progressBars[i].button.interactable = !(sp[i] > 0);
				if(sp[i] > 0)
				{
					progressBars[i].bar.fillAmount = (float)sp[i]/ max[i];
					progressBars[i].Number.text = sp[i].ToString();
				}
			}
		}
	}
}