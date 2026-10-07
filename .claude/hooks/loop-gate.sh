#!/bin/sh
# Stop hook. Reads the STATE line of steps/PROGRESS.md, the one page a session starts from, F139,
# Bader's message of 2026-10-06, Q139. While it reads OPEN the gate refuses a stop until the page
# has been rewritten since the last merge and the last run, and sends a session back once per
# change of the page, so a session cannot end with the loop half done, the page stale and nobody
# told.
#
# Written to the Claude Code hooks reference, read on 2026-09-27 at
# https://code.claude.com/docs/en/hooks. A Stop hook gets JSON on standard input carrying
# session_id and stop_hook_active. It blocks by exiting 0 with {"decision": "block", "reason":
# "..."} on standard output, and the reason goes back to Claude. The page warns that a Stop hook
# that always blocks never lets Claude finish, so every refusal here is one the session ends
# itself by rewriting steps/PROGRESS.md, and the send back blocks at most once per change of it.
# An agent of a workflow ends on SubagentStop and not Stop, and this gate is run for Stop only.
#
# ONLY OPEN BLOCKS. WAITING, RESTART, CLOSED, NEXT WAVE, any other word, no page and a page with
# no STATE line all let the stop through, because a gate that cannot read the state must never be
# the thing that traps a session. The STATE line is looked for in the first ten lines, read
# without regard to case, colons, backticks or asterisks, and the first word of letters after
# STATE is the state, so STATE OPEN, 2026-10-06 16:00 and **STATE** Open read OPEN, STATE NEXT
# WAVE reads NEXT, and STATE 2026-10-06 OPEN reads no state. A control character at a line's end
# is dropped, since steps files check out CRLF on this machine.
#
# While OPEN, three tests. Both the merge and the run are read off what the page SAYS, never off
# its file time, since a checkout of origin/main writes the page whenever a merge changed its
# counts, and the maker writes it whenever tracker.csv changed, and neither is the lead's rewrite.
# 1. A MERGE. The lead's rewrite always changes the STATE line, whose date and time are on it, and
#    a merge carrying the rewrite changes the STATE line main holds. So the page counts as
#    rewritten since the last merge when its STATE line differs from the STATE line of
#    steps/PROGRESS.md at the first parent of origin/main's head, which is main before that merge.
#    One git call answers it, git diff -G'^STATE ' of origin/main^1 against the page as it is
#    in the working copy, with core.autocrlf true reading a CRLF copy the same as its LF blob,
#    about 1.3 s, turn5\q139\hooks.md section 2. Exit 0, no change there, refuses every stop until
#    the page is rewritten. --no-optional-locks keeps git from writing the index of the clone. In
#    a worktree .git is a file naming its own folder under the main clone's .git, whose commondir
#    file names that .git, each read with read, and the reflog of origin/main must be there before
#    git is started. No reflog, a git that cannot answer and a head with no parent let the stop
#    through.
# 2. A RUN. The STATE line names the run the page was last rewritten after, as last run 05/item1
#    or last run 05\item1, the set and the run under %LOCALAPPDATA%\NwcFederatorLoop\runs, a dot
#    after it read as the end of a sentence. A record.txt of a run there holding a line that
#    starts VERDICT, which is how run.ps1's HasVerdict tells a run that finished, and newer than
#    the named run's record.txt, refuses every stop until the page names the newest such run. A
#    page naming no run, or a run with no record.txt, has every finished run newer than it. A run
#    still going, its record.txt holding no VERDICT line yet, does not count, since it moves with
#    every line. The records newer than the named one are taken newest first by file time and
#    read until one holds a VERDICT line, so none older than the newest finished run is read.
#    LOCALAPPDATA reaches the gate with its drive and backslashes, C:\Users\<name>\AppData\Local,
#    and the glob reads that spelling, proved by tools\loop\prove-hooks.sh and on the real runs
#    folder, turn5\f139b-gate-real.txt.
# 3. A CHANGE. A note per session, .claude/hooks/.loop-gate-<session id>, which git ignores,
#    written when the gate sends the session back. A note newer than the page lets the stop
#    through, so a session is sent back once per change of the page, as the gate did for
#    steps/loop.md before F139. A note that cannot be written lets the stop through rather than
#    blocking a stop it could never let go of.
#
# BUILTINS ONLY, as CLAUDE.md asks, since starting a program costs about two seconds here: read,
# case, test, : and printf. The one program is the git of test 1, on every OPEN stop in a clone
# that holds the reflog of origin/main. Before F139 the gate started 12 programs on a block and
# took 10 to 14 s, turn5\q139\hooks.md section 1.
#
# Its limits, said out loud. A merge this clone has not fetched is not seen. A working copy whose
# page holds another STATE line than main held before its last merge reads as rewritten, such as
# a fix branch checked out in the main clone. A run that finishes again in the folder the page
# already names is not seen, since its record is the named one. A page saved as UTF-16 or with a
# byte order mark before its STATE line reads no state there, and a STATE line below line ten is
# not read, both on the side that lets the stop through. Two writes of the page and the note in
# the same instant of the file system read as no change.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

