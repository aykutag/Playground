function Directives(){
    this.initDirectives = function(app){
        app.directive('instagram', instagram)
    };

    function instagram(){
        return {
            restrict: 'E',
            scope: {
                data:"="
            },
            templateUrl: 'partials/directives/instagram-directive.html',
            link: function (scope, element, attrs){

            }
        };
    }
}