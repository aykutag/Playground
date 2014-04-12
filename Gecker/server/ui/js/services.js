function ServiceInitializer(){
    this.initServices = function (app){
        //app.service('instagramService', instagramRss);
        app.service('realtime', realtime);
    };


    function instagramRss($http){

        function apiForTag(tag){
            return "http://instagram.com/tags/" + tag + "/feed/recent.rss";
        }

        this.query = function(tag){
            return $http.jsonp(apiForTag(tag) + "?callback=foo").then(function(result){
                console.log("here");
                return $.parseXML(result);
            }, console.log);
        }
    }

    function realtime(){
        function basePath(){
            var pathArray = window.location.href.split( '/' );
            var protocol = pathArray[0];
            var host = pathArray[2];
            return protocol + '//' + host;
        }

        var socket = io.connect(basePath());

        var clients = [];

        socket.on('data', function(data){
            _.forEach(function(client){
                client(data);
            });
        });

        this.register = function(client){
            clients.push(client);
        };

    }
}