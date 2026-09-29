#!/bin/sh
#
# Refuses a text file that carries this machine's name or an Autodesk licensing id.
#
# WHY THIS EXISTS. F102, measured on 2026-09-29. A probe result written on Bader's machine
# was committed to a pushed branch with the machine's name on its first line and, on two
# lines, the command lines of two AdskLicensingAgent processes, which carry the licensing
# agent's ids. Every probe prints MACHINE and the computer name, and the start probe of
# F100 printed a new process's command line whole. Nothing between a result and a commit
# read the file for them. tools\loop\mask-evidence.ps1 writes a masked copy of a result,
# and this is what refuses a file that was never masked, in the pre-commit over what is
# staged and in Actions over the whole tree.
#
# WHAT IT REFUSES, three kinds, and one it cannot read.
#   an analytics agent id   analyticsagentid=analytics- followed by a hex digit
#   a licensing id after -i a line naming AdskLicensing that holds a GUID in the word
#                           after -i
#   the machine name        the value of COMPUTERNAME as a whole word in any case, only
#                           when COMPUTERNAME is set. It is not set in a Linux container,
#                           and the last line says so when it was not read for
#   a UTF-16 text file      a file that opens with a UTF-16 byte order mark, which grep
#                           reads as binary and so cannot read for the three above
#
# IT NEVER PRINTS WHAT IT MATCHED. A line names the file, the line number and the kind and
# nothing else, because printing the match into an Actions log publishes it.
#
# WHAT IT CANNOT DO. It reads words. A GUID on another line is left alone, a COM CLSID or a
# WPF window class name, because it names no licence and no machine. An id spelled some
# other way, a UTF-16 file with no byte order mark, and a zip or a picture are not read.
# The machine is the one it runs on, so Actions reads for the runner's name and not for
# Bader's. mask-evidence.ps1 reads more widely than this before it writes a copy.
#
# Usage: sh tools/checks/check-evidence-ids.sh <folder> [<folder under it to leave out>]...
# .git, bin and obj are always left out.
#
# Exit 0 clean, exit 1 with one line per fault, exit 2 if it could not read the folder.

set -e

if [ $# -eq 0 ]; then
    echo "check-evidence-ids: name a folder to read" >&2
    exit 2
fi

root=$1
shift

case "$root" in
    */) root=${root%/} ;;
esac

if [ ! -d "$root" ]; then
    echo "check-evidence-ids: no folder named $root" >&2
    exit 2
fi

skips=""

for skip in "$@"; do
    skips="$skips$root/${skip%/}
"
done

export skips

list=$(mktemp)
trap 'rm -f "$list"' EXIT

if ! find "$root" \( -name .git -o -name bin -o -name obj \) -prune -o -type f -print > "$list"; then
    echo "check-evidence-ids: could not list every file under $root, so nothing is passed" >&2
    exit 2
fi

# The files named on standard input, bar those under a folder left out.
kept() {
    awk '
    BEGIN { n = split(ENVIRON["skips"], skip, "\n") }
    {
        for (i = 1; i <= n; i++) {
            if (skip[i] != "" && ($0 == skip[i] || index($0, skip[i] "/") == 1)) {
                next
            }
        }
        print
    }'
}

if [ ! -s "$list" ]; then
    echo "check-evidence-ids: no file under $root" >&2
    exit 2
fi

count=$(kept < "$list" | wc -l | tr -d ' ')

if [ "$count" -eq 0 ]; then
    echo "check-evidence-ids: every file under $root is in a folder left out, so none was read"
    exit 0
fi

hex='[0-9A-Fa-f]'
guid="$hex{8}-$hex{4}-$hex{4}-$hex{4}-$hex{12}"

# One kind: grep names the files, then each file is read again for its line numbers alone.
scan() {
    kind=$1
    shift
    status=0
    files=$(LC_ALL=C grep -r -I -l --exclude-dir=.git --exclude-dir=bin --exclude-dir=obj "$@" "$root") \
        || status=$?

    if [ "$status" -gt 1 ]; then
        echo "check-evidence-ids: grep could not read every file under $root, so nothing is passed" >&2
        exit 2
    fi

    if [ -z "$files" ]; then
        return 0
    fi

    printf '%s\n' "$files" | sort | kept | while IFS= read -r file; do
        LC_ALL=C grep -n "$@" "$file" | cut -d: -f1 | while IFS= read -r at; do
            printf '%s:%s  %s\n' "$file" "$at" "$kind"
        done
    done
}

said=$(
    scan "an analytics agent id" -i -E -e 'analyticsagentid=analytics-[0-9a-f]'

    scan "a licensing id after -i" -i -E \
        -e "AdskLicensing.*[[:space:]]-i[[:space:]]+[^[:space:]]*$guid" \
        -e "(^|[[:space:]])-i[[:space:]]+[^[:space:]]*$guid.*AdskLicensing"

    if [ -n "$COMPUTERNAME" ]; then
        scan "the machine name" -i -w -F -e "$COMPUTERNAME"
    fi

    # The first two bytes of every file, read by one awk rather than a program per file.
    status=0
    boms=$(kept < "$list" | tr '\n' '\000' | LC_ALL=C xargs -0 awk '
        BEGIN { le = sprintf("%c%c", 255, 254); be = sprintf("%c%c", 254, 255) }
        FNR == 1 {
            head = substr($0, 1, 2)
            if (head == le || head == be) {
                print FILENAME
            }
            nextfile
        }') || status=$?

    if [ "$status" -ne 0 ]; then
        echo "check-evidence-ids: could not read the start of every file under $root, so nothing is passed" >&2
        exit 2
    fi

    if [ -n "$boms" ]; then
        printf '%s\n' "$boms" | sort | while IFS= read -r file; do
            printf '%s:1  a UTF-16 text file, which this check cannot read\n' "$file"
        done
    fi
)

if [ -n "$COMPUTERNAME" ]; then
    read_for="an analytics agent id, a licensing id after -i and the machine name"
else
    read_for="an analytics agent id and a licensing id after -i. COMPUTERNAME is not set, so not for the machine name"
fi

if [ -n "$said" ]; then
    printf '%s\n' "$said"
    faults=$(printf '%s\n' "$said" | wc -l | tr -d ' ')
    echo "check-evidence-ids: $faults line(s) under $root refused, of $count files read for $read_for."
    echo "check-evidence-ids: write a masked copy with tools/loop/mask-evidence.ps1 and commit that instead."
    exit 1
fi

echo "check-evidence-ids: $count files under $root read for $read_for, and none carries one"
exit 0
