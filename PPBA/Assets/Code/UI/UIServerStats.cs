using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class UIServerStats : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _currentTick;
		[SerializeField] TextMeshProUGUI _maxInputTick;
		[SerializeField] RectTransform _content;
		[SerializeField] GameObject _itemPrefab;
		float itemWidth;

		List<UIServerStatsItemRefHolder> items;
		void Awake()
		{
			if(!_currentTick)
			{
				Debug.LogError("current Tick text field not set");
				Destroy(this);
				return;
			}
			if(!_maxInputTick)
			{
				Debug.LogError("max input Tick text field not set");
				Destroy(this);
				return;
			}
			if(!_content)
			{
				Debug.LogError("content reference not set");
				Destroy(this);
				return;
			}
			if(!_itemPrefab)
			{
				Debug.LogError("item prefab not set");
				Destroy(this);
				return;
			}
			var refHolder = _itemPrefab.GetComponent<UIServerStatsItemRefHolder>();
			if(!refHolder)
			{
				Debug.LogError("item prefab has no Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._id)
			{
				Debug.LogError("id text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._ip)
			{
				Debug.LogError("ip text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._isConnected)
			{
				Debug.LogError("conection text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._gsHightEnd)
			{
				Debug.LogError("Game State Hight End text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._gsLowEnd)
			{
				Debug.LogError("Game State Low End text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._isHighEnd)
			{
				Debug.LogError("Input State Hight End text field not set in Ref Holder");
				Destroy(this);
				return;
			}
			if(!refHolder._isLowEnd)
			{
				Debug.LogError("Input State Low End text field not set in Ref Holder");
				Destroy(this);
				return;
			}

			itemWidth = ((RectTransform)(refHolder.transform)).sizeDelta.x;
		}

		// Update is called once per frame
		void Update()
		{
			_currentTick.text = TickHandler.s_currentTick.ToString();
			if(GlobalVariables.Exists())
			{
				int max = -1;
				for(int i = items.Count; i < GlobalVariables.s_instance._clients.Count; i++)
				{
					RectTransform element = (RectTransform)(Instantiate(_itemPrefab, _content).transform);
					element.anchoredPosition = new Vector2(i * itemWidth, 0);
				}

				_content.sizeDelta = new Vector2(items.Count * itemWidth, _content.sizeDelta.y);

				for(int i = 0; i < GlobalVariables.s_instance._clients.Count; i++)
				{
					client it = GlobalVariables.s_instance._clients[i];
					max = max > it._inputStates.GetHighEnd() ? max : it._inputStates.GetHighEnd();

					items[i]._id.text = it._id.ToString();
					items[i]._ip.text = it._ep.ToString();
					items[i]._isConnected.text = it._isConnected ? "Connected" : "Disconnected";
					items[i]._gsHightEnd.text = it._gameStates.GetHighEnd().ToString();
					items[i]._gsLowEnd.text = it._gameStates.GetLowEnd().ToString();
					items[i]._isHighEnd.text = it._inputStates.GetHighEnd().ToString();
					items[i]._isLowEnd.text = it._inputStates.GetLowEnd().ToString();
				}
				_maxInputTick.text = max.ToString();
			}
			
		}
	}
}