#!/bin/sh
#
# Refuses a local variable declared twice in one method scope.
#
# WHY THIS EXISTS AT ALL, AND WHY IT IS NOT A COPY OF A RULE THAT LIVES SOMEWHERE ELSE.
# The compiler already knows this rule, and the compiler is the right place for it. The
# add-in cannot be compiled by anything but a machine with Navisworks Manage 2025 on it,
# so for src\Federator.Addin the compiler runs once, on Bader's machine, at step 8 of
# steps\03_bader_next.md, after every change of a round has already been written. F52
# shipped a method declaring views twice, which is CS0128, and nothing saw it for a day
# because nothing here can see it. This is the one place the rule can run before then.
#
# WHAT IT CANNOT DO. It reads text and not a program. It knows nothing about types,
# members, arguments or the Navisworks API, and it is not a build and never says a build
# passed. It catches one shape of fault, the one that cost a day, and nothing else.
#
# Exit 0 clean, exit 1 with one line per fault, exit 2 if it could not read the tree.

set -e

root=${1:-src}

if [ ! -d "$root" ]; then
    echo "check-locals: no folder named $root" >&2
    exit 2
fi

found=$(find "$root" -name '*.cs' ! -path '*/obj/*' ! -path '*/bin/*' | sort)

if [ -z "$found" ]; then
    echo "check-locals: no C# file under $root" >&2
    exit 2
fi

echo "$found" | while IFS= read -r file; do
    awk -v file="$file" '
    function flush_scopes(    i) {
        for (i in declared) {
            delete declared[i]
        }
    }

    {
        line = $0

        # A brace or a slash inside a literal is text and not code, so the literals go
        # first. Without this a string holding a brace moves the depth and every scope
        # after it reads wrong.
        gsub(/"([^"\\]|\\.)*"/, "\"\"", line)
        gsub(/'"'"'([^'"'"'\\]|\\.)'"'"'/, "@", line)

        sub(/\/\/.*$/, "", line)

        if (inmethod) {
            # A declaration with an initialiser, which is the only shape this repo uses
            # for a local. The type is anything, the name starts lower case, and what
            # follows is one equals sign and not two.
            if (match(line, /^[ \t]*[A-Za-z_][A-Za-z0-9_.<>,\[\]?]*[ \t]+[a-z_][A-Za-z0-9_]*[ \t]*=[^=]/)) {
                head = substr(line, RSTART, RLENGTH)
                sub(/^[ \t]*/, "", head)
                type = head
                sub(/[ \t].*$/, "", type)
                name = head
                sub(/^[^ \t]+[ \t]+/, "", name)
                sub(/[ \t]*=.*$/, "", name)

                if (type != "return" && type != "new" && type != "case" \
                    && type != "else" && type != "do" && type != "yield") {
                    if (name in declared) {
                        printf "%s:%d  the local %s is declared again, first at line %d\n", \
                            file, NR, name, declared[name]
                        faults++
                    } else {
                        declared[name] = NR
                        at[name] = depth
                    }
                }
            }
        }

        opens = gsub(/{/, "{", line)
        closes = gsub(/}/, "}", line)

        for (i = 0; i < opens; i++) {
            depth++

            if (!inmethod && depth >= 3) {
                inmethod = 1
                methoddepth = depth
                flush_scopes()
            }
        }

        for (i = 0; i < closes; i++) {
            depth--

            if (inmethod) {
                for (n in declared) {
                    if (at[n] > depth) {
                        delete declared[n]
                        delete at[n]
                    }
                }

                if (depth < methoddepth) {
                    inmethod = 0
                    flush_scopes()
                }
            }
        }
    }

    END { exit faults > 0 ? 1 : 0 }
    ' "$file" || echo "FAULT" >&2
done 2>&1 | {
    faults=0

    while IFS= read -r said; do
        if [ "$said" = "FAULT" ]; then
            faults=$((faults + 1))
        else
            echo "$said"
        fi
    done

    if [ "$faults" -gt 0 ]; then
        echo "check-locals: $faults file(s) declare a local twice in one scope. That is CS0128 and the build will refuse it."
        exit 1
    fi

    echo "check-locals: no local is declared twice in one scope under $root"
    exit 0
}
