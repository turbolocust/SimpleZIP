﻿// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Algorithm.Type
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the RAR archiving algorithm. Supports RAR4 only.
    /// </summary>
    public class Rar : ArchivingAlgorithm
    {
        /// <inheritdoc />
        public Rar(AlgorithmOptions options) : base(ArchiveType.Rar, options)
        {
        }

        /// <inheritdoc />
        protected override WriterOptions GetWriterOptions()
        {
            throw new NotSupportedException(I18N.Resources
                .GetString("WritingNotSupported/Text", nameof(Rar)));
        }
    }
}