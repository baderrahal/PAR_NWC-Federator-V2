---
paths:
  - "tests/**"
---

# Rules for the tests

- NUnit 3, one project, tests\Federator.Core.Tests. The folders are named after the
  Core folder they test, and Samples.cs sits at the root. A test file is named after
  what it proves, with Tests on the end
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
  under a temp folder it creates and removes
- A test that reads the checkout finds it by walking up from the test assembly, so
  moving the test file does not break it
- A test that only means anything on Windows, a UNC path or a drive letter, calls
  Assert.Ignore when the path separator is not a backslash, so it reads as skipped
  and not as failed elsewhere
- Set names and locators are compared Ordinal and never trimmed, because two set
  names in the reference file end in a space
- Sample data in a test is the reference file's, 1104-PAR_CLASH_AllInOne, with its
  measured numbers, 61 sets, 1830 tests, 102 conditions, 53 distinct. Search_Set_
  Building.xml and Search_Set_Infra.xml are damaged exports kept to prove HealthCheck
  catches them, never a reference for a good file
- The pre-commit hook in .githooks runs the whole set and refuses the commit on a
  failure. In a container without Windows a fixed set of path and file locking tests
  fail, and that count goes in the log entry before and after every fix
