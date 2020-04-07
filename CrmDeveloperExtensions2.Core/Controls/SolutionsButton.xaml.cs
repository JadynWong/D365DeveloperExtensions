﻿using CrmDeveloperExtensions2.Core.Enums;
using CrmDeveloperExtensions2.Core.Logging;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Tooling.Connector;
using System.Windows;
using System.Windows.Controls;

namespace CrmDeveloperExtensions2.Core.Controls
{
    public partial class SolutionsButton
    {
        public SolutionsButton()
        {
            InitializeComponent();
        }

        private void Solutions_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(Package.GetGlobalService(typeof(DTE)) is DTE dte))
                return;

            if (!(SharedGlobals.GetGlobal("CrmService", dte) is CrmServiceClient client))
            {
                OutputLogger.WriteToOutputWindow("Not connected to CRM/365.", MessageType.Error);
                return;
            }

            WebBrowser.OpenCrmPage(dte, client,
                "tools/Solution/home_solution.aspx?etc=7100&sitemappath=Settings|Customizations|nav_solution");
        }

        private void Solutions_OnLoaded(object sender, RoutedEventArgs e) => ((Button)sender).GetBindingExpression(IsEnabledProperty)
            ?.UpdateTarget();
    }
}