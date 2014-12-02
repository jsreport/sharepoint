using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Activation;
using System.Text;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.EventReceivers;
using Newtonsoft.Json;
using SharePointApp2Web;
using File = Microsoft.SharePoint.Client.File;

namespace SharePointAppWeb.Services
{
    public class RemoteEventReceiver : IRemoteEventService
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        ///     Handles events that occur before an action occurs, such as when a user adds or deletes a list item.
        /// </summary>
        /// <param name="properties">Holds information about the remote event.</param>
        /// <returns>Holds information returned from the remote event.</returns>
        public SPRemoteEventResult ProcessEvent(SPRemoteEventProperties properties)
        {
            Trace.WriteLine("Processing remote event. " + properties.EventType);

            var result = new SPRemoteEventResult();
            try
            {
                using (ClientContext clientContext = TokenHelper.CreateAppEventClientContext(properties, false))
                {
                    if (clientContext != null)
                    {
                        if (properties.EventType == SPRemoteEventType.AppInstalled)
                        {
                            Trace.WriteLine("Creating sharepoints objects.");
                            EnsureObjectsCreated(clientContext);
                            Trace.WriteLine("Sharepoints objects created.");
                        }

                        if (properties.EventType == SPRemoteEventType.AppUninstalling)
                        {
                            Trace.WriteLine("Removing sharepoint objects.");
                            RemoveObjects(clientContext);
                            Trace.WriteLine("Sharepoint objects removed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("An error occured during processing remote event: " + e);
                result.ErrorMessage = e.ToString();
            }

            return result;
        }

        /// <summary>
        ///     Handles events that occur after an action occurs, such as after a user adds an item to a list or deletes an item
        ///     from a list.
        /// </summary>
        /// <param name="properties">Holds information about the remote event.</param>
        public void ProcessOneWayEvent(SPRemoteEventProperties properties)
        {
        }

        private void RemoveObjects(ClientContext clientContext)
        {
            List existingList = clientContext.Web.Lists.GetByTitle("jsreport Templates");
            existingList.DeleteObject();
            clientContext.ExecuteQuery();

            clientContext.Load(clientContext.Site);
            clientContext.ExecuteQuery();

            DeleteScript(clientContext, "jsreport.templates.js");
            DeleteScript(clientContext, "jsreport.client.js");

            clientContext.ExecuteQuery();
        }

        private static void DeleteScript(ClientContext clientContext, string name)
        {
            File jsreportFile =
                clientContext.Web.GetFileByServerRelativeUrl(clientContext.Site.ServerRelativeUrl + "/SiteAssets/" +
                                                             name);
            clientContext.Load(jsreportFile);
            jsreportFile.DeleteObject();
        }

        private void EnsureObjectsCreated(ClientContext clientContext)
        {
            try
            {
                List existingList = clientContext.Web.Lists.GetByTitle("jsreport Templates");
                clientContext.ExecuteQuery();
                Trace.WriteLine("Template list already exists");
            }
            catch (Exception e)
            {
                CreateObjects(clientContext);
            }
        }

        private void CreateAssets(ClientContext clientContext)
        {
            Trace.WriteLine("Ensuring assets library");
            clientContext.Web.Lists.EnsureSiteAssetsLibrary();
            clientContext.ExecuteQuery();
            
            List assetsList = clientContext.Web.Lists.GetByTitle("Site Assets");
            clientContext.Load(clientContext.Site);

            clientContext.ExecuteQuery();

            Trace.WriteLine("Creating custom scripts");
            CreateScript(clientContext, assetsList, "jsreport.templates.js");
            CreateScript(clientContext, assetsList, "jsreport.client.js");

            clientContext.ExecuteQuery();
            Trace.WriteLine("Custom scripts created");
        }

        private void CreateScript(ClientContext clientContext, List assetsList, string name)
        {
            var newFile = new FileCreationInformation();
            newFile.Content =
                Encoding.UTF8.GetBytes(
                    System.IO.File.ReadAllText(Path.Combine(AssemblyDirectory, "Scripts", "jsreport.shared.js")).Replace("$$$clientId", ConfigurationManager.AppSettings["ClientId"]) +
                    System.IO.File.ReadAllText(Path.Combine(AssemblyDirectory, "Scripts", name)));
            newFile.Overwrite = true;
            newFile.Url = clientContext.Site.Url + "/SiteAssets/" + name;
            File uploadFile = assetsList.RootFolder.Files.Add(newFile);
            uploadFile.ListItemAllFields.Update();
        }

        private void CreateSampleReport(ClientContext clientContext, List list)
        {
            Trace.WriteLine("Creating sample report template");
            var template = new
            {
                engine = "jsrender",
                content = System.IO.File.ReadAllText(Path.Combine(AssemblyDirectory, "SampleReport", "SampleReport.html")),
                helpers = System.IO.File.ReadAllText(Path.Combine(AssemblyDirectory, "SampleReport", "SampleHelpers.js")),
                script = new
                {
                    content = System.IO.File.ReadAllText(Path.Combine(AssemblyDirectory, "SampleReport", "SampleScript.js")),
                }
            };

            ListItem listItem = list.AddItem(new ListItemCreationInformation());
            listItem["Title"] = "Sample report";
            listItem["Template"] = JsonConvert.SerializeObject(template); 
            listItem["Description"] = "This is sample report. You can print it using [Render jsreport] button from detail form. To edit it use [Open editor] button on edit form";
            listItem.Update();
            clientContext.ExecuteQuery();
            Trace.WriteLine("Sample report template created");
        }

        private void CreateObjects(ClientContext clientContext)
        {
            CreateAssets(clientContext);

            Trace.WriteLine("Creating jsreport Templates list");
            List list = clientContext.Web.Lists.Add(new ListCreationInformation
            {
                Title = "jsreport Templates",
                TemplateType = (int) ListTemplateType.GenericList,
            });

            list.ImageUrl = "https://sharepointapp.jsreport.net/Content/lmage.png";
            list.Update();

            list.Fields.AddNote("Template", false);
            list.Fields.AddNoteWithRichEditor("Description", false);

            Field titleField = list.Fields.GetByTitle("Title");
            clientContext.ExecuteQuery();

            clientContext.Load(list, l => l.Fields);
            clientContext.ExecuteQuery();
            titleField.JSLink = "sp.js|sp.ui.dialog.js|clienttemplates.js|~site/SiteAssets/jsreport.templates.js";
            titleField.Update();
            clientContext.ExecuteQuery();

            clientContext.Load(list, v => v.Views);
            clientContext.ExecuteQuery();

            View view = list.Views.First();
            view.ViewFields.Add("Modified");
            view.ViewFields.Add("Modified By");
            view.Update();

            clientContext.ExecuteQuery();
            
            clientContext.Load(list);
            CreateSampleReport(clientContext, list);

            //custom action require full prermissions what is not allowed
//            Trace.WriteLine("Adding custom actions");

//            clientContext.Load(list, l => l.UserCustomActions);
//            clientContext.ExecuteQuery();

//            UserCustomAction editCustomAction = list.UserCustomActions.Add();
//            editCustomAction.Location = "CommandUI.Ribbon.EditForm";
//            editCustomAction.Sequence = 100;
//            editCustomAction.Title = "jsreport";

//            editCustomAction.CommandUIExtension = @"<CommandUIExtension>
//<CommandUIDefinitions>
//<CommandUIDefinition Location='Ribbon.ListForm.Edit.Actions.Controls._children'>
//<Button Id='jsreport.OpenEditor'
//Alt='Open jsreport Editor'
//Sequence='100'
//Command='Open_jsreport_Editor'
//LabelText='Open jsreport Editor'
//TemplateAlias='o1'
//Image32by32='https://sharepointapp.jsreport.net/Content/jsreport32x32.png'
//Image16by16='https://sharepointapp.jsreport.net/Content/jsreport16x16.png' />
//</CommandUIDefinition>
//</CommandUIDefinitions>
//<CommandUIHandlers>
//<CommandUIHandler Command='Open_jsreport_Editor'
//CommandAction='javascript:jsreportTemplates.openEditor()'/>
//</CommandUIHandlers>
//</CommandUIExtension>";


//            editCustomAction.Update();
//            clientContext.ExecuteQuery();


//            UserCustomAction displayCustomAction = list.UserCustomActions.Add();

//            displayCustomAction.Location = "CommandUI.Ribbon.DisplayForm";
//            displayCustomAction.Sequence = 101;
//            displayCustomAction.Title = "jsreport";
//            displayCustomAction.CommandUIExtension = @"<CommandUIExtension>
//<CommandUIDefinitions>
//<CommandUIDefinition Location='Ribbon.ListForm.Display.Actions.Controls._children'>
//<Button Id='jsreport.Detail.Render'
//Alt='Render jsreport'
//Sequence='101'
//Command='Render_Detail_jsreport'
//LabelText='Render jsreport'
//TemplateAlias='o1'
//Image32by32='https://sharepointapp.jsreport.net/Content/jsreport32x32.png'
//Image16by16='https://sharepointapp.jsreport.net/Content/jsreport16x16.png' />
//</CommandUIDefinition>
//</CommandUIDefinitions>
//<CommandUIHandlers>
//<CommandUIHandler Command='Render_Detail_jsreport'
//CommandAction='javascript:jsreportTemplates.render()'/>
//</CommandUIHandlers>
//</CommandUIExtension>";


//            displayCustomAction.Update();
//            clientContext.ExecuteQuery();
        }
    }
}