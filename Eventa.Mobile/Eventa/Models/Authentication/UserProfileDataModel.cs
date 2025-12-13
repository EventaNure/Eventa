using System.Collections.Generic;

namespace Eventa.Models.Authentication;

public class UserProfileDataModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public string? Organization { get; set; }
    public IEnumerable<CommentDataModel> MyComments { get; set; } = [];
    public IEnumerable<CommentDataModel> CommentsAboutMe { get; set; } = [];
}