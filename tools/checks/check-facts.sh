#!/bin/sh
#
# F96. Refuses a document that restates a fact the code holds and gets it wrong.
#
# WHY THIS EXISTS. Four facts in this repo had drifted between the code and the prose
# that repeats them, and every one was found by a person reading carefully rather than
# by anything that runs. The solid category list was written as holding THREE when it
# holds FOUR, and as NOT holding Structural Foundations when it does. A reader building
# a fixture off that sentence would have built the wrong one. The rule this repo already
# keeps is that a number in a doc is measured and never estimated, and this is the first
# thing that can prove it.
#
# WHAT IT READS. `tools/checks/facts.tsv` names each fact: which reader gets its value
# off the CODE, which prose files may restate it, how to spot a line that is making a
# claim about it, and what that line must or must not then say. The value never comes
# out of the registry, because a registry carrying the number would be one more copy to
# go stale.
#
# WHAT IT DELIBERATELY DOES NOT DO. It does not validate prose. It reads the lines that
# make a QUANTITY or MEMBERSHIP claim about a NAMED fact and nothing else. A check that
# tried to read every sentence is one nobody can keep green, and a check nobody keeps
# green is a check everybody learns to ignore, which is worse than not having it. Adding
# a fact is a deliberate act and a fact nobody adds is a fact nobody checks.
#
# WHAT IT CANNOT DO, said here rather than left to be discovered.
#   It reads TEXT. It has no compiler and it resolves nothing.
#   It cannot see a fact restated in words it was not taught to recognise. The subject
#     column is a pattern and a sentence phrased another way is invisible to it.
#   It cannot tell a right number in the wrong sentence from a right one.
#   It reads the files the registry names and no others, so a fact repeated in a file
#     nobody listed is unchecked.
#
# Proved both ways, as this repo requires: it passes over the real files and refuses
# `tools/checks/broken/facts-broken.tsv`, which points at a document that is wrong on
# purpose.

registry=${1:-tools/checks/facts.tsv}
root=${2:-.}

if [ ! -f "$registry" ]; then
    echo "check-facts: no registry at $registry"
    exit 1
fi

# ---------- the readers, one per fact, each reading the CODE ----------

# How many categories the solid list holds. Counted off the quoted strings between the
# declaration and the brace that closes it, so adding one to the list changes the answer
# with nothing else edited.
read_solid() {
    awk '
        /DefaultSolidCategories/ { inside = 1 }
        inside && /"/ { for (i = 1; i <= gsub(/"[^"]*"/, "&"); i++) { } }
        inside && /^[[:space:]]*"/ { n++ }
        inside && /};/ { print n; exit }
    ' "$root/src/Federator.Core/Clash/PenetrationSettings.cs"
}

# The size threshold in millimetres, as a whole number.
read_threshold() {
    awk -F'=' '
        /DefaultThresholdMillimetres[[:space:]]*=/ {
            v = $2
            gsub(/[^0-9.]/, "", v)
            sub(/\.0*$/, "", v)
            print v
            exit
        }
    ' "$root/src/Federator.Core/Views/SizeSettings.cs"
}

# How many tick boxes are VISIBLE WITHOUT OPENING ANYTHING, which is the fact the prose
# states. Counting every CheckBox in the file gives 22 and answers a different question,
# because most of them sit inside an Expander that starts closed. This counts the ones at
# expander depth zero, which is what a person sees when the window opens.
read_tickboxes() {
    awk '
        /<Expander/ { depth++ }
        /<\/Expander>/ { if (depth > 0) depth-- }
        /<CheckBox/ && depth == 0 { n++ }
        END { print n + 0 }
    ' "$root/src/Federator.Addin/Ui/FederatorWindow.xaml"
}

# How many columns every row of the machine readable log has.
read_tsvcolumns() {
    awk -F'=' '
        /FieldCount[[:space:]]*=/ {
            v = $2
            gsub(/[^0-9]/, "", v)
            print v
            exit
        }
    ' "$root/src/Federator.Core/Diagnostics/EventRow.cs"
}

# The number as a person writes it, because prose says four and not 4.
in_words() {
    case "$1" in
        1) echo "one" ;;
        2) echo "two" ;;
        3) echo "three" ;;
        4) echo "four" ;;
        5) echo "five" ;;
        6) echo "six" ;;
        7) echo "seven" ;;
        8) echo "eight" ;;
        9) echo "nine" ;;
        10) echo "ten" ;;
        *) echo "$1" ;;
    esac
}

faults=0
checked=0
facts=0

