#!/bin/sh
# Feeds every case to each hook on standard input and prints what it answered.
# Usage: sh tools/loop/prove-hooks.sh <hooks folder> <repo root>
# Each verdict line is the proof. The totals are counted off the lines at the end.

hooks="$1"
repo="$2"
scratch=$(mktemp -d)
out="$scratch/verdicts"
: > "$out"

run() {
    # $1 hook file, $2 expected exit, $3 case name, $4 project dir. JSON on standard input.
    json=$(cat)
    err=$(printf '%s' "$json" | CLAUDE_PROJECT_DIR="$4" sh "$hooks/$1" 2>&1 >/dev/null)
    code=$?
    if [ "$code" = "$2" ]; then v=ok; else v=WRONG; fi
    printf '%-5s exit %s (want %s)  %s\n' "$v" "$code" "$2" "$3" | tee -a "$out"
    [ -n "$err" ] && printf '        %s\n' "$err"
}

gate() {
    # $1 block or allow, $2 case name, $3 project dir, $4 words the reason of a block must hold.
    # JSON on standard input. A Stop hook exits 0 either way. LOCALAPPDATA points at a throwaway
    # folder, so the gate never reads the loop's real run folders here.
    json=$(cat)
    o=$(printf '%s' "$json" | LOCALAPPDATA="$lapp" CLAUDE_PROJECT_DIR="$3" sh "$hooks/loop-gate.sh" 2>/dev/null)
    code=$?
    case "$o" in *'"decision": "block"'*) got=block ;; *) got=allow ;; esac
    v=ok
    if [ "$got" != "$1" ] || [ "$code" != 0 ]; then v=WRONG; fi
    if [ "$got" = block ] && [ -n "$4" ]; then
        case "$o" in *"$4"*) ;; *) v=WRONG ;; esac
    fi
    printf '%-5s %s, exit %s (want %s)  %s\n' "$v" "$got" "$code" "$1" "$2" | tee -a "$out"
    [ "$v" = WRONG ] && [ -n "$o" ] && printf '        %s\n' "$o"
}

W='C:\\Users\\p003653k\\OneDrive - Parsons Corp\\Documents\\GitHub\\PAR_NWC-Federator'
NM='C:\\Users\\p003653k\\OneDrive - Parsons Corp\\Desktop\\NM Fed'
NMU='/c/Users/p003653k/OneDrive - Parsons Corp/Desktop/NM Fed'
ACC='C:\\Users\\p003653k\\DC\\ACCDocs\\Parsons\\1104 NM\\Project Files'
P=refuse-protected-paths.sh
G=refuse-git-on-main.sh

