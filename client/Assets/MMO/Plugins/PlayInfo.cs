using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu (menuName = "自定义/PlayInfo")]
public class PlayInfo:ScriptableObject
{
	public int MaxHealth;
	public int DmgPerHit;
}
#endif
//public class create
//{
//	[MenuItem ("Assets/PlayInfo2")]
//	public static void create2 ()
//	{
//// 外部调用
//		ScriptableObject.CreateInstance<PlayInfo> ();
//	}
//}
