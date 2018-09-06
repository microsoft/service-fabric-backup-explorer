# Reliable Collection Backup Explorer Design

This is user scenario doc with long term goals.

## Overview

Reliable collection viewer is a tool which allows the client to view and edit reliable collection of an application.
It can do data operations on
  1. Live Cluster - View and edit reliable collections of user's application.
  Live data viewer functionality of this tool facilitates user to view the current state of reliable collections on her app.
  2. Offline Backups - View and edit backup of user's application.
  Backup viewer functionality of this tool makes it possible to view the historical state of reliable collections based on user provided search criterion and allows easy navigation.
  The user can modify the loaded state and is also capable of creating a backup with the new modified state.

## Scenarios

  1. User wants to explore and understand the data of her services. She should be able to view the data of her services either from live cluster or from her offline backups. The experience for using backup viewer and live data viewer should be similar.
  2. Backup viewer
      1. Data corruption due to bugs in user code or unwanted updates. With this tool, the client can explore and understand how the corruption affected the data. It is also possible to create a new, modified state from a specific point in the logs and back it up, which can be restored later.
      2. Backup viewer should facilitate user to browse historical data across multiple services and partitions, based on time or context (user specified tag/correlation ID).
      3. User wants to browse backups of all apps in order to create consistent data state across multiple services and partitions, this would later on be used to restore apps to a consistent state. Eg: In the event of Loss of entire cluster.
      4. Changes between backups/transactions - User wants to decide on which backup corruption started. For achieving this there should be a way for her to see the changes between backups.
      5. Secondary consumption of application backup data -  User wants to explore and understand how the data behaved through the backups, while not interfering in the system's current state. For example, a machine learning system could be coupled with this tool, allowing the user to make predictions on how the system will behave based on past activity.
      6. Backup viewer should be able to interpret the backups taken by services from different languages. The design should be easily pluggable/extensible to allow capabilities to support multiple languages.
  3. Live data viewer
      1. User should be able to connect to a live cluster and perform CRUD operations on her reliable collections.
  4. Library for advanced users. Alongside viewer we’re planning to provide the library, used by viewer, which would help users accomplish complex scenarios. Eg: Register for data changed events while restoring incremental backups to search a pattern.
  5. Queryable interface for advanced users - User should be able to do CRUD operations on her Reliable collections using queryable command line interface as well. This would be use the same implementation underneath, which is used by viewer.

## Capabilities needed / Requirements:

  1. User should be able to do CRUD operations on her service data. This could be reliable collection data from either live cluster or from offline backup.
  2. User should be able to edit multiple reliable collections within a partition in 1 transaction. The commit of such an  operation would result in only one transaction.
  3. User should be able to edit multiple partition data and commit in 1 transaction per partition.
  After user has completed all edit operations, user's commit action should create one transaction per partition in offline backup mode. In live mode there is no support for atomicity of transaction across partitions, this limitation exists in services as well.
  4. Custom data rendering : Viewer should be able to render custom data types (either in key or value) using user provided serializers.
  5. Capability to undo and redo user operations:
  User should have the capability of undoing, redoing her operations.
      1. In case of backup viewer, It is expected that any undo operation should not create more transactions over the backup.
      2. In case of live viewer undo/redo operation would result in new transactions.
  6. Backup Viewer
      1. View both incremental and full backups of a partition: 
      User should be able to see complete backup. She should be able to see resultant state of partition after some or all incremental backups have been applied. She should be able to go through transactions of incremental backup.
      2. Differential views between backups:
    User should be able to see differences in two backup files. Differences are in keys and values present, absent or changed in two backups. This should be supported across both incremental and full backups.
      3. Browse multiple partition backups: 
      User should be able to browse data in multiple partitions of multiple services. We may have to support:
          1. Partition level view showing all reliable collection data in partition.
          2. Horizontal view across multiple partitions of multiple services. Common field across partition/services could be either time or user specified field/correlationID.
      4. Generate new backup after committing current edits:
    Once user has done all the required changes, they can generate a backup file. They should be able to take new backup file and restore it in existing or new cluster.
      5. Backup file generated should be compatible with managed and/or native stack.
      6. Edit/Revert transactions: User can edit or revert particular transaction. All new changes including revert will create new transaction on top of current backup.
      7. Interaction with Backup Restore Service:
    User should be able to point Reliable Collections Viewer to Backup Restore Service (BRS) of a running cluster. Viewer should be able to pick the necessary details like storage account, storage location, service/partition info etc. on user’s behalf. Backups should be fetched and processed accordingly. To access backup storage, user would have to provide the auth token, and we may have to support auth tokens or protocol supported by storage.
  7. Offline viewer should be self-sufficient and able to run independently (shouldn't require SF runtime installation).
  8. Live Viewer specific requirements
      1. Use any generic protocol for CRUD operations
    Live Viewer should support a generic protocol (Eg: ODATA) to communicate with running services to do CRUD operations on Reliable collections of running services.
