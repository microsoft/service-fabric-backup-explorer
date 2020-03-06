# Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`namespace `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser`](#namespace_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser) | 

# namespace `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`enum `[`ReliableStateKind`](#namespace_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1a76f3a21af4fcee1bfb3337456657c18b)            | Lists different kind of ReliableState that are supported.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParser`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser) | [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser) parses the backup of Service Fabric stateful service viz. Reliable Collections. This class can be used to 1) Parse a backup chain, 2) Validate data via notifications, 3) Make additional changes and take a new backup.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParserImpl`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl) | Actual implementation of [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::CodePackageInfo`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_code_package_info) | Stores code package information required for parsing backup.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ComMtaHelper`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_com_mta_helper) | Helper COM methods
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::GenericUtils`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_generic_utils) | Utility functions for Generic types
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::NotifyTransactionAppliedEventArgs`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args) | [NotifyTransactionAppliedEventArgs](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args) is used for notifying [BackupParser.TransactionApplied](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) event. This event contains the changes that were applied in this transaction.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change) | [ReliableCollectionChange](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change) stores the changes in a List during a transaction.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ReliableStateKindUtils`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_state_kind_utils) | 
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager) | StateManager is IReliableStateManager for [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser). This wraps over the ReliabilitySimulator's TransactionalReplicator.
`class `[`Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::TransactionChangeManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager) | TransactionChangeManager keeps track of changes in Reliable Collections of a replica in a transaction.

## Members

#### `enum `[`ReliableStateKind`](#namespace_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1a76f3a21af4fcee1bfb3337456657c18b) 

 Values                         | Descriptions                                
--------------------------------|---------------------------------------------
ReliableDictionary            | 
ReliableQueue            | 
ReliableConcurrentQueue            | 

Lists different kind of ReliableState that are supported.

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParser` 

```
class Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParser
  : public IDisposable
```  

[BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser) parses the backup of Service Fabric stateful service viz. Reliable Collections. This class can be used to 1) Parse a backup chain, 2) Validate data via notifications, 3) Make additional changes and take a new backup.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`{property} EventHandler< `[`NotifyTransactionAppliedEventArgs`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args)` > `[`TransactionApplied`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) | Events fired when a transaction is committed. This event contains the changes that were applied in this transaction. During this event, user has a consistent view of the backup at this point in time. We can use StateManager to read (not write) complete Reliable Collections at this time.
`{property} IReliableStateManager `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a2bc298c001618b1702ec32db1c5bcf19) | IReliableStateManager which is used for reading and writing to the Reliable Collections of the backup. Writing is only allowed after backup has been fully parsed after [ParseAsync](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a8bff7a26fe3e82dc35ca9fb2922f51bc).
`public inline  `[`BackupParser`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1abc95ae6b4400d6799ad6380bf455b523)`(string backupChainPath,string codePackagePath)` | Constructor for [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).
`public inline async Task `[`ParseAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a8bff7a26fe3e82dc35ca9fb2922f51bc)`(CancellationToken cancellationToken)` | Parses a backup. Before parsing, register for [TransactionApplied](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) events. These events are fired when a transaction is committed. After parsing has finished, we can write to the Reliable Collections using [StateManager](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a2bc298c001618b1702ec32db1c5bcf19).
`public inline async Task `[`BackupAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac1fc7029bddd2b68ff2a69eb6e68e31d)`(BackupOption backupOption,TimeSpan timeout,CancellationToken cancellationToken,Func< BackupInfo, CancellationToken, Task< bool >> backupCallback)` | Takes a backup of the current state of Reliable Collections.
`public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a251929a83c7b6a375468e2bf28a91e81)`()` | Cleans up any resources like folders used by [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).

## Members

#### `{property} EventHandler< `[`NotifyTransactionAppliedEventArgs`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args)` > `[`TransactionApplied`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) 

Events fired when a transaction is committed. This event contains the changes that were applied in this transaction. During this event, user has a consistent view of the backup at this point in time. We can use StateManager to read (not write) complete Reliable Collections at this time.

#### `{property} IReliableStateManager `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a2bc298c001618b1702ec32db1c5bcf19) 

