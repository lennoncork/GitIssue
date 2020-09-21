#!/bin/bash
SCRIPTPATH=${0%/*}
PUBLISHDIR=$(realpath "$SCRIPTPATH/../.publish")
"$PUBLISHDIR/GitIssue.Tool.exe" "$@"