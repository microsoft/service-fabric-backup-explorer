using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Tools;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExitController : ControllerBase
    {
        public ExitController(RCBackupParser.RCBackupParser backupParser)
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

        RCBackupParser.RCBackupParser backupParser;
    }
}
