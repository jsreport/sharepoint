var q = require("q"),
    https = require("https");

module.exports = function (reporter, definition) {

    reporter['scripts'].allowedModules.push({
        id : 'sharepoint-request',
        path: require("path").join(__dirname, "sharepointRequest.js")
    });

    reporter.beforeRenderListeners.insert(0, definition.name, function(request) {

        var deferred = q.defer();

        if (!request.headers.sessionId) {
            return;
        }

        var options = {
            url : "https://sharepointapp.jsreport.net/api/Tokens",
            strictSSL : false,
            secureOptions: require("constants").SSL_OP_NO_TLSv1_2,
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: request.headers.sessionId
        };

        require("request").post(options, function(error, resp, body) {
            if (error) {
                return deferred.fail(error);
            }

            console.log("received token " + body);
            request.headers.token = JSON.parse(body);

            request.template.phantom = request.template.phantom || {};
            request.template.phantom.customHeaders = request.template.phantom.customHeaders || {};
            request.template.phantom.customHeaders.Authorization = "Bearer " + request.headers.token;

            return deferred.resolve();
        });

        return deferred.promise;
    });
};