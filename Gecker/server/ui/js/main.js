function App(){
    this.run = function(app){
        new ServiceInitializer().initServices(app);

        applyConfigs(app);
    };

    function applyConfigs(app){
        app.config(function($stateProvider, $urlRouterProvider){

            $urlRouterProvider.otherwise("/");

            $stateProvider.state('main', {
                url:"/",
                templateUrl: "partials/feed.html",
                controller: feedController
            })
        });
    }
}