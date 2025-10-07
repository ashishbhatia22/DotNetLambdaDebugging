using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Powertools.Tracing;
using AWS.Lambda.Powertools.Metrics;
using AWS.Lambda.Powertools.Logging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld;

public class Function
{
    private static readonly HttpClient client = new HttpClient();
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IAmazonSQS _sqs;
    private readonly string _tableName = Environment.GetEnvironmentVariable("TABLE_NAME") ?? "UnumDebugLambda";
    private readonly string _queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL") ?? "";
    
    public Function()
    {
        Tracing.RegisterForAllServices();
        _dynamoDb = new AmazonDynamoDBClient();
        _sqs = new AmazonSQSClient();
    }
    [Tracing(SegmentName = "Get Calling IP")]
    private static async Task<string> GetCallingIP()
    {
        try
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);
            // Custom Metric
            // https://awslabs.github.io/aws-lambda-powertools-dotnet/core/metrics/
            Metrics.AddMetric("ApiRequestCount", 1, MetricUnit.Count);

            return msg.Replace("\n","");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    [Tracing(SegmentName = "List DynamoDB Tables")]
    private async Task ListDynamoDBTables()
    {
        try
        {
            var response = await _dynamoDb.ListTablesAsync();
            Logger.LogInformation($"Available DynamoDB tables: {string.Join(", ", response.TableNames)}");
            Logger.LogInformation($"Looking for table: {_tableName}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to list DynamoDB tables: {ex.Message}");
        }
    }
    
    [Tracing(SegmentName = "Save to DynamoDB")]
    private async Task SaveToDynamoDB(string id, string location)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id },
                ["Location"] = new AttributeValue { S = location },
                ["Timestamp"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
            };
            
            await _dynamoDb.PutItemAsync(_tableName, item);
            Logger.LogInformation($"Saved item to DynamoDB: {id}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to save to DynamoDB: {ex.Message}");
        }
    }
    
    [Tracing(SegmentName = "Send to SQS")]
    private async Task SendToSQS(string message)
    {
        if (string.IsNullOrEmpty(_queueUrl)) return;
        
        try
        {
            await _sqs.SendMessageAsync(_queueUrl, message);
            Logger.LogInformation($"Sent message to SQS: {message}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to send to SQS: {ex.Message}");
        }
    }
    
    [Tracing(CaptureMode = TracingCaptureMode.ResponseAndError)]
    [Metrics(CaptureColdStart = true)]
    [Logging(CorrelationIdPath = CorrelationIdPaths.ApiGatewayRest)]
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
    {
        var requestId = context.AwsRequestId;
        var location = await GetCallingIP();
        
        // List available DynamoDB tables for debugging
        await ListDynamoDBTables();
        
        // Save to DynamoDB
        await SaveToDynamoDB(requestId, location);
        
        // Send to SQS
        var sqsMessage = JsonSerializer.Serialize(new { RequestId = requestId, Location = location, Timestamp = DateTime.UtcNow });
        await SendToSQS(sqsMessage);
        
        var body = new Dictionary<string, string>
        {
            { "message", "hello world" },
            { "location", location },
            { "requestId", requestId },
            { "saved", "dynamodb and sqs" }
        };
        
        Logger.LogInformation($"Processed request {requestId} - saved to DynamoDB and SQS");
        Metrics.AddMetric("ProcessedRequests", 1, MetricUnit.Count);

        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(body),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}