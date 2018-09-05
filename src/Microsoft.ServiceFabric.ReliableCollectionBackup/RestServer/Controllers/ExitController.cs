// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Tools;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExitController : ControllerBase
    {
        public ExitController(BackupParser backupParser)
        {
            this.backupParser = backupParser;
        }

        // GET api/Exit
        [HttpGet]
        public void Get()
        {
            // Dispose backup parser
            this.backupParser.Dispose();
            Environment.Exit(0);
        }

        BackupParser backupParser;
    }
}
