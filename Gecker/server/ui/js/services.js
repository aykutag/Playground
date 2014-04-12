function ServiceInitializer(){
    this.initServices = function (app){
        app.service('realtime', realtime);
    };

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
            _.forEach(clients, function(client){
                client(data);
            });
        });

        this.register = function(client){
            clients.push(client);
        };
    }
}