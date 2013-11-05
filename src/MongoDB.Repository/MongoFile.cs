﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace MongoDB.Repository
{
    public class MongoFile : Entity, IMongoFile
    {
        /// <summary>
        /// 构造函数（并加载文件）
        /// </summary>
        /// <param name="localFileName">本地路径（绝对路径）</param>
        /// <param name="remoteFileName">存储文件名</param>
        /// <param name="contentType">文件类型</param>
        public MongoFile(string localFileName, string remoteFileName, string contentType)
        {
            this.LocalFileName = localFileName;
            this.RemoteFileName = remoteFileName ?? localFileName;
            _contentType = contentType;

            using (Stream stream = File.OpenRead(localFileName))
            {
                _data = new byte[(int)stream.Length];
                _size = stream.Read(_data, 0, (int)stream.Length);
            }
            UploadDate = DateTime.Now;
        }
        /// <summary>
        /// 构造函数（并加载文件）
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="remoteFileName">存储文件名</param>
        /// <param name="contentType">文件类型</param>
        public MongoFile(Stream fileStream, string remoteFileName, string contentType)
        {
            this.RemoteFileName = remoteFileName;
            _contentType = contentType;
            _data = new byte[(int)fileStream.Length];
            _size = fileStream.Read(_data, 0, (int)fileStream.Length);
            UploadDate = DateTime.Now;
        }
        /// <summary>
        /// 构造函数（并设置属性值）
        /// </summary>
        /// <param name="file">MongoGridFSFileInfo</param>
        public MongoFile(MongoGridFSFileInfo file)
        {
            if (file == null) throw new MongoGridFSException(this.ToString());
            if (!file.Exists) return;
            Id = file.Id.ToString();

            _md5 = file.MD5;
            _contentType = file.ContentType;

            _data = new byte[(int)file.Length];
            _size = file.OpenRead().Read(_data, 0, (int)file.Length);

            _aliases.Clear();
            if (file.Aliases != null)
                _aliases.AddRange(file.Aliases);

            Metadata = file.Metadata;
            RemoteFileName = file.Name;
            UploadDate = file.UploadDate;
        }

        private string _contentType, _md5;
        private byte[] _data;
        private int _size = 256;
        private List<string> _aliases = new List<string>();

        public string RemoteFileName { get; set; }

        public string LocalFileName { get; set; }

        public byte[] Data { get { return _data; } }

        public string MD5 { get { return _md5; } }

        public string ContentType
        {
            get { return _contentType; }
        }

        public int Size
        {
            get { return _size; }
        }

        public string[] Aliases
        {
            get
            {
                if (_aliases.Count == 0) return null;
                return _aliases.ToArray();
            }
        }

        public Bson.BsonDocument Metadata { get; set; }

        public DateTime UploadDate { get; set; }

        public override void Save()
        {
            EntityOperationExtensions.DBSaveGridFS(this);
        }

        public override void Remove()
        {
            EntityOperationExtensions.DBRemoveGridFS(this.Id);
        }

        public void AddAlias(string alias)
        {
            _aliases.Add(alias);
        }

        public void AddAliases(string[] aliases)
        {
            _aliases.AddRange(aliases);
        }
        
        public void Download(string localFileName)
        {
            using (Stream stream = File.OpenWrite(localFileName))
            {
                Download(stream);
            }
        }

        public void Download(Stream stream)
        {
            stream.Write(Data, 0, Size);
        }
    }
}