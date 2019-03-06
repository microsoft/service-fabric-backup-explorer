// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    /// <summary>
    /// ExitController for exiting the Rest Server.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ExitController : ControllerBase
    {
        public ExitController(BackupParserManager backupParserManager)
        {
            this.backupParserManager = backupParserManager;
        }

        // GET api/exit
        [HttpGet]
        public void Get()
        {
            // Dispose backup parser for cleanup.
            this.backupParserManager.Dispose();
            Task.Run(async () =>
            {
                // give time for /api/exit to return 200.
                await Task.Delay(1000);
                // todo : if there are multiple web-host, don't just exit.
                // create a webhost manager to exit when all web host has exited.
                Environment.Exit(0);
            });
        }

        BackupParserManager backupParserManager;
    }
}
