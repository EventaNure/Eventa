using System.Collections.Generic;
using System.Linq;

namespace Eventa.Models.Events;

public class EventsRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<int>? TagIds { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? SubName { get; set; }

    public string ToQueryString()
    {
        var parameters = new List<string>
        {
            $"pageNumber={PageNumber}",
            $"pageSize={PageSize}"
        };

        if (TagIds != null && TagIds.Any())
        {
            parameters.AddRange(TagIds.Select(id => $"tagIds={id}"));
        }

        if (!string.IsNullOrWhiteSpace(StartDate))
        {
            parameters.Add($"startDate={System.Uri.EscapeDataString(StartDate)}");
        }

        if (!string.IsNullOrWhiteSpace(EndDate))
        {
            parameters.Add($"endDate={System.Uri.EscapeDataString(EndDate)}");
        }

        if (!string.IsNullOrWhiteSpace(SubName))
        {
            parameters.Add($"subName={System.Uri.EscapeDataString(SubName)}");
        }

        return string.Join("&", parameters);
    }
}