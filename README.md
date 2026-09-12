# Parsons NWC Federator

Navisworks Manage 2025 add-in. Reads a folder of discipline NWC files, gathers them
into federations, saves an NWF and an NWD per federation, runs the clash tests, and
writes one report per federation.

The rules the code keeps are in `CLAUDE.md`. The worker's notes are in `steps`.

## Where things are

- `docs/workflow.md` says what the tool does on a first run, a weekly run, a rebuild and
  the open file, and what the labels in the window mean
- `CLAUDE.md` holds the rules every file keeps, and `.claude/rules` holds the rules for
  one folder each, read when a file under that folder is touched
- `steps` holds the worker's notes: the analysis, what comes next, the open questions,
  the steps for Bader's machine, and the log
- `docs/history` holds the measurements the rules were read off, on the dates they were
  taken. History, not current
- `tools/probes` holds the scripts those measurements were taken with
- `build/install.ps1` builds the bundle and installs it
