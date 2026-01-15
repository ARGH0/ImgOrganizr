namespace ImgOrganizr.Application
{
    public class FileHandler
    {
        public void CreateFile(string filePath)
        {
            File.Create(filePath);
        }
        public void MoveFile(string sourceFilePath, string destinationFilePath)
        {
            File.Move(sourceFilePath, destinationFilePath);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }
    }
}
