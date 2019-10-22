using System.Collections;
using UnityEngine;

public class InventoryItem : ScriptableObject {

    public string itemName;
    public Sprite sprite;
    public AudioClip ConstructSound;
    public GameObject prefab;
    public int costs;
    public InventoryItem[] UnlockBuildings;
    public int IncreaseMoney;
    public int IncreaseEnergy;
    public int DecreaseMoney;
    public int DecreaseEnergy;

}
