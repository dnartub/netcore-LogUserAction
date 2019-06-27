using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace LogUserAction
{
    /// <summary>
    /// LogUserAction table
    /// </summary>
    public class LogUserAction
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime LogTime { get; set; }
        public string Login { get; set; }
        public string UserIP { get; set; }
        public string UserName { get; set; }
        public string ActionDescription { get; set; }
        public string ApiResult { get; set; }
        public string ErrorMessageResult { get; set; }

        public string JsonRequestDetails { get; set; }
        public string JsonDatabaseDetails { get; set; }
    }
}
