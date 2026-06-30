using Fitness_Club_01.Data;

namespace Fitness_Club_01.Services;

public static class Session
{
    public static UserAccount? CurrentUser { get; set; }
    public static UserRole? CurrentRole { get; set; }

    public static void Logout()
    {
        CurrentUser = null;
        CurrentRole = null;
    }

    public static bool IsAuthenticated => CurrentUser != null;

    public static bool IsRole(string roleName) => CurrentRole?.RoleName == roleName;

    public static bool IsAdmin => IsRole("Администратор");
    public static bool IsReception => IsRole("Ресепшн");
    public static bool IsTrainer => IsRole("Тренер");
    public static bool IsClient => IsRole("Клиент");

    public static string GetDisplayName()
    {
        if (CurrentUser?.IdClientNavigation != null)
            return PersonNameFormatter.FullName(CurrentUser.IdClientNavigation);

        if (CurrentUser?.IdStaffNavigation != null)
            return PersonNameFormatter.FullName(CurrentUser.IdStaffNavigation);

        if (CurrentUser?.IdTrainerNavigation != null)
            return PersonNameFormatter.FullName(CurrentUser.IdTrainerNavigation);

        return CurrentUser?.Login ?? "";
    }
}
