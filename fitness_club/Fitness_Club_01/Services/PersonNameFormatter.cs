using Fitness_Club_01.Data;

namespace Fitness_Club_01.Services;

public static class PersonNameFormatter
{
    public static string FullName(string lastName, string firstName, string? patronymic)
    {
        var parts = new[] { lastName, firstName, patronymic }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(' ', parts);
    }

    public static string FullName(Client client) =>
        FullName(client.LastName, client.FirstName, client.Patronymic);

    public static string FullName(Trainer trainer) =>
        FullName(trainer.LastName, trainer.FirstName, trainer.Patronymic);

    public static string FullName(Staff staff) =>
        FullName(staff.LastName, staff.FirstName, staff.Patronymic);
}
