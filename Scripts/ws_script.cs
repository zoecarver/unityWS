using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ws_script : MonoBehaviour
{
	public int NetworkSpeed = 5;
	public string ip = "localhost";
	public string port = "8000";

	private int x = 0;
	private int currentTime;

	IEnumerator Start ()
	{
		WebSocket w = new WebSocket (new Uri ("ws://" + ip + ":" + port));
		yield return StartCoroutine (w.Connect ());
		w.SendString ("START");

		while (true) {
			if ((int)((Time.time % 60) * NetworkSpeed) >= x) {
				w.SendString ("Open for: " + x.ToString ());
				x++;
			}

			string reply = w.RecvString ();
			if (reply != null) {
				Debug.Log (reply);
			}
			if (w.error != null) {
				Debug.LogError ("Error: " + w.error);
				break;
			}
			yield return 0;
		}
		w.Close ();
	}
}

