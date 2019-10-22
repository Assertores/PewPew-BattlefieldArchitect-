using UnityEditor;
//using System.Collections.Generic;
using UnityEngine;

public class CreateInventoryItem
{

    [MenuItem("Assets/Create/Inventory Item")]      // Definition eines neuen Buttons 

    static void CreateAsset()            // mit der dazugehörigen Methode
    {
        // gibt es einen Unterordner "Inventory Item " im projekt browser?
        if (!AssetDatabase.IsValidFolder("Assets/Inventory Items"))
        {
            AssetDatabase.CreateFolder("Assets", "Inventory Items");     // wenn nicht erstelle
        }

        ScriptableObject asset = ScriptableObject.CreateInstance(typeof(InventoryItem));        // neue instanz von inventoryItem erstellen

        AssetDatabase.CreateAsset(asset, "Assets/Inventory Items/" + "New InventoryItem " + System.Guid.NewGuid() + ".asset");   //  aus der erstellten instanz  ein asset im projekt browser erstellen

        AssetDatabase.SaveAssets(); // alle ungesicherten Assets änderungen speichern
        AssetDatabase.Refresh();    // alle änderungen neuladen
        EditorUtility.FocusProjectWindow();     // den fokus auf den brwoser legen
        Selection.activeObject = asset;         // neue asset im projekt brwoser selektieren

    }



}