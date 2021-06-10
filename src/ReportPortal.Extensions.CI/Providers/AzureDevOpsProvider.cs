using ReportPortal.Client.Abstractions.Filtering;
using ReportPortal.Shared.Extensibility;
using ReportPortal.Shared.Extensibility.ReportEvents;
using ReportPortal.Shared.Extensibility.ReportEvents.EventArgs;
using ReportPortal.Shared.Reporter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportPortal.Extensions.CI.Providers
{
    public class AzureDevOpsProvider : IReportEventsObserver
    {
        private const string KEY = "DTA.EnvironmentUri";

        public void Initialize(IReportEventsSource reportEventsSource)
        {
            // track context only if it's azure and rerun is set in pipeline
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TE.RerunMaxAttempts")))
                {
                    reportEventsSource.OnLaunchInitializing += ReportEventsSource_OnLaunchInitializing;

                    reportEventsSource.OnBeforeLaunchStarting += ReportEventsSource_OnBeforeLaunchStarting;
                }
            }
        }

        public string ContextValue { get; set; }

        private void ReportEventsSource_OnLaunchInitializing(ILaunchReporter launchReporter, LaunchInitializingEventArgs args)
        {
            ContextValue = Environment.GetEnvironmentVariable(KEY);

            var filter = new FilterOption()
            {
                Filters = new List<Filter> {
                    new Filter(FilterOperation.Contains, "description", ContextValue), },
                Sorting = new Sorting(new List<string> { "startTime" }, SortDirection.Descending)
            };

            var launches = args.ClientService.Launch.GetAsync(filter).GetAwaiter().GetResult();

            if (launches.Items.Count() != 0)
            {
                args.Configuration.Properties["Launch:RerunOf"] = launches.Items.First().Uuid;
            }
        }

        private void ReportEventsSource_OnBeforeLaunchStarting(ILaunchReporter launchReporter, BeforeLaunchStartingEventArgs args)
        {
            var hiddenInfo = $"<!-- {KEY}:{ContextValue} -->";

            if (args.StartLaunchRequest.Description != null)
            {
                args.StartLaunchRequest.Description += Environment.NewLine + hiddenInfo;
            }
            else
            {
                args.StartLaunchRequest.Description = hiddenInfo;
            }
        }
    }
}
