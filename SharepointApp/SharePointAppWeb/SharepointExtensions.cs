using System;
using Microsoft.SharePoint.Client;

namespace SharePointAppWeb
{
    public static class SharepointExtensions
    {        
            public static Field AddNote(this FieldCollection fields, String displayName, bool addToDefaultView)
            {
                return fields.AddFieldAsXml(String.Format("<Field DisplayName='{0}' Type='{1}' />", displayName, FieldType.Note), addToDefaultView, AddFieldOptions.DefaultValue);
            }

            public static Field AddNoteWithRichEditor(this FieldCollection fields, String displayName, bool addToDefaultView)
            {
                return fields.AddFieldAsXml(String.Format("<Field RichText='TRUE' DisplayName='{0}'  Type='{1}' />", displayName, FieldType.Note), addToDefaultView, AddFieldOptions.DefaultValue);
            }
    }
}