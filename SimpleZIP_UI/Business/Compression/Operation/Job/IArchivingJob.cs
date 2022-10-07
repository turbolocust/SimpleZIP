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

using SimpleZIP_UI.Business.Compression.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleZIP_UI.Business.Compression.Operation.Job
{
    internal interface IArchivingJob<T> where T : OperationInfo
    {
        /// <summary>
        /// Aggregated operation which is used to perform operations
        /// as defined in <see cref="OperationInfos"/>.
        /// </summary>
        ArchivingOperation<T> Operation { get; }

        /// <summary>
        /// Description of operations which are to be run.
        /// </summary>
        IReadOnlyList<T> OperationInfos { get; }

        /// <summary>
        /// Returns an asynchronous operation which can be used
        /// to run the operations as defined in <see cref="OperationInfos"/>.
        /// </summary>
        /// <param name="cancelReq">A request to check for cancellation.</param>
        /// <returns>An asynchronous operation which returns <see cref="Result"/>.</returns>
        Task<Result> Run(ICancelRequest cancelReq);
    }
}
