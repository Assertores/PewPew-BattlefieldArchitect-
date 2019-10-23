using System.Collections;
using UnityEngine;

public class InventoryItem : ScriptableObject {

    public string itemName;
    public Sprite sprite;
    public AudioClip ConstructSound;
    public GameObject prefab;
    public int costs;
    public bool canConnect = false;
    public GameObject ConnectingObject;
    public InventoryItem[] UnlockBuildings;

    public int IncreaseMoney;
    public int IncreaseEnergy;
    public int DecreaseMoney;
    public int DecreaseEnergy;

    public float SpeedBonus;
    public float ProtectionValue;

}