echo "== the paths wall, file tools"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/samples/x.xml","content":"a"}}' | run $P 2 "Write under samples" "$repo"
echo '{"tool_name":"Edit","tool_input":{"file_path":"'"$repo"'/steps/logs/run.log","old_string":"a","new_string":"b"}}' | run $P 2 "Edit under steps/logs" "$repo"
echo '{"tool_name":"MultiEdit","tool_input":{"file_path":"'"$repo"'/bundle/ParsonsNwcFederator.bundle/PackageContents.xml","edits":[]}}' | run $P 2 "MultiEdit under bundle" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$W"'\\samples\\x.xml","content":"a"}}' | run $P 2 "Write under samples, Windows spelling" "$repo"
echo '{"tool_name":"NotebookEdit","tool_input":{"notebook_path":"'"$repo"'/samples/n.ipynb","new_source":"a"}}' | run $P 2 "NotebookEdit under samples" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/steps/runs/00/first/run.tsv","content":"a"}}' | run $P 2 "Write under steps/runs, the run evidence" "$repo"
echo '{"tool_name":"Edit","tool_input":{"file_path":"samples/client-report/a.txt","old_string":"a","new_string":"b"}}' | run $P 2 "Edit a relative path under samples" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"Samples\\\\x.xml","content":"a"}}' | run $P 2 "Write under Samples, capital, relative, backslash" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"steps\\\\logs\\\\x.log","content":"a"}}' | run $P 2 "Write under steps\\logs, relative" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$NM"'\\x.txt","content":"a"}}' | run $P 2 "Write into NM Fed" "$repo"
echo '{"tool_name":"Edit","tool_input":{"file_path":"'"$NM"'\\NWC\\C06\\a.nwc","old_string":"a","new_string":"b"}}' | run $P 2 "Edit a file in NM Fed" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$ACC"'\\x.nwf","content":"a"}}' | run $P 2 "Write under DC ACCDocs" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"C:\\Users\\p003653k\\DC\\BIM 360\\a\\x.nwf","content":"a"}}' | run $P 2 "Write under DC BIM 360" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"C:/Users/p003653k/ONEDRI~1/Desktop/NMFED~1/a.txt","content":"x"}}' | run $P 2 "Write through the short name NMFED~1" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"C:/Users/p003653k/OneDrive - Parsons Corp/Desktop/q\"/../NM Fed/a.txt","content":"x"}}' | run $P 2 "Write to a path holding a quote" "$repo"
echo '{"tool_name":"Write","tool_input":{"content":"file_path","file_path":"'"$NM"'\\a.txt"}}' | run $P 2 "Write whose content is the word file_path, before the key" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/src/Federator.Core/X.cs","content":"a"}}' | run $P 0 "Write under src" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/steps/PROGRESS.md","content":"NM Fed is copied by prepare-copy, file_path"}}' | run $P 0 "Write to steps/PROGRESS.md, content naming NM Fed" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/src/logs/x.cs","content":"a"}}' | run $P 0 "a folder named logs that is not steps/logs" "$repo"
echo '{"tool_name":"NotebookEdit","tool_input":{"notebook_path":"'"$repo"'/tools/n.ipynb","new_source":"a"}}' | run $P 0 "NotebookEdit under tools" "$repo"
echo '{"tool_name":"Write","tool_input":{"content":"a"}}' | run $P 0 "a file tool call with no path" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"C:\\Temp\\Output\\","content":"x"}}' | run $P 0 "a path that really ends in a backslash is not a quote" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/.claude/settings.json","content":"{}"}}' | run $P 2 "Write to .claude/settings.json, a wall" "$repo"
echo '{"tool_name":"Edit","tool_input":{"file_path":"'"$W"'\\.claude\\hooks\\loop-gate.sh","old_string":"a","new_string":"b"}}' | run $P 2 "Edit a hook under .claude/hooks, Windows spelling" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":".claude/settings.local.json","content":"{}"}}' | run $P 2 "Write to .claude/settings.local.json, relative" "$repo"
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/.claude/rules/loop.md","content":"a"}}' | run $P 0 "Write to .claude/rules/loop.md, not a wall" "$repo"

echo "== the paths wall, command tools"
echo '{"tool_name":"Bash","tool_input":{"command":"ls \"'"$NMU"'\"","description":"list a folder"}}' | run $P 2 "Bash lists NM Fed" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Get-ChildItem \"'"$NM"'\" -Recurse","description":"list"}}' | run $P 2 "PowerShell lists NM Fed" "$repo"
echo '{"tool_name": "PowerShell", "tool_input": {"command": "Get-ChildItem \"'"$NM"'\""}}' | run $P 2 "PowerShell lists NM Fed, JSON spaced after colons" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Remove-Item \"'"$NM"'\\NWC\\C06\\a.nwc\"","description":"x"}}' | run $P 2 "PowerShell removes a file in NM Fed" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"cp \"/c/Users/p003653k/DC/ACCDocs/Parsons/x.nwf\" /tmp/","description":"x"}}' | run $P 2 "Bash copies out of DC ACCDocs" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"cd ~/DC && rm -rf ACCDocs/Parsons","description":"x"}}' | run $P 2 "Bash names ACCDocs after a cd" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"rm -f ~/OneDrive\\ -\\ Parsons\\ Corp/Desktop/NM\\ Fed/a.nwc","description":"x"}}' | run $P 2 "Bash names NM Fed with an escaped space" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"rm -rf /c/Users/x/Desktop/NM?Fed","description":"x"}}' | run $P 2 "Bash names NM Fed as a glob" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Remove-Item C:\\Users\\x\\Desktop\\NM` Fed","description":"x"}}' | run $P 2 "PowerShell names NM Fed with a backtick" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"rm -rf \"'"$NMU"'\"","description":"file_path"}}' | run $P 2 "a description reading file_path does not hide the command" "$repo"
echo '{"tool_name":"Monitor","tool_input":{"command":"rm -rf \"'"$NMU"'/a.nwc\"","description":"x","timeout_ms":1000}}' | run $P 2 "Monitor names NM Fed" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"powershell -ExecutionPolicy Bypass -File tools/loop/prepare-copy.ps1\nrm -rf \"'"$NMU"'\"","description":"x"}}' | run $P 2 "prepare-copy then a newline and a remove" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"powershell -ExecutionPolicy Bypass -File tools\\loop\\prepare-copy.ps1 -Listing (Remove-Item -Recurse \"'"$NM"'\")","description":"x"}}' | run $P 2 "prepare-copy with a subexpression" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"powershell -ExecutionPolicy Bypass -File tools\\loop\\prepare-copy.ps1 -Listing \"'"$NM"'\\listing.txt\"","description":"x"}}' | run $P 2 "prepare-copy with a listing into NM Fed" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"powershell -Command \"Remove-Item \"'"$NM"'\" #\" -File tools/loop/prepare-copy.ps1","description":"x"}}' | run $P 2 "a -Command before the -File" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"powershell -ExecutionPolicy Bypass -File tools\\loop\\prepare-copy.ps1 -Listing steps\\runs\\00\\source-listing.txt","description":"Refresh the NM Fed listing"}}' | run $P 0 "prepare-copy in its documented shape, description naming NM Fed" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"powershell -ExecutionPolicy Bypass -File tools\\loop\\prepare-copy.ps1 -Remove 1104-PAR-1C07BC-ZZZ-EL-MOD-000001.nwc","description":"x"}}' | run $P 0 "prepare-copy -Remove" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"gh pr create --draft --title F98 --body-file body.md","description":"Proved on the copy of NM Fed"}}' | run $P 0 "a pull request whose description names NM Fed" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Get-Content steps\\runs\\00\\nmfed-listing.txt","description":"read a listing"}}' | run $P 0 "a file name holding nmfed with no space" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"git status","description":"status"}}' | run $P 0 "Bash git status" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"dotnet build ParsonsNwcFederator.sln -c Release","description":"build"}}' | run $P 0 "PowerShell dotnet build" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"curl \"file:///C:/Users/p003653k/Desktop/NM%20Fed/x.nwc\"","description":"x"}}' | run $P 2 "NM Fed spelled as a URL" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"ls NM\" \"Fed","description":"peek"}}' | run $P 2 "NM Fed split by quotes, NM\" \"Fed" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"ls NM'"'"' '"'"'Fed","description":"peek"}}' | run $P 2 "NM Fed split by single quotes" "$repo"
echo '{"tool_name":"Bash","tool_input":{"command":"grep -r \"NM Federation\" docs","description":"search docs"}}' | run $P 0 "NM Federation is not NM Fed" "$repo"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Remove-Item (Join-Path $d (\"NM\" + \" Fed\"))","description":"x"}}' | run $P 0 "a name built at run time is NOT caught, the stated limit" "$repo"

