#!/bin/sh
#
# Refuses a file that carries an Autodesk licensing id or this machine's name, of the kinds
# tools\checks\evidence-ids.txt names.
#
# WHY THIS EXISTS. F102, measured on 2026-09-29. A probe result written on Bader's machine
# was committed to a pushed branch with the machine's name on its first line and, on two
# lines, the command lines of two AdskLicensingAgent processes, which carry the licensing
# agent's ids. Every probe prints MACHINE and the computer name, and the start probe of
# F100 printed a new process's command line whole. Nothing between a result and a commit
# read the file for them. tools\loop\mask-evidence.ps1 writes a masked copy of a result,
# and this refuses a file that carries one of the kinds it knows, in the pre-commit over
# what is staged and in Actions over the whole tree. It knows only those kinds.
#
# WHAT IT REFUSES is every kind in tools\checks\evidence-ids.txt, the one place that rule
# lives, which the mask reads too, read by the same rules as the mask reads it, so a rules
# file one refuses the other refuses. The machine name is read for only when COMPUTERNAME
# is set, and the last line says when it was not. It is the machine the check runs on, so
# Actions reads for the runner's name and never for Bader's, and only the pre-commit on
# his machine reads for his.
#
# IT READS EVERY FILE AS TEXT OR REFUSES IT. A file holding a NUL byte cannot be read as
# text, UTF-16 among them, and it is named and refused. The one exception is the binary
# rule below, and a file it leaves out is counted as left out and never as read.
#
# IT NEVER PRINTS WHAT IT MATCHED. A line names the file, the line number and the kind and
# nothing else, because printing the match into an Actions log publishes it.
#
# NO FAILURE IS LOST. Every program's answer is kept in a file or a status and read, never
# through a pipe, and a read that failed ends it with exit 2 and passes nothing.
#
# WHAT IT CANNOT DO. It reads words. A GUID on a line no kind reads is left alone, a COM
# CLSID or a WPF window class name, because it names no licence and no machine. An id
# spelled some other way is not seen. A kind's id is looked for in the file's path under
# the folder as well as in the line, so a line of a file whose own path carries an id may be
# refused for it, and a file whose path alone carries one and none of its lines is not.
#
# WHY IT IS SHAPED THIS WAY. Starting a program costs close to a second on Bader's machine,
# measured on 2026-09-29, so every read is one pass over all the files and not one a file.
#
# Usage: sh tools/checks/check-evidence-ids.sh <folder> [<folder under it to leave out>]...
# .git, bin and obj are always left out.
#
# Exit 0 clean, exit 1 with one line per fault, exit 2 if it could not read the folder or
# the rules, or any read failed.

set -e

# LEFT OUT AS BINARY, and never read, only when ALL THREE hold: its path under the folder
# matches, without regard to case, it opens with its format's first bytes, and its last so
# many bytes hold its format's closing bytes. A file at such a path that fails either byte
# test is read as text, or named and refused, like any other. One rule a line: the path, the
# first bytes in hex, how many of the last bytes are read, and the bytes in hex they hold.
#   a jpg opens FF D8 FF and its last two bytes are FF D9, the marker that ends the picture,
#     so text appended after the picture fails it
#   an xlsx is a zip. It opens PK 03 04, and its last 65557 bytes hold 50 4B 05 06, the
#     signature of the end of central directory, which is 22 bytes and a comment of at most
#     65535, so a zip cut short or with text appended past its end fails it
# Measured with git ls-files on 2026-09-29: the only files git reads as binary are 345 .jpg
# and 3 .xlsx under samples, the client's workbooks and the pictures they link to, and one
# probe result that is plain UTF-8 and is read. Read in full the same day: all 345 .jpg open
# FF D8 FF and end FF D9, and all 3 .xlsx open PK 03 04 and hold 50 4B 05 06 exactly 22
# bytes before their end. A zip is not here, so a zip is refused. No zip of run evidence sits
# anywhere in the repo, Bader's answer Q90 A: a run's file over 20 MB stays in the work folder
# and the turn names it with its size and sha256. A binary file no line names is refused with
# words that say so, so a person adds a line for its type rather than hunting for an id.
binary='^samples/.*\.jpg$ ffd8ff 2 ffd9
^samples/.*\.xlsx$ 504b0304 65557 504b0506'

