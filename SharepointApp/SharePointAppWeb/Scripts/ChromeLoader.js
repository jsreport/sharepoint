var hostweburl;

//load the SharePoint resources
$(document).ready(function () {
    //Get the URI decoded URL.
    // The SharePoint js files URL are in the form:
    // web_url/_layouts/15/resource
    var scriptbase = hostweburl + "_layouts/15/";

    // Load the js file and continue to the 
    //   success handler
    $.getScript(scriptbase + "SP.UI.Controls.js", renderChrome);
});

//Function to prepare the options and render the control
function renderChrome() {

    // The Help, Account and Contact pages receive the 
    //   same query string parameters as the main page
    var options = {
        "appIconUrl": "/Content/lmage.png",
        "appTitle": "jsreport sharepoint app",
        "appHelpPageUrl": "http://jsreport.net/learn/reports-in-sharepoint",
        "settingsLinks": [
            {
                "linkUrl": "http://jsreport.net/about",
                "displayName": "Contact us"
            }
        ]
    };

    var nav = new SP.UI.Controls.Navigation(
                            "chrome_ctrl_placeholder",
                            options
                        );
    nav.setVisible(true);
}