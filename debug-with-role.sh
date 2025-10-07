#!/bin/bash

# Example script to debug Lambda with assumed role
# Usage: ./debug-with-role.sh arn:aws:iam::123456789012:role/LambdaExecutionRole

ROLE_ARN=${1:-""}

if [ -z "$ROLE_ARN" ]; then
    echo "Usage: $0 <role-arn>"
    echo "Example: $0 arn:aws:iam::123456789012:role/LambdaExecutionRole"
    exit 1
fi

echo "Setting DEBUG_ROLE_ARN to: $ROLE_ARN"
export DEBUG_ROLE_ARN=$ROLE_ARN

echo "Building project..."
dotnet build src/HelloWorld/

echo "Running Lambda function with assumed role..."
dotnet run --project src/HelloWorld/