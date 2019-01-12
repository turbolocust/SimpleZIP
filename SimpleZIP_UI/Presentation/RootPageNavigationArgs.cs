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

using System;

namespace SimpleZIP_UI.Presentation
{
    internal class RootPageNavigationArgs
    {
        /// <summary>
        /// Page to be loaded in content frame.
        /// </summary>
        public Type Content { get; }

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="contentFrame">Page to be loaded in content frame.</param>
        public RootPageNavigationArgs(Type contentFrame)
        {
            Content = contentFrame;
        }
    }
}
