#!/bin/sh
# PreToolUse hook for Edit, Write and MultiEdit. Reads the tool call off standard
# input and refuses a change to a file under samples, steps/logs or bundle. The first
# two are evidence and the third is build output. Exit 2 refuses the call and the
# line on standard error goes back to Claude.

input=$(cat)

path=$(printf '%s' "$input" \
    | sed -n 's/.*"file_path"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' \
    | head -n 1)

if [ -z "$path" ]; then
    exit 0
fi

# JSON doubles a backslash, and Windows paths carry them. Read every one as a slash.
path=$(printf '%s' "$path" | sed 's#\\\\#/#g; s#\\#/#g')
root=$(printf '%s' "${CLAUDE_PROJECT_DIR:-$(pwd)}" | sed 's#\\#/#g')

case "$path" in
    "$root"/*) rel=${path#"$root"/} ;;
    *) rel=$path ;;
esac

case "/$rel" in
    */samples/*|*/steps/logs/*|*/bundle/*)
        echo "Refused. $rel is under samples, steps/logs or bundle, which are never edited. See CLAUDE.md." >&2
        exit 2
        ;;
esac

exit 0
