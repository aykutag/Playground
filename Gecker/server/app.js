var fs    = require('fs'),
    nconf = require('nconf'),
    _ = require('underscore')._,
    openurl = require("openurl"),
    Instagram = require("instagram-node-lib");

var Server = require("./src/server").Server,
    RealTime = require("./src/realtime").RealTime,
    InstagramRss = require("./src/instagramRss").InstagramRss;

var App = function(){

    var config = require('./config.json');

    var rss = new InstagramRss(config.tag, config.take);

    var server = new Server().start();

    var realtime = new RealTime(server).onLogin(rss.query);

    var hostUrl = "http://" + config.ip + ":3000";
    var callbackUrl = hostUrl + "/callback";

    this.run = function(){
        runOnTimer(config.interval);
    };

    function execRealTime(){
        addCallback();
        initInstagram();
        subscribeTo(config.tag);
    }

    function initInstagram(){
        Instagram.set('client_id', config.client_id);
        Instagram.set('client_secret', config.client_secret);
        Instagram.set('callback_url', callbackUrl);
        Instagram.set('redirect_uri', hostUrl);
    }

    function subscribeTo(tag){
        Instagram.subscriptions.subscribe({
            object: 'tag',
            object_id: tag,
            aspect: 'media',
            callback_url: callbackUrl,
            type: 'subscription',
            id: '#'
        });
    }

    function runOnTimer(interval){
        setInterval(function(){
            rss.query(realtime.push)
        }, interval * 1000);
    }

    function addCallback(){
        server.addRoutes(function(app){
            app.get('/callback', function(req, res){
                Instagram.subscriptions.handshake(req, res);
            });

            app.post("/callback", function(req, res){
                rss.query(realtime.push);
            });
        })
    }
};


new App().run();

openurl.open('http://localhost:3000');