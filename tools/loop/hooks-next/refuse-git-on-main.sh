#!/bin/sh
# PreToolUse hook for Bash, PowerShell and Monitor. Reads the tool call off standard input
# and refuses git making a commit on main or master, and any push that lands on main or
# master from any branch. Every fix goes on its own fix-FNN branch and reaches main
# through a merged pull request. Exit 2 refuses the call and the line on standard error
# goes back to Claude.
#
# What counts as making a commit on main: commit, cherry-pick, revert, am, rebase and
# commit-tree, and a merge or a pull that is not --ff-only. A fast forward only merge or
# pull is allowed, because it is how main is brought level with origin and it makes no
# commit. A push is refused when it names main or master as where it goes, whatever is
# checked out, and otherwise refused while main is checked out, except a push that only
# deletes a branch, which is how a merged fix branch goes. A command that switches to main
# and then does any of it is refused too, because the branch is read before it runs.
#
# PowerShell was added on 2026-09-27, F99, and git.exe and a quoted path to it with it.
# Rewritten the same day after the Phase 0 breaker: a quoted word between git and the
# verb, such as git -C "C:/Users/.../PAR_NWC-Federator" commit, hid the verb, and that is
# the form an agent writes, because this repo's path holds spaces.
#
# The call is read with read and first matched with case, because on Bader's machine
# starting a program costs about two seconds, measured on 2026-09-27. A command that does
# not say git followed by a space, git.exe or a quote leaves before anything is started.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

# What runs is the command, sent before its description, so words in a description can
# neither trip this wall nor hide from it.
body=${input%%\"description\":*}
case "$body" in *\"command\":*) ;; *) body=$input ;; esac

case "$body" in
    *[Gg][Ii][Tt]" "*|*[Gg][Ii][Tt].[Ee][Xx][Ee]*|*[Gg][Ii][Tt]\\\"*|*[Gg][Ii][Tt]\'*) ;;
    *) exit 0 ;;
esac

refuse() {
    printf '%s\n' "Refused. $1 Branch fix-FNN off main and open a pull request. See CLAUDE.md." >&2
    exit 2
}

# git, then any words, then the verb, quoted or not, then a space, a quote or the end. A
# word is any run of plain characters and quoted parts, so user.name="Bader Rahal" is one
# word. A quoted part is "..." with its quotes escaped as JSON sends them, or '...'.
GIT='git(\.exe)?(\\"|'"'"')?([[:space:]]+(\\"[^"]*"|'"'"'[^'"'"']*'"'"'|[^[:space:]";&|'"'"'])+)*[[:space:]]+(\\"|'"'"')?'
END='(\\"|'"'"')?([[:space:]]|"|$)'

has() {
    printf '%s' "$body" | grep -Eqi "$GIT($1)$END"
}

# git stash push is a stash and not a push.
body=$(printf '%s' "$body" | sed 's/[Ss][Tt][Aa][Ss][Hh][[:space:]]\{1,\}[Pp][Uu][Ss][Hh]/stash save/g')

commits=0; has 'commit|cherry-pick|revert|am|rebase|commit-tree' && commits=1
merges=0; has 'merge|pull' && merges=1
pushes=0; has 'push' && pushes=1
[ $commits = 0 ] && [ $merges = 0 ] && [ $pushes = 0 ] && exit 0

fastforward=0
printf '%s' "$body" | grep -Eqi -- '--ff-only' && fastforward=1
deletes=0
printf '%s' "$body" | grep -Eqi '[[:space:]](--delete|-d)[[:space:]]' && deletes=1
namesmain=0
printf '%s' "$body" | grep -Eqi '([[:space:]:]|refs/heads/)(main|master)([[:space:]]|\\"|"|$)' && namesmain=1

if [ $pushes = 1 ] && [ $namesmain = 1 ] && [ $deletes = 0 ]; then
    refuse "This push names main or master as where it goes, so it would land on main without a pull request."
fi

if printf '%s' "$body" | grep -Eqi '(checkout|switch)([[:space:]]+-[^[:space:]]+)*[[:space:]]+(main|master)([[:space:]]|;|&|"|$)'; then
    if [ $commits = 1 ] || [ $pushes = 1 ] || { [ $merges = 1 ] && [ $fastforward = 0 ]; }; then
        refuse "This command switches to main and then changes it."
    fi
fi

cd "${CLAUDE_PROJECT_DIR:-.}" 2>/dev/null || exit 0
branch=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)

case "$branch" in
    main|master)
        [ $commits = 1 ] && refuse "$branch is checked out and nothing is committed on it."
        [ $merges = 1 ] && [ $fastforward = 0 ] && refuse "$branch is checked out, and a merge or pull that is not --ff-only makes a commit on it."
        [ $pushes = 1 ] && [ $deletes = 0 ] && refuse "$branch is checked out and nothing is pushed from it."
        ;;
esac

exit 0
