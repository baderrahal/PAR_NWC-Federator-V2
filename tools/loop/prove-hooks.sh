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
    # $1 block or allow, $2 case name, $3 project dir. A Stop hook exits 0 either way.
    json=$(cat)
    o=$(printf '%s' "$json" | CLAUDE_PROJECT_DIR="$3" sh "$hooks/loop-gate.sh" 2>/dev/null)
    code=$?
    case "$o" in *'"decision": "block"'*) got=block ;; *) got=allow ;; esac
    if [ "$got" = "$1" ] && [ "$code" = 0 ]; then v=ok; else v=WRONG; fi
    printf '%-5s %s, exit %s (want %s)  %s\n' "$v" "$got" "$code" "$1" "$2" | tee -a "$out"
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
echo '{"tool_name":"Write","tool_input":{"file_path":"'"$repo"'/steps/loop.md","content":"NM Fed is copied by prepare-copy, file_path"}}' | run $P 0 "Write to steps/loop.md, content naming NM Fed" "$repo"
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
echo '{"tool_name":"Bash","tool_input":{"command":"git commit -m \"a; git push origin main\"","description":"x"}}' | run $G 0 "a commit on a fix branch whose message names a push to main" "$clone"

echo "== the Stop gate, a throwaway project folder"
proj="$scratch/proj"
mkdir -p "$proj/steps" "$proj/.claude/hooks"
A='{"session_id":"A","hook_event_name":"Stop","stop_hook_active":false}'
A2='{"session_id":"A","hook_event_name":"Stop","stop_hook_active":true}'
B='{"session_id":"B","hook_event_name":"Stop","stop_hook_active":false}'
echo "$A" | gate allow "no steps/loop.md" "$proj"
printf '# The loop\n\nno state line here\n' > "$proj/steps/loop.md"
echo "$A" | gate allow "steps/loop.md with no STATE line" "$proj"
printf '# The loop\n\nSTATE OPEN\n\nturn 1\n' > "$proj/steps/loop.md"
echo "$A" | gate block "OPEN, the first stop of session A" "$proj"
echo "$A2" | gate allow "OPEN, session A again, file unchanged" "$proj"
echo "$B" | gate block "OPEN, the first stop of session B, same file" "$proj"
sleep 2; printf '# The loop\n\nSTATE OPEN\n\nturn 1, one item done\n' > "$proj/steps/loop.md"
echo "$B" | gate block "OPEN, session B, the file changed since" "$proj"
echo "$B" | gate allow "OPEN, session B once more, nothing changed" "$proj"
for s in WAITING RESTART CLOSED; do
    printf '# The loop\n\nSTATE %s\n' "$s" > "$proj/steps/loop.md"
    echo "$A" | gate allow "$s" "$proj"
done
printf '# The loop\n\nSTATE OPENING\n' > "$proj/steps/loop.md"
echo "$A" | gate allow "a word that only starts with OPEN" "$proj"
printf '# The loop\n\n**STATE** OPEN\n' > "$proj/steps/loop.md"
echo '{"session_id":"C","hook_event_name":"Stop"}' | gate block "**STATE** OPEN, markdown emphasis" "$proj"
printf '# The loop\n\nSTATE: Open\n' > "$proj/steps/loop.md"
echo '{"session_id":"D","hook_event_name":"Stop"}' | gate block "STATE: Open, a colon and mixed case" "$proj"
printf '# The loop\r\n\r\nSTATE OPEN\r\n' > "$proj/steps/loop.md"
echo '{"session_id":"E","hook_event_name":"Stop"}' | gate block "STATE OPEN with Windows line endings" "$proj"
printf '# The loop\n\nSTATE OPEN\n\nturn 2\n' > "$proj/steps/loop.md"
echo '{"session_id":"G","hook_event_name":"Stop"}' | gate block "OPEN, session G first" "$proj"
echo '{"session_id":"H","hook_event_name":"Stop"}' | gate block "OPEN, session H first, same file" "$proj"
echo '{"session_id":"G","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, session G again after H, nothing changed" "$proj"
rm -f "$proj/.claude/hooks/.loop-gate-last"; mkdir "$proj/.claude/hooks/.loop-gate-last"
echo '{"session_id":"F","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, the note cannot be written, never blocks" "$proj"
echo '{"session_id":"F","hook_event_name":"Stop","stop_hook_active":true}' | gate allow "OPEN, the note still cannot be written" "$proj"

echo
echo "cases right: $(grep -c '^ok' "$out"), cases wrong: $(grep -c '^WRONG' "$out")"
rm -rf "$scratch"
