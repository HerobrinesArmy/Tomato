# Tomato

Tomato is the collective name for a series of tools built around DCPU-16 emulation. Tomato
itself is a .NET library for emulating DCPU-16 and all official 0x10c hardware. Also included
is Lettuce, a graphical debugger for DCPU-16 programs. Also included is Pickles, a text-based
debugger for the command line. All three run on Windows, Linux, and Mac.

[![Download latest version](http://i.imgur.com/cMhpN.png)](http://sircmpwn.github.com/Tomato/Lettuce.zip)

## Features

* Support for the [1.7 DCPU-16 specification](http://pastebin.com/raw.php?i=Q4JvQvnM)
* Support for the following officially adopted 0x10c devices:
  * LEM-1802
  * Generic Keyboard
  * Generic Clock
  * SPC-2000
  * SPED-3
  * M35FD
* Accurate 100 KHz timing
* Modular - use Tomato in your own projects easily
* Load up [Organic](https://github.com/SirCmpwn/Organic) listings for better debugging
* Record LEM-1802 and SPED-3 displays as an animated GIF
* Use any number of devices in any configuration

And many more features are included.

![Lettuce](http://i.imgur.com/DHQ9jbC.png)

**[Click to Enlarge](http://i.imgur.com/DHQ9jbC.png)**

## Installation

On Linux and Mac, you must first install Mono to use any Tomato-based software.

### Tomato

Tomato is easy. Simply head to the [Downloads](https://github.com/SirCmpwn/Tomato/downloads)
page and grab Tomato.dll, which you can immediately begin using in your projects.

### Lettuce

Lettuce is also easy, but requires OpenGL to emulate SPED-3 devices. Grab Lettuce from the
[Downloads](https://github.com/SirCmpwn/Tomato/downloads) page and head over to
http://www.opengl.org/ for information on installing OpenGL (hint: you probably already have
it installed).

### Pickles

Pickles is as easy as Tomato. Grab it from the
[Downloads](https://github.com/SirCmpwn/Tomato/downloads) page to get started.

## Usage

If you just want a quick emulator, grab [Lettuce](https://github.com/SirCmpwn/Tomato/downloads)
and run it. On Windows, double click the file. On Linux or Mac, run this:

    mono Lettuce.exe

A nice little wizard will guide you through the rest.

For advanced users, head over to the [wiki](https://github.com/SirCmpwn/Tomato/wiki) for
extensive documentation on all Tomato-related software.

## Building from Source

Clone the repository. If you're on Linux/Mac, make sure you have Mono installed. Then, open
the root of the repository and follow these instructions:

*Windows*:

Add "C:\Windows\Microsoft.NET\Framework\v4.0.30319" to your path. Run this:

    msbuild

*Linux/Mac*:

Install mono and run this from the root of the repository:

    xbuild
