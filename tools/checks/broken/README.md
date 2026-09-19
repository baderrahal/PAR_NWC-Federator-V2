# Files that are meant to be wrong

Not a project and not compiled by anything. `Fine.cs` holds every shape
`check-locals.sh` must leave alone and `DeclaredTwice.cs` holds the one it must refuse,
which is the fault F52 shipped and nothing here could see until F58.

A check that only ever runs against a clean tree proves nothing, so the check is run
against this folder too and has to come back with exactly one fault, naming the file,
the line and the name.
