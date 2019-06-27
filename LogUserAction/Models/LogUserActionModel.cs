using System;
using System.Collections.Generic;
using System.Text;

namespace LogUserAction
{
    /// <summary>
    /// LogUserAction Model
    /// </summary>
    public class LogUserActionModel
    {
        public Guid Id { get; set; }

        public DateTime LogTime { get; set; }
        public string Login { get; set; }
        public string UserIP { get; set; }
        public string UserName { get; set; }
        public string ActionDescription { get; set; }
        public string ApiResult { get; set; }
        public string ErrorMessageResult { get; set; }

        public LogRequestDetails RequestDetails { get; set; }
        public LogDatabaseDetails DatabaseDetails { get; set; }
    }
}
