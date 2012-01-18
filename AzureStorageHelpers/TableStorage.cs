﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureStorageHelpers
{
	/// <summary>
	/// Simple helper class for Windows Azure storage tables
	/// </summary>
	public class TableStorage<T> where T : TableServiceEntity
	{
		private string tableName;
		private CloudTableClient cloudTableClient;
		private TableServiceContext tableServiceContext;

		/// <summary>
		/// Creates a new TableStorage object
		/// </summary>
		/// <param name="tableStorageName">The name of the table to be managed</param>
		/// <param name="storageConnectionString">The connection string pointing to the storage account (this can be local or hosted in Windows Azure</param>
		/// <param name="useNagleAlgorithm">PUT HTTP requests that are smaller than 1460 bytes are inefficient with the Nagle algorithm turned on.</param>
		public TableStorage(string tableStorageName, string storageConnectionString, bool useNagleAlgorithm = false)
		{
			tableName = tableStorageName;

			CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);

			cloudTableClient = new CloudTableClient(cloudStorageAccount.TableEndpoint.AbsoluteUri, cloudStorageAccount.Credentials);
			cloudTableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
			cloudTableClient.CreateTableIfNotExist(tableName);

			if (!useNagleAlgorithm)
				DisableNagleOnEndpoint(cloudStorageAccount);
		}

		/// <summary>
		/// Creates a new entity in the table
		/// </summary>
		/// <param name="entity">The entity to store in the table</param>
		public void CreateEntity(T entity)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			tableServiceContext.AddObject(tableName, entity);
			tableServiceContext.SaveChanges();
		}

		/// <summary>
		/// Deletes an entity from the table
		/// </summary>
		/// <param name="partitionKey">
		/// The partition key of the entity to be deleted. 
		/// Note that a partition key can return more than one entity. 
		/// If more than one are returned, the first one is deleted.
		/// </param>
		public void DeleteEntityByPartitionKey(string partitionKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.PartitionKey == partitionKey
						  select g;

			T entity = results.FirstOrDefault<T>();

			tableServiceContext.DeleteObject(entity);
			tableServiceContext.SaveChanges();
		}

		/// <summary>
		/// Deletes an entity from the table
		/// </summary>
		/// <param name="rowKey">
		/// The row key of the entity to be deleted. 
		/// Note that a row key can return more than one entity. 
		/// If more than one are returned, the first one is deleted.
		/// </param>
		public void DeleteEntityByRowKey(string rowKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.RowKey == rowKey
						  select g;

			T entity = results.FirstOrDefault<T>();

			tableServiceContext.DeleteObject(entity);
			tableServiceContext.SaveChanges();
		}

		/// <summary>
		/// Deletes an entity from the table
		/// </summary>
		/// <param name="partitionKey">
		/// The partition key of the entity to be deleted. 
		/// The partition key and row key will always return a single entity.
		/// </param>
		/// <param name="rowKey">
		/// The row key of the entity to be deleted.
		/// The partition key and row key will always return a single entity.
		/// </param>
		public void DeleteEntityByPartitionKeyAndRowKey(string partitionKey, string rowKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.PartitionKey == partitionKey && g.RowKey == rowKey
						  select g;

			T entity = results.FirstOrDefault<T>();

			tableServiceContext.DeleteObject(entity);
			tableServiceContext.SaveChanges();
		}

		/// <summary>
		/// Gets an entity from the table
		/// </summary>
		/// <param name="partitionKey">
		/// The partition key of the entity to be returned.
		/// Note that a partition key can return more than one entity. 
		/// If more than one are returned, the first one is returned.
		/// </param>
		public T GetEntityByPartitionKey(string partitionKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.PartitionKey == partitionKey
						  select g;

			return results.FirstOrDefault<T>();
		}

		/// <summary>
		/// Gets an entity from the table
		/// </summary>
		/// <param name="rowKey">
		/// The row key of the entity to be returned.
		/// Note that a row key can return more than one entity. 
		/// If more than one are returned, the first one is returned.
		/// </param>
		public T GetEntityByRowKey(string rowKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.RowKey == rowKey
						  select g;

			return results.FirstOrDefault<T>();
		}

		/// <summary>
		/// Gets an entity from the table
		/// </summary>
		/// <param name="partitionKey">
		/// The partition key of the entity to be returned. 
		/// The partition key and row key will always return a single entity.
		/// </param>
		/// <param name="rowKey">
		/// The row key of the entity to be returned.
		/// The partition key and row key will always return a single entity.
		/// </param>
		public T GetEntityByPartitionKeyAndRowKey(string partitionKey, string rowKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.PartitionKey == partitionKey && g.RowKey == rowKey
						  select g;

			return results.FirstOrDefault<T>();
		}

		/// <summary>
		/// Gets all entities from the table
		/// </summary>
		public IEnumerable<T> GetEntities()
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  select g;

			return results;
		}

		/// <summary>
		/// Gets entities from the table
		/// </summary>
		/// <param name="partitionKey">
		/// The partition key of the entities to be returned.
		/// </param>
		public IEnumerable<T> GetEntitiesByPartitionKey(string partitionKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.PartitionKey == partitionKey
						  select g;

			return results;
		}

		/// <summary>
		/// Gets entities from the table
		/// </summary>
		/// <param name="rowKey">
		/// The row key of the entities to be returned.
		/// </param>
		public IEnumerable<T> GetEntitiesByRowKey(string rowKey)
		{
			tableServiceContext = cloudTableClient.GetDataServiceContext();

			var results = from g in tableServiceContext.CreateQuery<T>(tableName)
						  where g.RowKey == rowKey
						  select g;

			return results;
		}

		/// <summary>
		/// Disables the nagle algorithm on the given storage account
		/// </summary>
		/// <param name="account">The cloud storage account to disable nagle</param>
		private void DisableNagleOnEndpoint(CloudStorageAccount account)
		{
			var tableServicePoint = ServicePointManager.FindServicePoint(account.TableEndpoint);
			tableServicePoint.UseNagleAlgorithm = false;
		}
	}
}