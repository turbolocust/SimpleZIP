# SimpleZIP

<b>A very simple archiving/hash tool for the Windows UWP platform</b> *(optimized for mobile phones)*.

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

Supported message digest algorithms:
  - MD5
  - SHA1
  - SHA256
  - SHA384
  - SHA512

<br />This app makes use of the <a href="https://github.com/adamhathcock/sharpcompress">SharpCompress</a> library and depends on its quality when it comes to compression and decompression. While this app also runs on the desktop, the UI is mainly optimized for phones.
<br /><br />

# Plans for upcoming releases

  - Support for password protected ZIP files
  - Basic RAR support

# Screenshots

<img src="https://homepages.fhv.at/mfu7609/images/simplezip_main.PNG" alt="main page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_hashing.PNG" alt="hashing page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_compression.PNG" alt="compression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_decompression.PNG" alt="decompression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_readarchive.PNG" alt="read archive page image"/>
