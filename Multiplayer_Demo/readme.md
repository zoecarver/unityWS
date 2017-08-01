# Multiplayer game
If you have not already, please read through [this tutorial](https://github.com/pudility/unityWS) as I will be building off of it.

## Setting up the scene

Open the project that we worked on [last time](https://github.com/pudility/unityWS) and lets dive right in. First create some terrain. I created a cube with the dimensions of 20, 1, 20, but any thing that a player can stand on will do.

### Player

In your scene create a capsule and rename it "Player" then add a Rigidbody to it. Make sure you freeze the rotation under **Constraints**. Now create a script called PlayerController and add it to your player. Now drag your player into the project window to make it a prefab.

Open your PlayerController script for editing and add the following (feel free to write your own PlayerController script, the only thing it needs to do is move the player).

```
public int force = 10;

private Rigidbody rigidBody;

void Start ()
{
	rigidBody = GetComponent<Rigidbody> ();
}

void FixedUpdate ()
{
	float horizontalForce = Input.GetAxis ("Horizontal");
	float verticalForce = Input.GetAxis ("Vertical");

	Vector3 movement = new Vector3 (horizontalForce, 0.0f, verticalForce);

	rigidBody.AddForce (movement * force);
}
```

### Networking

Open the ws_script we worked on earlier. Add three variables `public int NetworkSpeed = 5;`, `private int x = 0;`, and `private int currentTime;`. Next, inside of our `while (true)` loop add this code:
```
if ((int)((Time.time % 60) * NetworkSpeed) >= x) {
  //we will send data to the server here
	x++;
}
```
 This checks to see how long it has been since it last sent an update to the server and then, depending on how long it has been, might send the server some data. For example, if `NetworkSpeed` is set to 1, it will send data every 1 second.

#### Sending data

As you know from the last tutorial you can send something to the server with `w.SendString()`. Now you just need to send the players position. This can be done by finding the player in the scene and sending their `transform.possition`. replace the comment in the if statement with this line of code:
`w.SendString (GameObject.Find ("Player").transform.position.ToString ());`
this sends the position of our player to the server.

Now if you start up the server from [last time](https://github.com/pudility/unityWS) and run the project you will see that the players position has sent and received.

But if you are going to have more than one player, you will have to give each player an ID. For this change `w.SendString ("START");` to the following in your ws_script.
```
string myId = UnityEngine.Random.Range (0.0f, 1000000.0f).ToString ();
w.SendString ("START_" + myId);
```

This creates a random id for your player then it sends it to the server with "START".

Now to keep track of our player, create a public Game Object variable called `player` and add this code after you send "START":
```
var myPlayer = Instantiate (player, new Vector3 (0, 10, 0), Quaternion.identity);
myPlayer.name = myId;
```
This Instantiates our player and changes its name to the id we created above.

Finally, change `GameObject.Find ("Player")` to `GameObject.Find (myId)`. So that you send **your** players position.

Now just add the player prefab to the ws_GameObject in the inspector, delete the player from the scene, and click play. You should see the player instantiate and the position show up in the console.

#### Updating the server script

Open ws.js for editing. In order for Untiy to move the player it will need two pieces of data, one is which player to move and the other is where to move it. The best way to do this will be to use an object. In the server script change `clients[i].send(message);` to :
```
clients[i].send(
  JSON.stringify({
    position: message.split("_")[0],
    id: message.split("_")[1]
  })
);
```
this splits our message into the first bit (the position) and the second bit (the id) and turns it into a string that we can safely send back to our client.

#### Receiving objects

Now we actually have to receive the strings and turn them back into objects. For this we can use [this library](https://github.com/JamesNK/Newtonsoft.Json) you can find a copy of it in the `.../Multiplayer_Demo/Librarys`. So go ahead and import that in Unity.

Then include it in your ws_script with `using Newtonsoft.Json;`. Now you can create a public class to hold your data, like this:
```
public class ChatData
{
	public string id;
	public string position;
};
```
Add a private ChatData variable called pos (or whatever you want) like this: `private ChatData pos;`.

You can set the value of the object by adding this code after you `Debug.Log` the reply:
```
string str = reply.ToString ();
pos = JsonConvert.DeserializeObject<ChatData> (str);
```
This creates a string variable of the reply data then it sets the value of the ChatData Object.

Now we need to create a function that will convert our sting to a Vector3. [Jessespike](http://answers.unity3d.com/questions/1134997/string-to-vector3.html) made this function wich works very well, so you can go ahead and add this code:
```
public static Vector3 StringToVector3(string sVector)
{
   // Remove the parentheses
   if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
       sVector = sVector.Substring(1, sVector.Length-2);
   }

   // split the items
   string[] sArray = sVector.Split(',');

   // store as a Vector3
   Vector3 result = new Vector3(
       float.Parse(sArray[0]),
       float.Parse(sArray[1]),
       float.Parse(sArray[2]));

   return result;
}
```

Now you have to figure out if it is your own id. If it is your own id you don't need to do anything. Otherwise, you need to move the correct player, or instantiate the player. The following code will do this for you:
```
//checks to see if player is our own
if (pos.id != myId) {
  //if it does not it sees if player exists
  if (GameObject.Find (pos.id)) {
    //the player exists
    GameObject.Find (pos.id).transform.position = StringToVector3 (pos.position);
  } else {
    //the player does not exist we must instantiate it
    if (pos.position == "START") {
      //checking if it is START if so we don't need to do anything
      Debug.Log ("START");
    } else {
      //instantiating the player
      var otherplayer = Instantiate (player, StringToVector3 (pos.position), Quaternion.identity);
      //renaming them
      otherplayer.name = pos.id;
      //disabling components so that we can only move/see out of our own player
      otherplayer.GetComponent <PlayerController> ().enabled = false;
      otherplayer.GetComponent <Rigidbody> ().isKinematic = true;
    }
  }
}
```

Finally add `+ "_" + myId` to where we send the server our position so that it looks like this:

`w.SendString (GameObject.Find (myId).transform.position.ToString () + "_" + myId);`

Now you can start up the server then go back to unity, build/run the project and run it in the editor, you should see two different players that can move independently of each other.

Here are the full scripts:

### ws.js
(Node.JS)
```
const WebSocket = require('ws');

const wss = new WebSocket.Server({ port: 8000 }, () => {
  console.log('listening on 8000');
});

var clients = [];

wss.on('connection', function connection(ws) {
  console.log("CONNECTION");

  clients.push(ws);

  ws.on('message', function incoming(message) {
    console.log('received: %s', message);

    for (var i = 0; i < clients.length; i++) {

      if(clients[i].readyState === 1){
        clients[i].send(
          JSON.stringify({
            position: message.toString().split("_")[0],
            id: message.toString().split("_")[1]
          })
        );
      }

    }
  });
});
```

### ws_script.cs
(Unity)
```
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
```

### PlayerController.cs
(Unity)

```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public int force = 10;

	private Rigidbody rigidBody;

	void Start ()
	{
		rigidBody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate ()
	{
		float horizontalForce = Input.GetAxis ("Horizontal");
		float verticalForce = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (horizontalForce, 0.0f, verticalForce);

		rigidBody.AddForce (movement * force);
	}
}
```

Thanks for reading, feel free to submit any issues or questions. The full source is available in this repository.