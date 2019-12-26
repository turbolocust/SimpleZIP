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

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleZIP_UI.Presentation.View.Model
{
    public class HashAlgorithmModel : INotifyPropertyChanged
    {
        private string _algorithm;

        public event PropertyChangedEventHandler PropertyChanged;

        public string HashAlgorithm
        {
            get => _algorithm;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (!value.Equals(_algorithm, StringComparison.Ordinal))
                {
                    _algorithm = value;
                    OnPropertyChanged(nameof(HashAlgorithm));
                }
            }
        }

        public HashAlgorithmModel(string algorithm = null)
        {
            _algorithm = algorithm;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
