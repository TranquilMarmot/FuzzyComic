# FuzzyComic

## About

FuzzyComic is a **simple** comic reader, tailored for use on touch-screen devices. It currently runs on Windows, Linux, and macOS.

Currently, FuzzyComic is capable of reading [`.cbz` (comic book zip) and `.cbr` (comic book rar)](https://en.wikipedia.org/wiki/Comic_book_archive) and `.pdf` files. It will go through images in archives in alphanumerical order.

Double-tap the middle of the screen to open up the main menu. FuzzyComic does _not_ have any fancy library or comics detection built-in. It simply opens the files you tell it to.

There is a small progress bar on the bottom of the screen, and a configurable background color.

<img src="Images/ui.png" width="500px">
<img src="Images/ui-reading.png" width="500px">

## Running

First, you will need to [install .NET Core](https://dotnet.microsoft.com/download) for your operating system.

Before running, you'll have to install dependencies with

```sh
dotnet restore
```

Then, the application can be run with

```sh
dotnet run
```

### Developing

The UI is cross-platform and is built using [Avalonia UI](http://avaloniaui.net/).

---

Archive decompression is done using [SharpCompress](https://github.com/adamhathcock/sharpcompress) which is capable of reading both zip and rar archives. Files are read in a streaming fashion; images are not opened until their page is opened.

---

PDF loading is done via ImageMagick using [Magick.NET](https://github.com/dlemstra/Magick.NET). This requires a dependency on [Ghostscript](https://www.ghostscript.com/) to rasterize PDFs.

Currently, loading PDFs on Linux/macOS requires that you install the latest version of [Ghostscript](https://www.ghostscript.com/download/gsdnld.html). On Windows, the proper `.dll` and `.exe` files are included in the `Ghostscript` folder in this repo.

**NOTE:** Ghostscript is licensed under AGPL, so be careful if you're distributing the source for this project!
