using System;
using System.Collections.Generic;
using System.Linq;
using ReportPortal.Client.Abstractions.Filtering;
using ReportPortal.Client.Abstractions.Models;
using ReportPortal.Shared.Configuration;
using ReportPortal.Shared.Extensibility;
using ReportPortal.Shared.Extensibility.ReportEvents;
using ReportPortal.Shared.Extensibility.ReportEvents.EventArgs;
using ReportPortal.Shared.Reporter;

namespace ReportPortal.Extensions.CI.Providers
{
    public class AzureDevOpsProvider : IReportEventsObserver
    {
        private const string CONTEXT_KEY = "DTA.EnvironmentUri";
        private const string AZURE_BUILD_KEY = "TF_BUILD";
        private const string VS_TEST_TASK_RERUN_KEY = "TE.RerunMaxAttempts";
        private const string AZURE_BRANCH_NAME_KEY = "BUILD_SOURCEBRANCHNAME";

        public string ContextValue { get; set; }

        public void Initialize(IReportEventsSource reportEventsSource)
        {
            // track context only if it's azure and rerun is set in pipeline
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AZURE_BUILD_KEY)))
            {
                reportEventsSource.OnBeforeLaunchStarting += BranchHandler;

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(VS_TEST_TASK_RERUN_KEY)))
                {
                    reportEventsSource.OnLaunchInitializing += ReportEventsSource_OnLaunchInitializing;

                    reportEventsSource.OnBeforeLaunchStarting += ReportEventsSource_OnBeforeLaunchStarting;
                }
            }
        }

        private static void BranchHandler(ILaunchReporter launchReporter, BeforeLaunchStartingEventArgs args)
        {
            // determine branch
            var branchName = Environment.GetEnvironmentVariable(AZURE_BRANCH_NAME_KEY);
            if (!string.IsNullOrEmpty(branchName))
            {
                if (args.StartLaunchRequest.Attributes == null)
                    args.StartLaunchRequest.Attributes = new List<ItemAttribute>();

                args.StartLaunchRequest.Attributes.Add(new ItemAttribute { Key = "branch", Value = branchName });
            }
        }

        private void ReportEventsSource_OnLaunchInitializing(ILaunchReporter launchReporter,
            LaunchInitializingEventArgs args)
        {
            ContextValue = Environment.GetEnvironmentVariable(CONTEXT_KEY);

            var filter = new FilterOption
            {
                Filters = new List<Filter>
                {
                    new Filter(FilterOperation.Contains, "description", ContextValue)
                },
                Sorting = new Sorting(new List<string> { "startTime" }, SortDirection.Descending)
            };

            var launches = args.Configuration.GetValue(ConfigurationPath.LaunchDebugMode, false)
                ? args.ClientService.Launch.GetDebugAsync(filter).GetAwaiter().GetResult()
                : args.ClientService.Launch.GetAsync(filter).GetAwaiter().GetResult();

            if (launches.Items.Any())
            {
                args.Configuration.Properties["Launch:Rerun"] = true;
                args.Configuration.Properties["Launch:RerunOf"] = launches.Items[0].Uuid;
            }
        }

        private void ReportEventsSource_OnBeforeLaunchStarting(ILaunchReporter launchReporter,
            BeforeLaunchStartingEventArgs args)
        {
            var hiddenInfo = $"<!-- {CONTEXT_KEY}:{ContextValue} -->";

            if (args.StartLaunchRequest.Description != null)
                args.StartLaunchRequest.Description += Environment.NewLine + hiddenInfo;
            else
                args.StartLaunchRequest.Description = hiddenInfo;
        }
    }
}