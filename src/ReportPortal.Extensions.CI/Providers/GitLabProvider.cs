using ReportPortal.Client.Abstractions.Models;
using ReportPortal.Shared.Extensibility;
using ReportPortal.Shared.Extensibility.ReportEvents;
using ReportPortal.Shared.Extensibility.ReportEvents.EventArgs;
using System;
using System.Collections.Generic;

namespace ReportPortal.Extensions.CI.Providers
{
    public class GitLabProvider : IReportEventsObserver
    {
        public void Initialize(IReportEventsSource reportEventsSource)
        {
            if (Environment.GetEnvironmentVariable("GITLAB_CI") == "true")
            {
                reportEventsSource.OnBeforeLaunchStarting += ReportEventsSource_OnBeforeLaunchStarting;
            }
        }

        private void ReportEventsSource_OnBeforeLaunchStarting(Shared.Reporter.ILaunchReporter launchReporter, BeforeLaunchStartingEventArgs args)
        {
            // determine branch
            var branchName = Environment.GetEnvironmentVariable("CI_COMMIT_BRANCH");
            if (!string.IsNullOrEmpty(branchName))
            {
                if (args.StartLaunchRequest.Attributes == null)
                {
                    args.StartLaunchRequest.Attributes = new List<ItemAttribute>();
                }

                args.StartLaunchRequest.Attributes.Add(new ItemAttribute { Key = "branch", Value = branchName });
            }
        }
    }
}