echo "== the git wall, a throwaway clone with main checked out"
clone="$scratch/clone"
git clone -q --no-hardlinks "$repo" "$clone" 2>/dev/null
git -C "$clone" checkout -q main 2>/dev/null || git -C "$clone" checkout -q -b main origin/main
echo "clone on: $(git -C "$clone" rev-parse --abbrev-ref HEAD)"
echo '{"tool_name":"Bash","tool_input":{"command":"git commit -m x"}}' | run $G 2 "git commit on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin main"}}' | run $G 2 "git push origin main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git -C . commit -F msg.txt"}}' | run $G 2 "git -C . commit on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git -C \"C:/Users/p003653k/OneDrive - Parsons Corp/Documents/GitHub/PAR_NWC-Federator\" commit -F msg.txt","description":"Commit"}}' | run $G 2 "git -C with a quoted path holding spaces, commit on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git -c user.name=\"Bader Rahal\" commit -m x"}}' | run $G 2 "git -c with a quoted value, commit on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git '"'"'push'"'"' origin main"}}' | run $G 2 "git with the verb quoted, push main" "$clone"
echo '{"tool_name":"PowerShell","tool_input":{"command":"git commit -F msg.txt"}}' | run $G 2 "PowerShell git commit on main, F99" "$clone"
echo '{"tool_name":"PowerShell","tool_input":{"command":"git.exe push -u origin main"}}' | run $G 2 "PowerShell git.exe push on main, F99" "$clone"
echo '{"tool_name":"PowerShell","tool_input":{"command":"& \"C:\\Users\\p003653k\\Tools\\PortableGit\\cmd\\git.exe\" commit -m x"}}' | run $G 2 "PowerShell quoted path to git.exe commit on main, F99" "$clone"
echo '{"tool_name":"Monitor","tool_input":{"command":"git commit -m x","description":"x"}}' | run $G 2 "Monitor git commit on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git merge --no-ff fix-F97","description":"Merge"}}' | run $G 2 "git merge that is not a fast forward, on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git cherry-pick abc123","description":"x"}}' | run $G 2 "git cherry-pick on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git revert abc123","description":"x"}}' | run $G 2 "git revert on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git merge --ff-only origin/main","description":"x"}}' | run $G 0 "git merge --ff-only on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git pull --ff-only","description":"x"}}' | run $G 0 "git pull --ff-only on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin --delete fix-F97","description":"Delete the merged branch"}}' | run $G 0 "git push that only deletes a fix branch, on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git stash push -m x","description":"x"}}' | run $G 0 "git stash push on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git config push.default simple","description":"x"}}' | run $G 0 "git config push.default on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git status","description":"Check git state before commit"}}' | run $G 0 "git status with a description naming commit, on main" "$clone"
echo '{"tool_name":"PowerShell","tool_input":{"command":"Write-Output commitment"}}' | run $G 0 "a word with commit in it, on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git pull --ff-only && git merge --no-ff fix-F97","description":"x"}}' | run $G 2 "a fast forward pull then a real merge, on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git log --grep push","description":"x"}}' | run $G 0 "git log naming push, on main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git commit -m \"a; git push origin main\"","description":"x"}}' | run $G 2 "a commit on main whose message names a push" "$clone"
git -C "$clone" checkout -q -b fix-F999-hook-proof
echo "clone on: $(git -C "$clone" rev-parse --abbrev-ref HEAD)"
echo '{"tool_name":"Bash","tool_input":{"command":"git commit -F msg.txt"}}' | run $G 0 "git commit on a fix branch" "$clone"
echo '{"tool_name":"PowerShell","tool_input":{"command":"git push -u origin fix-F999-hook-proof"}}' | run $G 0 "git push of the fix branch" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin HEAD:main","description":"Push"}}' | run $G 2 "git push HEAD:main from a fix branch" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin main","description":"Push"}}' | run $G 2 "git push origin main from a fix branch" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout main && git commit -am x","description":"Commit"}}' | run $G 2 "switch to main and commit in one command" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout main && git merge --ff-only origin/main","description":"x"}}' | run $G 0 "switch to main and fast forward it" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin main\n","description":"Push"}}' | run $G 2 "git push origin main ending in a newline" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout main\ngit commit -F msg.txt","description":"tidy up"}}' | run $G 2 "switch to main and commit on two lines" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout \"main\" && git commit -F msg.txt","description":"x"}}' | run $G 2 "switch to a quoted main and commit" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin HEAD:main; git status","description":"x"}}' | run $G 2 "push HEAD:main followed by a semicolon" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin '"'"'HEAD:main'"'"'","description":"x"}}' | run $G 2 "push a quoted HEAD:main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push -u origin fix-F97:main","description":"x"}}' | run $G 2 "push the fix branch onto main by refspec" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin HEAD:main && git branch -d fix-F97","description":"x"}}' | run $G 2 "push HEAD:main with a -d later in the command" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout main && git pull --ff-only && git merge --no-ff fix-F97","description":"x"}}' | run $G 2 "switch to main, fast forward, then a real merge" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push -u origin fix-F100 && gh pr create --draft --base main --body-file body.md","description":"x"}}' | run $G 0 "push the fix branch then open a pull request onto main" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push --all origin","description":"x"}}' | run $G 2 "push --all takes main with it" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git push origin --delete main","description":"x"}}' | run $G 2 "push that deletes main, from a fix branch" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git checkout - && git commit -F msg.txt","description":"x"}}' | run $G 2 "switch to the branch before, which may be main, and commit" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git branch -d fix-F97 && git fetch origin","description":"x"}}' | run $G 0 "delete a local branch and fetch" "$clone"
echo '{"tool_name":"Bash","tool_input":{"command":"git commit -m \"a; git push origin main\"","description":"x"}}' | run $G 0 "a commit on a fix branch whose message names a push to main" "$clone"

