var Server = require("./src/server").Server;
var RealTime = require("./src/realtime").RealTime;
var Instagram = require("./src/instagram").Instagram;
var _ = require('underscore')._;

var tag = process.argv[2];

console.log("checking for tag " + tag);

var instagram = new Instagram(tag);

var realtime = new RealTime(new Server(), instagram.query);
