namespace Fitness_Club_01.Models;

public class ClientRow
{
    public int IdClient { get; set; }
    public int Number { get; set; }
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? Patronymic { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string BirthDateText { get; set; } = "";
    public string HasAccount { get; set; } = "";
    public string Login { get; set; } = "";
}
