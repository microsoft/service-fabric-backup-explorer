## Overview 
```bkpctl``` is a command line tool written in Python  to help view Service Fabric Reliable Collections(currently Realiable Dictionaries) which have been backed up using Backup Restore Service , and to also make changes into the Backups as and when required in order to maintain consistency . 

It fetches data from the REST Server running on the LocalHost, and presents in on the Commnad Line. It provides multiple options, such as taking backup(once we have edited them) looking into backups from particular partitions etc. 

## Usage 

### Query Meta Data 

Returns the Metadata of the Reliable Dictionary that is currently loaded in the REST Server.


#### Command 
```console
    bkpctl query metadata 
```
#### Output

```json

<edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
        <edmx:DataServices m:DataServiceVersion="3.0" m:MaxDataServiceVersion="3.0" xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
                <Schema Namespace="ServiceFabric.Extensions.Services.Queryable" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                        <EntityType Name="Entity_2OfString_Int64">
                                <Property Name="PartitionId" Nullable="false" Type="Edm.Guid"/>
                                <Property Name="Key" Type="Edm.String"/>
                                <Property Name="Value" Nullable="false" Type="Edm.Int64"/>
                                <Property Name="Etag" Type="Edm.String"/>
                        </EntityType>
                </Schema>
                <Schema Namespace="Default" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                        <EntityContainer Name="Container" m:IsDefaultEntityContainer="true">
                                <EntitySet EntityType="ServiceFabric.Extensions.Services.Queryable.Entity_2OfString_Int64" Name="myDictionary"/>
                        </EntityContainer>
                </Schema>
        </edmx:DataServices>
</edmx:Edmx>
```

Backup contains reliable collection named `myDictionary` with entity type  `ServiceFabric.Extensions.Services.Queryable.Entity_2OfString_Int64`. 

The entity type `ServiceFabric.Extensions.Services.Queryable.Entity_2OfString_Int64` is defined with `key` as `Edm.String` and `value` as `Int64`

### Get Reliable Collection values by Name

Returns the values stored in the Backup of the Reliable Dictionary currently loaded.


#### Command 
```console
bkpctl query collection --name dictionaryName
```
#### Output
```json
{
    "odata.metadata": "",
    "value": [{
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "0",
        "Value": 10,
        "Etag": "10274657101337895921"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "1",
        "Value": 11,
        "Etag": "14728472660478170466"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "12",
        "Value": 112,
        "Etag": "2664785694504735852"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "123",
        "Value": 112,
        "Etag": "2664785694504735852"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "1233",
        "Value": 1212,
        "Etag": "2082849747875832623"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "2",
        "Value": 114,
        "Etag": "16729075919979402645"
    }, {
        "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
        "Key": "Counter",
        "Value": 2,
        "Etag": "13067947420601767663"
    }]
}
```

### Get all transaction performed 

Returns the list of all transactions performed on the Reliable Dictionary.

#### Command 
```console
bkpctl get transactions
```
#### Output
```json

[{
    "TransactionId": 132061041545879352,
    "CommitSequenceNumber": 8,
    "Changes": []
}, {
    "TransactionId": 132061041543659496,
    "CommitSequenceNumber": 7,
    "Changes": []
}, {
    "TransactionId": 132061041551272002,
    "CommitSequenceNumber": 11,
    "Changes": []
}, {
    "TransactionId": 132061041551276992,
    "CommitSequenceNumber": 15,
    "Changes": [{
        "Name": "urn:myDictionary",
        "Changes": [{
            "Key": "Counter",
            "Value": 0,
            "Transaction": {
                "TransactionId": 132061041551276992,
                "CommitSequenceNumber": 15,
                "Id": 132061041551276992,
                "LockContexts": []
            },
            "Action": 0
        }, {
            "Key": "0",
            "Value": 10,
            "Transaction": {
                "TransactionId": 132061041551276992,
                "CommitSequenceNumber": 15,
                "Id": 132061041551276992,
                "LockContexts": []
            },
            "Action": 0
        }]
    }]
}, {
    "TransactionId": 132061042164808899,
    "CommitSequenceNumber": 21,
    "Changes": [{
        "Name": "urn:myDictionary",
        "Changes": [{
            "Key": "1",
            "Value": 11,
            "Transaction": {
                "TransactionId": 132061042164808899,
                "CommitSequenceNumber": 21,
                "Id": 132061042164808899,
                "LockContexts": []
            },
            "Action": 0
        }]
    }]
}, {
    "TransactionId": 132061042214640855,
    "CommitSequenceNumber": 24,
    "Changes": []
}, {
    "TransactionId": 132061042775485912,
    "CommitSequenceNumber": 31,
    "Changes": [{
        "Name": "urn:myDictionary",
        "Changes": [{
            "Key": "2",
            "Value": 12,
            "Transaction": {
                "TransactionId": 132061042775485912,
                "CommitSequenceNumber": 31,
                "Id": 132061042775485912,
                "LockContexts": []
            },
            "Action": 0
        }]
    }]
}]

```

### Update Reliable Collection

Update the values of the current reliable collection with operation perfomed in requests.

## Command 
```console
bkpctl update collection --collectiontype Dictionary --operation Add --collection myDictionary --partitionId ed70fb1c-6972-452a-b183-6114b336e9a1 --key 32 --value 64
```
### Output


```json
[
    {
    "collection": "myDictionary",
     "description": "None",
     "key": "32",
     "operation": "None",
     "partition_id": "ed70fb1c-6972-452a-b183-6114b336e9a1",
     "status": "200"
     }
 ]
```

Updates the reliable collection with addition of the new value

### Update existing value

## Command 
```console
bkpctl update collection --collectiontype Dictionary --operation Update --collection myDictionary --partitionId ed70fb1c-6972-452a-b183-6114b336e9a1 --key 32 --value 96 --etag 5597826295554902436
```
### Output

```json
[
    {
    "collection": "myDictionary",
     "description": "None",
     "key": "32",
     "operation": "None",
     "partition_id": "ed70fb1c-6972-452a-b183-6114b336e9a1",
     "status": "200"
     }
 ]
```

Updates the reliable collection with update of the value


## Examples - Delete value
## Command 
```console
bkpctl update collection --collectiontype Dictionary --operation Delete --collection myDictionary --partitionId 5131d35d-2623-4d0b-bc11-ad65f365f801  --key 32 --value 96 --etag 3579969540437049356
```
### Output

```json
[
    {
        "collection": "myDictionary",
         "description": "None",
         "key": "32",
         "operation": "None",
         "partition_id": "5131d35d-2623-4d0b-bc11-ad65f365f801",
         "status": "200"
    }
 ]
```

Updates the reliable collection with deletion value.


# Backup the current state

Update the values of the current reliable collection with operation perfomed in requests.

## Examples - Take full backup

## Command 
```console
bkpctl backup partition --type Full --path E:\Newbackup
```
### Output

```json
{
    "status": "success",
    "backPath": "E:\\Newbackup\\0bcc9342d40a4210a35b46a78406f014"
}
```

Full Backup is taken successfully.


## Examples - Take incremental backup
## Command 
```console
bkpctl backup partition --type Full --path E:\Newbackup
```
### Output


```json
{
    "status": "success",
    "backPath": "E:\\Newbackup\\0bcc9342d40a4210a35b46a78406f014"
}
```

Incremental Backup is taken successfully.




