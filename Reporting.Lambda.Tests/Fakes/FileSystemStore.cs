internal class FileSystemStore : IStorage
{
    public async Task SaveAsync(BaseCommand command, Stream stream, CancellationToken token)
    {
        using var fileStream = new FileStream($"{command.ReportName}", FileMode.Create, FileAccess.Write);

        await stream.CopyToAsync(fileStream, token);
    }
}