# Unity and Websockets
## Installing
### Node
The following steps will show you how to run the example ws server using Node.JS, however any ws server will work.

You can find some wonderful documentation on how to set up websocket servers using Node.JS [here](https://www.npmjs.com/package/ws), and you can see the example that I built off of [here](https://www.npmjs.com/package/ws#server-example).

1) open your CLI and change directories into `.../ws_server/`.
2) if [Node.JS](https://nodejs.org/en/) is not installed, install it.
3) type `npm install` to install the packages we will use.
4) type `node ws.js` to start the server.

### Unity
1) make sure that you have unity beta installed.
2) open **edit** -> **project settings** -> **player** then scroll down to **Scripting Runtime Version** and change it to **Experimental (.NET 4.6)** and when prompted click re-start.
3) click **Assets** -> **import new asset...** and then select `.../Librarys/*` and click **import**.
4) create the script, or use the example one in `.../Scripts/ws_script.cs`.
5) add your script to an empty game object.
6) thats it! If you used the example script you should see that it is sending/receiving data in the console.

## How-To

### How to write the server
The [npm ws docs](https://www.npmjs.com/package/ws#sending-and-receiving-text-data) cover this very well. If you have any questions feel free to submit an issue and I will try my best to answer it.

### How to write the unity client
This is the trickier part. If you looked over the example script (which I suggest you do) you will notice that we use `IEnumerator Start ()` instead of `void Start ()`. For those of you who do not know, `IEnumerator` is used to run a function in another thread. The reason for this is to be able to yield things (like connecting to a server) that might take some time without being too expensive for the CPU. [RampantStudios](http://answers.unity3d.com/questions/31950/ienumerator-function.html) sums it up pretty well here:

>The IEnumerator allows the program to yield things like the WaitForSeconds function, which lets you tell the script to wait without hogging the CPU

Basically what I am trying to say is after you have created your C# script in unity, change `void` to `IEnumerator` before Start. And we can actually delete the Update function because we will not be using that.

Instead of update we will use an always true while loop. The reason for this is because if we put our connection code inside of the Update function we would have to reconnect every time, and as you can imagine this would be problematic. So you can go ahead and add this inside of your Start function:

```
while (true) {

}
```

Now for the actually fun part; connecting to the server. Above the while loop place `WebSocket w = new WebSocket (new Uri ("ws://<ip>:<port>"));`. This creates a WebSocket variable using the libraries we imported above. We can then connect to it like so `yield return StartCoroutine (w.Connect ());` (`StartCoroutine` runs our connecting code in another thread). Then we can send our first message to the server with `w.SendString ("START");`. After we have done everything we want to, we can close the connection like so `w.Close ();`.

Right now our script looks like this:
```
IEnumerator Start ()
	{
		WebSocket w = new WebSocket (new Uri ("ws://localhost:8000"));
		yield return StartCoroutine (w.Connect ());
		w.SendString ("START");

    while (true) {

    }

    w.Close ();
  }
```
Currently our script can send the word "START" to our server. If you want, try it out! (It is very satisfying to watch your unity project send something to a server and see it pop up in the terminal). But we will want our project to do more than just send "START", so lets add some code that allows our project to receive messages. Inside of our while loop add the following:
```
string reply = w.RecvString ();
if (reply != null) {
	Debug.Log (reply);
}
yield return 0;
```
The first line here creates a variable of the response that our server *might* have sent to us. Then we have an if statement that checks if anything was actually sent to us, and if so it prints it to the console. We then return 0 so that we do not create an infinite loop for our processor.

Next we need to have some way to log the errors that we will inevitably get. We can do this with the following:
```
if (w.error != null) {
	Debug.LogError ("Error: " + w.error);
	break;
}
```

Now you can go ahead an try the program, click play in unity and you should see the word "START" in the console.

Thats it! Here is the full script for TL; DR

```
IEnumerator Start ()
{
	WebSocket w = new WebSocket (new Uri ("ws://" + ip + ":" + port));
	yield return StartCoroutine (w.Connect ());
	w.SendString ("START");

	while (true) {
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
```

## Credits
The libraries I am using are from [Simple Web Sockets For Unity WebGL](http://u3d.as/gHp).