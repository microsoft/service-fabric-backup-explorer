# -----------------------------------------------------------------------------
# Adapted from Microsoft OSS
# see https://github.com/Microsoft/service-fabric-cli
# -----------------------------------------------------------------------------


"""Command and help loader for the Service Fabric Backup Explorer CLI.

Commands are stored as one to one mappings between command line syntax and
python function.
"""

from collections import OrderedDict
from knack.arguments import ArgumentsContext
from knack.commands import CLICommandsLoader, CommandSuperGroup
from knack.help import CLIHelp
from bkpctl.swagger_client import api_client as client_create
import copy

# Need to import so global help dict gets updated
import bkpctl.helps.main # pylint: disable=unused-import

class SFBackupExpCommandHelp(CLIHelp):
    """Service Fabric Backup Explorer CLI help loader"""

    def __init__(self, ctx=None):
        header_msg = 'Service Fabric Backup Explorer Command Line'

        super(SFBackupExpCommandHelp, self).__init__(ctx=ctx,
                                            welcome_message=header_msg)

class SFBackupExpCommandLoader(CLICommandsLoader):
    """Service Fabric CLI command loader, containing command mappings"""

    def load_command_table(self, args): #pylint: disable=too-many-statements
        """Load all Service Fabric commands"""

        with CommandSuperGroup(__name__, self, 'bkpctl.custom_backup_explorer#{}'
                               ) as super_group: 
            with super_group.group('query') as group:
                group.command('metadata', 'query_metadata')
                group.command('collection', 'query_collection')
            with super_group.group('get') as group:
                group.command('transactions', 'get_transactions')
            with super_group.group('update') as group:
                group.command('collection', 'add_transaction')
            with super_group.group('backup') as group:
                group.command('partition', 'trigger_backup')
            
        with ArgumentsContext(self, 'query') as ac:
            ac.argument('name', options_list=['--name', '-n'])
            
        with ArgumentsContext(self, 'update') as ac:
            ac.argument('collectiontype', options_list=['--collectiontype', '-ct'])
            ac.argument('operation', options_list=['--operation', '-op'])
            ac.argument('collection', options_list=['--collection', '-c'])
            ac.argument('partition_id', options_list=['--partitionId', '-p'])
            ac.argument('etag', options_list=['--etag', '-e'])
            ac.argument('key', options_list=['--key', '-k'])
            ac.argument('value', options_list=['--value', '-v'])

        with ArgumentsContext(self, 'backup') as ac:
            ac.argument('typeOfBackup', options_list=['--type', '-t'])
            ac.argument('path', options_list=['--path', '-p'])
            ac.argument('timeout', options_list=['--timeout', '-to'])
            ac.argument('cancellationInSecs', options_list=['--cancellationAfter', '-ca'])

        return OrderedDict(self.command_table)