echo "== the Stop gate, a throwaway project folder with no git"
# F139: the gate reads steps/PROGRESS.md. Every case of the page's STATE line, and the send back
# once per change of the page while it reads OPEN, in a folder where no merge can be read.
# LOCALAPPDATA reaches the gate spelt the Windows way, a drive and backslashes, as Claude Code
# hands it to a hook on this machine, so the glob over its runs folder is proved in that spelling.
# lappu is the same folder spelt for this script. With no cygpath, as in a container, both are
# the POSIX path.
lappu="$scratch/local"
mkdir -p "$lappu"
lapp=$(cygpath -w "$lappu" 2>/dev/null) || lapp=
[ -n "$lapp" ] || lapp=$lappu
echo "LOCALAPPDATA as the gate reads it: $lapp"
proj="$scratch/proj"
mkdir -p "$proj/steps" "$proj/.claude/hooks"
A='{"session_id":"A","hook_event_name":"Stop","stop_hook_active":false}'
A2='{"session_id":"A","hook_event_name":"Stop","stop_hook_active":true}'
B='{"session_id":"B","hook_event_name":"Stop","stop_hook_active":false}'
NC="The loop is not closed"
echo "$A" | gate allow "no steps/PROGRESS.md" "$proj"
printf '# a page\n\nno state line here\n' > "$proj/steps/PROGRESS.md"
echo "$A" | gate allow "steps/PROGRESS.md with no STATE line" "$proj"
printf 'STATE OPEN, 2026-10-06 16:00\n\n## Now\n- a lane\n' > "$proj/steps/PROGRESS.md"
echo "$A" | gate block "OPEN, 2026-10-06 16:00, the page's own shape, the first stop of session A" "$proj" "$NC"
echo "$A2" | gate allow "OPEN, session A again, the page unchanged" "$proj"
echo "$B" | gate block "OPEN, the first stop of session B, the same page" "$proj" "$NC"
sleep 1; printf 'STATE OPEN, 2026-10-06 16:05\n\n## Now\n- a lane, one item done\n' > "$proj/steps/PROGRESS.md"
echo "$B" | gate block "OPEN, session B, the page rewritten since" "$proj" "$NC"
echo "$B" | gate allow "OPEN, session B once more, nothing changed" "$proj"
for s in "WAITING" "RESTART" "CLOSED" "NEXT WAVE"; do
    printf 'STATE %s, 2026-10-06 16:10\n\n## Now\n- a lane\n' "$s" > "$proj/steps/PROGRESS.md"
    echo "$A" | gate allow "$s" "$proj"