rules="$(dirname "$0")/evidence-ids.txt"

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

if [ ! -f "$rules" ]; then
    echo "check-evidence-ids: no rules file at $rules" >&2
    exit 2
fi

failed() {
    echo "check-evidence-ids: $1, so nothing is passed" >&2
    exit 2
}

skips=""

for skip in "$@"; do
    skips="$skips./${skip%/}
"
done

machine=${COMPUTERNAME:-}
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
export skips root binary machine work

# THE RULES FILE, read EXACTLY as mask-evidence.ps1 reads it. Bytes, split at each line
# feed, one carriage return taken off the end. A line that is blank or starts with # is
# skipped. Any other line is a key, a colon and a value, and every key is compared with its
# case. A line with no colon, a key it does not know, a key of a kind before the first kind,
# a kind that lacks lines, id, mask or whole, a whole that is not yes or no, and a file with
# no guid, no machine or no kind are refused.
#
# What it writes, one kind a line: kind, lines, id, tab separated, with {guid} and {machine}
# put in. The kind for the machine is dropped when COMPUTERNAME is not set. Beside it, the
# expression a line must match to be read at all for each kind, the machine line, and the
# names of the kinds for the last line said.
if ! LC_ALL=C awk '
    function put(s, from, to,    at) {
        while ((at = index(s, from)) > 0) {
            s = substr(s, 1, at - 1) to substr(s, at + length(from))
        }
        return s
    }
    function close_kind() {
        if (kind == "") {
            return
        }
        if (lines == "" || id == "" || mask == "" || whole == "") {
            bad = "the kind " kind " lacks a lines, id, mask or whole line"
            return
        }
        if (whole != "yes" && whole != "no") {
            bad = "the kind " kind " has a whole that is not yes or no"
            return
        }
        count++
        if (index(id, "{machine}") > 0 && ENVIRON["machine"] == "") {
            return
        }
        lines = put(put(lines, "{guid}", guid), "{machine}", ENVIRON["machine"])
        id = put(put(id, "{guid}", guid), "{machine}", ENVIRON["machine"])
        printf "%s\t%s\t%s\n", kind, lines, id
        print ((lines == "every") ? id : lines) > (ENVIRON["work"] "/first")
        names = names (names == "" ? "" : ", ") kind
    }
    {
        sub(/\r$/, "")
    }
    /^[ \t]*(#|$)/ {
        next
    }
    index($0, ":") == 0 {
        bad = "a line has no key"
        exit 2
    }
    {
        key = substr($0, 1, index($0, ":") - 1)
        value = substr($0, index($0, ":") + 1)
        sub(/^[ \t]*/, "", value)
    }
    key == "guid" { guid = value; next }
    key == "machine" { shape = value; next }
    key == "kind" {
        close_kind()
        if (bad == "" && value == "") {
            bad = "a kind has no name"
        }
        if (bad != "") {
            exit 2
        }
        kind = value; lines = ""; id = ""; mask = ""; whole = ""
        next
    }
    key == "lines" || key == "id" || key == "mask" || key == "whole" {
        if (kind == "") {
            bad = key " comes before any kind"
            exit 2
        }
        if (key == "lines") lines = value
        if (key == "id") id = value
        if (key == "mask") mask = value
        if (key == "whole") whole = value
        next
    }
    { bad = "a line that is no key it knows, " key; exit 2 }
    END {
        if (bad == "") {
            close_kind()
        }
        if (bad == "" && (guid == "" || shape == "" || count == 0)) {
            bad = "it has no guid line, no machine line or no kind"
        }
        if (bad != "") {
            print "check-evidence-ids: the rules file cannot be read, " bad > "/dev/stderr"
            exit 2
        }
        print shape > (ENVIRON["work"] "/shape")
        print names > (ENVIRON["work"] "/names")
    }' "$rules" > "$work/kinds"; then
    exit 2
