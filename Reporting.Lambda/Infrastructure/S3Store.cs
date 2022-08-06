using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

internal class S3Store : IStorage
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _coreAmazonS3;

    public S3Store(IAmazonS3 coreAmazonS3, IOptions<Config> options)
    {
        _coreAmazonS3 = coreAmazonS3;
        _bucketName = options.Value.BucketName;
    }

    public async Task SaveAsync(BaseCommand command, Stream stream, CancellationToken token)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            InputStream = stream,
            Key = $"{command.UserId}/{command.ReportName}",
        };

        await _coreAmazonS3.PutObjectAsync(request, token);
    }
}