IReliableStateManager which is used for reading and writing to the Reliable Collections of the backup. Writing is only allowed after backup has been fully parsed after [ParseAsync](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a8bff7a26fe3e82dc35ca9fb2922f51bc).

#### `public inline  `[`BackupParser`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1abc95ae6b4400d6799ad6380bf455b523)`(string backupChainPath,string codePackagePath)` 

Constructor for [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).

#### Parameters
* `backupChainPath` Folder path that contains sub folders of one full and multiple incremental backups.

* `codePackagePath` Code packages of the service whose backups are provided in *backupChainPath* . Pass an empty string if code package is not allowed for backup parsing. e.g. when backup has only primitive types.

#### `public inline async Task `[`ParseAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a8bff7a26fe3e82dc35ca9fb2922f51bc)`(CancellationToken cancellationToken)` 

Parses a backup. Before parsing, register for [TransactionApplied](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) events. These events are fired when a transaction is committed. After parsing has finished, we can write to the Reliable Collections using [StateManager](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a2bc298c001618b1702ec32db1c5bcf19).

#### Parameters
* `cancellationToken` The token to monitor for cancellation requests.

#### Returns
Task that represents the asynchronous parse operation.

#### `public inline async Task `[`BackupAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac1fc7029bddd2b68ff2a69eb6e68e31d)`(BackupOption backupOption,TimeSpan timeout,CancellationToken cancellationToken,Func< BackupInfo, CancellationToken, Task< bool >> backupCallback)` 

Takes a backup of the current state of Reliable Collections.

#### Parameters
* `backupOption` The type of backup to perform.

* `timeout` The timeout for this operation.

* `cancellationToken` The token to monitor for cancellation requests.

* `backupCallback` Callback to be called when the backup folder has been created locally and is ready to be moved out of the node.

#### Returns
Task that represents the asynchronous backup operation.

#### `public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1a251929a83c7b6a375468e2bf28a91e81)`()` 

Cleans up any resources like folders used by [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParserImpl` 

```
class Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::BackupParserImpl
  : public IDisposable
```  

Actual implementation of [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser).

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`{property} StateManager `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a0c49d790baa1c3a262608171272e727c) | StateManager associated with this [Parser](#namespace_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser).
`{property} TransactionalReplicator `[`Replicator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1aa54a7c59efe4a51a964c6bf7a2cc0182) | 
`public inline  `[`BackupParserImpl`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1af9c2a01b7a74d05f595ba84598db13e9)`(string backupChainPath,string codePackagePath)` | Constructor for BackupParserImpl.
`public inline async Task `[`ParseAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1adbe0f8b357780e12bacc3a8c670c3406)`(CancellationToken cancellationToken)` | Parses the backup.
`public inline async Task `[`BackupAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a25a167a7d75e491bdb278f8598c9fa76)`(Func< BackupInfo, CancellationToken, Task< bool >> backupCallback,BackupOption backupOption,TimeSpan timeout,CancellationToken cancellationToken)` | Creates a backup of current replica.
`public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a2e9e52f02bb78eda924812e14526738f)`()` | 

## Members

#### `{property} StateManager `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a0c49d790baa1c3a262608171272e727c) 

StateManager associated with this [Parser](#namespace_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser).

#### `{property} TransactionalReplicator `[`Replicator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1aa54a7c59efe4a51a964c6bf7a2cc0182) 

#### `public inline  `[`BackupParserImpl`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1af9c2a01b7a74d05f595ba84598db13e9)`(string backupChainPath,string codePackagePath)` 

Constructor for BackupParserImpl.

#### Parameters
* `backupChainPath` Folder path that contains sub folders of one full and multiple incremental backups.

* `codePackagePath` Code packages of the service whose backups are provided in *backupChainPath* .

#### `public inline async Task `[`ParseAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1adbe0f8b357780e12bacc3a8c670c3406)`(CancellationToken cancellationToken)` 

Parses the backup.

#### Parameters
* `cancellationToken` Cancellation token to stop backup parsing.

#### Returns
Task associated with parsing.

#### `public inline async Task `[`BackupAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a25a167a7d75e491bdb278f8598c9fa76)`(Func< BackupInfo, CancellationToken, Task< bool >> backupCallback,BackupOption backupOption,TimeSpan timeout,CancellationToken cancellationToken)` 

Creates a backup of current replica.

