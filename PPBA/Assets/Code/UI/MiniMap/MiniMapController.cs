using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniMapController : MonoBehaviour, IPointerClickHandler
{
	//Drag Orthographic top down camera here
	public Camera miniMapCam;
	public Transform _MovingObject;

	public void OnPointerClick(PointerEventData eventData)
	{
		Vector2 localCursor = new Vector2(0, 0);

		if(RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localCursor))
		{
			Texture tex = GetComponent<RawImage>().texture;
			Rect r = GetComponent<RawImage>().rectTransform.rect;

			//Using the size of the texture and the local cursor, clamp the X,Y coords between 0 and width - height of texture
			float coordX = Mathf.Clamp(0, (((localCursor.x - r.x) * tex.width) / r.width), tex.width);
			float coordY = Mathf.Clamp(0, (((localCursor.y - r.y) * tex.height) / r.height), tex.height);

			//Convert coordX and coordY to % (0.0-1.0) with respect to texture width and height
			float recalcX = coordX / tex.width;
			float recalcY = coordY / tex.height;

			localCursor = new Vector2(recalcX, recalcY);

			CastMiniMapRayToWorld(localCursor);

		}

	}

	private void CastMiniMapRayToWorld(Vector2 localCursor)
	{
		Ray miniMapRay = miniMapCam.ScreenPointToRay(new Vector2(localCursor.x * miniMapCam.pixelWidth, localCursor.y * miniMapCam.pixelHeight));

		RaycastHit miniMapHit;

		if(Physics.Raycast(miniMapRay, out miniMapHit, Mathf.Infinity))
		{
#if DB_UI
			Debug.Log("miniMapHit: " + miniMapHit.collider.gameObject.name);
#endif
			_MovingObject.position = miniMapHit.point;
		}

	}


}


//{
//	public LayerMask ground;
//	public RectTransform _MiniMapCamera;
//	public Transform _MovingObject;

//	// Start is called before the first frame update
//	void Start()
//	{

//	}

//	// Update is called once per frame
//	void Update()
//	{

//		//if(Input.GetMouseButtonDown(0))
//		//{
//		//	var ray = _MiniMapCamera.ScreenPointToRay(Input.mousePosition);
//		//	RaycastHit hit;
//		//	if(Physics.Raycast(ray, out hit))
//		//	{
//		//		print("nkjuihuhuhuiohiuohiohuohn");
//		//		Debug.DrawLine(ray.origin, hit.point);
//		//	}
//		//}


//		//This handles all the input actions the player has done in the minimap.
//		Vector3 screenPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
//		if(_MiniMapCamera.rect.Contains(screenPoint))
//		{
//			print("fwwecwwwe " + screenPoint);

//			if(Input.GetMouseButtonDown(0))
//			{
//				float mainX = (screenPoint.x - _MiniMapCamera.rect.xMin) / (1.0f - _MiniMapCamera.rect.xMin);
//				float mainY = (screenPoint.y) / (_MiniMapCamera.rect.yMax);
//				Vector3 minimapScreenPoint = new Vector3(mainX, 0,mainY );

//				print("vsdvsdsdv "+  minimapScreenPoint);

//				_MovingObject.position = minimapScreenPoint;
//			}
//		}
//	}
//}

