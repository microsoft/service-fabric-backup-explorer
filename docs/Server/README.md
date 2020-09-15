# Backup Explorer Server

## Query Metadata
Returns the meta data of the reliable collection in the backup.

### Request

| METHOD        | Request URI   |
| ------------- |:-------------:|
|  GET          | \$query/\$metadata |

#### Parameters
| NAME          | Type          | Required |
| ------------- |:-------------:| :----:|
|  NULL          |  | |


### Response

| HTTP Status Code        | Description   |
| -------------           |:-------------:|
|  200                    | Success, with metadata of the reliable collection |

## Examples

```
GET http://localhost:5000/$query/$metadata
```

#### Response 200

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

# Get Reliable Collection values by Name

Return the values of the reliable collection loaded in the backup.

### Request 

| METHOD        | Request URI   | 
| ------------- |:-------------:| 
|  GET          | /$query/{ReliableCollectionName} | 



### Parameters
| NAME                    | Type          | Required  | 
| -------------           |:-------------:| :----:    |
| ReliableCollectionName  | string        |Yes        |


### Response

| HTTP Status Code        | Description   | 
| -------------           |:-------------:| 
|  200                    | Success, with values in the reliable collection | 

## Examples

```
GET http://localhost:5000/$query/$metadata
```

#### Response 200

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

Contains the entries of Reliable collections, with Partition Id, Etag and Key, Value pair.

## Examples

## Update Reliable Collection

Update the values of the current reliable collection with operation perfomed in requests.

### Request 

| METHOD        | Request URI   | 
| ------------- |:-------------:| 
|  POST         | /$query | 



### Parameters
| NAME                    | Type          | Required  | 
| -------------           |:-------------:| :----:    |
| NULL                    |               |           |

### Response

| HTTP Status Code        | Description   | 
| -------------           |:-------------:| 
|  200                    | Success, for updation of the reliable collection | 

## Add new value

```
POST http://localhost:5000/$query/
```
#### Request Body
```json
[
    {
    "Collection" : "myDictionary",
    "Operation": "Add",
    "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
    "Key": "32",
    "Value" : "64"
    }
]
```

#### Response 200

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

## Update existing value

```
POST http://localhost:5000/$query
```


#### Request Body
```json
[
    { 
    "Collection" : "myDictionary",
    "Operation": "Update",
    "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1",
    "Etag" : "5597826295554902436",
    "Key": "32",
    "Value" : "96"
  }
]
```

#### Response 200

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
```
POST http://localhost:5000/$query/
```

#### Request Body
```json
[
    {
    "Collection" : "myDictionary",
    "Operation": "Delete",
    "PartitionId": "ed70fb1c-6972-452a-b183-6114b336e9a1", 
    "Key": "32",
    "ETag": "3579969540437049356"
    }
]
```

#### Response 200

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


## Backup the current state

Update the values of the current reliable collection with operation perfomed in requests.

### Request 

| METHOD        | Request URI   | 
| ------------- |:-------------:| 
|  POST         | /backup/{backuptype} | 



### Parameters
| NAME                    | Type                          | Required  | 
| -------------           |:-------------:                | :----:    |
| backuptype              | string : full or incremental  | Yes       |

### Response

| HTTP Status Code        | Description   | 
| -------------           |:-------------:| 
|  200                    | Success, when backup happens succesfully | 

## Examples - Take full backup

```
POST http://localhost:5000/backup/full
```
#### Request Body
```json
{
    "BackupLocation " : "E:\\Newbackup",
    "TimeoutInSecs " : "300",
    "CancellationTokenInSecs " : "300"
}
```

#### Response 200

```json
{
    "status": "success",
    "backPath": "E:\\Newbackup\\0bcc9342d40a4210a35b46a78406f014"
}
```

Full Backup is taken successfully.

## Examples - Take incremental backup

```
POST http://localhost:5000/backup/incremental
```
#### Request Body
```json
{
    "BackupLocation " : "E:\\Newbackup",
    "TimeoutInSecs " : "300",
    "CancellationTokenInSecs " : "300"
}
```
#### Response 200

```json
{
    "status": "success",
    "backPath": "E:\\Newbackup\\0bcc9342d40a4210a35b46a78406f014"
}
```

Incremental Backup is taken successfully.
