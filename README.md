Automatically append build information from CI to the launch in Report Portal.

# Azure DevOps

## Branch name
Takes `BRANCH_NAME` env var and set it as launch attribute.

## Merge retried tests
If you use Visual Studio Test task to execute tests, and you retried failed tests. 

# Jenkins

Activated only if there is `JENKINS_URL` environment variable (already set by jenkins).

## Branch name
Takes `BRANCH_NAME` env var and set it as launch attribute.

## Build description
Put the launch link into build description (requires `username`/`apiToken`). It gets jenkins host via `BUILD_URL` environment variable.

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