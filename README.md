# Arbiter

Only run the tests you need based on the changes between builds.

## Getting Started

When run against an MSBuild solution file, and provided with a range of commits
to check and an ouput file, Arbiter will produce an [NUnit](https://nunit.org/)
project file that can be passed to nunit to execute.

```
Usage:
  arbiter [options] [command]

Options:
  -s, --solution <solution> (REQUIRED)                        The path to the solution (.sln) file to evaluate.
  -f, --from-commit <from-commit> (REQUIRED)                  The start of the commit range to evaluate.
  -t, --to-commit <to-commit> (REQUIRED)                      The end of the commit range to evaluate.
  -np, --nunit-project <nunit-project> (REQUIRED)             The nunit project file containing all test assemblies to
                                                              consider.
  -nc, --nunit-configuration <nunit-configuration>            The configuration in the project to consider.
  (REQUIRED)
  --version                                                   Show version information
  -?, -h, --help                                              Show help and usage information

Commands:
  clean-com    Return to a clean state without support for additional project types.
  setup-com    Setup support for projects with COM references.
  diff         Get the changed files between two commits
  sort         Output a topological sort of the projects in a solution.
  graph        Ouput a graph in dot format.
```

## Dependencies

- An installation of MSBuild, compatible with [MSBuildLocator](https://github.com/Microsoft/MSBuildLocator)
- An installation of [git](https://git-scm.com/) available on the PATH.
- .NET Framework 4.8

## Build

[![build](https://circleci.com/gh/ohlookitsben/arbiter.svg?style=svg)](https://circleci.com/gh/ohlookitsben/arbiter)