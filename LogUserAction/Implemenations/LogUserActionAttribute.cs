using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace LogUserAction
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogUserActionAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                LogUserAction(context).Wait();
            }
            catch (Exception)
            {
                // log
            }
        }

        private async Task LogUserAction(ActionExecutedContext context)
        {
            // scopped instance ILogUserActionService with scopped instance of LogUserActionModel
            var logUserActionModel = context.HttpContext.RequestServices.GetService<ILogUserActionService>()
                .Fill(context)
                .LogUserActionModel;

            var logUserAction = new LogUserAction()
            {
                Id = Guid.NewGuid(),
                LogTime = logUserActionModel.LogTime,
                Login = logUserActionModel.Login,
                UserIP = logUserActionModel.UserIP,
                UserName = logUserActionModel.UserName,
                ActionDescription = logUserActionModel.ActionDescription,
                ApiResult = logUserActionModel.ApiResult,
                ErrorMessageResult = logUserActionModel.ErrorMessageResult,
                JsonRequestDetails = JsonConvert.SerializeObject(logUserActionModel.RequestDetails),
                JsonDatabaseDetails = JsonConvert.SerializeObject(logUserActionModel.DatabaseDetails)
            };

            // find first di-service of LogContext class

            var services = context.HttpContext.RequestServices.GetService<IServiceCollection>();
            if (services == null)
            {
                return;
            }

            var logContextService = services.FirstOrDefault(s => s.ServiceType.IsSubclassOf(typeof(LogContext)));
            if (logContextService == null)
            {
                return;
            }

            // get db-context

            var logContext = context.HttpContext.RequestServices.GetService(logContextService.ServiceType) as LogContext;

            // write to db
            logContext.Add(logUserAction);
            await logContext.SaveChangesAsync();
        }
    }
}