done
printf 'STATE OPENING\n' > "$proj/steps/PROGRESS.md"
echo "$A" | gate allow "a word that only starts with OPEN" "$proj"
printf '# a page\n\n**STATE** OPEN\n' > "$proj/steps/PROGRESS.md"
echo '{"session_id":"C","hook_event_name":"Stop"}' | gate block "**STATE** OPEN, markdown emphasis" "$proj" "$NC"
printf '# a page\n\nSTATE: Open\n' > "$proj/steps/PROGRESS.md"
echo '{"session_id":"D","hook_event_name":"Stop"}' | gate block "STATE: Open, a colon and mixed case" "$proj" "$NC"
printf 'STATE OPEN, 2026-10-06 16:20\r\n\r\n## Now\r\n' > "$proj/steps/PROGRESS.md"
echo '{"session_id":"E","hook_event_name":"Stop"}' | gate block "STATE OPEN with Windows line endings" "$proj" "$NC"
printf 'STATE 2026-10-06 OPEN\n' > "$proj/steps/PROGRESS.md"
echo '{"session_id":"E2","hook_event_name":"Stop"}' | gate allow "a date before the word reads no state" "$proj"
printf 'STATE OPEN, 2026-10-06 16:30\n\n## Now\n- a lane, turn 2\n' > "$proj/steps/PROGRESS.md"
echo '{"session_id":"G","hook_event_name":"Stop"}' | gate block "OPEN, session G first" "$proj" "$NC"
echo '{"session_id":"H","hook_event_name":"Stop"}' | gate block "OPEN, session H first, the same page" "$proj" "$NC"
echo '{"session_id":"G","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, session G again after H, nothing changed" "$proj"
rm -f "$proj/.claude/hooks/.loop-gate-F"; mkdir "$proj/.claude/hooks/.loop-gate-F"
echo '{"session_id":"F","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, the note cannot be written, never blocks" "$proj"
echo '{"session_id":"F","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, the note still cannot be written" "$proj"
printf 'STATE OPEN, 2026-10-06 16:40\n' > "$proj/steps/loop.md"
rm -f "$proj/steps/PROGRESS.md"
echo '{"session_id":"L","hook_event_name":"Stop"}' | gate allow "no PROGRESS.md, an old steps/loop.md reading OPEN is not read" "$proj"

