using System;
using System.IO;
using Xunit;
using EasySave.Model;

namespace EasySave.Tests.Model {
    public class FileHandlerTests {
        private readonly string _testFilePath = "testfile.txt";

        public FileHandlerTests() {
            // Ensure a clean state before each test
            if (File.Exists(_testFilePath)) {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public void GetName_ShouldReturnFileName() {
            var fileHandler = new FileHandler(_testFilePath);
            var result = fileHandler.GetName();
            Assert.Equal("testfile.txt", result);
        }

        [Fact]
        public void GetSize_ShouldReturnFileSize_WhenFileExists() {
            File.WriteAllText(_testFilePath, "Test content");
            var fileHandler = new FileHandler(_testFilePath);
            var result = fileHandler.GetSize();
            Assert.Equal(new FileInfo(_testFilePath).Length, result);
        }

        [Fact]
        public void GetSize_ShouldThrowFileNotFoundException_WhenFileDoesNotExist() {
            var fileHandler = new FileHandler(_testFilePath);
            Assert.Throws<FileNotFoundException>(() => fileHandler.GetSize());
        }

        [Fact]
        public void Remove_ShouldDeleteFile_WhenFileExists() {
            File.WriteAllText(_testFilePath, "Test content");
            var fileHandler = new FileHandler(_testFilePath);
            fileHandler.Remove();
            Assert.False(File.Exists(_testFilePath));
        }

        [Fact]
        public void Remove_ShouldThrowFileNotFoundException_WhenFileDoesNotExist() {
            var fileHandler = new FileHandler(_testFilePath);
            Assert.Throws<FileNotFoundException>(() => fileHandler.Remove());
        }

        [Fact]
        public void Move_ShouldMoveFileToNewLocation() {
            var destinationPath = "movedfile.txt";
            File.WriteAllText(_testFilePath, "Test content");
            var directoryHandler = new DirectoryHandler(".");
            var fileHandler = new FileHandler(_testFilePath);

            fileHandler.Move(directoryHandler);

            Assert.False(File.Exists(_testFilePath));
            Assert.True(File.Exists(destinationPath));

            File.Delete(destinationPath);
        }

        [Fact]
        public void Copy_ShouldCopyFileToNewLocation() {
            var destinationPath = "copiedfile.txt";
            File.WriteAllText(_testFilePath, "Test content");
            var directoryHandler = new DirectoryHandler(".");
            var fileHandler = new FileHandler(_testFilePath);

            fileHandler.Copy(directoryHandler);

            Assert.True(File.Exists(_testFilePath));
            Assert.True(File.Exists(destinationPath));

            File.Delete(destinationPath);
        }

        [Fact]
        public void Rename_ShouldRenameFile() {
            var newName = "renamedfile.txt";
            File.WriteAllText(_testFilePath, "Test content");
            var fileHandler = new FileHandler(_testFilePath);

            fileHandler.Rename(newName);

            Assert.False(File.Exists(_testFilePath));
            Assert.True(File.Exists(newName));

            File.Delete(newName);
        }

        [Fact]
        public void Exists_ShouldReturnTrue_WhenFileExists() {
            File.WriteAllText(_testFilePath, "Test content");
            var fileHandler = new FileHandler(_testFilePath);
            var result = fileHandler.Exists();
            Assert.True(result);
        }

        [Fact]
        public void Exists_ShouldReturnFalse_WhenFileDoesNotExist() {
            var fileHandler = new FileHandler(_testFilePath);
            var result = fileHandler.Exists();
            Assert.False(result);
        }

        [Fact]
        public void GetExtension_ShouldReturnFileExtension() {
            var fileHandler = new FileHandler(_testFilePath);
            var result = fileHandler.GetExtension();
            Assert.Equal(".txt", result);
        }

        [Fact]
        public void Write_ShouldCreateAndWriteToFile() {
            var content = "Test content";
            var fileHandler = new FileHandler(_testFilePath);

            fileHandler.Write(content);

            Assert.True(File.Exists(_testFilePath));
            Assert.Equal(content, File.ReadAllText(_testFilePath));
        }

        [Fact]
        public void Append_ShouldAppendContentToFile() {
            var initialContent = "Initial content";
            var appendedContent = " Appended content";
            File.WriteAllText(_testFilePath, initialContent);
            var fileHandler = new FileHandler(_testFilePath);

            fileHandler.Append(appendedContent);

            Assert.Equal(initialContent + appendedContent, File.ReadAllText(_testFilePath));
        }
    }
}
