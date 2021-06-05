using ReportPortal.Shared.Extensibility;
using ReportPortal.Shared.Extensibility.ReportEvents;
using System;
using System.Text;

namespace ReportPortal.Extensions.CI
{
    public class Diagnostics : IReportEventsObserver
    {
        private const string ENABLED_PATH = "Extensions:CI:Diagnostics";

        public void Initialize(IReportEventsSource reportEventsSource)
        {
            reportEventsSource.OnAfterLaunchStarted += ReportEventsSource_OnAfterLaunchStarted;
        }

        private void ReportEventsSource_OnAfterLaunchStarted(Shared.Reporter.ILaunchReporter launchReporter, Shared.Extensibility.ReportEvents.EventArgs.AfterLaunchStartedEventArgs args)
        {
            var diagEnabled = args.Configuration.GetValue(ENABLED_PATH, false);

            if (diagEnabled)
            {
                var envVariables = Environment.GetEnvironmentVariables();

                var envVariablesStringBuilder = new StringBuilder();

                foreach (var envVariableKey in envVariables.Keys)
                {
                    envVariablesStringBuilder.AppendLine($"{envVariableKey}: {envVariables[envVariableKey]}");
                }

                launchReporter.Log(new Client.Abstractions.Requests.CreateLogItemRequest
                {
                    Level = Client.Abstractions.Models.LogLevel.Trace,
                    Text = "CI Env Diagnostics",
                    Time = DateTime.UtcNow,
                    Attach = new Client.Abstractions.Requests.LogItemAttach
                    {
                        Name = "ReportPortal.Extensions.CI",
                        MimeType = "text/plain",
                        Data = Encoding.UTF8.GetBytes(envVariablesStringBuilder.ToString())
                    }
                });
            }
        }
    }
}
