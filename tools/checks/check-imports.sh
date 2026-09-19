#!/bin/sh
#
# Refuses a file that names a type and does not import the namespace that type comes from.
#
# WHY THIS EXISTS AT ALL. The compiler already knows this rule and the compiler is the
# right place for it, and src\Federator.Addin cannot be compiled by anything but a machine
# with Navisworks Manage 2025 on it. So for half this repo the compiler runs once, on
# Bader's machine, at step 8 of steps\03_bader_next.md, after every change of a round has
# already been written. F52 shipped a local declared twice, which is CS0128, and F58 wrote
# check-locals.sh for it. Then F61 shipped DocumentCensusReader.cs naming
# DocumentSelectionSets with no Autodesk.Navisworks.Api.DocumentParts import, which is
# CS0246, and check-locals.sh reads a different shape of fault entirely, so nothing here
# saw that one either. Two rounds, two compiler errors, both shipped from here. This is the
# second rule of the compiler's that runs without it.
#
# WHAT IT READS, WHICH IS NARROWER THAN IT LOOKS. A type name only counts where it is in a
# TYPE POSITION: after new, is, as or typeof, in the head of a using block, as a field, a
# parameter, a local or a foreach type, or inside generic brackets. A bare capitalised word
# anywhere else is a member name, a property or an enum value, not a type, and reading
# those is what made a first attempt at this check return dozens of lines of noise.
#
# HOW IT KNOWS WHERE A TYPE LIVES, WHICH IS TWO DIFFERENT THINGS. For a type this repo
# DECLARES, the namespace is a fact read off the file that declares it, and a file naming
# that type from outside that namespace and its children must import it. For every other
# type, which is the whole BCL and the whole Navisworks API, nothing here can know where it
# lives, so the check LEARNS: a namespace is taken as the home of a type when every other
# file under the root that names that type imports it, and when at least MinimumShare per
# cent of the files importing that namespace name that type. Without that second condition
# the answer comes back as System or System.Collections.Generic, which nearly every file
# carries and which supplies nothing in particular.
#
# WHERE MinimumShare CAME FROM, MEASURED TWICE. On 2026-09-19 at 50 the check read clean
# over src, at 40 one line of noise, at 34 three and at 20 ten. Later the same day F72
# added Penetrations.cs, which names ModelItemCollection, a type only ClashRunner.cs and
# SetBuilder.cs name and which both of those import Autodesk.Navisworks.Api.DocumentParts
# for an unrelated reason. Two files of the four importing DocumentParts is exactly 50 per
# cent, so the check reported a file the compiler is perfectly happy with. Re-measured over
# src with F72 in: clean at 51 and at 60, and with the F65 import taken back out it still
# names DocumentParts on the real fault at both. At 75 it reads clean over src and MISSES
# the real fault, so 75 is too high and the window is 51 to 60. It sits at 60, which is the
# middle of the proven window rather than its edge.
#
# A COINCIDENCE LIKE THAT WILL HAPPEN AGAIN and the answer is to re-measure this number,
# not to add an import a file does not need. An unused import to quieten a check is a lie
# in the one place a later reader will trust.
#
# WHAT IT CANNOT DO, said here rather than left to be discovered.
#   It reads TEXT and not a program. It has no compiler, no Autodesk DLL and no reference
#     assembly, and it resolves nothing.
#   It cannot know a namespace no file here imports yet. The FIRST use of a brand new
#     Autodesk type, in the first file that ever names it, is invisible to this check, and
#     only the build on Bader's machine sees that one.
#   It reports a CORRELATION and never a fact about where a type lives. The line says what
#     the other files here do, because that is all it measured.
#   A type only one other file names teaches nothing reliable, so it is left alone.
#   IT IS NOT A BUILD AND IT NEVER SAYS A BUILD PASSED.
#
# Exit 0 clean, exit 1 with one line per fault, exit 2 if it could not read the tree.

set -e

root=${1:-src}

# At least this share of the files importing a namespace must name a type before that
# namespace is taken as the type's home. See the measurement above.
MinimumShare=${2:-60}

if [ ! -d "$root" ]; then
    echo "check-imports: no folder named $root" >&2
    exit 2
fi

