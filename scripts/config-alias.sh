#!/bin/bash
SCRIPTPATH="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
git config --global alias.issue '!f() { '"'$SCRIPTPATH/gitissue.sh'"' "$@"; }; f'
#dirname $0