fi

read -r shape < "$work/shape"
read -r names < "$work/names"

if [ -n "$machine" ]; then
    status=0
    grep -E -x -q -e "$shape" <<EOF || status=$?
$machine
EOF
    [ "$status" -eq 0 ] || failed "COMPUTERNAME is not the shape the machine line of the rules file allows"
fi

# Read from inside the folder, so every path is short and relative, ./ and the path under
# it, and is written as the folder and the path under it.
cd "$root" || failed "could not enter $root"

find . \( -name .git -o -name bin -o -name obj \) -prune -o -type f -print > "$work/all" \
    || failed "could not list every file under $root"
[ -s "$work/all" ] || failed "there is no file under $root"

# Every file is left out under a folder named on the command line, or is a candidate for
# the binary rule by its path, or is read. The path alone never leaves a file out.
LC_ALL=C awk '
    BEGIN {
        n = split(ENVIRON["skips"], skip, "\n")
        r = split(ENVIRON["binary"], rule, "\n")
        out = ENVIRON["work"]
    }
    {
        for (i = 1; i <= n; i++) {
            if (skip[i] != "" && ($0 == skip[i] || index($0, skip[i] "/") == 1)) {
                next
            }
        }
        path = tolower(substr($0, 3))
        for (i = 1; i <= r; i++) {
            split(rule[i], part, " ")
            if (path ~ part[1]) {
                print > (out "/maybe")
                next
            }
        }
        print > (out "/read")
    }' "$work/all" || failed "the files could not be sorted into read and left out"

touch "$work/read" "$work/maybe"
: > "$work/binary"

# The first and the last bytes of every candidate. One wc over all of them for their sizes,
# then one awk over all of them. awk reads a file as lines, so the bytes of a file are its
# lines with a line feed after each, and whether its very last byte was a line feed is
# known from its size. A candidate is left out only when both byte tests pass, and is read
# like any other file when either fails, or when its size and what was read disagree.
#
# BINMODE=1 makes gawk read the files as bytes. Without it the gawk of Git for Windows, GNU
# Awk 5.4.1, drops a carriage return before a line feed, measured on 2026-09-29: a sample
# jpg of 177893 bytes with 434 carriage returns read as 177892, so 238 of the 345 real jpgs
# failed the size test. It is a plain variable to any other awk.
if [ -s "$work/maybe" ]; then
    LC_ALL=C tr '\n' '\000' < "$work/maybe" > "$work/maybe0" || failed "the list of files could not be made"
    status=0
    LC_ALL=C xargs -0 wc -c < "$work/maybe0" > "$work/sizes" 2> "$work/errors" || status=$?

    if [ -s "$work/errors" ] || [ "$status" -ne 0 ]; then
        cat "$work/errors" >&2
        failed "the size of every file the binary rule names could not be read"
    fi

    status=0
    LC_ALL=C xargs -0 awk -v BINMODE=1 '
        function bytes(hex,    k, s) {
            s = ""
            for (k = 1; k < length(hex); k += 2) {
                s = s sprintf("%c", (index(digits, substr(hex, k, 1)) - 1) * 16 + index(digits, substr(hex, k + 1, 1)) - 1)
            }
            return s
        }
        function finish() {
            if (file == "") {
                return
            }
            # pos counts a line feed after every line, so it is the size when the last byte
            # was a line feed and one more when it was not. Anything else, and the file is read.
            if (pos == size[file] + 1) {
                tail = substr(tail, 1, length(tail) - 1)
            } else if (pos != size[file]) {
                ok = 0
            }
            if (length(tail) > window) {
                tail = substr(tail, length(tail) - window + 1)
            }
            if (ok && first == opens && index(tail, closes) > 0) {
                print file
            }
            file = ""
        }
        BEGIN {
            digits = "0123456789abcdef"
            r = split(ENVIRON["binary"], rule, "\n")
            sizes = ENVIRON["work"] "/sizes"
        }
        FILENAME == sizes {
            line = $0
            sub(/^[ \t]*/, "", line)
            at = index(line, " ")
            size[substr(line, at + 1)] = substr(line, 1, at - 1) + 0
            next
        }
        FNR == 1 {
            finish()
            file = FILENAME
            pos = 0
            tail = ""
            ok = 0
            path = tolower(substr(FILENAME, 3))
            for (i = 1; i <= r; i++) {
                split(rule[i], part, " ")
                if (path ~ part[1]) {
                    opens = bytes(part[2])
                    window = part[3] + 0
                    closes = bytes(part[4])
                    ok = 1
                    break
                }
            }
            if (!(file in size)) {
                ok = 0
            }
            first = substr($0, 1, length(opens))
        }
        {
            pos = pos + length($0) + 1
            if (pos > size[file] - window) {
                tail = tail $0 "\n"
                if (length(tail) > window + 1) {
                    tail = substr(tail, length(tail) - window)
                }
            }
        }
        END {
            finish()
        }' "$work/sizes" < "$work/maybe0" > "$work/binary" 2> "$work/errors" || status=$?

    if [ -s "$work/errors" ] || [ "$status" -ne 0 ]; then
        cat "$work/errors" >&2
        failed "the first and last bytes of every file the binary rule names could not be read"
    fi

    status=0
    LC_ALL=C grep -F -x -v -f "$work/binary" "$work/maybe" >> "$work/read" || status=$?
    [ "$status" -le 1 ] || failed "the list of files to read could not be made"
