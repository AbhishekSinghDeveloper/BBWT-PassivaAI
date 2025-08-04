using AutoMapper;

using BBWM.Core.Test;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace BBWM.FileStorage.Test;

public class FileStorageFixture
{
    public FileStorageFixture()
    {
        SetupFileStorageProvider();

        Mapper = AutoMapperConfig.CreateMapper(services =>
        {
            services
                .AddSingleton(FileStorageProvider.Object)
                .AddSingleton<FileDetailsUrlResolver>();
        });
    }

    public IMapper Mapper { get; }

    public Mock<IFileStorageProvider> FileStorageProvider { get; private set; }

    private void SetupFileStorageProvider()
    {
        FileStorageProvider = new Mock<IFileStorageProvider>();
        FileStorageProvider.Setup(a => a.DeleteFile(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(true);
        FileStorageProvider
            .Setup(a => a.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(() => new StorageFileData
            {
                IsImage = true,
                Key = "key",
                LastModifiedDate = DateTime.Now,
                Size = 4096,
                Url = "https://some-bucket.com/img/key",
            });
        FileStorageProvider
            .Setup(p => p.GetFile(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(() => new StorageFileData
            {
                IsImage = true,
                Key = "key",
                Url = "https://some-bucket.com/img/key",
                LastModifiedDate = DateTime.Now,
                Size = 4096,
            });
    }
}
