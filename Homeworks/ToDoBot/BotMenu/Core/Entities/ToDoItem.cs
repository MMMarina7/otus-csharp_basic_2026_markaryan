public class ToDoItem
{
    public Guid Id { get; }
    public ToDoUser User { get; }
    public string Name { get; }
    public DateTime CreatedAt { get; }
    public ToDoItemState State { get; set;}
    public DateTime? StateChangedAt { get; set; }

    public ToDoItem(string name, ToDoUser user)
    {
        Id = Guid.NewGuid();
        Name = name;
        User = user;
        CreatedAt = DateTime.UtcNow;
        State = ToDoItemState.Active;
        StateChangedAt = null;
    }
}