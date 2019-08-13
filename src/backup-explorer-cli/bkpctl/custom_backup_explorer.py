import requests
import json
import xmltodict
import xml.etree.ElementTree as ET
import sys
import time
import ast
from xml.etree.ElementTree import fromstring
from xmljson import BadgerFish 
from collections import OrderedDict
from pandas.io.json import json_normalize
from bkpctl.swagger_client.rest import *
from bkpctl.swagger_client.models import *
from bkpctl.swagger_client.api.default_api import *
from pprint import pprint
import jsbeautifier
import xml.dom.minidom

def print_json(value):
    print (jsbeautifier.beautify(value))

def print_xml(value):
    xmlValue = xml.dom.minidom.parseString(value)
    print (xmlValue.toprettyxml())

def query_metadata(name = None):
    api_instance = DefaultApi()
    try:
        # Returns metadata of Reliable Collections.
        api_response = api_instance.querymetadata_get()
        print_xml(''.join(api_response))
    except ApiException as e:
        print("Exception when calling DefaultApi->querymetadata_get: %s\n" % e)

def query_collection(name):
    api_instance = DefaultApi()
    try:
        api_response = api_instance.query_dictionary_name_get(name)
        print_json(api_response)
    except ApiException as e:
        print("Exception when calling DefaultApi->query_dictionary_name_get: %s\n" % e)

def get_transactions():
    api_instance = DefaultApi()
    try:
        api_response = api_instance.api_transactions_get()
        print_json(api_response)
    except ApiException as e:
        print("Exception when calling DefaultApi->api_transactions_get: %s\n" % e)

def add_transaction(operation,collection, partition_id, key, value = None, etag=None, collectiontype="dictionary"):
    api_instance = DefaultApi()
    try:
        if(collectiontype.lower()=="dictionary"):
            body = [AddTransactionRequest(collection=collection, partition_id= partition_id, key= key, etag= etag, value= value,operation=operation)]
            api_response = api_instance.query_post(body)
            pprint(api_response)
    except ApiException as e:
        print("Exception when calling DefaultApi->querymetadata_get: %s\n" % e)

def trigger_backup(typeOfBackup, path, timeout= 300, cancellationInSecs = 300) : 
    api_instance = DefaultApi()
    # create an instance of the API class
    api_instance = DefaultApi()
    body = BackupRequest(backup_location=path, timeout_in_secs=timeout, cancellation_token_in_secs=cancellationInSecs) # BackupRequest | Details to trigger the incremental Backup

    try:
    # Triggers a incremental backup of the current state of reliable collection
        if(typeOfBackup.lower() == "full"):
            api_response = api_instance.api_backup_full_post_with_http_info(body)    
            print_json(api_response[0])
        elif (typeOfBackup.lower() == "incremental"):
            api_response = api_instance.api_backup_incremental_post(body)
            print_json(api_response)
        else:
            print("Valid type of Backup is either full or incremental")

    except ApiException as e:
        print("Exception when calling DefaultApi->api_backup_incremental_post: %s\n" % e)
    
