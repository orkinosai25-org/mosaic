# GitHub Copilot Agent Guide for orkinosaiCMS

## Overview

This guide explains how GitHub Copilot agents work with this repository and best practices for ensuring successful workflow runs.

## How Copilot Agents Work

GitHub Copilot agents are AI-powered assistants that can:
- Analyze issues and pull requests
- Write and modify code
- Create comprehensive solutions
- Generate PR descriptions and documentation
- Run tests and fix failures

## Repository Configuration

### Default Branch

The default branch for this repository is `main`. All Copilot agent workflows should use this as the base branch for comparisons and merges.

### Branch Naming Convention

Copilot agents typically create branches with the following pattern:
- `copilot/<task-description>`
- Example: `copilot/implement-branded-blazor-website`
- Example: `copilot/resolve-pr-ambiguity-errors`

## Common Issues and Solutions

### Issue: Base Branch Not Found

**Symptom:**
```
fatal: ambiguous argument 'refs/heads/main': unknown revision or path not in the working tree
```

**Solution:**
This occurs when the workflow performs a shallow clone. The base branch needs to be explicitly fetched:

```bash
# In your local environment, if you encounter this:
git fetch origin main:refs/remotes/origin/main

# For comparisons, use:
git diff origin/main...HEAD
```

**Why This Matters:**
The Copilot agent needs to compare the feature branch against the base branch to:
- Generate accurate PR descriptions
- Identify what changes were made
- Run appropriate tests
- Create meaningful commits

### Issue: PR Summary Generation Failures

**Symptom:**
```
Invalid response from PR description request: {}
Failed to get a valid PR summary after 3 attempts
```

**Causes:**
1. Base branch comparison failed (see above)
2. Repository in invalid state
3. Agent model output was corrupted
4. Network or API issues

**Prevention:**
- Ensure repository is in clean state
- Keep commits focused and atomic
- Avoid very large changesets that might overwhelm the model
- Ensure all required files are present and accessible

### Issue: Workflow Timeout

**Symptom:**
Workflow runs longer than expected timeout period.

**Prevention:**
- Break large tasks into smaller, focused tasks
- Use clear, specific prompts
- Avoid requesting multiple unrelated changes in one task

## Best Practices

### 1. Clear Task Descriptions

When requesting work from a Copilot agent, be specific:

**Good:**
> "Add user authentication using ASP.NET Core Identity with email verification and password reset functionality."

**Less Optimal:**
> "Add auth"

### 2. Verify Repository State

Before initiating a Copilot agent task:
- Ensure the default branch is up to date
- Check that CI/CD is passing on the default branch
- Verify no merge conflicts exist
- Confirm all dependencies are properly configured

### 3. Review Agent Work

After a Copilot agent completes work:
- Review all changes thoroughly
- Run tests locally if possible
- Verify the PR description accurately reflects the changes
- Check that documentation was updated if needed

### 4. Provide Feedback

If an agent task fails or produces unexpected results:
- Document the issue clearly
- Include relevant error messages and logs
- Note what was expected vs. what occurred
- This helps improve future agent performance

## Workflow Integration

### CI/CD Pipeline

The repository has the following workflows that run automatically:
- `.github/workflows/ci.yml` - Builds and tests the solution
- `.github/workflows/docker-publish.yml` - Builds and publishes Docker images

These workflows will run on:
- Pushes to `main`, `develop`, and `copilot/*` branches
- Pull requests targeting `main` or `develop`

### Copilot Agent Workflow

The Copilot agent workflow:
1. Clones the repository
2. Checks out the feature branch
3. Fetches the base branch for comparison
4. Analyzes the task
5. Makes code changes
6. Runs tests
7. Generates PR description
8. Pushes changes

If any step fails, the workflow may be retried or require manual intervention.

## Troubleshooting

### Get Workflow Logs

To view logs from a Copilot agent run:
1. Navigate to the GitHub Actions tab
2. Find the "Running Copilot" workflow
3. Click on the specific run
4. Expand the "copilot" job
5. Review the "Processing Request" step for detailed logs

### Common Error Patterns

| Error Message | Meaning | Solution |
|---------------|---------|----------|
| `fatal: ambiguous argument 'refs/heads/main'` | Base branch not in local clone | Fetch base branch explicitly |
| `Failed to get a valid PR summary` | PR description generation failed | Check base branch access, retry |
| `Proceeding without custom agent` | Custom agent not found | Use default agent or configure custom agent |
| `Command failed with exit code 128` | Git operation failed | Check git state and permissions |

### Manual Recovery

If a Copilot agent workflow fails partway through:

1. **Check the branch:**
   ```bash
   git checkout copilot/<task-name>
   git pull origin copilot/<task-name>
   ```

2. **Verify the base branch:**
   ```bash
   git fetch origin main
   git log origin/main..HEAD
   ```

3. **Check for partial changes:**
   ```bash
   git status
   git diff origin/main...HEAD
   ```

4. **Complete the work manually if needed:**
   - Make necessary changes
   - Run tests: `dotnet test`
   - Commit and push

5. **Update the PR description manually:**
   - Go to the pull request on GitHub
   - Edit the description to explain what was completed
   - Add any relevant context

## Advanced Configuration

### Custom Agents

Custom agents can be configured for specific tasks. Contact your GitHub organization admin to set up custom agents for:
- Specific programming languages
- Domain-specific tasks
- Custom workflows

### Workflow Customization

To customize how Copilot agents work with this repository:
1. Review the agent configuration (if available)
2. Adjust branch protection rules
3. Configure required checks
4. Set up code owners

## Resources

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Troubleshooting Guide](./copilot-agent-troubleshooting.md)

## Support

For issues with Copilot agents:
1. Check this guide and the troubleshooting documentation
2. Review recent workflow logs
3. Check GitHub Status for any ongoing incidents
4. Contact your GitHub organization administrator
5. File an issue in this repository with the `copilot-agent` label

## Feedback

We're continuously improving how Copilot agents work with this repository. If you have suggestions or encounter issues, please:
- Document the issue in the repository
- Share what worked well
- Suggest improvements to this guide

Your feedback helps make the development process smoother for everyone!
