var jsreportClient = (function(global, jsreportShared) {

    function getTemplateById(id, success, error) {
        var context = SP.ClientContext.get_current();
        var web = context.get_web();

        var list = web.get_lists().getByTitle("jsreport Templates");
        var listItem = list.getItemById(id);

        context.load(listItem);

        context.executeQueryAsync(
            function() {
                success(listItem);
            },
            error
        );
    }

    function renderById(id, overrides, success, error) {
        if (!overrides || typeof overrides !== 'object') {
            if (error)
                error = success;

            success = overrides;
            overrides = {}
        }

        getTemplateById(id, function(item) {
            var template = JSON.parse(item.get_item("Template")) || {};
            template = $.extend(template, overrides);
            jsreport.render(template);

            if (success)
                success();
        }, function(e) {
            throw new Error("Unable to find template " + id + " details: " + e);
        }, error);
    };

    return {
        renderById: function (id, overrides, success, error) {
            jsreportShared.ensureInitialized(function() {
                renderById(id, overrides, success, error);
            });
        }
    };
}(this, jsreportShared));