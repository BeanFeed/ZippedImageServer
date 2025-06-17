namespace ZippedImageServer.Models.Image;

public class UploadImageModel
{
    public required string Category { get; set; }
    public required IFormFile File { get; set; }
}