fi

wc -l < "$work/read" > "$work/count" || failed "the files to read could not be counted"
wc -l < "$work/binary" > "$work/left" || failed "the files left out could not be counted"
read -r count < "$work/count"
read -r left < "$work/left"

if [ "$count" -eq 0 ]; then
    echo "check-evidence-ids: every file under $root is left out, $left of them as binary, so none was read"
    exit 0
fi

# grep over the files of a NUL separated list, through xargs. A read that failed is known
# by what grep writes on its error stream, since xargs answers 123 alike for a batch with
# no match and for one that could not read a file. Any other answer is a failure too.
over() {
    from=$1
    into=$2
    shift 2
    status=0
    LC_ALL=C xargs -0 grep "$@" < "$from" > "$into" 2> "$work/errors" || status=$?

    if [ -s "$work/errors" ] || { [ "$status" -ne 0 ] && [ "$status" -ne 123 ]; }; then
        # grep's own error names the file and why, and never a line of it.
        cat "$work/errors" >&2
        failed "grep could not read every file under $root"
    fi
}

LC_ALL=C tr '\n' '\000' < "$work/read" > "$work/read0" || failed "the list of files could not be made"
over "$work/read0" "$work/nul" -l -a -P '\x00'

# A file with a NUL byte is refused for that alone and not read for a kind. An empty list
# of patterns matches no line, so with no such file every file is kept.
status=0
LC_ALL=C grep -F -x -v -f "$work/nul" "$work/read" > "$work/texts" || status=$?
[ "$status" -le 1 ] || failed "the list of files to read could not be made"
: > "$work/said"

