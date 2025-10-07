using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Runtime;

namespace HelloWorld;

class Program
{
    static async Task Main(string[] args)
    {
        // Display current environment variables
        DisplayEnvironmentVariables();
        
        // Assume role for local debugging (optional)
        await AssumeRoleForDebugging();
        
        // Set environment variables for local debugging (fallback if not set in launch.json)
        Environment.SetEnvironmentVariable("TABLE_NAME", Environment.GetEnvironmentVariable("TABLE_NAME") ?? "UnumDebugLambda");
        Environment.SetEnvironmentVariable("QUEUE_URL", Environment.GetEnvironmentVariable("QUEUE_URL") ?? "https://sqs.us-east-1.amazonaws.com/123456789012/HelloWorldQueue");
        Environment.SetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE", Environment.GetEnvironmentVariable("POWERTOOLS_METRICS_NAMESPACE") ?? "AWSLambdaPowertools");
        
        var function = new Function();
        var context = new TestLambdaContext
        {
            AwsRequestId = Guid.NewGuid().ToString()
        };
        
        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "GET",
            Path = "/hello",
            Headers = new Dictionary<string, string> { { "User-Agent", "Test" } }
        };

        var response = await function.FunctionHandler(request, context);
        
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Body: {response.Body}");
    }
    
    private static async Task AssumeRoleForDebugging()
    {
        var roleArn = Environment.GetEnvironmentVariable("DEBUG_ROLE_ARN");
        if (string.IsNullOrEmpty(roleArn))
        {
            Console.WriteLine("No DEBUG_ROLE_ARN set, using default credentials");
            return;
        }
        
        try
        {
            var stsClient = new AmazonSecurityTokenServiceClient();
            var assumeRoleRequest = new AssumeRoleRequest
            {
                RoleArn = roleArn,
                RoleSessionName = "LocalLambdaDebug",
                DurationSeconds = 3600
            };
            
            var response = await stsClient.AssumeRoleAsync(assumeRoleRequest);
            
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", response.Credentials.AccessKeyId);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", response.Credentials.SecretAccessKey);
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", response.Credentials.SessionToken);
            
            Console.WriteLine($"Assumed role: {roleArn}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to assume role: {ex.Message}");
        }
    }
    
    private static void DisplayEnvironmentVariables()
    {
        Console.WriteLine("=== Environment Variables ===");
        var relevantVars = new[] { "TABLE_NAME", "QUEUE_URL", "DEBUG_ROLE_ARN", "AWS_REGION", "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "AWS_SESSION_TOKEN", "POWERTOOLS_METRICS_NAMESPACE" };
        
        foreach (var varName in relevantVars)
        {
            var value = Environment.GetEnvironmentVariable(varName);
            if (!string.IsNullOrEmpty(value))
            {
                // Mask sensitive values
                var displayValue = varName.Contains("SECRET") || varName.Contains("TOKEN") ? "***MASKED***" : value;
                Console.WriteLine($"{varName}: {displayValue}");
            }
        }
        Console.WriteLine("==============================\n");
    }
}