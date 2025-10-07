#!/bin/bash

echo "Building Lambda function..."
sam build

echo "Starting Lambda function in debug mode..."
echo "The function will wait for debugger attachment on port 5858"
echo "Use VS Code 'Debug Lambda Function' configuration to attach"

sam local invoke HelloWorldFunction \
    --event events/event.json \
    --debug-port 5858 \
    --debugger-path /tmp/lambci_debug_files \
    --debug-args "-agentlib:jdwp=transport=dt_socket,server=y,suspend=y,address=*:5858"