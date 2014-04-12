var io = require('socket.io');

exports.RealTime = function(server){

    var socketIO = io.listen(server);

    socketIO.sockets.on('connection', function(socket){
        console.log("connected");

        socket.on("disconnect", function(){
            console.log("disconnect");
        })
    });

    this.push = function(data) {
        socketIO.sockets.json.emit("data", data);
    }
};