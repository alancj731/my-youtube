using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace MyYt.Infrastructure.Persistence;

internal static class DbErrors
{
    private const int DuplicateEntry = 1062;

    public static bool IsDuplicateKey(this DbUpdateException ex) =>
        ex.InnerException is MySqlException { Number: DuplicateEntry };
}
