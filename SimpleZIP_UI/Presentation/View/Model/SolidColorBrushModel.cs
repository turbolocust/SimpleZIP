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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;

namespace SimpleZIP_UI.Presentation.View.Model
{
    public class SolidColorBrushModel : INotifyPropertyChanged
    {
        private SolidColorBrush _brush;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public SolidColorBrush ColorBrush
        {
            get => _brush;
            set
            {
                if (value != _brush)
                {
                    _brush = value;
                    OnPropertyChanged(nameof(ColorBrush));
                }
            }
        }

        public SolidColorBrushModel(SolidColorBrush brush)
        {
            _brush = brush;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
