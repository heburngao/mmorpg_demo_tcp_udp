using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MouseTools
{

	public static bool rayCheckOverUI (int pointID = 0)
	{
		 
		var eventSys = EventSystem.current; 
		var eventd = new PointerEventData (eventSys);

		#if UNITY_EDITOR || UNITY_STANDALONE_WIN  || UNITY_STANDALONE_OSX
		if (Input.GetMouseButton (pointID)) {
			eventd.position = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);    
		}

		#elif UNITY_IPHONE || UNITY_ANDROID
		if (Input.touchCount> 0) {
		eventd.position = new Vector2(Input.GetTouch(pointID).position.x,Input.GetTouch(pointID).position.y);    
		}
		#endif
		List<RaycastResult> list = new List<RaycastResult> ();
		eventSys.RaycastAll (eventd, list);
		//   if(list.Count > 0)Debug.Log("->" + list[0].gameObject);
		return list.Count > 0;
	}

	public static bool rayCheckOverUI (Canvas canvas, Vector2 screenPos)
	{
		var eventSys = EventSystem.current; 
		var eventd = new PointerEventData (eventSys);
		eventd.position = screenPos;

		var uiRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster> ();
		List<RaycastResult> list = new List<RaycastResult> ();
		uiRaycaster.Raycast (eventd, list);
		return list.Count > 0;
	}

	public static RaycastHit getRayInfo ()
	{
		RaycastHit hit;
		hit = new RaycastHit ();
		#if UNITY_EDITOR || UNITY_STANDALONE_WIN  || UNITY_STANDALONE_OSX
		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Physics.Raycast (ray, out hit);
		}
		#elif UNITY_IPHONE || UNITY_ANDROID
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {
		Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
		Physics.Raycast (ray, out hit);
		}
		#endif
		return hit;
	}

	 
}
