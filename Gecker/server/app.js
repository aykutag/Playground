var Server = require("./src/server").Server;
var RealTime = require("./src/realtime").RealTime;
var Instagram = require("./src/instagram").Instagram;
var _ = require('underscore')._;
var openurl = require("openurl");

var tag = process.argv[2];
var showing = process.argv[3];
var checkTimeSeconds = process.argv[4];

console.log("checking for tag " + tag);
console.log("showing the " + showing + " latest images");
console.log("checking every " + checkTimeSeconds + " second");

var instagram = new Instagram(tag, showing);

var realtime = new RealTime(new Server(), instagram.query);

setInterval(function(){

    instagram.query(realtime.push);

}, 1000 * checkTimeSeconds);

openurl.open("http://localhost:3000");