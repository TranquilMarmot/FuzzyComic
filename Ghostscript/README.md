# Ghostscript

This project is using the [Magick.NET](https://github.com/dlemstra/Magick.NET) bindings of the [ImageMagick](https://imagemagick.org/index.php) library to render PDF files.

[Ghostscript](https://www.ghostscript.com/) is the underlying library used by ImageMagick to rasterize images from PDF files.

This folder contains the AGPL licensed `.dll` and `.exe` files for `Ghostscript 9.50 for Windows (64 bit)` downloaded from https://www.ghostscript.com/download/gsdnld.html

They were acquired by installing the current version of Ghostscript on a Windows machine, locating the install directory, and copying over the needed files.

This folder is included in the output of this project by the following lines in [FuzzyComic.csproj](../FuzzyComic.csproj):

```xml
<ItemGroup>
    <Content Include="Ghostscript\*.*">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>  
</ItemGroup>
```

`Magick.NET` is then told to look in this directory to find the files it needs. This way, on Windows machines, the user does not need to install Ghostscript manually. The user does, however, need to manually install Ghostscript if they are on a Linux or macOS machine.