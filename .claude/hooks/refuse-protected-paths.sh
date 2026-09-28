#!/bin/sh
# PreToolUse hook for Edit, Write, MultiEdit, NotebookEdit, Bash, PowerShell and Monitor.
# Reads the tool call off standard input. Exit 2 refuses the call and the line on standard
# error goes back to Claude.
#
# Two walls.
#
# 1. A file tool never changes a file under samples, steps/logs, steps/runs or bundle, in
#    any case of those names and whether the path is whole or relative. The first three
#    are evidence and the last is the one manifest the install reads. Nor .claude/hooks
#    or .claude/settings, which are these walls, so a file tool cannot switch them off.
#
# 2. NM Fed and every ACC Desktop Connector folder are read only for the loop, so a file
#    tool never writes under either and a command tool never runs a command that names
#    either. NM Fed is the folder of real NWC files on Bader's desktop that the loop copies
#    and runs against. Desktop Connector keeps ACC projects under DC\ACCDocs on this
#    machine, measured on 2026-09-27, and under DC\Autodesk Docs or DC\BIM 360 on older
#    installs. NO command may name NM Fed, not even a run of tools/loop/prepare-copy.ps1,
#    the one thing that reads it, because that script finds the folder itself off the
#    desktop and never needs it named. The first version let one run of it name NM Fed,
#    and every way the breaker found round this wall went through that door, so there is
#    no door.
#
# WHAT THIS WALL IS AND IS NOT. It reads the WORDS of a call. It catches NM Fed written with
# a space, as %20, with a bash backslash, a PowerShell backtick or a one character glob, and
# as its short name NMFED~1, and ACCDocs anywhere. It cannot catch a script that builds the
# path at run time, which is why prepare-copy.ps1 is the only script that does, it refuses
# to write anywhere but the loop's work folder and a listing inside the repo, and every
# agent is told so in .claude/rules/loop.md. It is a wall against a mistake, not a sandbox.
#
# WHY IT IS WRITTEN WITH BUILTINS. Measured on 2026-09-27 on Bader's machine: starting sh
# costs 2.3 to 2.9 seconds and every pipe through grep, awk or sed about two more, so the
# first version, which piped every call through them, took 11.9 seconds a call. The call is
# read with read and matched with case, and a program is started only for a call already
# on its way to a refusal.
#
# Rewritten on 2026-09-27 after the Phase 0 reviewer and breaker. The exception let a second
# command through after a newline, a redirect or a subexpression. A description reading
# file_path sent a command down the file branch. A relative path and a short name got past.
# A description is no longer searched, so a pull request or a commit that talks about NM Fed
# is not refused for it.

input=
while IFS= read -r line || [ -n "$line" ]; do
    input="$input$line
"
done

refuse() {
    printf '%s\n' "Refused. $1" >&2
    exit 2
}

names_live_folder() {
    # NM, then one to six characters of anything, then Fed as a whole word. That is a
    # space, %20, a glob, a backslash or a backtick before the space, and a name split by
    # quotes such as NM" "Fed or NM' 'Fed, with the JSON escapes they arrive in. Fed must
    # end there, so NM Federation is not NM Fed. NM and Fed with nothing between are left
    # out on purpose, because the loop's own file names hold nmfed and it refused them.
    case "$1" in
        *[Nn][Mm]?[Ff][Ee][Dd]|*[Nn][Mm]?[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm]??[Ff][Ee][Dd]|*[Nn][Mm]??[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm]???[Ff][Ee][Dd]|*[Nn][Mm]???[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm]????[Ff][Ee][Dd]|*[Nn][Mm]????[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm]?????[Ff][Ee][Dd]|*[Nn][Mm]?????[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm]??????[Ff][Ee][Dd]|*[Nn][Mm]??????[Ff][Ee][Dd][!A-Za-z]*) return 0 ;;
        *[Nn][Mm][Ff][Ee][Dd]~*) return 0 ;;
        *[Aa][Cc][Cc][Dd][Oo][Cc][Ss]*|*[Aa][Cc][Cc][Dd][Oo]~*) return 0 ;;
        *[\\/][Dd][Cc][\\/][Aa][Uu][Tt][Oo][Dd][Ee][Ss][Kk]" "[Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][\\/][Aa][Uu][Tt][Oo][Dd][Ee][Ss][Kk]" "[Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][\\/][\\/][\\/][Aa][Uu][Tt][Oo][Dd][Ee][Ss][Kk]" "[Dd][Oo][Cc][Ss]*) return 0 ;;
        *[\\/][Dd][Cc][\\/][Bb][Ii][Mm]" 360"*|*[\\/][Dd][Cc][\\/][\\/][Bb][Ii][Mm]" 360"*) return 0 ;;
        *[\\/][Dd][Cc][\\/][\\/][\\/][\\/][Bb][Ii][Mm]" 360"*) return 0 ;;
    esac
    return 1
}

