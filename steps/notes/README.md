# steps\notes

Notes a later session of the loop depends on. They were written in the work folder,
%LOCALAPPDATA%\NwcFederatorLoop\turn1, during turns 1 and 2, bar the two F103 files, written
in its turn3 folder during turn 3, and are committed here so no session has to find them on
one machine. Each file is the note as it was written, copied on 2026-09-29, or on 2026-09-30
for f103-move-proof.txt, except where the table says masked. The first sixteen hex characters of the sha256 are of the note in the
work folder, before any mask.

| file | what it is | bytes | sha256 | copy |
| --- | --- | --- | --- | --- |
| f100-fix-list.md | the first fix list for the F100 probe, from its first reviewer and breaker | 9166 | 58759fd55743d49e | as written |
| f100-fix-list-2.md | the second fix list, fix attempt 2 | 5949 | a39b739b5c6f86ff | as written |
| f100-fix-list-3.md | the third fix list, fix attempt 3, the last the house rule allowed | 6553 | 1e1f1185356f2b4f | as written |
| f100-fourth-reading.md | the reading of fix attempt 3: F1 to F4 and B1 to B3, the faults fix attempt 4 fixes by Bader's answer to Q79 | 3569 | 0c2039e55acddaaa | as written |
| f101-design.md | the design of the no-click entry, Phase 1 item 5, with the prober's questions PQ1 to PQ8 | 15497 | eba1edce1f1473cb | masked, one machine name replaced by [the machine of 2026-09-19] |
| f103-design.md | the design of tools\loop\run.ps1, two designers and a judge, turn 3, which F103 part 1 builds from | 75698 | 9242dc91093ece35 | as written |
| f103-move-proof.txt | every line of tools\loop\nw-guard.ps1 at 377cb1a mapped to its line of the probe at 0eb4ede, or named wrapper, comment, blank or changed with both texts, made by replaying the move's generator and checked against the committed file, fix list 1 item 1 | 119303 | a988e552c34bb0f3 | as written |

Line numbers in the F100 notes are of the probe at the commit each note names, not of the
probe as it stands.
