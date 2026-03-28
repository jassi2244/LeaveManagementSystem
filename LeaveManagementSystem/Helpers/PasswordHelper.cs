using Microsoft.AspNetCore.Identity;

namespace LeaveManagementSystem.Helpers;

public static class PasswordHelper
{
    public static string HashPassword<TUser>(IPasswordHasher<TUser> hasher, TUser user, string password) where TUser : class
        => hasher.HashPassword(user, password);
}
