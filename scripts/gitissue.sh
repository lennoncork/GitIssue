#!/bin/bash
SCRIPTPATH="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
"$SCRIPTPATH/../src/output/GitIssue.Util/publish/Release/win10-x64/GitIssue.Util.exe" "$@"