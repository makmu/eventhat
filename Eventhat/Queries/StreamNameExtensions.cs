using Eventhat.Database.Entities;

namespace Eventhat.Queries;

public static class UserCredentialsQueryExtensions
{
    public static Task<UserCredentials?> ByEmailAsync(this IQueryable<UserCredentials> db, string email)
    {
        return Task.FromResult(db.FirstOrDefault(c => c.Email == email));
    }
}