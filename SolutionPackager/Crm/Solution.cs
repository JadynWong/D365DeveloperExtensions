﻿using CrmDeveloperExtensions2.Core.Enums;
using CrmDeveloperExtensions2.Core.Logging;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using SolutionPackager.ViewModels;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using EnvDTE;
using Task = System.Threading.Tasks.Task;

namespace SolutionPackager.Crm
{
    public static class Solution
    {
        public static EntityCollection RetrieveSolutionsFromCrm(CrmServiceClient client)
        {
            try
            {
                QueryExpression query = new QueryExpression
                {
                    EntityName = "solution",
                    ColumnSet = new ColumnSet("friendlyname", "solutionid", "uniquename", "version"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "isvisible",
                                Operator = ConditionOperator.Equal,
                                Values = {true}
                            },
                            new ConditionExpression
                            {
                                AttributeName = "ismanaged",
                                Operator = ConditionOperator.Equal,
                                Values = { false }
                            }
                        }
                    },
                    LinkEntities =
                    {
                        new LinkEntity
                        {
                            LinkFromEntityName = "solution",
                            LinkFromAttributeName = "publisherid",
                            LinkToEntityName = "publisher",
                            LinkToAttributeName = "publisherid",
                            Columns = new ColumnSet("customizationprefix"),
                            EntityAlias = "publisher"
                        }
                    },
                    Orders =
                    {
                        new OrderExpression
                        {
                            AttributeName = "friendlyname",
                            OrderType = OrderType.Ascending
                        }
                    }
                };

                EntityCollection solutions = client.RetrieveMultiple(query);

                return solutions;
            }
            catch (FaultException<OrganizationServiceFault> crmEx)
            {
                OutputLogger.WriteToOutputWindow(
                    "Error Retrieving Solutions From CRM: " + crmEx.Message + Environment.NewLine + crmEx.StackTrace, MessageType.Error);
                return null;
            }
            catch (Exception ex)
            {
                OutputLogger.WriteToOutputWindow(
                    "Error Retrieving Solutions From CRM: " + ex.Message + Environment.NewLine + ex.StackTrace, MessageType.Error);
                return null;
            }
        }

        public static async Task<string> GetSolutionFromCrm(CrmServiceClient client, CrmSolution selectedSolution, bool managed)
        {
            try
            {
                // Hardcode connection timeout to one-hour to support large solutions.
                if (client.OrganizationServiceProxy != null)
                    client.OrganizationServiceProxy.Timeout = new TimeSpan(1, 0, 0);
                if (client.OrganizationWebProxyClient != null)
                    client.OrganizationWebProxyClient.InnerChannel.OperationTimeout = new TimeSpan(1, 0, 0);

                ExportSolutionRequest request = new ExportSolutionRequest
                {
                    Managed = managed,
                    SolutionName = selectedSolution.UniqueName
                };
                ExportSolutionResponse response = await Task.Run(() => (ExportSolutionResponse)client.Execute(request));

                string fileName = FileHandler.FormatSolutionVersionString(selectedSolution.UniqueName, selectedSolution.Version, managed);
                string tempFile = FileHandler.WriteTempFile(fileName, response.ExportSolutionFile);

                return tempFile;
            }
            catch (FaultException<OrganizationServiceFault> crmEx)
            {
                OutputLogger.WriteToOutputWindow("Error Retrieving Solution From CRM: " + crmEx.Message + Environment.NewLine + crmEx.StackTrace, MessageType.Error);
                return null;
            }
            catch (Exception ex)
            {
                OutputLogger.WriteToOutputWindow("Error Retrieving Solution From CRM: " + ex.Message + Environment.NewLine + ex.StackTrace, MessageType.Error);
                return null;
            }
        }

        public static bool ImportSolution(CrmServiceClient client, string path)
        {
            byte[] solutionBytes = CrmDeveloperExtensions2.Core.FileSystem.GetFileBytes(path);
            if (solutionBytes == null)
                return false;

            try
            {
                ImportSolutionRequest request = new ImportSolutionRequest
                {
                    //TODO: make configurable
                    CustomizationFile = solutionBytes,
                    OverwriteUnmanagedCustomizations = true,
                    PublishWorkflows = true, 
                    ImportJobId = Guid.NewGuid()                  
                };

                ImportSolutionResponse response = (ImportSolutionResponse)client.Execute(request);

                return true;
            }
            catch (FaultException<OrganizationServiceFault> crmEx)
            {
                OutputLogger.WriteToOutputWindow("Error Importing Solution To CRM: " + crmEx.Message + Environment.NewLine + crmEx.StackTrace, MessageType.Error);
                return false;
            }
            catch (Exception ex)
            {
                OutputLogger.WriteToOutputWindow("Error Importing Solution To CRM: " + ex.Message + Environment.NewLine + ex.StackTrace, MessageType.Error);
                return false;
            }
        }
    }
}