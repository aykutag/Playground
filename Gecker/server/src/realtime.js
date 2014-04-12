var io = require('socket.io');

exports.RealTime = function(server, init){

    var socketIO = io.listen(server);

    socketIO.set('log level', 1);

    var root = this;

    socketIO.sockets.on('connection', function(socket){
        console.log("connected");

        socket.on("disconnect", function(){
            console.log("disconnect");
        });

        init(root.push);
    });

    this.push = function(data) {
        socketIO.sockets.json.emit("data", data);
    }
};