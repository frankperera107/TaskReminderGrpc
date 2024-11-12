namespace TaskReminderGrpc.Models;

public class TaskReminder
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "New";
}