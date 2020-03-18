#!/bin/bash
SCRIPTPATH=${0%/*}
"$SCRIPTPATH/../src/output/GitIssue.Tool/bin/Release/netcoreapp3.1/win10-x64/publish/GitIssue.Tool.exe" "$@"