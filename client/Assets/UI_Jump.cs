using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Jump : MonoBehaviour
{

	RoleController roleCtr;
	// Use this for initialization
	void Start ()
	{
		
		EventTriggerListener.Get (gameObject).onDown = mouseDown;
	}

	public void mouseDown (GameObject obj)
	{
		if (roleCtr == null) {
			roleCtr = GameProxy.mine.comp.GetComponent<RoleController> ();
		}
		#region send to server 
//		var player = GameProxy.mine.comp;
//		if (player.sendStatus != null) {
//			player.sendStatus (Vector3.one * -.999999f, -.999999f, msg.State.JUMP);	
//		}
		roleCtr.JumpUp (true);
		#endregion
	}
	// Update is called once per frame
	void Update ()
	{
		
	}
}
