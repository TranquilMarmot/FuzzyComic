# FuzzyComic

![Release](https://github.com/TranquilMarmot/FuzzyComic/workflows/Release/badge.svg)

## About

FuzzyComic is a **simple** comic reader, tailored for use on touch-screen devices. It currently runs on Windows, Linux, and macOS.

FuzzyComic is capable of reading [`.cbz` (comic book zip) and `.cbr` (comic book rar)](https://en.wikipedia.org/wiki/Comic_book_archive) and `.pdf` files. It will go through images in archives in alphanumerical order.

Double-tap the middle of the screen to open up the main menu. FuzzyComic does _not_ have any fancy library or comics detection built-in. It simply opens the files you tell it to. It will, however, keep track of where you are in each comic so when you open a comic back up you can continue where you left off.

There is a small progress bar on the bottom of the screen, and a configurable background color.

<img src="Images/ui.png" width="500px">
<img src="Images/ui-reading.png" width="500px">

## Running

First, you will need to [install .NET Core](https://dotnet.microsoft.com/download) for your operating system.

Before running, you'll have to install dependencies with:

```sh
dotnet restore
```

Then, the application can be run with:

```sh
dotnet run
```

To run on Linux:

```sh
dotnet run -r linux-x64
```

To run on macOS:

```sh
dotnet run -r osx-x64
```

### Developing

The UI is cross-platform and is built using [Avalonia UI](http://avaloniaui.net/).

---

Archive decompression is done using [SharpCompress](https://github.com/adamhathcock/sharpcompress) which is capable of reading both zip and rar archives. Files are read in a streaming fashion; images are not opened until their page is opened.

---

PDF loading is done via ImageMagick using [Magick.NET](https://github.com/dlemstra/Magick.NET). This requires a dependency on [Ghostscript](https://www.ghostscript.com/) to rasterize PDFs.

Currently, loading PDFs on Linux/macOS requires that you install the latest version of [Ghostscript](https://www.ghostscript.com/download/gsdnld.html). On Windows, the proper `.dll` and `.exe` files are included in the `Ghostscript` folder in this repo. See [Ghostscript/README.md](./Ghostscript/README.md) for more information.

**NOTE:** Ghostscript is licensed under AGPL, so be careful if you're distributing the source for this project!

## Publishing

To publish a single, self-contained executable that has _no dependencies_, run:

```sh
dotnet publish -r win-x64 -c Release FuzzyComic.csproj
```

`win-x64` can be replaced with i.e. `linux-x64` or `osx-x64` or any other [Runtime Identifier](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) to publish for different platforms.

**NOTE:** Running on macOS or Linux requires the user to also have [Ghostscript](https://www.ghostscript.com/download/gsdnld.html) installed to open PDFs. Image archives work out-of-the-box. See [Ghostscript/README.md](./Ghostscript/README.md) for more information.

This command will output the directory that it is publishing the executable to.

### Releasing

See https://github.com/TranquilMarmot/FuzzyComic/releases for a list of releases.

To cut a release, simply push up a tag that matches `v*` (i.e. `v1.0` or `v2.0-beta`)

When a tag is pushed, GitHub actions will automatically pick it up and build artifacts for Windows, Linux, and macOS, and upload them to the release.

See [.github/workflows/release.yml](./.github/workflows/release.yml) for the steps in the workflow.