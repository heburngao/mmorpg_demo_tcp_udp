using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		float f = -234.567f;
		print ((int)f);
		var list = new List<int> (){ 1, 2, 3, 4, 5 };
		var list3 = new List<int> (){ 2, 3 };
		var list2 = new List<int> (){ 11, 22 };

		int nn = 0;
//		for (int i = 0; i < list.Count; i++) {
		for (int j = 0; j < list3.Count; j++) {
//				if (list [i] == list3 [j]) {
//					list.RemoveAt (i);
			var idx = list.FindIndex (a => a == list3 [j]);
//			print (idx);
			list.RemoveRange (idx, list3.Count);
			list.InsertRange (idx, list2);
//					i--;
//				}
		}
//		}
		for (int i = 0; i < list.Count; i++) {
			print ("ok:" + list [i]);
			
		}
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
