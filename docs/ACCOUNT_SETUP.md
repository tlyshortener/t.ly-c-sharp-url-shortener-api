# T.LY Account Setup

## Create an account

1. Open [https://t.ly/register](https://t.ly/register).
2. Fill out the registration form.
3. Confirm your email address if T.LY asks you to verify it.
4. Sign in to your T.LY account.

## Get an API key

1. Open [https://t.ly/settings#/api](https://t.ly/settings#/api).
2. If you are not logged in yet, T.LY redirects that URL to the login page first.
3. After signing in, return to the API settings page.
4. Generate or copy your API token.
5. Store the token securely and load it from configuration or environment variables in production.

## Recommended local setup

```bash
export TLY_API_KEY="your-api-key"
```

```csharp
using Tly;

var apiKey = Environment.GetEnvironmentVariable("TLY_API_KEY")
    ?? throw new InvalidOperationException("Missing TLY_API_KEY.");

var client = new TlyClient(apiKey);
```
