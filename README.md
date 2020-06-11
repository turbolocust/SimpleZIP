<p align="center">
  <img alt="szip logo" src="./SimpleZIP_UI/Assets/Square44x44Logo.altform-lightunplated_targetsize-256.png" width="100px" />
  <h1 align="center">SimpleZIP</h1>
</p>

[![Build status](https://ci.appveyor.com/api/projects/status/ofso840eiw7woaq2?svg=true&style=flat-square)](https://ci.appveyor.com/project/turbolocust/simplezip)
[![Store link](https://img.shields.io/badge/Microsoft%20Store-Download-orange.svg?style=flat-square)](https://www.microsoft.com/store/productId/9nz7l8c54zln)
[![Release](https://img.shields.io/github/release/turbolocust/SimpleZIP.svg?style=flat-square)](https://github.com/turbolocust/SimpleZIP/releases)
[![Stars](https://img.shields.io/github/stars/turbolocust/SimpleZIP.svg?style=flat-square)](https://github.com/turbolocust/SimpleZIP/stargazers)
[![Contributors](https://img.shields.io/github/contributors/turbolocust/SimpleZIP?style=flat-square)](https://github.com/turbolocust/SimpleZIP/graphs/contributors)

<b>A very simple archiving application and hash tool for the Universal Windows Platform</b>.

<br />Supported formats for compression:
  - ZIP (Deflate)
  - GZIP
  - TAR (Uncompressed)
  - TAR+GZIP (=Tarball)
  - TAR+BZIP2 (=Tarball)
  - TAR+LZMA (=Tarball)
  
Supported formats for decompression:
  - ZIP
  - GZIP
  - BZIP2
  - TAR
  - TAR+GZIP
  - TAR+BZIP2
  - TAR+LZMA
  - RAR4
  - RAR5

Supported message digest algorithms:
  - MD5
  - SHA1
  - SHA256
  - SHA384
  - SHA512

<br />
As of the current state, this application supports encrypted archives of type ZIP and RAR4 (fully encrypted).
<br /><br />
This application uses the <a href="https://github.com/adamhathcock/sharpcompress">SharpCompress</a> library and depends on its quality when it comes to reading and writing archives. Unit tests exist to test the used algorithms of this library (see project SimpleZIP_UI_TEST). In addition, starting from version 3.0 the <a href="https://github.com/icsharpcode/SharpZipLib">SharpZipLib</a> library is used as a fallback because of some issues I experienced with SharpCompress.
<br />

# Development requirements

The minimum requirements for this project are currently as follows:

  - Visual Studio 2019
  - Microsoft Windows 1803 (April 2018 Update)

If you are planning to fork this application you may want to change this in the project configuration file.

# Screenshots

Please refer to the store page to see some screenshots.
