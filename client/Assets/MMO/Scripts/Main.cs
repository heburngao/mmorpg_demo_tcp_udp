using UnityEngine;
using System.Collections;
using ghbc.Net;
using ghbc;
using msg;
using System;

public class Main : MonoBehaviour
{
	//	public NetConnection connect;
	//	public NetworkConfig conf;
	//	// Use this for initialization
	IEnumerator Start ()
	{
		Application.targetFrameRate = 24;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		DebugTool.setDebug (true);
//		string sss = "gaohebing" + UnityEngine.Random.Range (0, 100);
		 
//		DebugTool.Log (sss.Length);
//		NetConnection.MSG_haldlerPool = new System.Collections.Generic.Dictionary<ushort, Type> () {
//			{ 10000,typeof(Rsp_Matching) }
//
//		};
//		DebugTool.setDebug (true);
//
//		LoginInfo.LoginState = true;
//		LoginInfo.loginInfo [0] = 1;
//		LoginInfo.loginInfo [1] = 1;
//		conf = NetworkConfig.GetInstance ();
////		conf.ip = "192.168.78.226";
//		conf.ip = "127.0.0.1";
////		conf.ip = "172.30.58.7";
//		conf.port = 2020;
//		conf.write_timeout = 2000;
//		conf.receiv_buffer_size = 1024;
//		conf.connect_timeout = 2000;
//
//		connect = new NetConnection (conf);
//		//NetworkConfig.GetInstance () = conf;
//		var data = new Request_Matching ();
//		data.UserID = 9999;
//		data.NickName = "高贺兵是中华人民共和国的公民!";
//		data.Level = 1;
//
//		connect.Send<Request_Matching> ((ushort)998, data);
//		//Array.Copy(;
//		print ("ok");
//			
		//测试
		// var ss = "DB";
		// var bt = System.Text.Encoding.ASCII.GetBytes(ss);
		// foreach(var item in bt){

		// 	DebugTool.Log("oo  "+ item);
		// }
		 var bts = new byte[]{68,66};
		 //var xx = IPAdress.NetworkToHostOrder(BitConverter.ToInt16(bts,0));
		var lxl = new byte[]{0,0,0,47};
		 if(BitConverter.IsLittleEndian){Array.Reverse(lxl);}
		 DebugTool.Log("txt: " + System.Text.Encoding.ASCII.GetString(bts)  + " , " + System.BitConverter.ToInt32(lxl,0));

		// var iii = 1 * 10000 * 0.0001m;
		// print("iiii:: " + iii);
		net = new Netmanager ();
		yield return new WaitForEndOfFrame ();
//		yield return new WaitForSeconds (3f);
		net.init ();
	}

	private Netmanager net = null;
	
	// Update is called once per frame
	void Update ()
	{
//		if (connect != null)
		FrameAction.Update ();
	}

	void OnDestroy ()
	{
		Netmanager.Close ();
	}
}
