using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		
	}

	//	float ang = 0f;
	// Update is called once per frame
	void Update ()
	{
//		ang += Time.deltaTime;
		transform.RotateAround (transform.position, Vector3.up, Time.deltaTime * 100);
	}
}
