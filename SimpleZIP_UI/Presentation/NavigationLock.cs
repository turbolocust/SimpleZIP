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

using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation
{
    /// <summary>
    /// Singleton class which allows global locking
    /// of user navigation in NavigationView
    /// (see <see cref="NavigationViewRootPage"/>).
    /// </summary>
    internal class NavigationLock
    {
        private volatile bool _isLocked;

        /// <summary>
        /// True if navigation has been locked, false otherwise.
        /// </summary>
        internal bool IsLocked
        {
            get => _isLocked;
            set => _isLocked = value;
        }

        private NavigationLock()
        {
            // is singleton
        }

        /// <summary>
        /// Lock object which is used for double-checked locking
        /// when retrieving the singleton instance of this class.
        /// </summary>
        private static readonly object LockObj = new object();

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        private static NavigationLock _instance;

        /// <summary>
        /// The singleton instance of this class. This property is thread-safe.
        /// </summary>
        public static NavigationLock Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObj)
                    {
                        _instance = new NavigationLock();
                    }
                }

                return _instance;
            }
        }
    }
}
