# -----------------------------------------------------------------------------
# Adapted from Microsoft OSS
# see https://github.com/Microsoft/service-fabric-cli
# -----------------------------------------------------------------------------


"""Read and modify configuration settings related to the CLI"""

import os
from knack.config import CLIConfig
from knack import CLI

# Default names
BKPCTL_CLI_NAME = 'bkpctl'
BKPCTL_CLI_CONFIG_DIR = os.path.join('~', '.{}'.format(BKPCTL_CLI_NAME))
BKPCTL_CLI_ENV_VAR_PREFIX = BKPCTL_CLI_NAME

def get_config_value(name, fallback=None):
    """Gets a config by name.

    In the case where the config name is not found, will use fallback value."""

    cli_config = CLIConfig(BKPCTL_CLI_CONFIG_DIR, BKPCTL_CLI_ENV_VAR_PREFIX)

    return cli_config.get('servicefabric', name, fallback)

def get_config_bool(name):
    """Checks if a config value is set to a valid bool value."""

    cli_config = CLIConfig(BKPCTL_CLI_CONFIG_DIR, BKPCTL_CLI_ENV_VAR_PREFIX)
    return cli_config.getboolean('servicefabric', name, False)

def set_config_value(name, value):
    """Set a config by name to a value."""

    cli_config = CLIConfig(BKPCTL_CLI_CONFIG_DIR, BKPCTL_CLI_ENV_VAR_PREFIX)
    cli_config.set_value('servicefabric', name, value)

def client_endpoint():
    """Cluster HTTP gateway endpoint address and port, represented as a URL."""

    return get_config_value('endpoint', None)

def security_type():
    """The selected security type of client."""

    return get_config_value('security', None)

def set_cluster_endpoint(endpoint):
    """Configure cluster endpoint"""
    set_config_value('endpoint', endpoint)

class VersionedCLI(CLI):
    """Extend CLI to override get_cli_version."""
    def get_cli_version(self):
        import pkg_resources

        pkg = pkg_resources.get_distribution("bkpctl")
        bkpctl_version = pkg.version
        return '{0}'.format(bkpctl_version)
