using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace PPBA
{
	public class UIEndScreenHandler : Singleton<UIEndScreenHandler>
	{
		[SerializeField] [TextArea] private string _winnerText;
		[SerializeField] [TextArea] private string _LoserText;
		[SerializeField] private string _titlePrefix;
		[SerializeField] private string _titleSufix;
		[SerializeField] private GameObject _itemPrefab;
		[SerializeField] private TextMeshProUGUI _winningLable;
		[SerializeField] private RectTransform _content;

		private float _itemHight;

		private void Start()
		{
			if(_itemPrefab == null)
			{
				Debug.LogError("Item Prefab not set");
				Destroy(this);
				return;
			}
			UIEndScreenItemRefHolder prefabRefHolder = _itemPrefab.GetComponent<UIEndScreenItemRefHolder>();
			if(prefabRefHolder == null)
			{
				Debug.LogError("Item has no RefHolder");
				Destroy(this);
				return;
			}
			if(prefabRefHolder._title == null)
			{
				Debug.LogError("title in RefHolder not set");
				Destroy(this);
				return;
			}
			if(prefabRefHolder._aiCount == null)
			{
				Debug.LogError("aiCount in RefHolder not set");
				Destroy(this);
				return;
			}
			if(prefabRefHolder._resources == null)
			{
				Debug.LogError("resources in RefHolder not set");
				Destroy(this);
				return;
			}
			if(_winningLable == null)
			{
				Debug.LogError("winning lable reference not set");
				Destroy(this);
				return;
			}
			if(_content == null)
			{
				Debug.LogError("content area reference not set");
				Destroy(this);
				return;
			}

			_itemHight = ((RectTransform)(_itemPrefab.transform)).sizeDelta.y;
		}

		/// <summary>
		/// call this to initialice the endscreen informations
		/// </summary>
		/// <param name="amITheWinner">true if the client was the winning client</param>
		/// <param name="stats">item1 = playerID, item2 = totalAICount, Item3 = totalResources</param>
		public void Init(bool amITheWinner, System.Tuple<int, int, int>[] stats)
		{
			_winningLable.text = amITheWinner ? _winnerText : _LoserText;

			for(int i = 0; i < stats.Length; i++)
			{
				RectTransform element = (RectTransform)Instantiate(_itemPrefab, _content).transform;
				element.anchoredPosition = new Vector2(element.anchoredPosition.x, -i * _itemHight);
				UIEndScreenItemRefHolder refHolder = element.gameObject.GetComponent<UIEndScreenItemRefHolder>();
				refHolder._title.text = _titlePrefix + stats[i].Item1 + _titleSufix;
				refHolder._aiCount.text = stats[i].Item2.ToString();
				refHolder._resources.text = stats[i].Item3.ToString();
			}

			_content.sizeDelta = new Vector2(_content.sizeDelta.x, stats.Length * _itemHight);
		}

		public void BackToMainMenu()
		{
			SceneManager.LoadScene(StringCollection.MAINMENU);
		}
	}
}