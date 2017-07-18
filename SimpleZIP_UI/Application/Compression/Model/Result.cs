﻿// ==++==
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
using System;

namespace SimpleZIP_UI.Application.Compression.Model
{
    internal class Result
    {
        /// <summary>
        /// Constructs a new instance of this class with status 
        /// code set to <see cref="Status.Success"/>.
        /// </summary>
        internal Result()
        {
            StatusCode = Status.Success;
        }

        internal Status StatusCode { get; set; }

        internal string Message { get; set; }

        internal TimeSpan ElapsedTime { get; set; }

        public enum Status
        {
            Fail, PartialFail, Success, Interrupt
        }
    }
}
