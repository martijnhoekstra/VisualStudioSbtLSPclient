# Visual Studio LSP extensions for Scala

This repository holds a number of clients and utitilities for different LSP servers in Scala for use in Visual Studio.

LSP is the new hotness. With sbt including a built-in LSP server, ensime providing an LSP server, and MetaLS providing an experimental LSP server,
the Scala tooling landscape is quickly evolving (for better or worse) to make use of the LSP as the standard means of providing rich
editing tools for Scala.

Visual Studio Code already has various LSP clients, Atom has LSP integration, and many other editors have plugins to do the same as well.

This project aims to provide clients for Visual Studio.

## Status

At the moment, this project provides two LSP clients, one for sbt, and one for Metals. The sbt client works. The metals client needs more work. An Ensime client isn't implemented yet.

For specific instructions for each client can be found in the readme of the clients themselves.

All clients depend on the Microsoft.VisualStudio.LanguageServer.Client package. This package is available as a preview only, and the clients will only work for Visual Studio 2017 Preview.

That also means that they're built on top of experiemental software. Things may break without notice. Though it is the intention to keep up-to-date on the changes as they happen,
and keep things in good working order, I can't give you a response time promise or anything of that sort.

In addition, the servers themselves are also in various states of experimentality. Metals is alpha software, not everything works, and not everything that works works
as it should. SBT server isn't very rich on features. Ensime LSP is at the moment not very maintained, and if it does end up being maintained, it's probably going to be split
out, requiring additional work to integrate it again.

When all dependencies of a language clien are no longer experimental, and the language client has a straight-forward installation option in mainline Visual Studio,
the client should see its first stable release.

## License
Copyright Martijn Hoekstra.

This software is offered under the LGPLv3: see LICENSE.md

All the textmate stuff found in the TextMate directory is shamelessly stolen from Mads Hartmann Jensen, and is available under the MIT license. See the LICENSE file in the TextMate directory for details.