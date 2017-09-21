// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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
namespace SimpleZIP_UI.Application.Compression
{
    /// <summary>
    /// An observer that gets informed on progress updates.
    /// </summary>
    /// <typeparam name="T">Type of the progress value.</typeparam>
    public interface IProgressObserver<in T>
    {
        /// <summary>
        /// Updates any value using the specified new one.
        /// </summary>
        /// <param name="value">The updated value.</param>
        void Update(T value);
    }
}
