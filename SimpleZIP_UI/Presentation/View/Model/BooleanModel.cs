﻿// ==++==
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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleZIP_UI.Presentation.View.Model
{
    public class BooleanModel : INotifyPropertyChanged
    {
        private bool _isTrue;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTrue
        {
            get => _isTrue;
            set
            {
                if (value != _isTrue)
                {
                    _isTrue = value;
                    OnPropertyChanged(nameof(IsTrue));
                }
            }
        }

        public BooleanModel(bool isTrue = false)
        {
            _isTrue = isTrue;
        }

        public static implicit operator BooleanModel(bool value)
        {
            return new BooleanModel(value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
