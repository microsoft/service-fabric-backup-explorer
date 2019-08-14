"""Entry or launch point for Service Fabric Backup Explorer CLI.

Handles creating and launching a CLI to handle a user command."""

import sys
from bkpctl.config import VersionedCLI
from bkpctl.config import BKPCTL_CLI_CONFIG_DIR, BKPCTL_CLI_ENV_VAR_PREFIX, BKPCTL_CLI_NAME
from bkpctl.commands import SFBackupExpCommandLoader, SFBackupExpCommandHelp

def cli():
    """Create CLI environment"""
    return VersionedCLI(cli_name=BKPCTL_CLI_NAME,
                        config_dir=BKPCTL_CLI_CONFIG_DIR,
                        config_env_var_prefix=BKPCTL_CLI_ENV_VAR_PREFIX,
                        commands_loader_cls=SFBackupExpCommandLoader,
                        help_cls=SFBackupExpCommandHelp)

def launch():
    """Entry point for Service Fabric Backup Explorer CLI.

    Configures and invokes CLI with arguments passed during the time the python
    session is launched"""
    cli_env = cli()
    print (sys.argv[1:])
    return cli_env.invoke(sys.argv[1:])
