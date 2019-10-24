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
    public float SpeedBonus;
    public float ProtectionValue;
    public float HarvestIntensity;
    public float Radius;
}
