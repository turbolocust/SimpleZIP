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

namespace SimpleZIP_UI.Presentation.View.Model
{
    /// <summary>
    /// Represents a list box item for the <see cref="MessageDigestPage"/>.
    /// </summary>
    public class MessageDigestModel
    {
        /// <summary>
        /// The name of the file whose hash value is to be displayed.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The physical location (full path) of the file.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// The calculated hash value of the file.
        /// </summary>
        public string HashValue { get; internal set; }

        /// <summary>
        /// Displays the location in the model if set to true.
        /// </summary>
        public BooleanModel IsDisplayLocation { get; set; } = false;

        /// <summary>
        /// Sets the color brush of the <see cref="FileName"/>.
        /// </summary>
        public SolidColorBrushModel FileNameColorBrush { get; set; }

        /// <summary>
        /// Constructs a new model for the ListBox in <see cref="MessageDigestPage"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to be displayed.</param>
        /// <param name="location">The location of the file to be displayed.</param>
        /// <param name="hashValue">The hash value of the file to be displayed.</param>
        public MessageDigestModel(string fileName, string location, string hashValue)
        {
            FileName = fileName;
            Location = location;
            HashValue = hashValue;
        }
    }
}
