using System;
using System.Collections.Generic;
using System.Text;

namespace LogUserAction
{
    /// <summary>
    /// Db-changes model
    /// </summary>
    public class LogDatabaseDetails
    {
        public List<LogDatabaseObject> Added { get; set; }
        public List<LogDatabaseObject> Modified { get; set; }
        public List<LogDatabaseObject> Deleted { get; set; }
    }
}
