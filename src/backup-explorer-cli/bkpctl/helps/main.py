# -----------------------------------------------------------------------------
# Adapted from Microsoft OSS
# see https://github.com/Microsoft/service-fabric-cli
# -----------------------------------------------------------------------------

"""Help documentation for Reliable Collection groups"""

from knack.help_files import helps

helps[''] = """
    type: group
    short-summary: Commands for managing Service Fabric reliable collections dictionary after loading it from backup.
    long-summary: Commands follow the noun-verb pattern. See subgroups for more
        information.
"""

helps['query'] = """
    type: group
    short-summary: Query the meta data and content of the reliable collections.                                 
                bkpctl query metadata returns the details about all the reliable collections and its  data type.
                bkpctl query collection --name <name of collection> return the content of reliable collection loaded in backup explorer.
"""

helps['get'] = """
    type: group
    short-summary: Returns the list of all transactions performed on reliable collection
"""

helps['update'] = """
    type: group
    short-summary: Allows update of the content of reliable collections by adding new transaction.  New transaction can add, update and delete the entries in reliable collection
"""

helps['backup'] = """
    type: group
    short-summary: Create backup of the current reliable collections which are modified by the update commands. The backup created can be loaded in service fabric cluster to provide updated reliable collections. Backup can be triggered in Full or Incremental form
"""

