using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Data.UnitOfWork
{
    public class UnitOfWork(AppDbContext db): IUnitOfWork
    {
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync();

            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync();

            try
            {
                T result = await action();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
