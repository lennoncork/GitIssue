#!/bin/bash
SCRIPTPATH=$(realpath ${0%/*})
git config --global alias.issue '!f() { '"'$SCRIPTPATH/gitissue.sh'"' "$@"; }; f'
