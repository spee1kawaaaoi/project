using localhost.Models;

public class RequestComment
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int AuthorId { get; set; }
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    
    public User Author { get; set; } = null!;
}
