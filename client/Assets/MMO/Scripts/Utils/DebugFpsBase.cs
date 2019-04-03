using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

namespace ghbc
{
	[System.Reflection.Obfuscation (Exclude = true)]
	public class DebugFpsBase : MonoBehaviour
	{
		public bool showFPS = false;

		#region implemented abstract members of ViewBasic

		 

		void Start ()
		{
			if (showFPS == false)
				return;
//			if (UGUItools.RootCanvasGo () != null && UGUItools.RootCanvasGo ().transform.Find ("RateImage/Text") != null) {
//				tt = UGUItools.RootCanvasGo ().transform.Find ("RateImage/Text").GetComponent<Text> ();
//			}
			Started ();
		}

		void Update ()
		{
			if (showFPS == false)
				return;
			UpdateTick ();
			DrawFps ();
			Updated ();

			
		}

		#endregion




		//		protected Text tt;
		//		void OnGUI()
		//		{
		//			DrawFps();
		//		}
		//		Color col;

		private void DrawFps ()
		{
			if (mLastFps > 50) {
				GUI.color = new Color (0, 1, 0);
//				col = new Color (0, 1, 0);
			} else if (mLastFps > 30) {
				GUI.color = new Color (1, 1, 0);
//				col = new Color (1, 1, 0);
			} else {
				GUI.color = new Color (1.0f, 0, 0);
//				col = new Color (1.0f, 0, 0);
			}

//			GUI.Label (new Rect (5, 1, 64, 30), "fps: " + mLastFps);
//			if (tt != null) {
//				tt.color = col;
//				tt.text = "帧: " + mLastFps;
//			}
		}

		private long mFrameCount = 0;
		private long mLastFrameTime = 0;
		public long mLastFps = 0;

		private void UpdateTick ()
		{
//			if (true) {
			mFrameCount++;
			long nCurTime = TickToMilliSec (System.DateTime.Now.Ticks);
			if (mLastFrameTime == 0) {
				mLastFrameTime = TickToMilliSec (System.DateTime.Now.Ticks);
			}

			if ((nCurTime - mLastFrameTime) >= 1000) {
				long fps = (long)(mFrameCount * 1.0f / ((nCurTime - mLastFrameTime) / 1000.0f));

				mLastFps = fps;

				mFrameCount = 0;

				mLastFrameTime = nCurTime;
			}
//			}
		}

		public static long TickToMilliSec (long tick)
		{
			return tick / (10 * 1000);
		}

		//-------------------------------------------------
		private static float _uiSize = 1f;
		private static int _fontSize = 14;
		private static GUIStyle _logStyle;
		private static float _lastCalculateTime = 0;
		private static uint _tam;
		private static uint _trm;
		private static float _fps;
		//
		private static int _gms;
		private static int _sms;
		private static int _pc;

		private void Started ()
		{
			_gms = SystemInfo.graphicsMemorySize;
			_sms = SystemInfo.systemMemorySize;
			_pc = SystemInfo.processorCount;
			if (Application.isMobilePlatform) {
				_uiSize = Screen.dpi / 295;
			}
			_fontSize = (int)(14 * _uiSize);
			_logStyle = new GUIStyle ();
			_logStyle.normal.textColor = new Color (0.6f, 0, 0);
			_logStyle.fontSize = _fontSize;
		}

		private void Updated ()
		{
			if (Time.realtimeSinceStartup - _lastCalculateTime >= 0.5f) {
				_tam = Profiler.GetTotalAllocatedMemory () / 1024 / 1024;// GetTotalAllocatedMemoryLong();// / 1024 / 1024;
				_trm = Profiler.GetTotalReservedMemory () / 1024 / 1024; //GetTotalReservedMemoryLong();/// 1024 / 1024;
				_fps = 1f / Time.deltaTime;
				_lastCalculateTime = Time.realtimeSinceStartup;

			}
		}

		void OnGUI ()
		{
			if (showFPS == false)
				return;
			DrawFps ();
			GUI.skin.textField.fontSize = _fontSize;
			GUI.skin.button.fontSize = _fontSize;
//			GUI.TextField (new Rect (0, 0, 250 * _uiSize, 70 * _uiSize), "系统显存:" + _gms + " 系统内存:" + _sms + " 核心数:" + _pc + "\n总内存:" + _tam + " 总保留内存:" + _trm + "\nFPS: " + _fps.ToString ("f2") + "\ndpi:" + Screen.dpi);
			GUI.TextField (new Rect (0, 0, 250 * _uiSize, 70 * _uiSize), "系统显存:" + _gms + " 系统内存:" + _sms + " 核心数:" + _pc + "\n总内存:" + _tam + " 总保留内存:" + _trm + "\nFPS: " + mLastFps.ToString ("f2") + "\ndpi:" + Screen.dpi);
		}
	}
}

