using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class ColorfulDebugTool : MonoBehaviour
{
	[SerializeField]
	private bool m_debugger;
	 
	public bool debugBlue = false;
	public bool debugOrange = false;
	public bool debugWhite = false;
	public bool debugGreen = false;
	public bool debugRed = false;
	public bool debugYellow = false;
	public bool debugCyan = false;
	public bool debugMagenta = false;
	public bool debugNormal = false;
	public bool debugPink = false;
	public bool debugPurple = false;
	public List<ProgramerName> debugTypes = new List<ProgramerName> ();
	//bug:: 大量帧同步打印，导致移动终端无法正常运行，必需去掉

	//	#if UNITY_EDITOR

	void doo ()
	{
		if (debugBlue) {
			if (debugTypes.Contains (ProgramerName.BLUE) == false) {
				debugTypes.Add (ProgramerName.BLUE);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.BLUE) == true) {
				debugTypes.Remove (ProgramerName.BLUE);
			}
		}
		//1

		if (debugOrange) {
			if (debugTypes.Contains (ProgramerName.ORANGE) == false) {
				debugTypes.Add (ProgramerName.ORANGE);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.ORANGE) == true) {
				debugTypes.Remove (ProgramerName.ORANGE);
			}
		}
		//2
		if (debugWhite) {
			if (debugTypes.Contains (ProgramerName.WHITE) == false) {
				debugTypes.Add (ProgramerName.WHITE);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.WHITE) == true) {
				debugTypes.Remove (ProgramerName.WHITE);
			}
		}
		//3
		if (debugGreen) {
			if (debugTypes.Contains (ProgramerName.GREEN) == false) {
				debugTypes.Add (ProgramerName.GREEN);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.GREEN) == true) {
				debugTypes.Remove (ProgramerName.GREEN);
			}
		}
		//4
		if (debugRed) {
			if (debugTypes.Contains (ProgramerName.RED) == false) {
				debugTypes.Add (ProgramerName.RED);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.RED) == true) {
				debugTypes.Remove (ProgramerName.RED);
			}
		}
		//5
		if (debugYellow) {
			if (debugTypes.Contains (ProgramerName.YELLOW) == false) {
				debugTypes.Add (ProgramerName.YELLOW);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.YELLOW) == true) {
				debugTypes.Remove (ProgramerName.YELLOW);
			}
		}
		//6
		if (debugCyan) {
			if (debugTypes.Contains (ProgramerName.CYAN) == false) {
				debugTypes.Add (ProgramerName.CYAN);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.CYAN) == true) {
				debugTypes.Remove (ProgramerName.CYAN);
			}
		}
		//7
		if (debugMagenta) {
			if (debugTypes.Contains (ProgramerName.MGENTA) == false) {
				debugTypes.Add (ProgramerName.MGENTA);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.MGENTA) == true) {
				debugTypes.Remove (ProgramerName.MGENTA);
			}
		}
		//8
		if (debugNormal) {
			if (debugTypes.Contains (ProgramerName.NORMAL) == false) {
				debugTypes.Add (ProgramerName.NORMAL);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.NORMAL) == true) {
				debugTypes.Remove (ProgramerName.NORMAL);
			}
		}
		//9
		if (debugPink) {
			if (debugTypes.Contains (ProgramerName.PINK) == false) {
				debugTypes.Add (ProgramerName.PINK);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.PINK) == true) {
				debugTypes.Remove (ProgramerName.PINK);
			}
		}
		//10
		if (debugPurple) {
			if (debugTypes.Contains (ProgramerName.PURPLE) == false) {
				debugTypes.Add (ProgramerName.PURPLE);
			}
		} else {
			if (debugTypes.Contains (ProgramerName.PURPLE) == true) {
				debugTypes.Remove (ProgramerName.PURPLE);
			}
		}
		//11
	}

	void Awake ()
	{
		doo ();

	}

	// Use this for initialization
	//	void Start () {
	////		DebugTool.LogMagenta
	////		DontDestroyOnLoad(this.gameObject);
	//	}
	
	// Update is called once per frame
	void Update ()
	{
		doo ();
		if (m_debugger) {
			DebugTool.setDebug (m_debugger, debugTypes);
		} else {
			DebugTool.setDebug (m_debugger);
		}
	}
	//	#else
	//	void Awake ()
	//	{
	//		DebugTool.setDebug (false);
	//	}
	//	#endif
}
