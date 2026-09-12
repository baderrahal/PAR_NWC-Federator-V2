#!/bin/sh
# PreToolUse hook for Bash. Reads the tool call off standard input and refuses a git
# commit or a git push while main or master is checked out. Every fix goes on its own
# fix-FNN branch and reaches main through a merged pull request. Exit 2 refuses the
# call and the line on standard error goes back to Claude.

input=$(cat)

if ! printf '%s' "$input" | grep -Eq 'git( +[^ ";&|]+)* +(commit|push)([^a-zA-Z0-9_-]|$)'; then
    exit 0
fi

cd "${CLAUDE_PROJECT_DIR:-.}" 2>/dev/null || exit 0

branch=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)

case "$branch" in
    main|master)
        echo "Refused. $branch is checked out and nothing is committed or pushed on it. Branch fix-FNN off main and open a pull request. See CLAUDE.md." >&2
        exit 2
        ;;
esac

exit 0
