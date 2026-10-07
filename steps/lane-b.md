# Lane B, the cloud session

Lane B works in Anthropic's cloud at night on the release items Bader named on 7 Oct 2026, and
takes only what a Core test proves. This is its one record. It writes nowhere else under steps,
.claude, steps\history, the tracker or PROGRESS.md, so the loop reads this file and moves each
line into its own records.

## What the loop has to do for lane B's pull requests

- fix-F115 and fix-F127 stay on origin and are not deleted. Their own records, the rules lines of
  .claude\rules\core.md, the section of steps\01_next.md and the entry of steps\history\log.md,
  are on those branches and not in lane B's pull requests, since lane B may not edit those paths.
  The loop lands them from the branches and then deletes them
- Every status in steps\tracker.csv for an item below is the loop's to set from this file

## Points the readers raised on F115 that lane B dropped, for the loop

Each needs the add-in or a measurement, or is older than F115, or changes no number the team sees.
Lane B fixed three: the EMPTY SETS wording that said no model in the project carries a value where
only the models measured so far were read, the window totals that called a rebuilt set left alone,
and the row that said how many worksets a stopped walk had seen.

- Add-in, UNKNOWN until a run with Navisworks: the EMPTY SETS block is written only after a test
  runs, so a picked file of sets alone and a clash step that throws write none, and the Build sets
  button never prints it. A set AddCopy put nowhere findable is still counted created. A set
  rebuilt and then a throw leaves RebuiltCount low. A rebuilt set is not checked against the file
  and what SearchCondition.Options reads back for an Or set is unmeasured, so such sets may read
  DRIFTED every run. Two sets of one name in one folder read the wrong one. A set the plan skipped
  but the file names can be removed as a leftover. A damaged document is still saved after Q74.
  A set with a search that is null reads as nothing at all. The Closing line of the window for a
  rebuilt set per set still says left alone
- Core, left as designed by the loop: a present set whose search could not be read is never judged
  empty, so the SETS count at zero is higher than the EMPTY SETS header and the SETS block says
  how many could not be read. The judge reads only the first value of a set and ignores Or groups,
  where HEALTH reads them. The HEALTH count of categories no model carries has no project guard.
  SETS ACROSS THE RUN says every set found something over a run with no group. A failed list read
  is cached for the life of the window. The not-a-typo pairs are one project's
- Words and polish: three comments say 374 categories and 39 worksets where the judge adds more,
  two summary elements in a row in SetResult, the test of Is.EqualTo on collections in five tests,
  a second ContainsTest in SetWarnings, the walk not disposing five sub objects which FR-029 holds
- Members with no caller that F115 did not add: RevitWorksets.DecidedCount, PlannedSet.GroupCount
  and Groups, and the long forms of AddCreated and EmptySets.Lines used by tests only

## The record

One line per item: its ID, what changed, the pull request and its status.

| ID | What changed | PR | Status |
|---|---|---|---|
| F115 | fix-F115 carried on: main merged in, two source conflicts resolved, main's own AlsoAskTests moved to the judge form the branch introduced, attempt 2 words of three lines made true | 142 | second reading |
