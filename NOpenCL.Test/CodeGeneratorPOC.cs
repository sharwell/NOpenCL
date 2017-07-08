// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NOpenCL.Generator;

    [TestClass]
    public class CodeGeneratorConcept
    {
        [TestMethod]
        public async Task ProofOfConceptAsync()
        {
            var solutionFilePath = @"J:\dev\github\sharwell\NOpenCL\NOpenCL.sln";
            var codeGenerator = await OpenCLCodeGenerator.CreateAsync(solutionFilePath, CancellationToken.None).ConfigureAwait(false);
            await codeGenerator.GenerateCodeForProjectAsync(@"J:\dev\github\sharwell\NOpenCL\NOpenCL.Test\NOpenCL.Test.csproj", CancellationToken.None).ConfigureAwait(false);
        }
    }
}
