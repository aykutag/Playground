function feedController($scope, realtime){
    realtime.register(function(data){
       console.log(data);
    });
}