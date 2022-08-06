using Amazon.S3;
using Amazon.S3.Model;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

public class GetReports : EndpointBaseAsync
    .WithRequest<string>
    .WithResult<List<ReportsModel>>
{
    private readonly IAmazonS3 _s3Client;
    private readonly Config _config;

    public GetReports(
        IAmazonS3 amazonS3,
        IOptions<Config> options)
    {
        _s3Client = amazonS3;
        _config = options.Value;
    }

    [HttpGet("api/reports")]
    public override async Task<List<ReportsModel>> HandleAsync(string userId, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _config.BucketName,
            Prefix = userId,
        });

        ConcurrentBag<ReportsModel> result = new();

        Parallel.ForEach(
            response.S3Objects,
            item =>
            {
                result.Add(new ReportsModel
                {
                    Created = item.LastModified,
                    SignedUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                    {
                        Key = item.Key,
                        BucketName = _config.BucketName,
                        Expires = DateTime.UtcNow.AddHours(1)
                    })
                });
            });

        return result.ToList();
    }
}

public class ReportsModel 
{
    public string SignedUrl { get; set; }

    public DateTime Created { get; set; }   
}
