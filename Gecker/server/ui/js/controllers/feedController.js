function feedController($scope, realtime){
    $scope.feed = [];

    function id(x) { return x };

    realtime.register(function(data){
        console.log("got data");

        _.forEach(data, function(item) {
            $scope.feed.unshift(item);
        });

        $scope.$apply();
    });
}