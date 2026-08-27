using System;
using System.Text;
using Autodesk.Navisworks.Api;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Clearing the document throws away whatever the user has open. This works out what
    /// exactly would be lost so the warning can name it rather than saying "your work".
    /// </summary>
    public static class DocumentGuard
    {
        /// <summary>
        /// Null when there is nothing to lose. Otherwise the text of the warning, naming
        /// the open file and how many models are loaded.
        /// </summary>
        public static string WhatClearWouldDiscard()
        {
            Document document = NavisworksApplication.ActiveDocument;

            if (document == null)
            {
                return null;
            }

            bool clear;

            try
            {
                clear = document.IsClear;
            }
            catch (Exception)
            {
                // If the state cannot be read, warn rather than assume there is nothing
                // to lose.
                return "Navisworks would not say whether anything is open.";
            }

            if (clear)
            {
                return null;
            }

            StringBuilder what = new StringBuilder();
            string fileName = SafeFileName(document);

            if (!string.IsNullOrEmpty(fileName))
            {
                what.Append("The open file ").Append(fileName);
            }
            else
            {
                what.Append("The current unsaved document");
            }

            int models = SafeModelCount(document);

            if (models > 0)
            {
                what.Append(", holding ").Append(models).Append(models == 1 ? " model" : " models");
            }

            what.Append('.');
            return what.ToString();
        }

        private static string SafeFileName(Document document)
        {
            try
            {
                if (!string.IsNullOrEmpty(document.CurrentFileName))
                {
                    return document.CurrentFileName;
                }

                return document.FileName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int SafeModelCount(Document document)
        {
            try
            {
                return document.Models == null ? 0 : document.Models.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
