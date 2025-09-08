namespace MVCFileUpload.Models
{
    public class Assignment
    {
        public int Id { get; set; }  
        public string FileName { get; set; }
        public string UploaderName { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
