#!/bin/sh
# Stop hook. Reads the STATE line of steps/loop.md and sends a session back once while the
# loop is OPEN, so a session cannot end with the loop half done and nobody told.
#
# Written to the Claude Code hooks reference, read on 2026-09-27 at
# https://code.claude.com/docs/en/hooks. A Stop hook gets JSON on standard input carrying
# session_id and stop_hook_active, true when an earlier Stop hook already blocked. It blocks
# by exiting 0 with {"decision": "block", "reason": "..."} on standard output, and the
# reason goes back to Claude. The page warns that a Stop hook that always blocks never lets
# Claude finish, so this one can only ever block once per change to steps/loop.md.
#
# ONLY OPEN BLOCKS. CLOSED, WAITING, RESTART, any other word, a missing steps/loop.md and a
# file with no STATE line all let the stop through, because a gate that cannot read the
# state must never be the thing that traps a session. The STATE line is looked for in the
# first ten lines, read without regard to case, colons or markdown emphasis, so STATE Open,
# STATE: OPEN and **STATE** OPEN all read as OPEN.
#
# It keeps, per session, the size and the time of steps/loop.md at its last block in
# .claude/hooks/.loop-gate-last, which git ignores. A session sent back once and stopping
# again over an unchanged file is let through. A note that cannot be written lets the stop
# through rather than blocking a stop it could never let go of.
#
# Rewritten on 2026-09-27 after the Phase 0 reviewer and breaker: the note was shared by
# every session, a note that could not be written blocked forever, and **STATE** OPEN read
# as no state at all.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

session=${input#*\"session_id\":}
session=${session#*\"}
session=${session%%\"*}

root="${CLAUDE_PROJECT_DIR:-.}"
loop="$root/steps/loop.md"
last="$root/.claude/hooks/.loop-gate-last"

[ -f "$loop" ] || exit 0

state=$(head -n 10 "$loop" | tr -d '*`:' | tr '[:lower:]' '[:upper:]' | sed -n 's/^[[:space:]]*STATE[[:space:]]\{1,\}\([A-Z]\{1,\}\).*$/\1/p' | head -n 1)

case "$state" in
    OPEN) ;;
    *) exit 0 ;;
esac

size=$(wc -c < "$loop" | tr -d ' ')
time=$(date -r "$loop" +%s 2>/dev/null || echo 0)
now="$session $size $time"

if [ -f "$last" ] && [ "$(cat "$last" 2>/dev/null)" = "$now" ]; then
    exit 0
fi

printf '%s\n' "$now" > "$last" 2>/dev/null || exit 0
[ "$(cat "$last" 2>/dev/null)" = "$now" ] || exit 0

printf '%s\n' '{"decision": "block", "reason": "The loop is not closed. Read steps/loop.md and carry on from the next open item."}'
exit 0
