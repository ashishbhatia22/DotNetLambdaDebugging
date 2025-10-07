# Lambda Debugging Guide

## Prerequisites
- VS Code with C# extension installed
- SAM CLI installed
- Docker installed and running

## Debugging Options

### 1. Local Function Debugging (Recommended)
1. Open VS Code in the project root
2. Set breakpoints in `src/HelloWorld/Function.cs`
3. Press F5 or use "Debug Lambda with Test Tool" configuration
4. The function will build and be ready for debugging

### 2. Debug with Assumed Role
To test with specific IAM permissions:
```bash
./debug-with-role.sh arn:aws:iam::123456789012:role/LambdaExecutionRole
```
Or set environment variable:
```bash
export DEBUG_ROLE_ARN=arn:aws:iam::123456789012:role/LambdaExecutionRole
dotnet run --project src/HelloWorld/
```

### 3. SAM Local API Debugging
1. Use "SAM Local Debug" configuration
2. This starts the API Gateway locally on port 3000
3. Send requests to `http://localhost:3000/hello`
4. Attach debugger when prompted

### 4. Manual SAM Debugging
Run the debug script:
```bash
./debug-lambda.sh
```

## Testing the Function
After starting the local API:
```bash
curl http://localhost:3000/hello
```

## Debugging Tips
- Ensure Docker is running before using SAM local commands
- Use `sam build` before debugging if you make changes
- Check the integrated terminal for any error messages
- The function uses Powertools for logging, tracing, and metrics