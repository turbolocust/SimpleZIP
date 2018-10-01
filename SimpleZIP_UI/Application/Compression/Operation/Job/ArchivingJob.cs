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

using SimpleZIP_UI.Application.Compression.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleZIP_UI.Application.Compression.Operation.Job
{
    internal abstract class ArchivingJob<T> : IArchivingJob<T> where T : OperationInfo
    {
        /// <inheritdoc />
        public ArchivingOperation<T> Operation { get; }

        /// <inheritdoc />
        public IReadOnlyList<T> OperationInfos { get; }

        protected ArchivingJob(ArchivingOperation<T> operation, params T[] infos)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            OperationInfos = infos ?? throw new ArgumentNullException(nameof(infos));
        }

        /// <inheritdoc />
        public abstract Task<Result> Run(ICancelRequest cancelReq);
    }
}
