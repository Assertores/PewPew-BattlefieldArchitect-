using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    private BuildManager buildManager;
    public InventoryItem[] StartBuilds;
    public GameObject inventoryPanel;
    public GameObject inventoryTiles;
    private Button button;


    public List<Image> guiItemImages = new List<Image>();
    public Dictionary<InventoryItem, int> items = new Dictionary<InventoryItem, int>();

    private void Awake()
    {
        buildManager = GetComponent<BuildManager>();
    }

    private void Start()
    {
        AddStartItem();
    }

    public bool IsItemContains(InventoryItem ip)
    {
        if (items.ContainsKey(ip))
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

    public bool AddItem(InventoryItem ip)
    {
        if (!items.ContainsKey(ip))
        {
            AddInventoryImage();        // erzeugt nen image (button)
            if (items.Count < guiItemImages.Count)
            {
                items.Add(ip, 1);
                button.onClick.AddListener(delegate { buildManager.HandleNewObject(ip); }); // weißt button function zu
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

    public bool RemoveItem(InventoryItem ip)
    {
        // item vorhanden?
        if (items.ContainsKey(ip))
        {
            // anzahl höher als 0?
            if (items[ip] >0)
            {
                items[ip]--;
                // nun 0?
                if (items[ip] <= 0)
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

        for (int i = 0; i < guiCount; i++)
        {
            guiItemImages[i].enabled = false;
            guiItemImages[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        int index = 0;
        foreach (KeyValuePair<InventoryItem,int> current in items)
        {
            guiItemImages[index].enabled = true;
            guiItemImages[index].sprite = current.Key.sprite;
            guiItemImages[index].GetComponentInChildren<TextMeshProUGUI>().text = current.Value.ToString();
            index++;
        }
    }

    public TextMeshPro Text;

    void AddInventoryImage()
    {
        GameObject Tiles = (GameObject)Instantiate(inventoryTiles, new Vector3(0, 0, 0), Quaternion.identity);
        Tiles.transform.SetParent(inventoryPanel.transform);
        guiItemImages.Add(Tiles.transform.GetChild(0).GetComponent<Image>());
        button = Tiles.GetComponent<Button>();

    }

    void AddStartItem()
    {
        for (int i = 0; i < StartBuilds.Length; i++)
        {
            AddItem(StartBuilds[i]);
        }
    }
}
