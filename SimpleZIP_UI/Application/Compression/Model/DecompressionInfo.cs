// ==++==
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


namespace SimpleZIP_UI.Application.Compression.Model
{
    internal class DecompressionInfo : OperationInfo
    {
        /// <summary>
        /// Aggregated item which is to be extracted.
        /// </summary>
        internal ExtractableItem Item { get; }

        /// <summary>
        /// True to collect extracted file names, false otherwise.
        /// </summary>
        internal bool IsCollectFileNames { get; set; }

        internal DecompressionInfo(ExtractableItem item, ulong size) : base(size)
        {
            Item = item;
        }
    }
}
