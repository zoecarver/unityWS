using System.Collections;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class ChatData
{
	public string id;
	public string position;
};

public class ws_script : MonoBehaviour
{
	public GameObject player;
	public int NetworkSpeed = 5;
	public string ip = "localhost";
	public string port = "8000";

	private ChatData pos;
	private int x = 0;
	private int currentTime;

	IEnumerator Start ()
	{
		WebSocket w = new WebSocket (new Uri ("ws://" + ip + ":" + port));
		yield return StartCoroutine (w.Connect ());
		string myId = UnityEngine.Random.Range (0.0f, 1000000.0f).ToString ();
		w.SendString ("START_" + myId);

		var myPlayer = Instantiate (player, new Vector3 (0, 10, 0), Quaternion.identity);
		myPlayer.name = myId;

		while (true) {
			if ((int)((Time.time % 60) * NetworkSpeed) >= x) {
				w.SendString (GameObject.Find (myId).transform.position.ToString () + "_" + myId);
				x++;
			}

			string reply = w.RecvString ();
			if (reply != null) {
				Debug.Log (reply);
				string str = reply.ToString ();
				pos = JsonConvert.DeserializeObject<ChatData> (str);

				if (pos.id != myId) {
					if (GameObject.Find (pos.id)) {
						//the player exists
						GameObject.Find (pos.id).transform.position = StringToVector3 (pos.position);
					} else {
						//the player does not exist we must instanciate it
						if (pos.position == "START") {
							//checking if it is START if so we don't need to do anything
							Debug.Log ("START");
						} else {
							//instantiating the player
							var otherplayer = Instantiate (player, StringToVector3 (pos.position), Quaternion.identity);
							//renaming them
							otherplayer.name = pos.id;
							//removing components
							otherplayer.GetComponent <PlayerController> ().enabled = false;
							otherplayer.GetComponent <Rigidbody> ().isKinematic = true;
						}
					}
				}

			}
			if (w.error != null) {
				Debug.LogError ("Error: " + w.error);
				break;
			}
			yield return 0;
		}
		w.Close ();
	}

	public static Vector3 StringToVector3 (string sVector)
	{
		// Remove the parentheses
		if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
			sVector = sVector.Substring (1, sVector.Length - 2);
		}

		// split the items
		string[] sArray = sVector.Split (',');

		// store as a Vector3
		Vector3 result = new Vector3 (
			                 float.Parse (sArray [0]),
			                 float.Parse (sArray [1]),
			                 float.Parse (sArray [2]));

		return result;
	}
}