echo "== the Stop gate, the lead's flow, a clone of a throwaway origin checked out as the lead does"
# F139, attempt 2. A merge on GitHub is made in a throwaway bare origin by git's plumbing, which
# runs no hook. The clone fetches it and checks out origin/main detached, as the lead does after
# every merge, with core.autocrlf true as Git for Windows sets it on this machine, so a checkout
# that changes the page writes it with CRLF, as the working copy of the main clone holds it. The
# lead's rewrite is the page written on a records branch with CRLF, staged with git add,
# committed by write-tree and commit-tree, fetched into the origin and merged there. The merge
# test reads the STATE line of the page in the working copy against the one at the first parent
# of origin/main, so what a checkout does to the page's file time does not move it.
og="$scratch/origin.git"
git init -q --bare "$og"
repo2="$scratch/repo2"
git init -q "$repo2"
git -C "$repo2" config core.autocrlf true
git -C "$repo2" remote add origin "$og"
mkdir -p "$repo2/.claude/hooks"
cr=$(printf '\r')
who() {
    GIT_AUTHOR_NAME=proof GIT_AUTHOR_EMAIL=proof@example.invalid GIT_COMMITTER_NAME=proof GIT_COMMITTER_EMAIL=proof@example.invalid "$@"
}
tree_of() {
    # $1 the page, $2 a file of code. Prints a tree of the origin holding steps/PROGRESS.md and code.txt.
    pb=$(printf '%s' "$1" | git --git-dir="$og" hash-object -w --stdin)
    cb=$(printf '%s' "$2" | git --git-dir="$og" hash-object -w --stdin)
    st=$(printf '100644 blob %s\tPROGRESS.md\n' "$pb" | git --git-dir="$og" mktree)
    printf '040000 tree %s\tsteps\n100644 blob %s\tcode.txt\n' "$st" "$cb" | git --git-dir="$og" mktree
}
commit() {
    # $1 a tree, then the parents. Prints the new commit of the origin.
    tree=$1
    shift
    ps=
    for p in "$@"; do ps="$ps -p $p"; done
    who git --git-dir="$og" commit-tree "$tree" $ps -m "a commit of the proof"
}
merge_on_github() {
    # $1 the branch commit. A merge commit on the origin's main, its first parent main, holding
    # the branch's tree, as GitHub makes it for a branch that has main in. Prints it.
    commit "$(git --git-dir="$og" rev-parse "$1^{tree}")" "$(git --git-dir="$og" rev-parse main)" "$1"
}
land() {
    # main of the origin moves to $1, then the clone fetches and checks out origin/main detached.
    git --git-dir="$og" update-ref refs/heads/main "$1"
    git -C "$repo2" fetch -q origin
    git -C "$repo2" checkout -q --detach origin/main
}
rewrite() {
    # The lead's rewrite: the page $1 written with CRLF line ends on a new branch $2 of the clone,
    # staged, committed by plumbing and fetched into the origin. Prints the commit.
    git -C "$repo2" checkout -q -b "$2"
    printf '%s' "$1" | sed "s/\$/$cr/" > "$repo2/steps/PROGRESS.md"
    git -C "$repo2" add steps/PROGRESS.md
    rt=$(git -C "$repo2" write-tree)
    rc=$(who git -C "$repo2" commit-tree "$rt" -p HEAD -m "the lead's records")
    git -C "$repo2" update-ref "refs/heads/$2" "$rc"
    git --git-dir="$og" fetch -q "$repo2" "refs/heads/$2:refs/heads/$2"
    printf '%s\n' "$rc"
}
crlf() {
    # $1 the page, $2 the case. A checked case: every line of the page ends in CRLF, as a checkout
    # writes it with core.autocrlf true. Git for Windows' grep drops a carriage return before it
    # matches unless given -U, measured on 2026-10-07, so both counts read with -U.
    all=$(grep -U -c '' "$1")
    with=$(grep -U -c "$cr" "$1")
    if [ "$with" -gt 0 ] && [ "$with" = "$all" ]; then v=ok; else v=WRONG; fi
    printf '%-5s %s of %s lines end in CRLF  %s\n' "$v" "$with" "$all" "$2" | tee -a "$out"
}
looks() {
    # How the page in the working copy reads: its carriage returns, and whether its file time is
    # newer than the reflog of origin/main, which is what the gate before attempt 2 read.
    n_cr=$(grep -U -c "$cr" "$repo2/steps/PROGRESS.md")
    if [ "$repo2/steps/PROGRESS.md" -nt "$repo2/.git/logs/refs/remotes/origin/main" ]; then nt=newer; else nt="not newer"; fi
    echo "    the page: $n_cr lines ending in CRLF, its file time $nt than the last fetch of main, its STATE line: $(head -n 1 "$repo2/steps/PROGRESS.md" | tr -d '\r')"
}
P0='STATE OPEN, 2026-10-06 10:00
<!-- counts -->
| 1 | 3 |
<!-- end -->
'
P1='STATE OPEN, 2026-10-06 11:00
<!-- counts -->
| 1 | 3 |
<!-- end -->
'
P1C='STATE OPEN, 2026-10-06 11:00
<!-- counts -->
| 1 | 4 |
<!-- end -->
'
P2='STATE OPEN, 2026-10-06 12:00
<!-- counts -->
| 1 | 4 |
<!-- end -->
'
P3='STATE OPEN, 2026-10-06 13:00
<!-- counts -->
| 1 | 4 |
<!-- end -->
'
S='{"session_id":"S","hook_event_name":"Stop"}'
ML="STATE line"
c0=$(commit "$(tree_of "$P0" "code 0")")
land "$c0"
echo "origin/main at $(git -C "$repo2" rev-parse --short origin/main), the clone detached at $(git -C "$repo2" rev-parse --short HEAD)"
looks
echo "$S" | gate block "the page new on main, whose head has no parent, sent back once" "$repo2" "$NC"
echo "$S" | gate allow "the same, let through, a head with no parent is no merge to read" "$repo2"
r1=$(rewrite "$P1" records-1)
looks
echo "$S" | gate block "the lead's rewrite on a records branch, not merged yet, sent back once" "$repo2" "$NC"
echo "$S" | gate allow "the same, let through" "$repo2"
sleep 1; land "$(merge_on_github "$r1")"
echo "a records merge carrying the rewrite, fetched and checked out"
looks
echo "$S" | gate allow "a records merge carrying the rewrite, fetched and checked out, let through" "$repo2"
fx=$(commit "$(tree_of "$P1C" "code 1, a fix that changed a row")" "$(git --git-dir="$og" rev-parse main)")
sleep 1; land "$(merge_on_github "$fx")"
echo "a fix merge that changed the counts, fetched, then origin/main checked out, which writes the page"
looks
crlf "$repo2/steps/PROGRESS.md" "the page as the checkout of the fix merge wrote it"
echo "$S" | gate block "a fix merge that changed the counts, the page written by the checkout after the fetch, refused" "$repo2" "$ML"
echo "$S" | gate block "the same, every stop refused until the lead rewrites" "$repo2" "$ML"
r2=$(rewrite "$P2" records-2)
looks
echo "$S" | gate block "the lead rewrites after the merge, sent back once for the change" "$repo2" "$NC"
echo "$S" | gate allow "the lead rewrites after the merge, let through" "$repo2"
sleep 1; land "$(merge_on_github "$r2")"
looks
echo "$S" | gate allow "the merge carrying the rewrite, fetched and checked out, let through" "$repo2"
fy=$(commit "$(tree_of "$P2" "code 2, a fix that left the page alone")" "$(git --git-dir="$og" rev-parse main)")
sleep 1; land "$(merge_on_github "$fy")"
echo "a fix merge that left the page alone, fetched and checked out"
looks
echo "$S" | gate block "a fix merge that left the page alone, refused until the lead rewrites" "$repo2" "$ML"
sleep 1; printf '%s' "$P3" > "$repo2/steps/PROGRESS.md"
echo "$S" | gate block "the page rewritten with LF line ends, as a script writes it, sent back once" "$repo2" "$NC"
echo "$S" | gate allow "the same, let through" "$repo2"

