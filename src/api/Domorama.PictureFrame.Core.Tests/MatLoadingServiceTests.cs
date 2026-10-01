using Domorama.PictureFrame.Core.DataSource.Filesystem;
using OpenCvSharp;

namespace Domorama.PictureFrame.Core.Tests;

public class MatLoadingServiceTests
{
    [Fact]
    public async Task LoadMatThroughImageMagick_ShouldLoad_FujifilmImage()
    {
        // Arrange
        var service = new MatLoadingService();
        var path = @"Data/fujifilm-raw-image-001.RAF";
        
        // Act
        var mat = await service.LoadMatThroughImageMagick(path);
        
        // Asset
        Assert.NotNull(mat);
    }
}