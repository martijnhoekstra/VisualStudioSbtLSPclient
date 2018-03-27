# Visual Studio LSP client for sbt Server

This is an LSP client for sbt Server, providing a rich editor bringing the features of [https://www.scala-sbt.org/1.x/docs/sbt-server.html](sbt-servers LSP implementation
to Visual Studio) for Visual Studio 2017 Preview

## Installation

As a prerequisite, you need to have sbt installed, minimum version 1.1.0, and the sbt launcher should be available on your path. Particularly, running `sbt` in the root directory
of your project should launch sbt.

If sbt runs fine, all is fine. If not, look at your java install. At the moment I recommend having your JAVA_HOME set to a Java 8 for the best compatibility, as Java 9 support
in scala is still having kinks ironed out, and launching sbt will use the java in your JAVA_HOME.

For issues with sbt, I unceremoniously redirect you to sbt.

Install the vsix from **marketplacelink**

## Usage

In Visual Studio 2017 Preview, use "open folder" on the root directory of your scala sbt project (i.e. the folder where your `build.sbt` is located). When you open any 
`.scala` file, sbt should start, and this client should connect to the language server it provides.

### Un-usage

* This extension **does not work** on any other editor than Visual Studio 2017 Preview
* This extension **does not work** in any other mode than "Open Folder"

## Features

This client simply connects the Visual Studio LSP Client package with sbt server. As such, it should have the intersection of features the Visual Studio LSP Client package supports, 
and all features exposed by sbt server. If sbt server grows new features, these will be picked up automatically for as far the Visual Studio LSP Client package supports them.

## License
Copyright Martijn Hoekstra.

This software is offered under the LGPLv3: see LICENSE.md

All the textmate stuff found in the TextMate directory is shamelessly stolen from Mads Hartmann Jensen, and is available under the MIT license. See the LICENSE file in the TextMate directory for details.