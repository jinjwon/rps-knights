#!/bin/zsh
set -eu
cd -- "$(dirname -- "$0")"
exec /Applications/Godot.app/Contents/MacOS/Godot --editor --path "$PWD"
