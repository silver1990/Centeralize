using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Raybod.SCM.DataAccess.Core
{
    public interface IUnitOfWork
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbQuery<TEntity> Query <TEntity>() where TEntity : class;

        int SaveChange();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

    }
}
