namespace FuriaKYFApi.Source.Models.Dtos
{
    public class DocumentValidationDto
    {
        public string FileName { get; set; }
        public DocumentDto OcrContents { get; set; }
    }
}