if [ -s "$work/texts" ]; then
    LC_ALL=C tr '\n' '\000' < "$work/texts" > "$work/texts0" || failed "the list of files to read could not be made"

    # One pass over every file for every line any kind reads, as the file, a NUL, the line
    # number and the line. Then each is turned round to the line, the file and the number,
    # so a kind's expression sees the line first and ^ is the start of the line.
    over "$work/texts0" "$work/lines" -Z -H -n -i -E -f "$work/first"
    LC_ALL=C tr '\000' '\001' < "$work/lines" > "$work/split" || failed "the lines read could not be turned round"
    LC_ALL=C awk '{
        at = index($0, "\001")
        file = substr($0, 1, at - 1)
        rest = substr($0, at + 1)
        colon = index(rest, ":")
        printf "%s\001%s\001%s\n", substr(rest, colon + 1), file, substr(rest, 1, colon - 1)
    }' "$work/split" > "$work/turned" || failed "the lines read could not be turned round"

    # Each kind: the lines it reads, then those with its id. Both are read on the line as it
    # is in the file. The last two fields of what is left are the file and the number, and
    # the text is never written.
    tab=$(printf '\t')
    number=0

    while IFS="$tab" read -r kind lines id; do
        number=$((number + 1))
        found="$work/found.$number"
        status=0

        if [ "$lines" = "every" ]; then
            LC_ALL=C grep -a -i -E -e "$id" "$work/turned" > "$found" || status=$?
        else
            LC_ALL=C grep -a -i -E -e "$lines" "$work/turned" > "$work/kind" || status=$?
            [ "$status" -le 1 ] || failed "grep could not read the lines found for $kind"
            status=0
            LC_ALL=C grep -a -i -E -e "$id" "$work/kind" > "$found" || status=$?
        fi

        [ "$status" -le 1 ] || failed "grep could not read the lines found for $kind"
        printf '%s\n' "$kind" > "$found.kind"
    done < "$work/kinds"

    number=0

    while [ -f "$work/found.$((number + 1))" ]; do
        number=$((number + 1))
        read -r kind < "$work/found.$number.kind"
        kind="$kind" LC_ALL=C awk '{
            n = split($0, field, "\001")
            printf "%s/%s:%s  %s\n", ENVIRON["root"], substr(field[n - 1], 3), field[n], ENVIRON["kind"]
        }' "$work/found.$number" >> "$work/said" || failed "the lines found for $kind could not be written"
    done
fi

# A file with a NUL byte is refused as binary, in words that say so and not as an id, so a
# person adds a line for its type to the binary rule, or writes text as UTF-8, rather than
# hunting for an id. One the rule names by its path failed its byte tests, and says that.
: > "$work/binaries"
LC_ALL=C awk '
    FILENAME == ENVIRON["work"] "/maybe" {
        named[$0] = 1
        next
    }
    {
        file = ENVIRON["root"] "/" substr($0, 3)
        if ($0 in named) {
            printf "%s  is binary at a path the binary rule names, but its first or last bytes are not those of its format, so it is not left out\n", file
        } else {
            printf "%s  is binary, a NUL byte is in it, and no line of the binary rule at the top of the check names it\n", file
        }
    }' "$work/maybe" "$work/nul" > "$work/binaries" || failed "the binary files could not be named"

# Said last, whether it passed or not.
about() {
    echo "check-evidence-ids: read for $names."

    if [ -z "$machine" ]; then
        echo "check-evidence-ids: COMPUTERNAME is not set, so no file was read for the machine name."
    fi
}

if [ -s "$work/said" ] || [ -s "$work/binaries" ]; then
    faults=0

    while IFS= read -r said; do
        echo "$said"
        faults=$((faults + 1))
    done < "$work/said"

    while IFS= read -r said; do
        echo "$said"
        faults=$((faults + 1))
    done < "$work/binaries"

    echo "check-evidence-ids: $faults line(s) under $root refused. $count files were to be read, and $left left out as binary by the rule at the top."

    if [ -s "$work/said" ]; then
        echo "check-evidence-ids: for a line with an id or the machine name, write a masked copy with tools/loop/mask-evidence.ps1 and commit that instead."
    fi

    if [ -s "$work/binaries" ]; then
        echo "check-evidence-ids: a binary file is refused for being binary, not for an id. If it belongs in the repo, add a line for its type to the binary rule at the top of this check. Text written as UTF-16 is refused the same way and goes in as UTF-8."
    fi

    about
    exit 1
fi

echo "check-evidence-ids: $count files under $root read, $left left out as binary by the rule at the top, and none carries one."
about
exit 0
