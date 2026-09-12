---
paths:
  - "tests/**"
---

# Rules for the tests

- NUnit 3, one project, tests\Federator.Core.Tests. The folders are named after the
  Core folder they test, and Samples.cs sits at the root holding the three helpers every
  fixture shares: Samples for the files, TestPaths for a path, TempFolder for a folder to
  write in. A test file is named after what it proves, with Tests on the end
- Nothing in the tests references Navisworks. What calls the Navisworks API is proved
  by a run on the local machine and reviewed here, never mocked
- Every check gets a test that breaks one thing and asserts the check names it. A test
  that only asserts the good file passes would have passed against every real
  difference the report checks have missed
- A written file is read back off the disk as a file, never through the object that
  wrote it. The xlsx is opened as the zip it is to read a relationship, and the page
  is opened as text to read a src, because the object model has reported a broken
  file as fine more than once
- A list read off the client's files, the column set, the colours, the row heights and
  the widths, is asserted against those files on every run, so the list answers to
  the files and not to whoever typed it
- The samples are read and never written. A test that needs a file on disk writes it
  under a temp folder it creates and removes, made by TempFolder.Make and removed by
  TempFolder.Remove. The same three lines and the same sentence about a leftover folder
  sat in nineteen files before that, which is one rule written nineteen times
- A path in a test is BUILT and never typed. TestPaths.At gives a rooted path in the
  spelling of whatever machine is running, C:\a\b on Windows and /a/b elsewhere. Typing
  C:\out\reports made 28 tests fail in the container on the separator alone while all of
  them passed on the Windows runner, and it made one pass off Windows without proving
  anything, because a path with no separator in it is just a long file name whose parts
  still read correctly
- A list is compared element by element, the count first and then each one. Three tests
  compared a ReadOnlyCollection against an array with Is.EqualTo, which passed on the
  Windows runner and failed under mono, and whether that is a Windows difference or a
  mono one is UNKNOWN. Comparing the way the assertion reads answers neither question and
  needs no answer
- A test that reads the checkout finds it by walking up from the test assembly, so
  moving the test file does not break it
- A test that only means anything on Windows calls TestPaths.OnWindowsOnly, which skips
  it off Windows with a line saying what it needs, so it reads as skipped and not as
  failed. That is for a rule of the file SYSTEM and never for a rule of this tool: a UNC
  path, a drive letter that names no drive, matching two paths without case, and a file
  held open refusing to be deleted or read. Where the tool's own rule can be proved
  another way, it is proved rather than skipped. A copy into a folder that cannot exist
  is now a folder under a file, which no system makes
- Set names and locators are compared Ordinal and never trimmed, because two set
  names in the reference file end in a space
- Sample data in a test is the reference file's, 1104-PAR_CLASH_AllInOne, with its
  measured numbers, 61 sets, 1830 tests, 102 conditions, 53 distinct. Search_Set_
  Building.xml and Search_Set_Infra.xml are damaged exports kept to prove HealthCheck
  catches them, never a reference for a good file
- The pre-commit hook in .githooks runs the whole set and refuses the commit on a
  failure. Since F16 the whole set PASSES in the container, with only the tests that
  need Navisworks or a Windows file system rule skipping, so a failure here is a real
  one. The passed, failed and skipped counts still go in the log entry before and after
  every fix
