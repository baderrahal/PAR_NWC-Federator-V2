#!/bin/sh
# PreToolUse hook for Bash and PowerShell. Reads the tool call off standard input and
# refuses a git commit or a git push while main or master is checked out. Every fix goes
# on its own fix-FNN branch and reaches main through a merged pull request. Exit 2
# refuses the call and the line on standard error goes back to Claude.
#
# PowerShell was added on 2026-09-27, F99. Until then the wall was matched on the Bash
# tool alone, so a commit on main sent through the PowerShell tool went straight past it.
# PowerShell also calls git as git.exe, or by a quoted path ending in git.exe, and both
# are read here as git.
#
# The call is read with read and first matched with case, because on Bader's machine
# starting a program costs about two seconds, measured on 2026-09-27, and this wall runs
# before every command. A command that does not mention git with commit or push after it
# leaves before anything is started.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

case "$input" in
    *[Gg][Ii][Tt]*[Cc][Oo][Mm][Mm][Ii][Tt]*|*[Gg][Ii][Tt]*[Pp][Uu][Ss][Hh]*) ;;
    *) exit 0 ;;
esac

if ! printf '%s' "$input" | grep -Eqi 'git(\.exe)?(\\?")?( +[^ ";&|]+)* +(commit|push)([^a-zA-Z0-9_-]|$)'; then
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
