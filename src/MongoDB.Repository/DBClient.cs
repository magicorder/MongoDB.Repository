﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Repository
{
    public class DBClient : IDBClient
    {
        private MongoClient client;
        private string dbName;
        Type type;
        public DBClient(MongoUrl url, Type type)
        {
            dbName = url.DatabaseName;
            this.type = type;
            client = new MongoClient(url);
        }
        /// <summary>
        /// database name
        /// </summary>
        public string DBName
        {
            get
            {
                return dbName;
            }
        }
        /// <summary>
        /// MongoCollection
        /// </summary>
        public MongoCollection Collection
        {
            get
            {
                return client.GetServer().GetDatabase(DBName).GetCollection(type.Name);
            }
        }

        #region 资源回收
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Close()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    client = null;
                    type = null;
                    dbName = null;
                }
                // Release unmanaged resources
                m_disposed = true;
            }
        }
        ~DBClient()
        {
            Dispose(false);
        }

        private bool m_disposed;
        #endregion
    }
}