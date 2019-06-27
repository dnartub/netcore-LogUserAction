using System;
using System.Collections.Generic;
using System.Text;

namespace LogUserAction
{
    public class LogDatabaseObject
    {
        public string TableName { get; set; }
        /// <summary>
        /// Data object
        /// * insert/delete operations:
        /// key - field name
        /// value - field value
        /// * update operations
        /// key - field name
        /// value = object[2]
        /// value[0] - old field value
        /// value[1] - new field value
        /// </summary>
        public Dictionary<string, object> Values { get; set; }
    }
}
