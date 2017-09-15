# SimpleZIP

<b>A very simple archiving application for the Windows UWP platform</b> *(optimized for mobile phones)*.

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

<br />This app makes use of the <a href="https://github.com/adamhathcock/sharpcompress">SharpCompress</a> library and depends on its quality when it comes to compression and decompression. While this app also runs on the desktop, the UI is mainly optimized for phones.
<br /><br />

# Plans for upcoming releases

  - Support for password protected ZIP files
  - Feature to compute hash values (MD5, SHA256...)
  - Basic RAR support

# Screenshots

<img src="https://homepages.fhv.at/mfu7609/images/simplezip_main_page.PNG" alt="main page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_sidebar.PNG" alt="side bar image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_compression_page.PNG" alt="compression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_decompression_page.PNG" alt="decompression page image"/><br />
<img src="https://homepages.fhv.at/mfu7609/images/simplezip_readarchive_page.PNG" alt="read archive page image"/>
