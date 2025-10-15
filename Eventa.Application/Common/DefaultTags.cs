using Eventa.Domain;

namespace Eventa.Application.Common
{
    public class DefaultTags
    {
        public readonly static Tag[] Tags =
        {
            new Tag
            {
                Id = 1,
                Name = "Concerts",
                IsMain = true
            },
            new Tag
            {
                Id = 2,
                Name = "Theaters",
                IsMain = true
            },
            new Tag
            {
                Id = 3,
                Name = "Stand-Up",
                IsMain = true
            },
            new Tag
            {
                Id = 4,
                Name = "Tours",
                IsMain = true
            },
            new Tag
            {
                Id = 5,
                Name = "Philharmonic",
                IsMain = true
            },
            new Tag
            {
                Id = 6,
                Name = "Rock",
                IsMain = false
            },
            new Tag
            {
                Id = 7,
                Name = "Pop",
                IsMain = false
            },
            new Tag
            {
                Id = 8,
                Name = "Opera",
                IsMain = false
            },
            new Tag
            {
                Id = 9,
                Name = "Ballet",
                IsMain = false
            },
        };
    }
}
