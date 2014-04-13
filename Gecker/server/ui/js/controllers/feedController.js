function feedController($scope, realtime, $http){
    $scope.feed = [];

    realtime.registerRssPush(function (data) {
        console.log("got data");

        _.forEach(data, function (item) {
            $scope.feed.unshift(item);
        });

        $scope.$apply();
    });

    realtime.registerRealTime(function (data){
       console.log("got realtime");

        $http.jsonp(data.url).success(function(result){
            console.log(result);
        });
    });
}