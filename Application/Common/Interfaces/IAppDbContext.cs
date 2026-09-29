using System.Collections.Generic;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Book> Books { get; }
    DbSet<Paper> Papers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}