using System;
using Autodesk.Navisworks.Api;
using Federator.Core.Rerun;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// Clearing the document throws away whatever the user has open. This works out what
    /// exactly would be lost so the warning can name it rather than saying "your work".
    /// The words are Federator.Core.Rerun.DiscardWarning's, FR-169.
    /// </summary>
    public static class DocumentGuard
    {
        /// <summary>
        /// Null when there is nothing to lose. Otherwise the text of the warning, naming
        /// the open file and how many models are loaded. A read that throws is said in the
        /// text with the reason it gave, FR-169, never read as no name or no model.
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
            catch (Exception error)
            {
                // If the state cannot be read, warn rather than assume there is nothing
                // to lose.
                return "Navisworks would not say whether anything is open (" + error.Message + ").";
            }

            if (clear)
            {
                return null;
            }

            string whyNameUnread;
            string fileName = FileName(document, out whyNameUnread);
            string whyModelsUnread;
            int models = ModelCount(document, out whyModelsUnread);

            return DiscardWarning.For(fileName, whyNameUnread, models, whyModelsUnread);
        }

        private static string FileName(Document document, out string whyUnread)
        {
            whyUnread = null;

            try
            {
                if (!string.IsNullOrEmpty(document.CurrentFileName))
                {
                    return document.CurrentFileName;
                }

                return document.FileName;
            }
            catch (Exception error)
            {
                whyUnread = error.Message ?? string.Empty;
                return null;
            }
        }

        private static int ModelCount(Document document, out string whyUnread)
        {
            whyUnread = null;

            try
            {
                return document.Models == null ? 0 : document.Models.Count;
            }
            catch (Exception error)
            {
                whyUnread = error.Message ?? string.Empty;
                return 0;
            }
        }
    }
}
