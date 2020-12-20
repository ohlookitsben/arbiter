# Arbiter

Only run the tests you need based on the changes between builds.

## Getting Started

When run against an MSBuild solution file, and provided with a range of commits
to check and an ouput file, Arbiter will produce an [NUnit](https://nunit.org/)
project file that can be passed to nunit to execute.

```
Usage: arbiter [solution] [from_commit] [to_commit] [output]

solution:
  The path to the solution (.sln) file to evaluate.

from_commit:
  The start of the commit range to evaluate.

to_commit:
  The end of the commit range to evaluate.

output:
  The path to write the output file.
```

## Dependencies

- An installation of MSBuild, compatible with [MSBuildLocator](https://github.com/Microsoft/MSBuildLocator)
- An installation of [git](https://git-scm.com/) available on the PATH.
- .NET Framework 4.8

## Build

[![build](https://circleci.com/gh/ohlookitsben/arbiter.svg?style=svg)](https://circleci.com/gh/ohlookitsben/arbiter)