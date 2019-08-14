# -----------------------------------------------------------------------------
# Adapted from Microsoft OSS
# see https://github.com/Microsoft/service-fabric-cli
# -----------------------------------------------------------------------------

"""Azure Service Fabric Backup Explorer command line environment for querying, updating and taking backup of reliable collections. The environment is created from the backup of existing reliable collection. 

This package contains the following exports:
launch -- main entry point for the command line environment
"""

from pkgutil import extend_path
__path__ = extend_path(__path__, __name__)

from bkpctl.entry import launch
