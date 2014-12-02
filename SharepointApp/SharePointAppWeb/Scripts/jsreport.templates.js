var jsreportTemplates = (function(global, jsreportShared) {
    
    function overrideClientSideRendering() {
        var hiddenField = {
            DisplayForm: function(ctx) {
                jsreportShared.showWait();

                return "<span class='csrHiddenField' id='displayFormMark' />";
            },
            EditForm: function(ctx) {
                jsreportShared.showWait();

                return $(SPFieldText_Edit(ctx)).hide().prop('outerHTML') + "<span class='csrHiddenField' id='editFormMark' />";
            },
            NewForm: function(ctx) {
                jsreportShared.showWait();

                return "<span class='csrHiddenField' />";
            }
        };

        SPClientTemplates.TemplateManager.RegisterTemplateOverrides({
            Templates: {
                OnPostRender: function() {
                    $(".csrHiddenField").closest("tr").hide();

                    if (!$("#jsreportRenderCommand").length && ($("#displayFormMark").length || $("#editFormMark").length)) {
                        var imageCss = "style='background-image: url(https://sharepointapp.jsreport.net/Content/jsreport16x16.png);background-repeat: no-repeat;background-position: left 5px center;padding-left:21px'";
                        var text = $("#displayFormMark").length ? "Render report" : "Open editor";
                        var onClick = $("#displayFormMark").length ? "jsreportTemplates.render()" : "jsreportTemplates.openEditor()";
                        var jsreportButton = "<td nowrap='nowrap' class='ms-toolbar'><table width='100%' cellspacing='0' cellpadding='0'><tbody><tr>"
                            + "<td width='100%' nowrap='nowrap' align='right'><input type='button' id='jsreportRenderCommand' class='ms-ButtonHeightWidth' onClick='" + onClick +
                            "' value='" + text + "'" + imageCss + "></td></tr></tbody></table></td>";
                        $(".ms-formtoolbar").find("td.ms-toolbar:last").after(jsreportButton);
                    }

                    jsreportShared.closeWait();
                },
                Fields: {
                    "Template": hiddenField
                }
            }
        });
    }

    function render(success, error) {
        jsreportShared.showWait();

        jsreportShared.ensureInitialized(function() {
            getCurrentItem(function(item) {
                jsreportShared.closeWait();

                jsreport.render(JSON.parse(item.get_item("Template") || "{}"));

                if (success)
                    success();
            }, error);
        });
    }

    function getCurrentItem(success, error) {
        var context = SP.ClientContext.get_current();
        var web = context.get_web();
        var itemId = parseInt(GetUrlKeyValue('ID'));

        var list = web.get_lists().getById(_spPageContextInfo.pageListId);
        var listItem = list.getItemById(itemId);

        context.load(listItem);

        context.executeQueryAsync(
            function() {
                if (success)
                    success(listItem);
            },
            error
        );
    }

    function openEditor() {
        jsreportShared.showWait();

        jsreportShared.ensureInitialized(function() {
            var template = JSON.parse($("[title='Template']").val() || "{}");
            
            jsreportShared.closeWait();
            
            jsreport.openEditor(template, { fetch: false }, function () {
                $("[title='Template']").val(JSON.stringify(template));
            });
        });
    }

    overrideClientSideRendering();

    return {
        openEditor: openEditor,
        render: render
    };
}(this, jsreportShared));