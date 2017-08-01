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