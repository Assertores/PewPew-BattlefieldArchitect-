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
		private bool _isAll = false;

		public List<Image> guiItemImages = new List<Image>();

		public Dictionary<GameObject, int> items = new Dictionary<GameObject, int>();

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.M))
			{
#if DB_UI
				foreach(KeyValuePair<GameObject, int> item in items)
				{		
					print(item.Key.name);
				}
#endif
			}
		}


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

		Dictionary<GameObject, GameObject> h_inventory = new Dictionary<GameObject, GameObject>();
		public bool AddItem(GameObject ip)
		{
			//check if element
			if(h_inventory.ContainsKey(ip))
				return false;
			IRefHolder rh = ip.GetComponent<IRefHolder>();
			if(null == rh)
				return false;
			if(null == rh._UIElement)
				return false;
			if(!rh._UIElement.GetComponent<Button>())
				return false;

			//element erzeugen
			GameObject ui = Instantiate(rh._UIElement, inventoryPanel.transform);
			ui.name = ip.name + "_ELEMENT";

			h_inventory.Add(ip, ui);

			//funktion an button dran hängen
			ui.GetComponent<Button>().onClick.AddListener(delegate	{ BuildingManager.s_instance.HandleNewObject(rh); });
			return true;
#if Obsolete
			if(!items.ContainsKey(ip))
			{
				AddInventoryImage();        // erzeugt nen image (button)
				if(items.Count < guiItemImages.Count)
				{
					items.Add(ip, 1);
					//	button.onClick.AddListener(delegate
					//	{ BuildingManager.s_instance.HandleNewObject(ip.GetComponent<IRefHolder>()); }); // weißt button function zu
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
#endif
		}

		public bool RemoveItem(GameObject ip)
		{
			Debug.Log("removing item: " + ip);
			if(!h_inventory.ContainsKey(ip))
				return false;

			Debug.Log("found item: " + h_inventory[ip]);

			Destroy(h_inventory[ip]);
			h_inventory.Remove(ip);
			return true;
#if Obsolete
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
#endif
		}

		void UpdateView()
		{
#if Obsolete
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

				guiItemImages[index].GetComponentInParent<Button>()?.onClick.AddListener(delegate
				{ BuildingManager.s_instance.HandleNewObject(current.Key.GetComponent<IRefHolder>()); }); // weißt button function zu

				index++;
			}
#endif
		}

		void AddInventoryImage()
		{
#if Obsolete
			GameObject Tiles = Instantiate(inventoryTiles, new Vector3(0, 0, 0), Quaternion.identity);
			Tiles.transform.SetParent(inventoryPanel.transform);
			guiItemImages.Add(Tiles.transform.GetChild(0).GetComponent<Image>());
			Tiles.transform.GetComponent<Image>().rectTransform.localScale = new Vector3(1, 1, 1);
			button = Tiles.GetComponent<Button>();
			Tiles.transform.GetComponent<Image>().rectTransform.position = new Vector3(Tiles.transform.GetComponent<Image>().rectTransform.position.x, Tiles.transform.GetComponent<Image>().rectTransform.position.y, 0);
#endif
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
			Debug.Log("Adding the other items");
			if(!_isAll)
			{
				_isAll = true;
				Debug.Log("Removing start items");
				RemoveStartBuildings();

				for(int i = 0; i < NormalBuilds.Length; i++)
				{
					AddItem(NormalBuilds[i]);
				}
			}
		}

		public void RemoveStartBuildings()
		{
			//foreach(KeyValuePair<GameObject, int> item in items)
			//{
			//	RemoveItem(item.Key);
			//}
			//print("remove buildings in UIinventory");

			for(int i = 0; i < StartBuilds.Length; i++)
			{
				RemoveItem(StartBuilds[i]);
			}
		}
	}
}

