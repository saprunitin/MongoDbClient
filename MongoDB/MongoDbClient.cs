﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Contracts;
using MongoDB.Driver;
using MongoDB.Exceptions;

namespace MongoDB
{
    public class MongoDbClient : IMongoDbClient
    {
        private readonly MongoClient mongoDB;

        /// <summary>
        ///     Initializes the class of type <see cref="MongoDbClient"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        public MongoDbClient(string connectionString)
        {
            connectionString.ShouldNotBeNullOrEmpty();
            this.mongoDB = new MongoClient(connectionString);
        }

        /// <summary>
        ///     Gets the mongoDB client object.
        /// </summary>
        /// <param name="connectionString">The connection string of the mongoDB database.</param>
        /// <returns>The mongoDB client.</returns>
        public MongoClient GetMongoDbClient
        {
            get
            {
                return this.mongoDB;
            }
        }

        /// <summary>
        ///     Gets the list of database in a MongoDB server.
        /// </summary>
        /// <returns>The list of database names in MongoDB server.</returns>
        public async Task<IEnumerable<string>> ListDatabasesAsync()
        {
            try
            {
                var databaseNames = await this.mongoDB.ListDatabaseNamesAsync().ConfigureAwait(false);
                return await databaseNames.ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException($"Failed to get the database names due to exception.", ex);
            }
        }

        /// <summary>
        ///     Gets the list of collections in a MongoDB database.
        /// </summary>
        /// <param name="databaseName">The name of the database in which the collections are present.</param>
        /// <returns>The list of collections names in MongoDB database.</returns>
        public async Task<IEnumerable<string>> ListCollectionsAsync(string databaseName)
        {
            try
            {
                var collections = await this.mongoDB.GetDatabase(databaseName)
                    .ListCollectionNamesAsync().ConfigureAwait(false);

                return await collections.ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException(ex);
            }
        }

        /// <summary>
        ///     Gets the collection using collection name & database name.
        /// </summary>
        /// <typeparam name="T">The type of object which replicates the JSON body of the document.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="collection">The collection name.</param>
        /// <returns>The collection object.s.</returns>
        public IMongoCollection<T> GetCollection<T>(string database, string collection)
        {
            database.ShouldNotBeNullOrEmpty();
            collection.ShouldNotBeNullOrEmpty();

            try
            {
                return this.mongoDB.GetDatabase(database).GetCollection<T>(collection);
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException(ex);
            }
        }

        /// <summary>
        ///     Gets the list of documents from a collection.
        /// </summary>
        /// <typeparam name="T">The type which replicates the JSON model of the document.</typeparam>
        /// <param name="database">The database name.</param>
        /// <param name="collection">The collection name.</param>
        /// <returns>The list of items present in collection.</returns>
        public async Task<IEnumerable<T>> ListDocumentsFromCollection<T>(string database, string collection)
        {
            database.ShouldNotBeNullOrEmpty();
            collection.ShouldNotBeNullOrEmpty();

            try
            {
                var collectionObject = this.mongoDB.GetDatabase(database).GetCollection<T>(collection);
                var items = collectionObject.Find(collection);

                return await items.ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException(ex);
            }
        }

        /// <summary>
        ///     Gets the list of documents from a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mongoCollection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ListDocumentsFromCollection<T>(IMongoCollection<T> mongoCollection, FilterDefinition<T> filter)
        {
            mongoCollection.ShouldNotBeNull();
            filter.ShouldNotBeNull();

            try
            {
                return await mongoCollection.Find(filter).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException(ex);
            }
        }

        /// <summary>
        ///     Inserts bulk documents into MongoDB collection.
        /// </summary>
        /// <typeparam name="T">The type of the document used in current transaction.</typeparam>
        /// <param name="mongoCollection">The mongo DB collection.</param>
        /// <param name="documents">The list of documents.</param>
        /// <returns>The count which lets the caller know, how many documents have been inserted into specified collection.</returns>
        public async Task<int> AddDocumentsToCollection<T>(IMongoCollection<T> mongoCollection, IList<T> documents)
        {
            mongoCollection.ShouldNotBeNull();
            documents.ShouldNotBeNull();

            try
            {
                await mongoCollection.InsertManyAsync(documents).ConfigureAwait(false);
                return documents.Count;
            }
            catch (Exception ex)
            {
                throw new MongoDbClientException(ex);
            }
        }

        /// <summary>
        ///     Creates a collection in a database if it does not exist.
        /// </summary>
        /// <typeparam name="T">The type of the document used in current transaction.</typeparam>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="databaseName">The database name.</param>
        /// <returns>The created mongoDB collection if it is newly created or returns the existing mongoDB collection.</returns>
        public async Task<IMongoCollection<T>> CreateCollectionIfNotExists<T>(string collectionName, string databaseName)
        {
            try
            {
                var database = mongoDB.GetDatabase(databaseName);

                var collection = database.GetCollection<T>(collectionName);

                if(collection != null)
                {
                    return collection;
                }

                await database.CreateCollectionAsync(collectionName).ConfigureAwait(false);

                return database.GetCollection<T>(collectionName);
            }
            catch(Exception ex)
            {
                throw new MongoDbClientException($"Failed to create mongoDB collection in database {databaseName}", ex);
            }

        }
    }
}
