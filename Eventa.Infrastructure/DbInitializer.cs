using Eventa.Application.Common;
using Eventa.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure
{
    public static class DbInitializer
    {
        public async static Task Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();
            ApplicationUser[] applicationUsers = new ApplicationUser[]
            {
                new ApplicationUser
                {
                    UserName = "pivovarov@eventa.ua",
                    NormalizedUserName = "PIVOVAROV@EVENTA.UA",
                    Email = "pivovarov@eventa.ua",
                    NormalizedEmail = "PIVOVAROV@EVENTA.UA",
                    Name = "Артем Пивоваров",
                    Organization = "Pivovarov Music",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "okeanelzy@eventa.ua",
                    NormalizedUserName = "OKEANELZY@EVENTA.UA",
                    Email = "okeanelzy@eventa.ua",
                    NormalizedEmail = "OKEANELZY@EVENTA.UA",
                    Name = "Океан Ельзи",
                    Organization = "OE Production",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "monatik@eventa.ua",
                    NormalizedUserName = "MONATIK@EVENTA.UA",
                    Email = "monatik@eventa.ua",
                    NormalizedEmail = "MONATIK@EVENTA.UA",
                    Name = "MONATIK",
                    Organization = "Monatik Corporation",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "opera@eventa.ua",
                    NormalizedUserName = "OPERA@EVENTA.UA",
                    Email = "opera@eventa.ua",
                    NormalizedEmail = "OPERA@EVENTA.UA",
                    Name = "Національна опера України",
                    Organization = "Національна опера України",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "dizelshow@eventa.ua",
                    NormalizedUserName = "DIZELSHOW@EVENTA.UA",
                    Email = "dizelshow@eventa.ua",
                    NormalizedEmail = "DIZELSHOW@EVENTA.UA",
                    Name = "Дизель Шоу",
                    Organization = "Дизель Студіо",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "bezobmezhen@eventa.ua",
                    NormalizedUserName = "BEZOBMEZHEN@EVENTA.UA",
                    Email = "bezobmezhen@eventa.ua",
                    NormalizedEmail = "BEZOBMEZHEN@EVENTA.UA",
                    Name = "Без Обмежень",
                    Organization = "BO Band",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "franka@eventa.ua",
                    NormalizedUserName = "FRANKA@EVENTA.UA",
                    Email = "franka@eventa.ua",
                    NormalizedEmail = "FRANKA@EVENTA.UA",
                    Name = "Театр ім. І. Франка",
                    Organization = "Національний академічний театр ім. І. Франка",
                    PasswordHash = "123",
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "odesaphilharmonic@eventa.ua",
                    NormalizedUserName = "ODESAPHILHARMONIC@EVENTA.UA",
                    Email = "odesaphilharmonic@eventa.ua",
                    NormalizedEmail = "ODESAPHILHARMONIC@EVENTA.UA",
                    Name = "Одеська філармонія",
                    Organization = "Одеський симфонічний оркестр",
                    PasswordHash = "123",
                    EmailConfirmed = true
                }
            };

            if (applicationUsers.Count() > await context.Users.CountAsync())
            {
                await context.Users.ExecuteDeleteAsync();
                context.Users.AddRange(applicationUsers);
                await context.SaveChangesAsync();
            }

            IdentityUserRole<string>[] userRoles = applicationUsers
                .Select(u => new IdentityUserRole<string>
                {
                    UserId = u.Id,
                    RoleId = "2"
                })
                .ToArray();

            if (userRoles.Count() > await context.UserRoles.CountAsync())
            {
                await context.UserRoles.ExecuteDeleteAsync();
                context.UserRoles.AddRange(userRoles);
                await context.SaveChangesAsync();
            }

            Place[] places = new Place[] {
                new Place
                {
                    Name = "МЦКМ",
                    Address = "м. Київ, Алея Героїв Небесної Сотні, 1"
                },
                new Place
                {
                    Name = "Палац Спорту",
                    Address = "м. Київ, Спортивна площа, 1"
                },
                new Place
                {
                     Name = "Одеський академічний театр опери та балету",
                     Address = "м. Одеса, пров. Чайковського, 1"
                },
                new Place
                {
                     Name = "Палац культури ім. Гната Хоткевича",
                     Address = "м. Львів, вул. Кушевича, 1"
                }
            };

            if (places.Count() > await context.Places.CountAsync())
            {
                await context.Places.ExecuteDeleteAsync();
                context.Places.AddRange(places);
                await context.SaveChangesAsync();
            }

            Event[] events = 
            {
                new Event
                {
                    Title = "Артем Пивоваров",
                    Description = "12 та 14 листопада 2025 року у Палаці Спорту відбудуться концерти Артема Пивоварова у Києві.",
                    Price = 300,
                    Duration = new TimeSpan(2, 0, 0),
                    PlaceId = places[0].Id,
                    OrganizerId = applicationUsers[0].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Океан Ельзи",
                    Description = "Головний рок-гурт України виступить у Львові на стадіоні «Арена Львів» у червні 2025 року.",
                    Price = 550,
                    Duration = new TimeSpan(2, 30, 0),
                    PlaceId = places[3].Id,
                    OrganizerId = applicationUsers[1].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "MONATIK",
                    Description = "Шоу MONATIK «Made With Love and Rhythm» у Палаці Спорту в Києві.",
                    Price = 650,
                    Duration = new TimeSpan(2, 0, 0),
                    PlaceId = places[1].Id,
                    OrganizerId = applicationUsers[2].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Національна опера України — «Лебедине озеро»",
                    Description = "Класичний балет Петра Чайковського у виконанні трупи Національної опери України.",
                    Price = 600,
                    Duration = new TimeSpan(2, 15, 0),
                    PlaceId = places[0].Id,
                    OrganizerId = applicationUsers[3].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Вечір Стендапу від «Дизель Шоу»",
                    Description = "Популярні коміки з «Дизель Шоу» виступлять з новою програмою у Львові.",
                    Price = 550,
                    Duration = new TimeSpan(1, 30, 0),
                    PlaceId = places[3].Id,
                    OrganizerId = applicationUsers[4].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Тур «Без обмежень» — Разом до Перемоги",
                    Description = "Всеукраїнський тур гурту «Без обмежень» із новою програмою у підтримку ЗСУ.",
                    Price = 450,
                    Duration = new TimeSpan(2, 0, 0),
                    PlaceId = places[1].Id,
                    OrganizerId = applicationUsers[5].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "«Майстер і Маргарита» — Національний академічний театр ім. І. Франка",
                    Description = "Постановка легендарного роману Булгакова на сцені театру у Києві.",
                    Price = 500,
                    Duration = new TimeSpan(2, 30, 0),
                    PlaceId = places[0].Id,
                    OrganizerId = applicationUsers[6].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Одеський симфонічний оркестр — «Музика кіно»",
                    Description = "Концерт симфонічного оркестру з найвідомішими саундтреками у виконанні музикантів Одеської філармонії.",
                    Price = 750,
                    Duration = new TimeSpan(2, 0, 0),
                    PlaceId = places[2].Id,
                    OrganizerId = applicationUsers[7].Id,
                    IsApproved = true
                }
            };

            if (events.Count() > await context.Events.CountAsync())
            {
                await context.Events.ExecuteDeleteAsync();
                context.Events.AddRange(events);
                await context.SaveChangesAsync();
            }

            EventTag[] eventTags = {
                new EventTag { EventId = events[0].Id, TagId = DefaultTags.Tags[0].Id },
                new EventTag { EventId = events[0].Id, TagId = DefaultTags.Tags[6].Id },
                new EventTag { EventId = events[1].Id, TagId = DefaultTags.Tags[0].Id },
                new EventTag { EventId = events[1].Id, TagId = DefaultTags.Tags[5].Id },
                new EventTag { EventId = events[2].Id, TagId = DefaultTags.Tags[0].Id },
                new EventTag { EventId = events[2].Id, TagId = DefaultTags.Tags[6].Id },
                new EventTag { EventId = events[3].Id, TagId = DefaultTags.Tags[1].Id },
                new EventTag { EventId = events[3].Id, TagId = DefaultTags.Tags[8].Id },
                new EventTag { EventId = events[4].Id, TagId = DefaultTags.Tags[2].Id },
                new EventTag { EventId = events[5].Id, TagId = DefaultTags.Tags[0].Id },
                new EventTag { EventId = events[5].Id, TagId = DefaultTags.Tags[5].Id },
                new EventTag { EventId = events[6].Id, TagId = DefaultTags.Tags[1].Id },
                new EventTag { EventId = events[7].Id, TagId = DefaultTags.Tags[4].Id },
                new EventTag { EventId = events[7].Id, TagId = DefaultTags.Tags[7].Id }
            };

            if (eventTags.Count() > await context.EventTags.CountAsync())
            {
                await context.EventTags.ExecuteDeleteAsync();
                context.EventTags.AddRange(eventTags);
                await context.SaveChangesAsync();
            }

            EventDateTime[] eventDateTimes =
            {
                new EventDateTime
                {
                    EventId = events[0].Id,
                    StartDateTime = new DateTime(2024, 12, 10, 19, 0, 0)
                },

                new EventDateTime
                {
                    EventId = events[1].Id,
                    StartDateTime = new DateTime(2025, 11, 5, 20, 0, 0)
                },
                new EventDateTime
                {
                    EventId = events[1].Id,
                    StartDateTime = new DateTime(2025, 11, 6, 20, 0, 0)
                },

                new EventDateTime
                {
                    EventId = events[2].Id,
                    StartDateTime = new DateTime(2025, 11, 15, 19, 30, 0)
                },

                new EventDateTime
                {
                    EventId = events[3].Id,
                    StartDateTime = new DateTime(2025, 11, 22, 18, 0, 0)
                },
                new EventDateTime
                {
                    EventId = events[3].Id,
                    StartDateTime = new DateTime(2025, 11, 23, 18, 0, 0)
                },

                new EventDateTime
                {
                    EventId = events[4].Id,
                    StartDateTime = new DateTime(2025, 12, 1, 19, 0, 0)
                },

                new EventDateTime
                {
                    EventId = events[5].Id,
                    StartDateTime = new DateTime(2025, 12, 10, 20, 0, 0)
                },
                new EventDateTime
                {
                    EventId = events[5].Id,
                    StartDateTime = new DateTime(2025, 12, 11, 20, 0, 0)
                },

                new EventDateTime
                {
                    EventId = events[6].Id,
                    StartDateTime = new DateTime(2025, 12, 5, 18, 30, 0)
                },

                new EventDateTime
                {
                    EventId = events[7].Id,
                    StartDateTime = new DateTime(2025, 12, 18, 19, 0, 0)
                },
                new EventDateTime
                {
                    EventId = events[7].Id,
                    StartDateTime = new DateTime(2025, 12, 19, 19, 0, 0)
                }
            };

            if (eventDateTimes.Count() > await context.EventDateTimes.CountAsync())
            {
                await context.EventDateTimes.ExecuteDeleteAsync();
                context.EventDateTimes.AddRange(eventDateTimes);
                await context.SaveChangesAsync();
            }
        }
    }
}
