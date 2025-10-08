# Lambda Debugging Sample with IAM Role Authentication

⚠️ **IMPORTANT: This is sample code for demonstration purposes only. Do not use in production environments.**

## Overview

This project demonstrates how to debug AWS Lambda functions locally while using IAM roles to authenticate against AWS STS (Security Token Service). It shows how to assume roles for local development and testing scenarios.

## What This Sample Demonstrates

- **Local Lambda Debugging**: Running Lambda functions locally with proper environment setup
- **IAM Role Assumption**: Using STS AssumeRole for authentication during local debugging
- **Environment Variable Management**: Handling AWS credentials and configuration for local development
- **AWS Powertools Integration**: Basic setup with Powertools for .NET logging, metrics, and tracing

## Key Features

- Assumes IAM roles using STS for local debugging
- Displays and manages environment variables securely
- Provides fallback configuration for local development
- Demonstrates proper credential handling and masking

## Prerequisites

- .NET 8 SDK
- AWS CLI configured with appropriate permissions
- SAM CLI (optional, for deployment)
- An IAM role with necessary permissions for your Lambda function

## Local Debugging Setup

1. Set the `DEBUG_ROLE_ARN` environment variable to the ARN of the role you want to assume:
   ```bash
   export DEBUG_ROLE_ARN=arn:aws:iam::123456789012:role/YourLambdaExecutionRole
   ```

2. Run the application locally:
   ```bash
   dotnet run --project src/HelloWorld
   ```

## Environment Variables

The sample uses these environment variables:
- `DEBUG_ROLE_ARN`: IAM role ARN to assume for local debugging
- `TABLE_NAME`: DynamoDB table name (fallback: "UnumDebugLambda")
- `QUEUE_URL`: SQS queue URL for testing
- `AWS_REGION`: AWS region
- `POWERTOOLS_METRICS_NAMESPACE`: Metrics namespace

## Security Considerations

- Credentials are masked in console output
- Temporary credentials from STS are used
- Role assumption is limited to 1 hour sessions
- This approach is suitable only for development/debugging

## Limitations

- **Not for production use**: This code includes debugging utilities and hardcoded fallbacks
- **Local development only**: The role assumption pattern is designed for local debugging scenarios
- **Temporary credentials**: STS tokens expire and need renewal for long debugging sessions

## Related AWS Services

- **AWS Lambda**: Serverless compute service
- **AWS STS**: Security Token Service for temporary credentials
- **AWS IAM**: Identity and Access Management for roles and permissions
- **AWS Powertools**: Developer toolkit for Lambda best practices

---

**Disclaimer**: This sample code is provided for educational and demonstration purposes. Always follow AWS security best practices and your organization's security policies when working with IAM roles and credentials in production environments.