namespace cmsUserManagment.Application.DTO;

public class FileUploadedEvent
{
    public string EntryId { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Originalname { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
