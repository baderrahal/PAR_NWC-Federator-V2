#!/bin/sh
# Stop hook. Reads the STATE line of steps/loop.md and keeps a session going while the
# loop is OPEN, so a session cannot end with the loop half done and nobody told.
#
# Written to the Claude Code hooks reference, read on 2026-09-27 at
# https://code.claude.com/docs/en/hooks. A Stop hook gets JSON on standard input, with
# stop_hook_active true when an earlier Stop hook already blocked. It blocks by exiting 0
# with {"decision": "block", "reason": "..."} on standard output, and the reason goes back
# to Claude. The page warns that a Stop hook that always blocks never lets Claude finish.
#
# So it lets the stop through when the state is CLOSED, WAITING or RESTART, when
# steps/loop.md is missing or carries no STATE line, and when it already blocked once and
# steps/loop.md has not changed since. It keeps the size and the time of steps/loop.md at
# its last block in .claude/hooks/.loop-gate-last, which git ignores. Any other state blocks.

cat >/dev/null

root="${CLAUDE_PROJECT_DIR:-.}"
loop="$root/steps/loop.md"
last="$root/.claude/hooks/.loop-gate-last"

if [ ! -f "$loop" ]; then
    exit 0
fi

state=$(sed -n 's/^STATE[[:space:]]\{1,\}\([A-Z][A-Z]*\).*$/\1/p' "$loop" | head -n 1)

case "$state" in
    OPEN) ;;
    *) exit 0 ;;
esac

size=$(wc -c < "$loop" | tr -d ' ')
time=$(date -r "$loop" +%s 2>/dev/null || echo 0)
now="$size $time"

if [ -f "$last" ] && [ "$(cat "$last")" = "$now" ]; then
    exit 0
fi

printf '%s\n' "$now" > "$last"
printf '%s\n' '{"decision": "block", "reason": "The loop is not closed. Read steps/loop.md and carry on from the next open item."}'
exit 0
