var request = require('request');

exports.Instagram = function(tag){
    var options = {
        host: "http://instagram.com/tags/" + tag + "/feed/recent.rss",
        method: 'GET'
    };

    this.query = function(callback){
        request(options.host, function (error, response, body) {
            if (!error && response.statusCode == 200) {
                callback(body);
            }
        })
    };
};