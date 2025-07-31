using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.Entities;

namespace VidaPositiva.Api.Persistence;

public class Context(DbContextOptions<Context> options) : DbContext(options), IDataProtectionKeyContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Pote> Potes { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}