# The tool, read off the tool_name key. Claude Code sends it before tool_input, and a key
# is a quoted word followed by a colon, which no string value can fake, because a quote
# inside a value arrives escaped.
t=${input#*\"tool_name\":}
t=${t#*\"}
tool=${t%%\"*}

case "$tool" in
    Bash|PowerShell|Monitor)
        # What runs is the command. Claude Code sends the command before its description,
        # so everything before the description is searched and a description can neither
        # trip the wall nor hide a command from it.
        body=${input%%\"description\":*}
        case "$body" in *\"command\":*) ;; *) body=$input ;; esac
        names_live_folder "$body" || exit 0
        refuse "This command names NM Fed or an ACC Desktop Connector folder, which the loop never touches. Only tools/loop/prepare-copy.ps1 reads NM Fed, and it finds the folder itself, so its command never names it. See .claude/rules/loop.md."
        ;;
    Edit|Write|MultiEdit|NotebookEdit)
        ;;
    *)
        exit 0
        ;;
esac

# A file tool. The path is the value of the file_path key, or notebook_path. The content is
# never searched, so a file may talk about NM Fed and still be written.
p=${input#*\"file_path\":}
[ "$p" = "$input" ] && p=${input#*\"notebook_path\":}
[ "$p" = "$input" ] && exit 0
p=${p#*\"}
path=${p%%\"*}
[ -z "$path" ] && exit 0

# A path is cut at the first quote. JSON writes a backslash as two, so a cut value ending
# in an ODD number of backslashes ended on an escaped quote, and a path with a quote in it
# is not a Windows path, so it is refused rather than half read. An even number is a path
# that really ends in a backslash, and it is read as it is.
tail=$path
while :; do
    case "$tail" in
        *'\\') tail=${tail%??} ;;
        *) break ;;
    esac
done
case "$tail" in
    *'\') refuse "The path in this call holds a quote, so it cannot be read whole. See .claude/rules/loop.md." ;;
esac

# A short name such as NMFED~1 or ONEDRI~1 is read in full before it is judged. cygpath
# ships with Git for Windows, and it runs only for a path carrying a short name.
case "$path" in
    *~[0-9]*)
        long=$(cygpath -l -w "$(printf '%s' "$path" | sed 's#\\\\#\\#g')" 2>/dev/null)
        [ -n "$long" ] && names_live_folder "$long" && refuse "$long is under NM Fed or an ACC Desktop Connector folder, which the loop never writes to. See .claude/rules/loop.md."
        ;;
esac

if names_live_folder "$path"; then
    refuse "$path is under NM Fed or an ACC Desktop Connector folder, which the loop never writes to. See .claude/rules/loop.md."
fi

# JSON doubles a backslash, so a Windows path arrives with two between each folder, and a
# path typed with doubled backslashes arrives with four. The slash put in front lets a
# relative path such as samples/a.txt match too. The last branch is the walls themselves,
# so a file tool can never switch them off: a change to a hook or to the settings is
# written somewhere else, proved, and copied in by a command.
case "/$path" in
    *[\\/][Ss][Aa][Mm][Pp][Ll][Ee][Ss][\\/]*|*[\\/][Bb][Uu][Nn][Dd][Ll][Ee][\\/]*)
        refuse "$path is under samples or bundle, which are never edited. See CLAUDE.md." ;;
    *[\\/][Ss][Tt][Ee][Pp][Ss][\\/][Ll][Oo][Gg][Ss][\\/]*|*[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][Ll][Oo][Gg][Ss][\\/]*|\
    *[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][\\/][Ll][Oo][Gg][Ss][\\/]*|*[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][\\/][\\/][Ll][Oo][Gg][Ss][\\/]*)
        refuse "$path is under steps/logs, the run logs Bader sent back, which are never edited. See CLAUDE.md." ;;
    *[\\/][Ss][Tt][Ee][Pp][Ss][\\/][Rr][Uu][Nn][Ss][\\/]*|*[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][Rr][Uu][Nn][Ss][\\/]*|\
    *[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][\\/][Rr][Uu][Nn][Ss][\\/]*|*[\\/][Ss][Tt][Ee][Pp][Ss][\\/][\\/][\\/][\\/][Rr][Uu][Nn][Ss][\\/]*)
        refuse "$path is under steps/runs, the loop's run evidence, which the runner collects with a command and nothing edits. See .claude/rules/loop.md." ;;
    *[\\/].[Cc][Ll][Aa][Uu][Dd][Ee][\\/][Hh][Oo][Oo][Kk][Ss][\\/]*|*[\\/].[Cc][Ll][Aa][Uu][Dd][Ee][\\/][\\/][Hh][Oo][Oo][Kk][Ss][\\/]*|\
    *[\\/].[Cc][Ll][Aa][Uu][Dd][Ee][\\/][Ss][Ee][Tt][Tt][Ii][Nn][Gg][Ss]*|*[\\/].[Cc][Ll][Aa][Uu][Dd][Ee][\\/][\\/][Ss][Ee][Tt][Tt][Ii][Nn][Gg][Ss]*)
        refuse "$path is one of the walls, .claude/hooks or .claude/settings, which no file tool changes. Write the change elsewhere, prove it, and copy it in with a command. See .claude/rules/loop.md." ;;
esac

exit 0