#### Parameters
* `backupCallback` Backup callback to trigger at finish of Backup operation.

* `backupOption` The type of backup to perform.

* `timeout` The timeout for this operation.

* `cancellationToken` The token to monitor for cancellation requests.

#### Returns
Task representing this asynchronous backup operation.

#### `public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_impl_1a2e9e52f02bb78eda924812e14526738f)`()` 

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::CodePackageInfo` 

```
class Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::CodePackageInfo
  : public IDisposable
```  

Stores code package information required for parsing backup.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`CodePackageInfo`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_code_package_info_1a7df7fe3eebcb2711eb1b133fb4a3f389)`(string packagePath)` | 
`public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_code_package_info_1aa96a76c8dfa9afa0a0d293f4b3aca04d)`()` | 

## Members

#### `public inline  `[`CodePackageInfo`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_code_package_info_1a7df7fe3eebcb2711eb1b133fb4a3f389)`(string packagePath)` 

#### `public inline void `[`Dispose`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_code_package_info_1aa96a76c8dfa9afa0a0d293f4b3aca04d)`()` 

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ComMtaHelper` 

Helper COM methods

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::GenericUtils` 

Utility functions for Generic types

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::NotifyTransactionAppliedEventArgs` 

```
class Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::NotifyTransactionAppliedEventArgs
  : public EventArgs
```  

[NotifyTransactionAppliedEventArgs](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args) is used for notifying [BackupParser.TransactionApplied](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser_1ac5dd8318451de05cbd7192e7cb1a618a) event. This event contains the changes that were applied in this transaction.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`{property} long `[`TransactionId`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a7ef879885f860febe130d99e04b3538e) | Gets a value identifying the transaction.
`{property} long `[`CommitSequenceNumber`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1af637dfa0df8f689db77d0992e2c2cb3d) | Sequence number for the commit operation.
`{property} IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > `[`Changes`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a8e6ae1cc71e65c0ca5d439ce78ac92b5) | List of reliable collection changes that were made during this transaction.
`public inline  `[`NotifyTransactionAppliedEventArgs`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a66630a84e65506dc45e4bd30d4e5a800)`(ITransaction transaction,IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > changes)` | Constructor of [NotifyTransactionAppliedEventArgs](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args).

## Members

#### `{property} long `[`TransactionId`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a7ef879885f860febe130d99e04b3538e) 

Gets a value identifying the transaction.

#### Returns
The transaction id.

#### `{property} long `[`CommitSequenceNumber`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1af637dfa0df8f689db77d0992e2c2cb3d) 

Sequence number for the commit operation.

The sequence number at which the the transaction was committed.

#### `{property} IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > `[`Changes`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a8e6ae1cc71e65c0ca5d439ce78ac92b5) 

List of reliable collection changes that were made during this transaction.

#### `public inline  `[`NotifyTransactionAppliedEventArgs`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args_1a66630a84e65506dc45e4bd30d4e5a800)`(ITransaction transaction,IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > changes)` 

Constructor of [NotifyTransactionAppliedEventArgs](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_notify_transaction_applied_event_args).

#### Parameters
* `transaction` Transaction which was committed.

* `changes` Reliable Collection changes applied in this Transaction.

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ReliableCollectionChange` 

[ReliableCollectionChange](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change) stores the changes in a List during a transaction.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`{property} Uri `[`Name`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a631f5a68e21a6590740382dd122da376) | Name of ReliableState
`{property} List< EventArgs > `[`Changes`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a643340418308f41c62e81806eff85b39) | List of changes that are received for this Reliable State.
`public inline  `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a294d3422651a8be82584a4fabbd7044b)`(Uri name)` | Constructor of [ReliableCollectionChange](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)

## Members

#### `{property} Uri `[`Name`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a631f5a68e21a6590740382dd122da376) 

Name of ReliableState

#### `{property} List< EventArgs > `[`Changes`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a643340418308f41c62e81806eff85b39) 

List of changes that are received for this Reliable State.

#### `public inline  `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change_1a294d3422651a8be82584a4fabbd7044b)`(Uri name)` 

Constructor of [ReliableCollectionChange](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)

#### Parameters
* `name` Name of Reliable Collection whose changes wer are collecting.

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::ReliableStateKindUtils` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::StateManager` 