echo "== the Stop gate, the runs, LOCALAPPDATA spelt the Windows way"
# F139, attempt 2. The page names on its STATE line the run it was last rewritten after. A run
# whose record.txt holds a VERDICT line and is newer than the named run's record refuses every
# stop until the page names it. A checkout or the maker writing the page moves the page's time,
# never a record's, so what the page says is the test.
runs="$lappu/NwcFederatorLoop/runs"
mkdir -p "$runs/05/item1" "$runs/05/item2" "$runs/06/item1"
sleep 1; printf 'a line of the run\r\nVERDICT: RAN\r\n' > "$runs/05/item1/record.txt"
echo "$S" | gate block "a run that finished, which the page does not name, refused naming it" "$repo2" "05/item1"
echo "$S" | gate block "the same, every stop refused" "$repo2" "05/item1"
sleep 1; printf 'STATE OPEN, 2026-10-06 13:00\n<!-- counts -->\n| 1 | 5 |\n<!-- end -->\n' > "$repo2/steps/PROGRESS.md"
echo "    the page written again after the run, as the maker writes new counts, its STATE line as it was"
echo "$S" | gate block "the page newer than the run's record, still not naming the run, refused" "$repo2" "05/item1"
sleep 1; printf 'STATE OPEN, 2026-10-06 14:00, last run 05/item1\n<!-- counts -->\n| 1 | 5 |\n<!-- end -->\n' > "$repo2/steps/PROGRESS.md"
echo "$S" | gate block "the page naming the run on its STATE line, sent back once for the change" "$repo2" "$NC"
echo "$S" | gate allow "the page naming the run, let through" "$repo2"
sleep 1; printf 'a line of a run still going\r\n' > "$runs/05/item2/record.txt"
echo "$S" | gate allow "a run still going, its record.txt with no VERDICT line, newer than the named run" "$repo2"
sleep 1; printf 'VERDICT: STOPPED\r\n' >> "$runs/05/item2/record.txt"
echo "$S" | gate block "that run finished, the page naming the run before it, refused naming the new one" "$repo2" "05/item2"
sleep 1; printf 'STATE OPEN, 2026-10-06 15:00, last run 05\\item2\r\n<!-- counts -->\r\n| 1 | 5 |\r\n<!-- end -->\r\n' > "$repo2/steps/PROGRESS.md"
echo "$S" | gate block "the page naming it with a backslash and CRLF line ends, sent back once" "$repo2" "$NC"
echo "$S" | gate allow "the same, let through" "$repo2"
sleep 1; printf 'a line\r\nVERDICT: RAN\r\n' > "$runs/06/item1/record.txt"
sleep 1; printf 'STATE OPEN, 2026-10-06 16:00, last run 06/item10\n<!-- counts -->\n| 1 | 5 |\n<!-- end -->\n' > "$repo2/steps/PROGRESS.md"
echo "$S" | gate block "a page naming 06/item10, written after 06/item1 finished, does not name 06/item1, refused" "$repo2" "06/item1 under"
sleep 1; printf 'STATE OPEN, 2026-10-06 16:05, last run 06/item1.\n<!-- counts -->\n| 1 | 5 |\n<!-- end -->\n' > "$repo2/steps/PROGRESS.md"
echo "$S" | gate block "the page naming 06/item1 at the end of a sentence, sent back once" "$repo2" "$NC"
echo "$S" | gate allow "the same, let through" "$repo2"
sleep 1; printf 'a line\r\nVERDICT: RAN\r\n' > "$runs/05/item1/record.txt"
sleep 1; printf 'a line of a run still going, the newest of all\r\n' > "$runs/05/item2/record.txt"
echo "$S" | gate block "the newest run still going, and a run newer than the named one finished before it, refused naming the finished one" "$repo2" "05/item1 under"
printf 'STATE NEXT WAVE, 2026-10-06 17:00\n' > "$repo2/steps/PROGRESS.md"
echo "$S" | gate allow "NEXT WAVE, naming no run, let through" "$repo2"

