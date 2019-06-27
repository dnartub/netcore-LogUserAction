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
    public class LogUserActionService: ILogUserActionService
    {
        public LogUserActionModel LogUserActionModel { get; private set; }

        public LogUserActionService(LogUserActionModel logUserActionModel)
        {
            LogUserActionModel = logUserActionModel;
        }

        public LogUserActionService Fill(ActionExecutedContext context)
        {
            LogUserActionModel.LogTime = DateTime.Now;
            LogUserActionModel.RequestDetails = new LogRequestDetails();

            FillByActionResult(context);
            FillByClaims(context.HttpContext.User);
            FillByActionDescriptor(context.ActionDescriptor as ControllerActionDescriptor);
            FillByRequest(context.HttpContext.Request);
            FillParameters(context).Wait();

            return this;
        }

        public LogDatabaseDetails GetChangesBeforeSave(DbContext context)
        {
            var entries = context.ChangeTracker.Entries();

            var addedEntities = entries
                .Where(entry => entry.State == EntityState.Added)
                .Where(entry => entry.Metadata.ClrType.CustomAttributes.Any(a => a.AttributeType == typeof(LogEntityAttribute)))
                .ToList();
            var modifiedEntities = entries
                .Where(entry => entry.State == EntityState.Modified)
                .Where(entry => entry.Metadata.ClrType.CustomAttributes.Any(a => a.AttributeType == typeof(LogEntityAttribute)))
                .ToList();
            var deletedEntities = entries
                .Where(entry => entry.State == EntityState.Deleted)
                .Where(entry => entry.Metadata.ClrType.CustomAttributes.Any(a => a.AttributeType == typeof(LogEntityAttribute)))
                .ToList();

            if (addedEntities.Count + modifiedEntities.Count + deletedEntities.Count == 0)
            {
                return null;
            }

            return new LogDatabaseDetails()
            {
                Added = GetAddedEntries(addedEntities),
                Modified = GetModifiedEntries(modifiedEntities),
                Deleted = GetDeletedEntries(deletedEntities)
            };
        }

        public void AddChangesAfterSave(LogDatabaseDetails changes)
        {
            if (changes != null)
            {
                LogUserActionModel.DatabaseDetails = LogUserActionModel.DatabaseDetails ?? new LogDatabaseDetails();
                if (changes.Added.Count != 0)
                {
                    LogUserActionModel.DatabaseDetails.Added = LogUserActionModel.DatabaseDetails.Added ?? new List<LogDatabaseObject>();
                    LogUserActionModel.DatabaseDetails.Added.AddRange(changes.Added);
                }
                if (changes.Modified.Count != 0)
                {
                    LogUserActionModel.DatabaseDetails.Modified = LogUserActionModel.DatabaseDetails.Modified ?? new List<LogDatabaseObject>();
                    LogUserActionModel.DatabaseDetails.Modified.AddRange(changes.Modified);
                }
                if (changes.Deleted.Count != 0)
                {
                    LogUserActionModel.DatabaseDetails.Deleted = LogUserActionModel.DatabaseDetails.Deleted ?? new List<LogDatabaseObject>();
                    LogUserActionModel.DatabaseDetails.Deleted.AddRange(changes.Deleted);
                }
            }
        }

        #region == Request context ==
        private void FillByActionResult(ActionExecutedContext context)
        {
            // only for ObjectResult and ObjectResultValue with Result property

            var actionResult = context.Result as ObjectResult;
            if (actionResult == null)
            {
                return;
            }

            var propResult = actionResult.Value.GetType().GetProperty("Result");
            if (propResult == null || !propResult.GetMethod.IsPublic)
            {
                return;
            }

            LogUserActionModel.ApiResult = propResult.GetValue(actionResult.Value) as string;
        }

        private void FillByClaims(ClaimsPrincipal user)
        {
            if (user?.Identity?.Name == null)
            {
                LogUserActionModel.Login = "<anonymous>";
                LogUserActionModel.UserName = "not authorized";

            }
            else
            {
                LogUserActionModel.Login = user.Identity.Name;
                LogUserActionModel.UserName = user.Claims
                    .FirstOrDefault(cliam => cliam.Type.Equals("UserName"))
                    ?.Value;
            }
        }

        private void FillByActionDescriptor(ControllerActionDescriptor controllerActionDescriptor)
        {
            var actionMethodInfo = controllerActionDescriptor.MethodInfo;

            // описание вызываемого метода
            var methodInfo = $"{actionMethodInfo.DeclaringType.FullName}.{actionMethodInfo.Name}({string.Join(", ", actionMethodInfo.GetParameters().Select(pi => $"{pi.ParameterType.Name} {pi.Name}"))})";
            LogUserActionModel.RequestDetails.MethodInfo = methodInfo;

            // атрибут [DisplayName] на методе действия
            LogUserActionModel.ActionDescription = actionMethodInfo.GetCustomAttributes(false)
                .Where(attr => attr is DisplayNameAttribute)
                .Select(attr => attr as DisplayNameAttribute)
                .FirstOrDefault()
                ?.DisplayName;
        }

        private void FillByRequest(HttpRequest request)
        {
            LogUserActionModel.UserIP = request.HttpContext.Connection.RemoteIpAddress.ToString();

            LogUserActionModel.RequestDetails.RoutePath = request.Path;

            LogUserActionModel.RequestDetails.Headers = request.Headers;
        }

        private async Task FillParameters(ActionExecutedContext context)
        {
            LogUserActionModel.RequestDetails.RouteValues = context.RouteData.Values;

            var query = context.HttpContext.Request.Query.ToDictionary(x => x.Key, x => x.Value);
            LogUserActionModel.RequestDetails.QueryValues = query;

            var formCollection = await ReadFormAsync(context);
            if (formCollection != null)
            {
                var form = formCollection.ToDictionary(x => x.Key, x => x.Value);
                LogUserActionModel.RequestDetails.QueryValues = form;
            }

            var bodyStr = await ReadBodyAsync(context);
            if (bodyStr != null)
            {
                // скрываем значение полей 'password'
                var regEx = new Regex(@"""(?i)password"": *""([^\\""]|\\"")*""");
                if (regEx.IsMatch(bodyStr))
                {
                    bodyStr = regEx.Replace(bodyStr, @"""password"": ""*******""");
                }

                var jObject = JsonConvert.DeserializeObject(bodyStr) as JObject;
                LogUserActionModel.RequestDetails.BodyValues = jObject;
            }
        }

        private async Task<IFormCollection> ReadFormAsync(ActionExecutedContext context)
        {
            if (!context.HttpContext.Request.HasFormContentType)
            {
                return null;
            }

            var result = await context.HttpContext.Request.ReadFormAsync();
            return result;
        }

        private async Task<string> ReadBodyAsync(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.ContentType == null)
            {
                return null;
            }

            if (!context.HttpContext.Request.ContentType.ToLower().Contains("json"))
            {
                return null;
            }

            string result = null;

            context.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(context.HttpContext.Request.Body, System.Text.Encoding.UTF8, true, 1024, true))
            {
                result = await reader.ReadToEndAsync();
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
            }

            return result;
        }
        #endregion

        #region == Db context ==

        private List<LogDatabaseObject> GetAddedEntries(List<EntityEntry> entries)
        {
            var result = new List<LogDatabaseObject>();
            if (entries.Count == 0)
            {
                return result;
            }

            foreach (var entry in entries)
            {
                var databaseObject = new LogDatabaseObject()
                {
                    TableName = entry.Metadata.ClrType.Name,
                    Values = new Dictionary<string, object>()
                };

                foreach (var property in GetProperties(entry.Entity))
                {
                    var value = entry.CurrentValues[property];
                    databaseObject.Values.Add(property, value);
                }

                result.Add(databaseObject);
            }

            return result;
        }

        private List<LogDatabaseObject> GetModifiedEntries(List<EntityEntry> entries)
        {
            var result = new List<LogDatabaseObject>();
            if (entries.Count == 0)
            {
                return result;
            }

            foreach (var entry in entries)
            {
                var databaseObject = new LogDatabaseObject()
                {
                    TableName = entry.Metadata.ClrType.Name,
                    Values = new Dictionary<string, object>()
                };

                foreach (var property in GetProperties(entry.Entity))
                {
                    var value = new object[2];
                    value[0] = entry.OriginalValues[property];
                    value[1] = entry.CurrentValues[property];
                    databaseObject.Values.Add(property, value);
                }

                result.Add(databaseObject);
            }

            return result;
        }

        private List<LogDatabaseObject> GetDeletedEntries(List<EntityEntry> entries)
        {
            var result = new List<LogDatabaseObject>();
            if (entries.Count == 0)
            {
                return result;
            }

            foreach (var entry in entries)
            {
                var databaseObject = new LogDatabaseObject()
                {
                    TableName = entry.Metadata.ClrType.Name,
                    Values = new Dictionary<string, object>()
                };

                foreach (var property in GetProperties(entry.Entity))
                {
                    var value = entry.OriginalValues[property];
                    databaseObject.Values.Add(property, value);
                }

                result.Add(databaseObject);
            }

            return result;
        }

        private List<string> GetProperties(object entity)
        {
            return entity
                .GetType()
                .GetProperties()
                .Where(p => !p.GetMethod.IsVirtual)
                .Select(p => p.Name)
                .ToList();
        } 
        #endregion
    }
}