```
class Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::StateManager
  : public IReliableStateManager
```  

StateManager is IReliableStateManager for [BackupParser](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_backup_parser). This wraps over the ReliabilitySimulator's TransactionalReplicator.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`{property} EventHandler< NotifyTransactionChangedEventArgs > `[`TransactionChanged`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a4db8ce5bdf3916d2c33bc042e72be122) | 
`{property} EventHandler< NotifyStateManagerChangedEventArgs > `[`StateManagerChanged`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2771ba0381b267f54a03f58fb799bf95) | 
`{property} TransactionalReplicator `[`replicator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a581ac4cd64f7b8d526f43a97b2855429) | 
`public inline  `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aff4b3c98351a058b948c6db184d569fd)`(ReliabilitySimulator reliabilitySimulator)` | Constructor of StateManager.
`public inline ITransaction `[`CreateTransaction`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1afabe3be59581ebd052e42404feddc0c6)`()` | 
`public inline IAsyncEnumerator< IReliableState > `[`GetAsyncEnumerator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2911366b08b5fc692f1ba54017c022fa)`()` | 
`public inline async Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2738ab630937a8bd6adb324876c2c639)`(ITransaction tx,Uri name,TimeSpan timeout)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aa27dc243e7ec0660c8da29d92520795d)`(ITransaction tx,Uri name)` | 
`public inline async Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a920d15ccde69ab16f7703b251dc0ea3d)`(Uri name,TimeSpan timeout)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a6fb012b5d9e979dd5227636658349c1f)`(Uri name)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1add2e977c6865a5c82236f92038628f03)`(ITransaction tx,string name,TimeSpan timeout)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a12678f62e75020b71c6f2236acf71ba3)`(ITransaction tx,string name)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a11bdeeebbf7919600dd231dc382f4ffa)`(string name,TimeSpan timeout)` | 
`public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a4a096f7904f6d3d0826e0b12ae3f0f83)`(string name)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a83a4fdd595ea8bdff7008fab9c631209)`(ITransaction tx,Uri name,TimeSpan timeout)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a08531fb243fb0a094cedfb214bd574db)`(ITransaction tx,Uri name)` | 
`public inline async Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ac9110004f1d5f5e96a9ab4d47fbbbd4c)`(Uri name,TimeSpan timeout)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ad1e555494470eb2866d641a407f15df1)`(Uri name)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1af7304972df185911570e3c304fb6cb4e)`(ITransaction tx,string name,TimeSpan timeout)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aab0feac07db2b3c87338866d8322d4c3)`(ITransaction tx,string name)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1af5fe98264a66f644eb75f3de9f3a258d)`(string name,TimeSpan timeout)` | 
`public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a890292a370404d4040d595b44ecebbd4)`(string name)` | 
`public inline Task< ConditionalValue< T > > `[`TryGetAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aee717887cbcddf58e02e610b6352e76d)`(Uri name)` | 
`public inline Task< ConditionalValue< T > > `[`TryGetAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ac1e51f431a70f9133dbb8ab653bde5a0)`(string name)` | 
`public inline bool `[`TryAddStateSerializer< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a931e99290e318017df370ef740e3f95b)`(IStateSerializer< T > stateSerializer)` | 

## Members

#### `{property} EventHandler< NotifyTransactionChangedEventArgs > `[`TransactionChanged`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a4db8ce5bdf3916d2c33bc042e72be122) 

#### `{property} EventHandler< NotifyStateManagerChangedEventArgs > `[`StateManagerChanged`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2771ba0381b267f54a03f58fb799bf95) 

#### `{property} TransactionalReplicator `[`replicator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a581ac4cd64f7b8d526f43a97b2855429) 

#### `public inline  `[`StateManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aff4b3c98351a058b948c6db184d569fd)`(ReliabilitySimulator reliabilitySimulator)` 

Constructor of StateManager.

#### Parameters
* `reliabilitySimulator` ReliabilitySimulator that

#### `public inline ITransaction `[`CreateTransaction`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1afabe3be59581ebd052e42404feddc0c6)`()` 

#### `public inline IAsyncEnumerator< IReliableState > `[`GetAsyncEnumerator`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2911366b08b5fc692f1ba54017c022fa)`()` 

