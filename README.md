Automatically append build information from CI to the launch in the Report Portal. Install [ReportPortal.Extensions.CI](https://www.nuget.org/packages/ReportPortal.Extensions.CI) NuGet package and enjoy.

Supported CI:
- [Azure DevOps](#azure-devops)
- [Jenkins](#jenkins)
- [GitLab]

# Azure DevOps

## Branch name
Takes `BRANCH_NAME` env var and sets it as a launch attribute.

## Merge retried tests
If you use the Visual Studio Test task to execute tests, your failed tests after rerun will be added as retries to the launch.

## Agent job configuration support
Only Single agent configuration is supported, Multi-agent is not supported.

## Branch name
Sets branch name as a launch attribute.

# Jenkins

It is activated only if there is a `JENKINS_URL` environment variable (already set by Jenkins).

## Branch name
Takes `BRANCH_NAME` env var and sets it as a launch attribute.

## Build description
Put the launch link into build description (requires `username`/`apiToken`). It gets Jenkins host via the `BUILD_URL` environment variable.

Configuration
```json
{
  "extensions": {
    "ci": {
      "jenkins": {
        "username": "<your_jenkins_username>",
        "apiToken": "<user_api_token>"
      }
    }
  }
}
```


# Troubleshooting

Set the `Extensions:CI:Diagnostics` configuration property to `true`:

```json
{
  "extensions": {
    "ci": {
      "diagnostics": true
    }
  }
}
```

Then you can find the list of env variables in logs attached to the launch in the Report Portal. It helps to understand your execution environment.
