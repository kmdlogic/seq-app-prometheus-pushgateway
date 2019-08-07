#!/bin/bash
export PATH=$HOME/.dotnetcli:$PATH
pwsh ./build.ps1 "$@"
