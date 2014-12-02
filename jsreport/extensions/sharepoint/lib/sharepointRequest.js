module.exports = function(request, url, cb) {

    var options = {
        url: request.headers["sharepoint-host-url"] + url,
        json: true,
        headers: {
            Authorization : "Bearer " + request.headers.token
        },
        secureOptions: require("constants").SSL_OP_NO_TLSv1_2
    };

    require("request").get(options, function(error, resp, body) {
        if (error) {
            return cb(error);
        }
        cb(null, body);
    });
};



