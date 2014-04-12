function feedController($scope, realtime){
    realtime.register(function(data){
        $scope.feed = data;

        $scope.$apply();
    });
}