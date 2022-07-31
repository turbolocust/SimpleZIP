// ==++==
// 
// Copyright (C) 2022 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleZIP_UI.Application.Compression.Compressor;
using System;

namespace SimpleZIP_UI_TEST.Application.Compression.Algorithm
{
    [TestClass]
    public sealed class StringGzipCompressorTests
    {
        [TestMethod]
        public void Compress_ShouldReturnBase64EncodedString()
        {
            // ARRANGE
            const string toBeCompressed = "4711";
            var compressor = new StringGzipCompressor();

            // ACT
            var compressedString = compressor.Compress(toBeCompressed);

            // ASSERT
            compressedString.Invoking(s => Convert.FromBase64String(s)).Should().NotThrow<FormatException>();
        }

        [TestMethod]
        public void Decompress_ShouldReturnExpectedString_WhenCompressedAndBase64EncodedStringIsProvided()
        {
            // ARRANGE
            const string toBeCompressed = "4711";
            var compressor = new StringGzipCompressor();

            // ACT
            var compressedString = compressor.Compress(toBeCompressed);
            var decompressedString = compressor.Decompress(compressedString);

            // ASSERT
            decompressedString.Should().Be(toBeCompressed);
        }
    }
}
