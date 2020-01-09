using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PPBA
{
	public class UiInventory : Singleton<UiInventory>
	{
		public GameObject[] StartBuilds;
		public GameObject[] NormalBuilds;
		public GameObject inventoryPanel;
		public GameObject inventoryTiles;
		private Button button;


		public List<Image> guiItemImages = new List<Image>();
		public Dictionary<GameObject, int> items = new Dictionary<GameObject, int>();
		 

		private void Start()
		{
			if(inventoryPanel != null)
			{
				AddStartItem();
			}
		}

		public bool IsItemContains(GameObject ip)
		{
			if(items.ContainsKey(ip))
			{
				return true;
			}
			else
				return false;

		}

		public int ItemCounts()     // wie viele item sind drin
		{
			return items.Count;
		}

		public bool AddItem(GameObject ip)
		{
			if(!items.ContainsKey(ip))
			{
				AddInventoryImage();        // erzeugt nen image (button)
				if(items.Count < guiItemImages.Count)
				{
					items.Add(ip, 1);
					button.onClick.AddListener(delegate { BuildingManager.s_instance.HandleNewObject(ip.GetComponent<IRefHolder>()); }); // weißt button function zu
					//button.onClick.AddListener(delegate { BuildingController.s_instance.HandleNewObject(ip.GetComponent<IUIElement>().); }); // weißt button function zu
				}
				else
				{
					return false;
				}
			}
			else
			{
				items[ip]++;
			}
			UpdateView();
			return true;
		}

		public bool RemoveItem(GameObject ip)
		{
			// item vorhanden?
			if(items.ContainsKey(ip))
			{
				// anzahl höher als 0?
				if(items[ip] > 0)
				{
					items[ip]--;
					// nun 0?
					if(items[ip] <= 0)
					{
						items.Remove(ip);
					}
					UpdateView();
					return true;
				}
			}
			return false;
		}

		void UpdateView()
		{
			int guiCount = guiItemImages.Count;

			for(int i = 0; i < guiCount; i++)
			{
				guiItemImages[i].enabled = false;
				guiItemImages[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
			}

			int index = 0;
			foreach(KeyValuePair<GameObject, int> current in items)
			{
				guiItemImages[index].enabled = true;
				guiItemImages[index].sprite = current.Key.GetComponent<IRefHolder>()._Image;
				guiItemImages[index].GetComponentInChildren<TextMeshProUGUI>().text = current.Value.ToString();
				index++;
			}
		}

		void AddInventoryImage()
		{
			GameObject Tiles = (GameObject)Instantiate(inventoryTiles, new Vector3(0, 0, 0), Quaternion.identity);
			Tiles.transform.SetParent(inventoryPanel.transform);
			guiItemImages.Add(Tiles.transform.GetChild(0).GetComponent<Image>());
			Tiles.transform.GetComponent<Image>().rectTransform.localScale = new Vector3(1, 1, 1);
			button = Tiles.GetComponent<Button>();
			Tiles.transform.GetComponent<Image>().rectTransform.position = new Vector3(Tiles.transform.GetComponent<Image>().rectTransform.position.x, Tiles.transform.GetComponent<Image>().rectTransform.position.y, 0);

		}

		void AddStartItem()
		{
			for(int i = 0; i < StartBuilds.Length; i++)
			{
				AddItem(StartBuilds[i]);
			}
		}

		public void AddLastBuildings()
		{
			for(int i = 0; i < NormalBuilds.Length; i++)
			{
				AddItem(NormalBuilds[i]);
			}
		}

		public void RemoveStartBuildings()
		{
			print("remove buildings in UIinventory");
			for(int i = 0; i < StartBuilds.Length; i++)
			{
				RemoveItem(StartBuilds[i]);
			}
		}
	}
}