echo "== the Stop gate, a worktree session"
# The worktree's .git is a file naming its folder under the clone's .git, whose commondir names
# that .git, so the reflog and git's answer are the clone's. Main gets a records merge carrying
# the page of 13:00 and then a fix merge that leaves the page alone.
rc3=$(commit "$(tree_of "$P3" "code 2, a fix that left the page alone")" "$(git --git-dir="$og" rev-parse main)")
git --git-dir="$og" update-ref refs/heads/main "$(merge_on_github "$rc3")"
fz=$(commit "$(tree_of "$P3" "code 3, a fix that left the page alone")" "$(git --git-dir="$og" rev-parse main)")
git --git-dir="$og" update-ref refs/heads/main "$(merge_on_github "$fz")"
git -C "$repo2" fetch -q origin
wt="$scratch/wt"
git -C "$repo2" worktree add -q --detach "$wt" origin/main
mkdir -p "$wt/.claude/hooks"
echo "the worktree's .git reads: $(cat "$wt/.git")"
echo "    its page as checked out, its STATE line: $(head -n 1 "$wt/steps/PROGRESS.md" | tr -d '\r')"
crlf "$wt/steps/PROGRESS.md" "the worktree's page as checked out"
W='{"session_id":"W","hook_event_name":"Stop"}'
echo "$W" | gate block "a worktree session, its page as checked out holding the STATE line main held before its head, read through its .git file, refused" "$wt" "$ML"
sleep 1; printf 'STATE OPEN, 2026-10-06 18:00, last run 05/item1\n<!-- counts -->\n| 1 | 4 |\n<!-- end -->\n' > "$wt/steps/PROGRESS.md"
echo "$W" | gate block "a worktree session, its page rewritten, naming the last finished run, sent back once" "$wt" "$NC"
echo "$W" | gate allow "the same, let through" "$wt"
rm -f "$wt/steps/PROGRESS.md"
echo "$W" | gate allow "a worktree session with no page" "$wt"

echo
echo "cases right: $(grep -c '^ok' "$out"), cases wrong: $(grep -c '^WRONG' "$out")"
rm -rf "$scratch"
