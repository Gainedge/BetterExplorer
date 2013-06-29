using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects
{
    internal class BaseFileEntry : ICloudFileSystemEntry
    {
        protected ICloudDirectoryEntry _parent;
        protected IStorageProviderService _service;
        protected IStorageProviderSession _session;

        private Dictionary<String, Object> _properties = new Dictionary<string, Object>();

        internal Boolean IsDeleted { get; set; }

        public BaseFileEntry(String Name, long Length, DateTime Modified, IStorageProviderService service, IStorageProviderSession session )
        {
            this.Name = Name;
            this.Length = Length;
            this.Modified = Modified;
            _service = service;
            _session = session;

            IsDeleted = false;
        }

        #region ICloudFileSystemEntry Members

        public string Name { get; set; }
        public long Length { get; set; }
        public DateTime Modified { get; set; }        

        public ICloudDirectoryEntry Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }        

        public string GetPropertyValue(string key)
        {
            return _properties[key].ToString();
        }

        public void SetPropertyValue(string key, Object value)
        {
            if (_properties.ContainsKey(key))
                _properties.Remove(key);

            _properties.Add(key, value);
        }        

        public ICloudFileDataTransfer GetDataTransferAccessor()
        {
            return new BaseFileEntryDataTransfer(this, _service, _session);
        }

        #endregion
    }
}