found=$(find "$root" -name '*.cs' ! -path '*/obj/*' ! -path '*/bin/*' | sort)

if [ -z "$found" ]; then
    echo "check-imports: no C# file under $root" >&2
    exit 2
fi

case "$found" in
    *\ *)
        echo "check-imports: a path under $root has a space in it, which this check cannot read" >&2
        exit 2
        ;;
esac

# Every file goes to ONE awk, because the map is learned from the whole tree and a batch of
# it would learn a different map and answer differently.
said=$(awk -v share="$MinimumShare" '
function note(word) {
    # A name written out in full needs no import, and the leading part of it is a namespace
    # rather than a type, so a dotted name is not read at all.
    if (index(word, ".") > 0) {
        return
    }

    if (word ~ /^[A-Z][A-Za-z0-9_]*$/) {
        types[FILENAME "\t" word] = 1
    }
}

function generics(text,    inner, n, i, parts) {
    while (match(text, /<[^<>]*>/)) {
        inner = substr(text, RSTART + 1, RLENGTH - 2)
        text = substr(text, 1, RSTART - 1) " " substr(text, RSTART + RLENGTH)

        n = split(inner, parts, /[ ,]+/)

        for (i = 1; i <= n; i++) {
            note(parts[i])
        }
    }
}

{
    line = $0

    # A literal and a comment are text and not code. Both go first, or a type name written
    # in a sentence about the API would be read as a use of it. That is the whole of the
    # difference between SavedViewpoints.cs, which names DocumentSavedViewpoints four times
    # in comments and needs no import, and DocumentCensusReader.cs, which named
    # DocumentSelectionSets once in a parameter list and did not build.
    gsub(/"([^"\\]|\\.)*"/, "\"\"", line)
    sub(/\/\/.*$/, "", line)

    if (line ~ /^[ \t]*\/\*/ || line ~ /^[ \t]*\*/) {
        next
    }

    if (line ~ /^[ \t]*namespace[ \t]/) {
        own = line
        sub(/^[ \t]*namespace[ \t]+/, "", own)
        sub(/[ \t{].*$/, "", own)
        mine[FILENAME] = own
        next
    }

    # A plain import. "using X = Y;" declares an ALIAS, which brings its own name into the
    # file and is not an import, and "using (" is a statement.
    if (line ~ /^[ \t]*using[ \t]+[A-Za-z_][A-Za-z0-9_.]*[ \t]*;[ \t]*$/) {
        ns = line
        sub(/^[ \t]*using[ \t]+/, "", ns)
        sub(/[ \t]*;.*$/, "", ns)
        imports[FILENAME "\t" ns] = 1
        importers[ns] = importers[ns] " " FILENAME
        next
    }

    if (line ~ /^[ \t]*using[ \t]+[A-Za-z_][A-Za-z0-9_.]*[ \t]*=/) {
        alias = line
        sub(/^[ \t]*using[ \t]+/, "", alias)
        sub(/[ \t]*=.*$/, "", alias)
        aliases[alias] = 1
        next
    }

    if (match(line, /(^|[^A-Za-z0-9_])(class|struct|interface|enum|delegate)[ \t]+[A-Za-z_][A-Za-z0-9_]*/)) {
        name = substr(line, RSTART, RLENGTH)
        sub(/^[^A-Za-z0-9_]*(class|struct|interface|enum|delegate)[ \t]+/, "", name)
        home[name] = home[name] " " FILENAME
    }

    # 1. Inside generic brackets.
    generics(line)

    rest = line

    # 2. After new, is, as and typeof.
    while (match(rest, /(^|[^A-Za-z0-9_])(new|is|as|typeof)[ \t(]+[A-Z][A-Za-z0-9_.]*/)) {
        piece = substr(rest, RSTART, RLENGTH)
        rest = substr(rest, RSTART + RLENGTH)
        sub(/^[^A-Za-z0-9_]*(new|is|as|typeof)[ \t(]+/, "", piece)
        note(piece)
    }

    rest = line

    # 3. A type and a name together, which covers a field, a parameter, a local, a catch
    #    and the head of a using block. The NAME starts lower case and a closing character
    #    follows it, so a member call and a return type are not read as types.
    while (match(rest, /(^|[ \t(,])[A-Z][A-Za-z0-9_.]*(\[\])?\??[ \t]+[a-z_][A-Za-z0-9_]*[ \t]*[,);=]/)) {
        piece = substr(rest, RSTART, RLENGTH)
        rest = substr(rest, RSTART + RLENGTH - 1)
        sub(/^[ \t(,]*/, "", piece)
        sub(/[ \t].*$/, "", piece)
        sub(/(\[\])?\??$/, "", piece)
        note(piece)
    }

    rest = line

    # 4. The foreach type, which ends in the word in rather than in a closing character.
    while (match(rest, /foreach[ \t]*\([ \t]*[A-Z][A-Za-z0-9_.]*(\[\])?[ \t]+[a-z_][A-Za-z0-9_]*[ \t]+in[ \t]/)) {
        piece = substr(rest, RSTART, RLENGTH)
        rest = substr(rest, RSTART + RLENGTH)
        sub(/^foreach[ \t]*\([ \t]*/, "", piece)
        sub(/[ \t].*$/, "", piece)
        sub(/(\[\])?$/, "", piece)
        note(piece)
    }
}

END {
    # A type this repo declares in exactly one namespace. Two namespaces declaring one name
    # is ambiguous and is left alone rather than guessed at.
    for (name in home) {
        n = split(home[name], where, " ")
        one = ""
        many = 0

        for (i = 1; i <= n; i++) {
            if (where[i] == "") {
                continue
            }

            if (one == "") {
                one = mine[where[i]]
            } else if (mine[where[i]] != one) {
                many = 1
            }
        }

        if (one != "" && !many) {
            declared[name] = one
        }
    }

    for (ns in importers) {
        howmany[ns] = split(importers[ns], ignore, " ") - 1
    }

    for (key in types) {
        split(key, bit, "\t")
        users[bit[2]] = users[bit[2]] " " bit[1]
    }

    faults = 0

    for (key in types) {
        split(key, bit, "\t")
        file = bit[1]
        type = bit[2]

        if (type in aliases) {
            continue
        }

        if (type in declared) {
            want = declared[type]

            # A namespace is in scope inside its own children, so Federator.Addin.Engine
            # sees Federator.Addin with no import and that is not a fault.
            if (want != mine[file] \
                && substr(mine[file], 1, length(want) + 1) != want "." \
                && !((file "\t" want) in imports)) {
                printf "%s  names %s, which this repo declares in %s, and does not import it\n", \
                    file, type, want
                faults++
            }

            continue
        }

        count = split(users[type], who, " ")
        others = 0
        delete common
        first = 1

        for (i = 1; i <= count; i++) {
            other = who[i]

            if (other == "" || other == file) {
                continue
            }

            others++

            if (first) {
                first = 0

                for (k in imports) {
                    split(k, ib, "\t")

                    if (ib[1] == other) {
                        common[ib[2]] = 1
                    }
                }
            } else {
                for (ns in common) {
                    if (!((other "\t" ns) in imports)) {
                        delete common[ns]
                    }
                }
            }
        }

        # One other file naming a type teaches nothing reliable about where it lives.
        if (others < 2) {
            continue
        }

        best = ""
        bestcount = 0

        for (ns in common) {
            if ((file "\t" ns) in imports || ns == mine[file]) {
                continue
            }

            if (others * 100 < share * howmany[ns]) {
                continue
            }

            # The most specific candidate left wins, which is the one the fewest files
            # here import.
            if (best == "" || howmany[ns] < bestcount) {
                best = ns
                bestcount = howmany[ns]
            }
        }

        if (best != "") {
            printf "%s  names %s, and every other file here that names it imports %s\n", \
                file, type, best
            faults++
        }
    }

    exit faults > 0 ? 1 : 0
}
' $found) && clean=0 || clean=1

if [ -n "$said" ]; then
    echo "$said" | sort
fi

if [ "$clean" -ne 0 ]; then
    echo "check-imports: a file names a type and imports no namespace here that has it. That is CS0246 and the build will refuse it."
    exit 1
fi

echo "check-imports: every type named under $root is covered by an import this repo already uses"
echo "check-imports: this is not a build. The first use of a type no file here imports yet is invisible to it."
exit 0
