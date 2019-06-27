using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Threading;


namespace LogUserAction
{
    /// <summary>
    /// Contex with overriding SaveChanges
    /// </summary>
    public class LogContext : DbContext
    {
        private ILogUserActionService LogUserActionService { get; set; }

        // scopped instance ILogUserActionService with scopped instance of LogUserActionModel
        public LogContext(DbContextOptions options, ILogUserActionService logUserActionService) : base(options)
        {
            LogUserActionService = logUserActionService;
        }

        public DbSet<LogUserAction> LogUserAction { get; set; }

        public override int SaveChanges()
        {
            if (LogUserActionService == null)
            {
                return base.SaveChanges();
            }

            LogDatabaseDetails changes = null;
            try
            {
                changes = LogUserActionService.GetChangesBeforeSave(this);
            }
            catch (Exception)
            {
            }

            var result =  base.SaveChanges();

            LogUserActionService.AddChangesAfterSave(changes);

            return result;
        }

        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (LogUserActionService == null)
            {
                return await base.SaveChangesAsync();
            }

            LogDatabaseDetails changes = null;
            try
            {
                changes = LogUserActionService.GetChangesBeforeSave(this);
            }
            catch (Exception)
            {
            }

            var result = await base.SaveChangesAsync();

            LogUserActionService.AddChangesAfterSave(changes);

            return result;
        }
    }
}