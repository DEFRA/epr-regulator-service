namespace EPR.Common.Functions.Test.Extensions;

using System.Text;
using EPR.Common.Functions.Extensions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FileSystemInfoExtensionsTests
{
    private DirectoryInfo tempDir;

    [TestInitialize]
    public void Setup()
    {
        this.tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        this.tempDir.Create();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (this.tempDir.Exists)
        {
            this.tempDir.Delete(true);
        }
    }

    [TestMethod]
    public void Directory_ReturnsSubdirectory()
    {
        var subDir = this.tempDir.Directory("subdir");

        subDir.FullName.Should().Be(Path.Combine(this.tempDir.FullName, "subdir"));
    }

    [TestMethod]
    public void File_ReturnsFileInDirectory()
    {
        var file = this.tempDir.File("test.txt");

        file.FullName.Should().Be(Path.Combine(this.tempDir.FullName, "test.txt"));
    }

    [TestMethod]
    public void RelativeTo_ReturnsRelativePath()
    {
        var subDir = new DirectoryInfo(Path.Combine(this.tempDir.FullName, "sub", "dir"));

        var relative = subDir.RelativeTo(this.tempDir);

        relative.Should().Be(Path.Combine("sub", "dir"));
    }

    [TestMethod]
    public void ReadAllText_ReadsFileContents()
    {
        var file = this.tempDir.File("test.txt");
        System.IO.File.WriteAllText(file.FullName, "hello");

        var content = file.ReadAllText();

        content.Should().Be("hello");
    }

    [TestMethod]
    public void ReadAllLines_ReadsFileLines()
    {
        var file = this.tempDir.File("test.txt");
        System.IO.File.WriteAllLines(file.FullName, new[] { "line1", "line2" });

        var lines = file.ReadAllLines();

        lines.Should().BeEquivalentTo(new[] { "line1", "line2" });
    }

    [TestMethod]
    public void NameWithoutExtension_ReturnsNameOnly()
    {
        var file = new FileInfo("/path/to/file.txt");

        var name = file.NameWithoutExtension();

        name.Should().Be("file");
    }

    [TestMethod]
    public void WriteAllText_WritesContents()
    {
        var file = this.tempDir.File("test.txt");

        file.WriteAllText("content");

        System.IO.File.ReadAllText(file.FullName).Should().Be("content");
    }

    [TestMethod]
    public void WriteAllText_WithEncoding_WritesContents()
    {
        var file = this.tempDir.File("test.txt");

        file.WriteAllText("content", Encoding.UTF8);

        System.IO.File.ReadAllText(file.FullName).Should().Be("content");
    }

    [TestMethod]
    public void WriteAllLines_WritesLines()
    {
        var file = this.tempDir.File("test.txt");

        file.WriteAllLines(new[] { "a", "b" });

        System.IO.File.ReadAllLines(file.FullName).Should().BeEquivalentTo(new[] { "a", "b" });
    }

    [TestMethod]
    public void ChangeExtension_ReturnsFileWithNewExtension()
    {
        var file = new FileInfo("/path/to/file.txt");

        var changed = file.ChangeExtension(".md");

        changed.FullName.Should().EndWith("file.md");
    }
}
