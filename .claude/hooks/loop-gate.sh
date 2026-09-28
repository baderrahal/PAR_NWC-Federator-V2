#!/bin/sh
# Stop hook. Reads the STATE line of steps/loop.md and sends a session back once while the
# loop is OPEN, so a session cannot end with the loop half done and nobody told.
#
# Written to the Claude Code hooks reference, read on 2026-09-27 at
# https://code.claude.com/docs/en/hooks. A Stop hook gets JSON on standard input carrying
# session_id and stop_hook_active, true when an earlier Stop hook already blocked. It blocks
# by exiting 0 with {"decision": "block", "reason": "..."} on standard output, and the
# reason goes back to Claude. The page warns that a Stop hook that always blocks never lets
# Claude finish, so this one blocks a session at most once per change to steps/loop.md.
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
# Rewritten on 2026-09-27 and 2026-09-28 after the Phase 0 reviews: the note held one
# session so sessions stopping in turn wiped each other's mark, a note that could not be
# written blocked forever, and **STATE** OPEN read as no state at all. A steps/loop.md
# saved as UTF-16 reads as no state and lets the stop through, which is the safe side, so
# the lead writes that file as UTF-8. Two more ways it lets a stop through, both on the
# safe side: a STATE line below line ten, which the rules put on line three, and a second
# edit of steps/loop.md in the same second and of the same size, because a change is read
# off the size and the time, which is what the loop prompt asked it to keep.

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

# One line per session. This session's line is swapped for the new one and every other
# session's line is kept, so two sessions stopping in turn never wipe each other's mark.
if [ -f "$last" ] && grep -qxF -- "$now" "$last" 2>/dev/null; then
    exit 0
fi

{ [ -f "$last" ] && awk -v s="$session" '$1 != s' "$last" 2>/dev/null; printf '%s\n' "$now"; } > "$last.$$" 2>/dev/null || exit 0
mv -f "$last.$$" "$last" 2>/dev/null || exit 0
grep -qxF -- "$now" "$last" 2>/dev/null || exit 0

printf '%s\n' '{"decision": "block", "reason": "The loop is not closed. Read steps/loop.md and carry on from the next open item."}'
exit 0
