using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using EDM.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IConfiguration configuration, ILogger<S3Service> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private (AmazonS3Client client, string bucket) CreateClient()
    {
        var accessKey = _configuration["Aws:AccessKey"] ?? throw new InvalidOperationException("Aws:AccessKey is not configured");
        var secretKey = _configuration["Aws:SecretKey"] ?? throw new InvalidOperationException("Aws:SecretKey is not configured");
        var region = _configuration["Aws:Region"] ?? "eu-west-1";
        var bucketName = _configuration["Aws:BucketName"] ?? throw new InvalidOperationException("Aws:BucketName is not configured");
        return (new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(region)), bucketName);
    }

    public async Task<string> UploadAsync(Stream fileStream, string s3Key, CancellationToken cancellationToken)
    {
        var (s3Client, bucketName) = CreateClient();
        using var _ = s3Client;
        var transferUtility = new TransferUtility(s3Client);

        await transferUtility.UploadAsync(new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = s3Key,
            BucketName = bucketName,
            ContentType = "video/mp4"
        }, cancellationToken);

        _logger.LogInformation("S3'e yüklendi: s3://{Bucket}/{Key}", bucketName, s3Key);
        return s3Key;
    }

    public string GetPresignedUrl(string s3Key, TimeSpan expiry)
    {
        var (s3Client, bucketName) = CreateClient();
        using var _ = s3Client;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = s3Key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET
        };

        return s3Client.GetPreSignedURL(request);
    }
}