session=${input#*\"session_id\":}
session=${session#*\"}
session=${session%%\"*}
case $session in
    ""|*[!A-Za-z0-9-]*) session=unknown ;;
esac

root="${CLAUDE_PROJECT_DIR:-.}"
page="$root/steps/PROGRESS.md"

[ -f "$page" ] || exit 0

block() {
    printf '{"decision": "block", "reason": "%s"}\n' "$1"
    exit 0
}

state=
named=
n=0
while [ "$n" -lt 10 ] && { IFS= read -r line || [ -n "$line" ]; }; do
    n=$((n + 1))
    line=${line%[[:cntrl:]]}
    while :; do
        case $line in
            *'*'*) line=${line%%'*'*}${line#*'*'} ;;
            *':'*) line=${line%%':'*}${line#*':'} ;;
            *'`'*) line=${line%%'`'*}${line#*'`'} ;;
            *) break ;;
        esac
    done
    line=${line#"${line%%[![:space:]]*}"}
    case $line in
        [Ss][Tt][Aa][Tt][Ee][[:space:]]*) ;;
        *) continue ;;
    esac
    word=${line#?????}
    word=${word#"${word%%[![:space:]]*}"}
    word=${word%%[![:alpha:]]*}
    [ -n "$word" ] || continue
    state=$word
    case $line in
        *"last run "*)
            named=${line#*last run }
            while :; do
                case $named in
                    *'\'*) named=${named%%'\'*}/${named#*'\'} ;;
                    *) break ;;
                esac
            done
            named=${named%%[!A-Za-z0-9._/-]*}
            while :; do
                case $named in
                    *.) named=${named%.} ;;
                    *) break ;;
                esac
            done
            ;;
    esac
    break
done < "$page"

case $state in
    [Oo][Pp][Ee][Nn]) ;;
    *) exit 0 ;;
esac

# 1. A merge fetched since the page's STATE line was last changed.
dot="$root/.git"
common=
if [ -d "$dot" ]; then
    common=$dot
elif [ -f "$dot" ]; then
    first=
    IFS= read -r first < "$dot"
    first=${first%[[:cntrl:]]}
    case $first in
        "gitdir: "*)
            gitdir=${first#gitdir: }
            case $gitdir in
                /*|[A-Za-z]:*) ;;
                *) gitdir="$root/$gitdir" ;;
            esac
            common=$gitdir
            if [ -f "$gitdir/commondir" ]; then
                back=
                IFS= read -r back < "$gitdir/commondir"
                back=${back%[[:cntrl:]]}
                case $back in
                    "") ;;
                    /*|[A-Za-z]:*) common=$back ;;
                    *) common="$gitdir/$back" ;;
                esac
            fi
            ;;
    esac
fi
if [ -n "$common" ] && [ -f "$common/logs/refs/remotes/origin/main" ]; then
    git --no-optional-locks -C "$root" diff --quiet -G'^STATE ' 'origin/main^1' -- steps/PROGRESS.md >/dev/null 2>&1
    if [ "$?" = 0 ]; then
        block "The STATE line of steps/PROGRESS.md is the one main held before its last merge fetched from origin/main, so the page was not rewritten since that merge. Rewrite the page, never append, then carry on or stop."
    fi
fi

# 2. A run that finished after the run the page names.
runs=${LOCALAPPDATA:+$LOCALAPPDATA/NwcFederatorLoop/runs}
if [ -n "$runs" ]; then
    since=
    [ -n "$named" ] && [ -f "$runs/$named/record.txt" ] && since="$runs/$named/record.txt"
    # Newest first: the newest record not read yet is found by file time alone, then read, until
    # one holds a VERDICT line, so a record older than the newest finished one is never read.
    newest=
    read_already='
'
    while :; do
        top=
        for rec in "$runs"/*/*/record.txt; do
            [ -f "$rec" ] || continue
            case $read_already in
                *"
$rec
"*) continue ;;
            esac
            if [ -n "$since" ]; then
                [ "$rec" -nt "$since" ] || continue
            fi
            if [ -n "$top" ]; then
                [ "$rec" -nt "$top" ] || continue
            fi
            top=$rec
        done
        [ -n "$top" ] || break
        while IFS= read -r l || [ -n "$l" ]; do
            case $l in
                VERDICT*)
                    newest=$top
                    break
                    ;;
            esac
        done < "$top"
        [ -n "$newest" ] && break
        read_already="$read_already$top
"
    done
    if [ -n "$newest" ]; then
        where=${newest#"$runs"/}
        where=${where%/record.txt}
        case $where in
            *[!A-Za-z0-9._/-]*) where="a run folder" ;;
        esac
        block "A run finished that steps/PROGRESS.md does not name, $where under the loop's runs folder. Rewrite the page, never append, its STATE line ending last run $where, then carry on or stop."
    fi
fi

# 3. Once per session per change of the page.
note="$root/.claude/hooks/.loop-gate-$session"
if [ -f "$note" ] && [ "$note" -nt "$page" ]; then
    exit 0
fi
# printf and not :, since a redirection that fails on : , a special builtin, ends the shell
# with exit 1 before || is read, measured by prove-hooks.sh on 2026-10-06.
printf '' 2>/dev/null > "$note" || exit 0
[ -f "$note" ] || exit 0
block "The loop is not closed. Read steps/PROGRESS.md and carry on from the next open item."
