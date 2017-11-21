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

Supported message digest algorithms:
  - MD5
  - SHA1
  - SHA256
  - SHA384
  - SHA512

<br />This app makes use of the <a href="https://github.com/adamhathcock/sharpcompress">SharpCompress</a> library and depends on its quality when it comes to compression and decompression. While this app also runs on the desktop, the UI is (currently) optimized for smaller devices.
<br /><br />

# Plans for upcoming releases

  - Support for password protected ZIP files

# Development requirements

The minimum requirements for this project are currently as follows:

  - Visual Studio 2017
  - Microsoft Windows 1709 (Fall Creators Update)

If you are planning to fork this application you may want to change this in the project configuration file.

# Screenshots

<img src="https://homepages.fhv.at/mfu7609/images/simplezip_hashing.PNG" width="489" height="870" alt="hashing page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_compression.PNG" width="489" height="870" alt="compression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_decompression.PNG" width="489" height="870" alt="decompression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_readarchive.PNG" width="489" height="870" alt="read archive page image"/>
