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

    private System.Text.StringBuilder output = new System.Text.StringBuilder();
    EventSystem eventSystem;
    PointerEventData pointerData;
    List<RaycastResult> results = new List<RaycastResult>();


    private void Start()
    {
#if DB_UI
		Debug.Log("UIRayCastCheck: " + gameObject.name + "(" + gameObject.scene.name + ")");
#endif
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

            Transform next;
            Canvas canvas;
#if DB_UI
			output.Clear();
            output.Append("_____" + activeCanvases.Length + " Canvas active!_____");
            for (int i = 0; i < activeCanvases.Length; i++)
            {
                next = activeCanvases[i].transform;
                canvas = activeCanvases[i].GetComponent<Canvas>();
                output.Append("\n" + activeCanvases[i].gameObject.name + "\t " + canvas.sortingLayerName + " " + canvas.sortingOrder);
                while (next.parent)
                {
                    next = next.parent;
                    output.Append("\n\t " + next.gameObject.name);
                }
                output.Append("\n\t(" + activeCanvases[i].gameObject.scene.name + ")");
            }
            Debug.Log(output);
#endif


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
#if DB_UI
					output.Clear();
                    output.Append(target.name + "\n");
#endif
                    next = target.transform;
                    while (next.parent)
                    {
                        next = next.parent;
#if DB_UI
						output.Append(" " + next.gameObject.name + "\n");
#endif
                    }
#if DB_UI
					output.Append("(" + target.scene.name + ")\n" + results[ri]);
                    Debug.Log(output);
#endif
                    activeRaycastTargets.Add(target);
                }
                target = activeRaycastTargets[0];
            }
            results.Clear();
        }
    }
}