# THE CARRIAGE RETURN IS STRIPPED BEFORE A SINGLE LINE IS READ. This registry is tab
# separated and read by sh, so on a CRLF checkout the last column would end in a \r and
# every pattern built from it would quietly stop matching. That is the same failure the
# two walls hit on 2026-09-18, and it fails the worst possible way here: not with an
# error but with a check that reads nothing and reports a pass. .gitattributes pins the
# file to LF as well, and this is the belt beside that brace, because a check whose
# correctness depends on a checkout setting is one nobody can rely on.
clean_registry=$(mktemp 2>/dev/null || echo "${TMPDIR:-/tmp}/check-facts-$$")
tr -d '\r' < "$registry" > "$clean_registry"

# ---------- one fact at a time ----------

while IFS='	' read -r name reader prose subject must never; do
    case "$name" in
        '#'*|'') continue ;;
        name) continue ;;
    esac

    value=""

    case "$reader" in
        solid) value=$(read_solid) ;;
        threshold) value=$(read_threshold) ;;
        tickboxes) value=$(read_tickboxes) ;;
        tsvcolumns) value=$(read_tsvcolumns) ;;
        *)
            echo "check-facts: $name names a reader called $reader and there is no such reader"
            faults=$((faults + 1))
            continue
            ;;
    esac

    if [ -z "$value" ]; then
        echo "check-facts: $name could not be read off the code at all, so nothing was compared"
        faults=$((faults + 1))
        continue
    fi

    facts=$((facts + 1))
    words=$(in_words "$value")

    # {V} and {W} are the value as a digit and as a word, put in here so the expectation
    # follows the code rather than this file.
    wants=$(printf '%s' "$must" | sed "s/{V}/$value/g; s/{W}/$words/g")
    forbids=$(printf '%s' "$never" | sed "s/{V}/$value/g; s/{W}/$words/g")

    # HOW MANY LINES THIS FACT ACTUALLY EXAMINED. A subject that matches nothing makes the
    # fact pass without reading a word, and three of the first five did exactly that
    # before this counted. A check that examines nothing and reports a pass is the fault
    # this whole script exists to catch, so it is a fault here too.
    seen=0

    for file in $prose; do
        [ -f "$root/$file" ] || continue
        found=$(grep -c -i -E "$subject" "$root/$file" 2>/dev/null)
        seen=$((seen + ${found:-0}))
    done

    if [ "$seen" -eq 0 ]; then
        echo "check-facts: $name is named in the registry and NO prose line claims it, so nothing was compared. Either the subject pattern is wrong or the restatement is gone and the entry should go with it."
        faults=$((faults + 1))
        continue
    fi

    echo "check-facts: $name is $words, and $seen line(s) claiming it were read"

    for file in $prose; do
        [ -f "$root/$file" ] || continue

        lines=$(grep -n -i -E "$subject" "$root/$file" 2>/dev/null) || continue
        [ -n "$lines" ] || continue

        echo "$lines" | while IFS= read -r hit; do
            body=$(printf '%s' "$hit" | cut -d: -f2-)
            at=$(printf '%s' "$hit" | cut -d: -f1)

            if [ "$wants" != "-" ]; then
                if ! printf '%s' "$body" | grep -q -i -E "$wants"; then
                    echo "$file:$at  says something about $name and does not say $words, which is what the code holds"
                    echo "FAULT" >> "$root/.check-facts-faults"
                fi
            fi

            if [ "$forbids" != "-" ]; then
                if printf '%s' "$body" | grep -q -i -E "$forbids"; then
                    echo "$file:$at  says something about $name that the code contradicts"
                    echo "FAULT" >> "$root/.check-facts-faults"
                fi
            fi
        done

        checked=$((checked + 1))
    done
done < "$clean_registry"

rm -f "$clean_registry"

# The loop above runs in a subshell because of the pipe, so the count comes back through
# a file rather than a variable. It is removed whether or not anything was written.
if [ -f "$root/.check-facts-faults" ]; then
    found=$(wc -l < "$root/.check-facts-faults" | tr -d ' ')
    rm -f "$root/.check-facts-faults"
    faults=$((faults + found))
fi

if [ "$faults" -ne 0 ]; then
    echo "check-facts: a document restates a fact this repo holds in code and gets it wrong."
    exit 1
fi

echo "check-facts: $facts fact(s) read off the code and every prose line claiming one of them agrees"
echo "check-facts: it reads only the facts named in $registry. A fact nobody added is a fact nobody checks."
exit 0
