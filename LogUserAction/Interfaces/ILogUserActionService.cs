using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LogUserAction
{
    public interface ILogUserActionService
    {
        LogUserActionModel LogUserActionModel { get; }


        /// <summary>
        /// Fill LogUserActionModel from Request Context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        LogUserActionService Fill(ActionExecutedContext context);

        /// <summary>
        /// Get changes from EF
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        LogDatabaseDetails GetChangesBeforeSave(DbContext context);
        /// <summary>
        /// Add db-changes to LogUserActionModel
        /// </summary>
        /// <param name="changes"></param>
        void AddChangesAfterSave(LogDatabaseDetails changes);
    }
}