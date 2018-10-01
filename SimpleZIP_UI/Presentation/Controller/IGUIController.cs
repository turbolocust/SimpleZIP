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

using SimpleZIP_UI.Application;

namespace SimpleZIP_UI.Presentation.Controller
{
    /// <summary>
    /// Defines the minimum contract for a GUI controller.
    /// </summary>
    internal interface IGuiController
    {
        /// <summary>
        /// Aggregated navigation handler for navigation between pages.
        /// </summary>
        INavigation Navigation { get; }

        /// <summary>
        /// Aggregated object for password requests. Can be <code>null</code>.
        /// </summary>
        IPasswordRequest PasswordRequest { get; }
    }
}
