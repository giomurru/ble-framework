using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CallNativeCode : MonoBehaviour {

	[DllImport("native")]
	private static extern float add(float x, float y);

	void OnGUI ()
	{
		float x = 3;
		float y = 10;
		GUI.Label (new Rect (15, 125, 450, 100), "adding " + x  + " and " + y + " in native code equals " + add(x,y));
	}
}
