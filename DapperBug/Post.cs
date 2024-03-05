namespace DapperBug;

public class Post
{
    public required int PostId { get; set; }
    public required string Title { get; set; }
    public required PostContent Content { get; set; }
}