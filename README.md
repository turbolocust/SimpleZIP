# SimpleZIP

[![Build status](https://ci.appveyor.com/api/projects/status/ofso840eiw7woaq2?svg=true)](https://ci.appveyor.com/project/turbolocust/simplezip)

<b>A very simple archiving application and hash tool for the Universal Windows Platform</b>.

This app can be found in the Windows Store: https://www.microsoft.com/en-us/store/p/simplezip/9nz7l8c54zln

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
At the current state, this application supports encrypted archives of type ZIP (legacy encryption) and RAR4 (fully encrypted).
<br /><br />
This application uses the <a href="https://github.com/adamhathcock/sharpcompress">SharpCompress</a> library and depends on its quality when it comes to compression and decompression. Unit tests exist to test updates of this library (see project _SimpleZIP_UI_TEST_). 
<br />

# Development requirements

The minimum requirements for this project are currently as follows:

  - Visual Studio 2017
  - Microsoft Windows 1709 (Fall Creators Update)

If you are planning to fork this application you may want to change this in the project configuration file.

# Screenshots

Please refer to the store page to see some screenshots.
