var jsreportShared = (function(global, undefined) {

    (window.jQuery || document.write('<script src="//ajax.aspnetcdn.com/ajax/jquery/jquery-1.10.0.min.js"><\/script>'));
    
    var dialog;

    function showWait() {
        if (!dialog) {
            try {
                dialog = SP.UI.ModalDialog.showWaitScreenWithNoClose("Please wait...", "Please wait...");
            } catch (e) {
            }
        }
    }

    function closeWait() {
        if (dialog) {
            try {
                dialog.close(SP.UI.DialogResult.OK);
            } catch (e) {
            }

            dialog = null;
        }
    }

    window.addEventListener("message", receiveMessage, false);

    function receiveMessage(event) {

        if (event.data.command == "SessionId") {
            authToken = event.data.value;

            (function(d, s, id) {
                var js, fjs = d.getElementsByTagName(s)[0];
                if (d.getElementById(id)) {
                    return;
                }
                js = d.createElement(s);
                js.id = id;
                js.src = "https://sharepoint.jsreport.net/extension/embedding/public/embed.js";
                fjs.parentNode.insertBefore(js, fjs);
            }(document, 'script', 'jsreport-embedding'));
        }
    }


    function ensureInitialized(cb) {
		if (window.jsreport) {
		    return cb();
		}

        waitFor(function() {
            return window.jQuery;
        }, function() {
            var src = _spPageContextInfo.siteServerRelativeUrl;
            src += "/_layouts/15/appredirect.aspx?client_id=$$$clientId&redirect_uri=";
            src += "https://sharepointapp.jsreport.net%3FSPHostUrl=";
            src += encodeURIComponent(window.location.protocol + "//" + window.location.host + _spPageContextInfo.siteServerRelativeUrl);
            src += "%26SPLanguage=null%26SPClientTag=null%26SPProductNumber=null%26SessionId=null";

            $("body").append($("<iframe id='jsreport-iframe' src='" + src + "'/>"));

            waitFor(function() {
                return window.jsreport;
            }, cb);
        });
    };

    function waitFor(condition, cb) {
        if (condition())
            return cb();

        setTimeout(function() {
            waitFor(condition, cb);
        }, 50);
    }

    var authToken;

    window.jsreportInit = function() {
        window.jsreport.headers = {
            "sessionId": authToken,
            "sharepoint-host-url": window.location.protocol + "//" + window.location.host
        }
    };

    return {
        showWait: showWait,
        closeWait: closeWait,
        ensureInitialized: ensureInitialized
    }
}(this));