#### `public inline async Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a2738ab630937a8bd6adb324876c2c639)`(ITransaction tx,Uri name,TimeSpan timeout)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aa27dc243e7ec0660c8da29d92520795d)`(ITransaction tx,Uri name)` 

#### `public inline async Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a920d15ccde69ab16f7703b251dc0ea3d)`(Uri name,TimeSpan timeout)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a6fb012b5d9e979dd5227636658349c1f)`(Uri name)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1add2e977c6865a5c82236f92038628f03)`(ITransaction tx,string name,TimeSpan timeout)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a12678f62e75020b71c6f2236acf71ba3)`(ITransaction tx,string name)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a11bdeeebbf7919600dd231dc382f4ffa)`(string name,TimeSpan timeout)` 

#### `public inline Task< T > `[`GetOrAddAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a4a096f7904f6d3d0826e0b12ae3f0f83)`(string name)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a83a4fdd595ea8bdff7008fab9c631209)`(ITransaction tx,Uri name,TimeSpan timeout)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a08531fb243fb0a094cedfb214bd574db)`(ITransaction tx,Uri name)` 

#### `public inline async Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ac9110004f1d5f5e96a9ab4d47fbbbd4c)`(Uri name,TimeSpan timeout)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ad1e555494470eb2866d641a407f15df1)`(Uri name)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1af7304972df185911570e3c304fb6cb4e)`(ITransaction tx,string name,TimeSpan timeout)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aab0feac07db2b3c87338866d8322d4c3)`(ITransaction tx,string name)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1af5fe98264a66f644eb75f3de9f3a258d)`(string name,TimeSpan timeout)` 

#### `public inline Task `[`RemoveAsync`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a890292a370404d4040d595b44ecebbd4)`(string name)` 

#### `public inline Task< ConditionalValue< T > > `[`TryGetAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1aee717887cbcddf58e02e610b6352e76d)`(Uri name)` 

#### `public inline Task< ConditionalValue< T > > `[`TryGetAsync< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1ac1e51f431a70f9133dbb8ab653bde5a0)`(string name)` 

#### `public inline bool `[`TryAddStateSerializer< T >`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_state_manager_1a931e99290e318017df370ef740e3f95b)`(IStateSerializer< T > stateSerializer)` 

# class `Microsoft::ServiceFabric::ReliableCollectionBackup::Parser::TransactionChangeManager` 

TransactionChangeManager keeps track of changes in Reliable Collections of a replica in a transaction.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`TransactionChangeManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1afcea01bcb2188af86eb8fbf1bbdbcc73)`()` | Constructor of TransactionChangeManager.
`public inline void `[`CollectChanges`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1a815c104032cf5be7a83c1e6184c18488)`(Uri reliableCollectionName,EventArgs changes)` | Add a new change in the transaction.
`public inline void `[`TransactionCompleted`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1a7523e6dea30ec7480d356f9204ac9b15)`()` | Clears the changes in the transaction to get prepared for next transaction.
`public inline IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > `[`GetAllChanges`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1ac9a81f9bff560a6ca35a3f9a8150929a)`()` | Gets Reliable Collection changes collected till now.

## Members

#### `public inline  `[`TransactionChangeManager`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1afcea01bcb2188af86eb8fbf1bbdbcc73)`()` 

Constructor of TransactionChangeManager.

#### `public inline void `[`CollectChanges`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1a815c104032cf5be7a83c1e6184c18488)`(Uri reliableCollectionName,EventArgs changes)` 

Add a new change in the transaction.

#### Parameters
* `reliableCollectionName` Name of Reliable Collection which changed in this transaction.

* `changes` Changes in ReliableCollection.

#### `public inline void `[`TransactionCompleted`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1a7523e6dea30ec7480d356f9204ac9b15)`()` 

Clears the changes in the transaction to get prepared for next transaction.

#### `public inline IEnumerable< `[`ReliableCollectionChange`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_reliable_collection_change)` > `[`GetAllChanges`](#class_microsoft_1_1_service_fabric_1_1_reliable_collection_backup_1_1_parser_1_1_transaction_change_manager_1ac9a81f9bff560a6ca35a3f9a8150929a)`()` 

Gets Reliable Collection changes collected till now.

#### Returns
All reliable collection changes collected.
