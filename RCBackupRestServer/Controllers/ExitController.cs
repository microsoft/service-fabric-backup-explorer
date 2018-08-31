using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.ServiceFabric.Tools.RCBackupRestServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExitController : ControllerBase
    {
        public ExitController()
        {

        }

        // GET api/Exit
        [HttpGet]
        public void Get()
        {
            // Dispose backup parser
            Environment.Exit(0);
        }
    }
}
