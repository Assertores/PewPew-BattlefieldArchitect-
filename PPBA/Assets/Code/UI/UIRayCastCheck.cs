/// @author J-D Vbk
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A simple tool to check which UI Raycast Target is hit by a Mouseclick on a certain Position
/// </summary>
public class UIRayCastCheck : MonoBehaviour
{
    enum MouseButton
    {
        LeftButton,
        RightButton,
        Middle
    }

    [SerializeField]
    MouseButton trigger = MouseButton.RightButton;

    [Header("Last Result")]
    public Vector3 clickedPosition;
    [Space(5)]
    public GameObject target;
    public string layer;
    public int order;
    [SerializeField]
    private List<GameObject> activeRaycastTargets = new List<GameObject>();
    [SerializeField]
    private UnityEngine.UI.GraphicRaycaster[] activeCanvases;

    private string output;
    EventSystem eventSystem;
    PointerEventData pointerData;
    List<RaycastResult> results = new List<RaycastResult>();


    private void Start()
    {
        Debug.Log("UIRayCastCheck: " + gameObject.name + "(" + gameObject.scene.name + ")");
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown((int)trigger))
        {
            pointerData = new PointerEventData(EventSystem.current);
            clickedPosition = Input.mousePosition;
            pointerData.position = clickedPosition;


            // currently active canvas
            activeCanvases = FindObjectsOfType<UnityEngine.UI.GraphicRaycaster>();

            output = "_____" + activeCanvases.Length + " Canvas active!_____";
            Transform next;
            Canvas canvas;
            for (int i = 0; i < activeCanvases.Length; i++)
            {
                next = activeCanvases[i].transform;
                canvas = activeCanvases[i].GetComponent<Canvas>();
                output += "\n" + activeCanvases[i].gameObject.name + "\t " + canvas.sortingLayerName + " " + canvas.sortingOrder;
                while (next.parent)
                {
                    next = next.parent;
                    output += "\n\t " + next.gameObject.name;
                }
                output += "\n\t(" + activeCanvases[i].gameObject.scene.name + ")";
            }
            Debug.Log(output);


            // hits
            target = null;
            activeRaycastTargets.Clear();
            for (int ci = 0; ci < activeCanvases.Length; ci++)
            {
                // append hits to results
                activeCanvases[ci].Raycast(pointerData, results);

            }
            if (results.Count > 0)
            {
                for (int ri = 0; ri < results.Count ; ri++)
                {
                    target = results[ri].gameObject;
                    layer = SortingLayer.IDToName(results[ri].sortingLayer);
                    order = results[ri].sortingOrder;
                    output = target.name + "\n";
                    next = target.transform;
                    while (next.parent)
                    {
                        next = next.parent;
                        output += " " + next.gameObject.name + "\n";
                    }
                    output += "(" + target.scene.name + ")\n" + results[ri];
                    Debug.Log(output);
                    activeRaycastTargets.Add(target);
                }
                target = activeRaycastTargets[0];
            }
            results.Clear();
        }
    }
}
