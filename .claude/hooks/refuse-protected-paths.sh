#!/bin/sh
# PreToolUse hook for Edit, Write, MultiEdit, NotebookEdit, Bash and PowerShell. Reads the
# tool call off standard input. Exit 2 refuses the call and the line on standard error
# goes back to Claude.
#
# Two walls.
#
# 1. A file tool never changes a file under samples, steps/logs or bundle. The first two
#    are evidence and the third is the one manifest the install reads.
#
# 2. NM Fed and every ACC Desktop Connector folder are read only, so a file tool never
#    writes under either, and a command tool never runs a command that names either. NM Fed
#    is the folder of real NWC files on Bader's desktop that the loop copies and runs
#    against. Desktop Connector keeps ACC projects under DC\ACCDocs on this machine,
#    measured on 2026-09-27, and under DC\Autodesk Docs or DC\BIM 360 on older installs.
#    The one command allowed to name NM Fed is a single run of tools/loop/prepare-copy.ps1,
#    which is the only thing that reads it. A command tool's whole input is searched, the
#    description included, because a wall that parses shell can be walked around.
#
# WHY IT IS WRITTEN WITH BUILTINS. Measured on 2026-09-27 on Bader's machine: starting sh
# costs 2.3 to 2.9 seconds and every pipe through grep, awk or sed about two more, so the
# first version of this wall, which piped every call through them, took 11.9 seconds a
# call, on every command and every edit. The call is read with read and matched with case,
# and a program is started only for a call already on its way to being refused.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

names_live_folder() {
    case "$1" in
        *[Nn][Mm]" "[Ff][Ee][Dd]*|*[Nn][Mm]%20[Ff][Ee][Dd]*) return 0 ;;
        *[\\/][Aa][Cc][Cc][Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][Aa][Uu][Tt][Oo][Dd][Ee][Ss][Kk]" "[Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][\\/][Aa][Uu][Tt][Oo][Dd][Ee][Ss][Kk]" "[Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][Bb][Ii][Mm]" 360"*|*[\\/][Dd][Cc][\\/][\\/][Bb][Ii][Mm]" 360"*) return 0 ;;
    esac
    return 1
}

# A file tool carries file_path, or notebook_path for NotebookEdit, and a command tool
# carries neither. Telling them apart by the key rather than by tool_name keeps this from
# depending on how the JSON is spaced. Inside a string a quote arrives escaped, so a
# command or a file's content that merely mentions file_path cannot look like the key.
case "$input" in
    *'"file_path"'*|*'"notebook_path"'*) ;;
    *)
        names_live_folder "$input" || exit 0

        # On its way to a refusal. The one exception is a single run of prepare-copy.ps1,
        # so read the command out of the call, escapes kept, and look at it alone.
        command=$(printf '%s' "$input" | awk '
            { buf = buf $0 "\n" }
            END {
                i = index(buf, "\"command\""); if (!i) exit
                rest = substr(buf, i + 9)
                if (!match(rest, /^[ \t\r\n]*:[ \t\r\n]*"/)) exit
                rest = substr(rest, RLENGTH + 1)
                out = ""; esc = 0
                for (k = 1; k <= length(rest); k++) {
                    c = substr(rest, k, 1)
                    if (esc) { out = out "\\" c; esc = 0; continue }
                    if (c == "\\") { esc = 1; continue }
                    if (c == "\"") break
                    out = out c
                }
                printf "%s", out
            }')
        case "$command" in
            *\;*|*\&*|*\|*|*\$\(*|*\`*) ;;
            *prepare-copy.ps1*)
                if printf '%s' "$command" | grep -Eqi '^[[:space:]]*(powershell|powershell\.exe|pwsh)[[:space:]].*-file[[:space:]]+[\\"]*tools[\\/]+loop[\\/]+prepare-copy\.ps1'; then
                    exit 0
                fi
                ;;
        esac
        echo "Refused. This command names NM Fed or an ACC Desktop Connector folder, which the loop never touches. Only a single run of tools/loop/prepare-copy.ps1 reads NM Fed. See .claude/rules/loop.md." >&2
        exit 2
        ;;
esac

# A file tool. The path is the value of file_path, or notebook_path for NotebookEdit. The
# content is never searched, so a file may talk about NM Fed and still be written.
rest=${input#*\"file_path\"}
if [ "$rest" = "$input" ]; then
    rest=${input#*\"notebook_path\"}
fi
if [ "$rest" = "$input" ]; then
    exit 0
fi
rest=${rest#*\"}
path=${rest%%\"*}

if [ -z "$path" ]; then
    exit 0
fi

if names_live_folder "$path"; then
    echo "Refused. $path is under NM Fed or an ACC Desktop Connector folder, which the loop never writes to. See .claude/rules/loop.md." >&2
    exit 2
fi

# JSON doubles a backslash, so a Windows path arrives with two between each folder.
case "$path" in
    *[\\/]samples[\\/]*|*[\\/]bundle[\\/]*|*[\\/]steps[\\/]logs[\\/]*|*[\\/]steps[\\/][\\/]logs[\\/]*)
        echo "Refused. $path is under samples, steps/logs or bundle, which are never edited. See CLAUDE.md." >&2
        exit 2
        ;;
esac

exit 0
