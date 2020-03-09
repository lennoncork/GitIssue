#!/bin/bash
SCRIPTPATH="$( cd "$(dirname "$0/..")" >/dev/null 2>&1 ; pwd -P )"
"$SCRIPTPATH/src/output/GitIssue.Util/bin/Release/netcoreapp3.1/win10-x64/publish/GitIssue.Util.exe" "$@"