namespace Eventa.Application.DTOs.Sections
{
    public class RowTypeDto
    {
        public string Name { get; set; } = string.Empty;

        public RowDto[] Rows { get; set; } = [];
    }
}
