// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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

namespace SimpleZIP_UI.Application
{
    internal interface ICancelRequest
    {
        /// <summary>
        /// True if cancel request has been made, false otherwise.
        /// </summary>
        bool IsCancelRequest { get; }

        /// <summary>
        /// Resets the current request for cancellation. After this operation
        /// <see cref="IsCancelRequest"/> returns <code>false</code>.
        /// </summary>
        void Reset();
    }
}
