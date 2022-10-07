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

using SimpleZIP_UI.Business;
using SimpleZIP_UI.Business.Compression.Model;
using SimpleZIP_UI.Business.Compression.Operation;
using SimpleZIP_UI.Business.Compression.Operation.Job;
using System.Threading.Tasks;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class DecompressionController : SummaryPageController<DecompressionInfo>
    {
        internal DecompressionController(INavigable navHandler,
            IPasswordRequest pwRequest) : base(navHandler, pwRequest)
        {
        }

        /// <inheritdoc cref="SummaryPageController{T}.GetArchivingOperation"/>
        protected override ArchivingOperation<DecompressionInfo> GetArchivingOperation()
        {
            return Operations.ForDecompression();
        }

        /// <inheritdoc cref="SummaryPageController{T}.PerformOperation"/>
        protected override async Task<Result> PerformOperation(DecompressionInfo[] operationInfos)
        {
            IArchivingJob<DecompressionInfo> job =
                new DecompressionJob(Operation, operationInfos)
                {
                    PasswordRequest = PasswordRequest
                };

            return await job.Run(this);
        }
    }
}
