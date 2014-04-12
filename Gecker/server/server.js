var express = require('express');
var http = require('http');
var app = express();

exports.Server = function(){
    app.use(express.static(__dirname + '/ui'));

    var server = http.createServer(app);

    server.listen(process.env.PORT || 3000);

    return server;
};