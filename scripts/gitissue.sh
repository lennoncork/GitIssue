#!/bin/bash
SCRIPTPATH="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
"$SCRIPTPATH/../src/output/GitIssue.Util/bin/Debug/netcoreapp3.1/win10-x64/GitIssue.Util.exe" "$@"