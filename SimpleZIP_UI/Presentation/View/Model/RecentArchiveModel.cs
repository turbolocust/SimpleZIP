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

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleZIP_UI.Presentation.View.Model
{
    public class RecentArchiveModel
    {
        /// <summary>
        /// The last time the archive was used as date/time.
        /// </summary>
        [XmlElement("WhenUsed")]
        public string WhenUsed { get; set; }

        /// <summary>
        /// The name of the file to be displayed.
        /// </summary>
        [XmlElement("FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// The physical location (full path) of the file.
        /// </summary>
        [XmlElement("Location")]
        public string Location { get; set; }

        /// <summary>
        /// The token used in the Most Recently Used list.
        /// </summary>
        [XmlElement("Token")]
        public string MruToken { get; set; }

        /// <summary>
        /// Constructs a new model for the ListBox in <see cref="HomePage"/>.
        /// </summary>
        public RecentArchiveModel()
        {
        }

        /// <summary>
        /// Constructs a new model for the ListBox in <see cref="HomePage"/>.
        /// </summary>
        /// <param name="whenUsed">Date/Time when archive was used.</param>
        /// <param name="fileName">The name of the file to be displayed.</param>
        /// <param name="location">The location of the file to be displayed.</param>
        /// <param name="mruToken">The token used in the Most Recently Used list.</param>
        public RecentArchiveModel(string whenUsed, string fileName, string location, string mruToken)
        {
            WhenUsed = whenUsed;
            FileName = fileName;
            Location = location;
            MruToken = mruToken;
        }

        [XmlRoot("RecentArchivesCollection")]
        public class RecentArchiveModelCollection : ISerializable
        {
            [XmlArray("RecentArchives")]
            [XmlArrayItem("RecentArchive", typeof(RecentArchiveModel))]
            public RecentArchiveModel[] Models { get; set; } = new RecentArchiveModel[0];

            /// <summary>
            /// Factory method to deserialize the specified XML string.
            /// </summary>
            /// <param name="xml">The XML string to be deserialized.</param>
            /// <returns>The deserialized XML string as <see cref="RecentArchiveModelCollection"/>.</returns>
            public static RecentArchiveModelCollection From(string xml)
            {
                if (!string.IsNullOrEmpty(xml))
                {
                    var serializer = new XmlSerializer(typeof(RecentArchiveModelCollection));
                    var stringReader = new StringReader(xml);
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        if (serializer.CanDeserialize(xmlReader))
                        {
                            return (RecentArchiveModelCollection)serializer.Deserialize(xmlReader);
                        }
                    }
                }
                return new RecentArchiveModelCollection();
            }

            /// <inheritdoc />
            public string Serialize()
            {
                string xml; // may be empty at return
                var serializer = new XmlSerializer(typeof(RecentArchiveModelCollection));
                var stringWriter = new StringWriter();
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(xmlWriter, this);
                    xml = stringWriter.ToString();
                }
                return xml;
            }
        }
    }
}
