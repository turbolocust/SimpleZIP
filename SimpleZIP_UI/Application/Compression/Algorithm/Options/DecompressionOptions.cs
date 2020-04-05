﻿// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using System.Text;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Options
{
    public class DecompressionOptions : IDecompressionOptions
    {
        /// <inheritdoc />
        public bool LeaveStreamOpen { get; }

        /// <inheritdoc />
        public Encoding ArchiveEncoding { get; }

        /// <inheritdoc />
        public string Password { get; set; }

        public DecompressionOptions(bool leaveStreamOpen, Encoding encoding, string password = null) =>
            (LeaveStreamOpen, ArchiveEncoding, Password) = (leaveStreamOpen, encoding, password);
    }
}