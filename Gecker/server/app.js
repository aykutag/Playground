var Server = require("./server").Server;
var RealTime = require("./realtime").RealTime;
var Instagram = require("./instagram").Instagram;

new RealTime(new Server());

var instagram = new Instagram("nofilter");

instagram.query(console.log);
