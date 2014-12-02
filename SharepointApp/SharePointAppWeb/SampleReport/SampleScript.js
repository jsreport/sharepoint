require("sharepoint-request")(request, "/sites/develop_apps/_api/web/lists?$top=10",
function (err, data) {
    if (err) {
        return done(err);
    }
    request.data = data;

    //
    done();
});

