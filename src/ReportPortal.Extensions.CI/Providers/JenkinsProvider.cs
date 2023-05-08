using ReportPortal.Client.Abstractions.Models;
using ReportPortal.Shared.Extensibility;
using ReportPortal.Shared.Extensibility.ReportEvents;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ReportPortal.Extensions.CI.Providers
{
    public class JenkinsProvider : IReportEventsObserver
    {
        public void Initialize(IReportEventsSource reportEventsSource)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")))
            {
                reportEventsSource.OnBeforeLaunchStarting += ReportEventsSource_OnBeforeLaunchStarting;
                reportEventsSource.OnAfterLaunchFinished += ReportEventsSource_OnAfterLaunchFinished;
            }
        }

        private void ReportEventsSource_OnBeforeLaunchStarting(Shared.Reporter.ILaunchReporter launchReporter, Shared.Extensibility.ReportEvents.EventArgs.BeforeLaunchStartingEventArgs args)
        {
            // determine branch
            var branchName = Environment.GetEnvironmentVariable("BRANCH_NAME");
            if (!string.IsNullOrEmpty(branchName))
            {
                if (args.StartLaunchRequest.Attributes == null)
                {
                    args.StartLaunchRequest.Attributes = new List<ItemAttribute>();
                }

                args.StartLaunchRequest.Attributes.Add(new ItemAttribute { Key = "branch", Value = branchName });
            }
        }

        private void ReportEventsSource_OnAfterLaunchFinished(Shared.Reporter.ILaunchReporter launchReporter, Shared.Extensibility.ReportEvents.EventArgs.AfterLaunchFinishedEventArgs args)
        {
            // update build description in jenkins
            var jenkinsUsername = args.Configuration.GetValue<string>("Extensions:CI:Jenkins:Username");
            var jenkinsApiToken = args.Configuration.GetValue<string>("Extensions:CI:Jenkins:ApiToken");

            var buildUrl = Environment.GetEnvironmentVariable("BUILD_URL");

            using (var httpClient = new HttpClient())
            {
                var base64Token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{jenkinsUsername}:{jenkinsApiToken}"));

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Token);

                // getting current description
                string oldBuildDescription;

                var response = httpClient.GetStringAsync($"{buildUrl}api/json").GetAwaiter().GetResult();

                oldBuildDescription = System.Text.Json.JsonDocument.Parse(response).RootElement.GetProperty("description").GetString();

                string newBuildDescription;

                if (string.IsNullOrEmpty(oldBuildDescription))
                {
                    newBuildDescription = launchReporter.Info.Url;
                }
                else
                {
                    newBuildDescription = $"{oldBuildDescription}\n\n{launchReporter.Info.Url}";
                }

                var formContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("description", newBuildDescription)
                    });

                httpClient.PostAsync($"{buildUrl}submitDescription", formContent).GetAwaiter().GetResult();
            }
        }
    }
}
