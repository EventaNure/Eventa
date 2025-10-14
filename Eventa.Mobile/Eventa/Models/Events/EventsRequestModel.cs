using System.Collections.Generic;
using System.Linq;

namespace Eventa.Models.Events;

public class EventsRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<int>? TagIds { get; set; }

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

        return string.Join("&", parameters);
    }
}