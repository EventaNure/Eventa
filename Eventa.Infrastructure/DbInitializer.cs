using Eventa.Application.Common;
using Eventa.Application.Services;
using Eventa.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure
{
    public static class DbInitializer
    {
        public async static Task Initialize(ApplicationDbContext context, IFileService fileService)
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

            Place[] places =
            {
                new Place
                {
                    Name = "Національний палац мистецтв «Україна»",
                    Address = "вулиця Велика Васильківська, 103, Київ",
                    Latitude = 50.422280197756535,
                    Longitude = 30.520940255567332
                },
                new Place
                {
                    Name = "Національний академічний театр опери та балету України імені Тараса Шевченка",
                    Address = "вулиця Володимирська, 50, Київ",
                    Latitude = 50.44673045546744,
                    Longitude = 30.51236950063882
                },
                new Place
                {
                    Name = "Львівський національний академічний театр опери та балету імені Соломії Крушельницької",
                    Address = "проспект Свободи, 28, Львів",
                    Latitude = 49.844047776957396,
                    Longitude = 24.02622302985318
                },
                new Place
                {
                    Name = "Одеський національний академічний театр опери та балету",
                    Address = "провулок Чайковського, 1, Одеса",
                    Latitude = 46.485486087190566,
                    Longitude = 30.74111502698567
                }
            };

            if (await context.Places.CountAsync() == 0)
            {
                await context.Places.ExecuteDeleteAsync();
                context.Places.AddRange(places);
                await context.SaveChangesAsync();
            }

            if (await context.RowTypes.CountAsync() == 0)
            for (int i = 0; i < places.Length; i++)
            {
                if (i == 2)
                {
                    fileService.MoveFile($"places/{i + 1}.gif", $"places/{places[i].Id}.gif");
                } else if (i == 3)
                {
                    fileService.MoveFile($"places/{i + 1}.png", $"places/{places[i].Id}.png");
                } else
                {
                    fileService.MoveFile($"places/{i + 1}.jpg", $"places/{places[i].Id}.jpg");
                }

                RowType[] rowTypes = {
                        new RowType
                        {
                            Name = "Партер",
                            IsSeparatedSeats = false,
                            Place = places[i]
                        },
                        new RowType
                        {
                            Name = "Балкон",
                            IsSeparatedSeats = false,
                            Place = places[i]
                        }
                    };

                context.RowTypes.AddRange(rowTypes);
                await context.SaveChangesAsync();

                List<Row> rows = new List<Row>();

                for (int j = 1; j <= 51; j++)
                {
                    RowType rowType = j > 34 ? rowTypes[1] : rowTypes[0];
                    List<Seat> seats = new List<Seat>();
                    int rowNumber = j > 34 ? j - 34 : j;
                    double priceMultiplier = 0;
                    switch (j)
                    {
                        case < 10:
                            priceMultiplier = 3.1;
                            break;
                        case < 17:
                            priceMultiplier = 2.9;
                            break;
                        case < 21:
                            priceMultiplier = 2.7;
                            break;
                        case < 25:
                            priceMultiplier = 2.4;
                            break;
                        case < 30:
                            priceMultiplier = 2.2;
                            break;
                        case < 35:
                            priceMultiplier = 1.6;
                            break;
                        case > 34 and < 40:
                            priceMultiplier = 1.5;
                            break;
                        case > 39 and < 45:
                            priceMultiplier = 1.2;
                            break;
                        case > 44:
                            priceMultiplier = 1;
                            break;
                    }
                    int seatsCount = 0;
                    switch (j)
                    {
                        case 1:
                            seatsCount = 56;
                            break;
                        case 2:
                            seatsCount = 58;
                            break;
                        case 3:
                            seatsCount = 60;
                            break;
                        case 4:
                            seatsCount = 64;
                            break;
                        case 5:
                            seatsCount = 66;
                            break;
                        case 6:
                            seatsCount = 68;
                            break;
                        case > 6 and < 11:
                            seatsCount = 70;
                            break;
                        case > 10 and < 14:
                            seatsCount = 72;
                            break;
                        case > 13 and < 16:
                            seatsCount = 74;
                            break;
                        case > 16 and < 20:
                            seatsCount = 76;
                            break;
                        case 20:
                            seatsCount = 69;
                            break;
                        case 21:
                            seatsCount = 71;
                            break;
                        case > 21 and < 25:
                            seatsCount = 78;
                            break;
                        case > 25 and < 30:
                            seatsCount = 80;
                            break;
                        case > 29 and < 34:
                            seatsCount = 82;
                            break;
                        case 34:
                            seatsCount = 84;
                            break;
                        case 35:
                            seatsCount = 36;
                            break;
                        case 36:
                            seatsCount = 8;
                            break;
                        case 37:
                            seatsCount = 10;
                            break;
                        case 38:
                            seatsCount = 12;
                            break;
                        case 39:
                            seatsCount = 14;
                            break;
                        case 40:
                            seatsCount = 16;
                            break;
                        case 41:
                            seatsCount = 82;
                            break;
                        case 42:
                            seatsCount = 81;
                            break;
                        case 43:
                            seatsCount = 74;
                            break;
                        case 44:
                            seatsCount = 36;
                            break;
                        case 45:
                            seatsCount = 40;
                            break;
                        case 46:
                            seatsCount = 69;
                            break;
                        case 47:
                            seatsCount = 69;
                            break;
                        case 48:
                            seatsCount = 71;
                            break;
                        case 49:
                            seatsCount = 67;
                            break;
                        case 50:
                            seatsCount = 79;
                            break;
                        case 51:
                            seatsCount = 79;
                            break;
                        case 52:
                            seatsCount = 81;
                            break;
                        case 53:
                            seatsCount = 91;
                            break;
                        case 54:
                            seatsCount = 93;
                            break;
                        case 55:
                            seatsCount = 103;
                            break;
                    }
                    for (int k = 1; k <= seatsCount; k++)
                    {
                        double priceM = k < 10 & k > 17 && k < 41 ? 3.6 : priceMultiplier;
                        seats.Add(new Seat
                        {
                            PriceMultiplier = priceM,
                            SeatNumber = k
                        });
                    }
                    rows.Add(new Row
                    {
                        RowNumber = j,
                        RowType = rowType,
                        Seats = seats
                    });
                }

                context.Rows.AddRange(rows);
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
                    Place = places[0],
                    OrganizerId = applicationUsers[0].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Океан Ельзи",
                    Description = "Головний рок-гурт України виступить у Львові на стадіоні «Арена Львів» у червні 2025 року.",
                    Price = 550,
                    Duration = new TimeSpan(2, 30, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[1].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "MONATIK",
                    Description = "Шоу MONATIK «Made With Love and Rhythm» у Палаці Спорту в Києві.",
                    Price = 650,
                    Duration = new TimeSpan(2, 0, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[2].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Національна опера України — «Лебедине озеро»",
                    Description = "Класичний балет Петра Чайковського у виконанні трупи Національної опери України.",
                    Price = 600,
                    Duration = new TimeSpan(2, 15, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[3].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Вечір Стендапу від «Дизель Шоу»",
                    Description = "Популярні коміки з «Дизель Шоу» виступлять з новою програмою у Львові.",
                    Price = 550,
                    Duration = new TimeSpan(1, 30, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[4].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Тур «Без обмежень» — Разом до Перемоги",
                    Description = "Всеукраїнський тур гурту «Без обмежень» із новою програмою у підтримку ЗСУ.",
                    Price = 450,
                    Duration = new TimeSpan(2, 0, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[5].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "«Майстер і Маргарита» — Національний академічний театр ім. І. Франка",
                    Description = "Постановка легендарного роману Булгакова на сцені театру у Києві.",
                    Price = 500,
                    Duration = new TimeSpan(2, 30, 0),
                    Place = places[0],
                    OrganizerId = applicationUsers[6].Id,
                    IsApproved = true
                },
                new Event
                {
                    Title = "Одеський симфонічний оркестр — «Музика кіно»",
                    Description = "Концерт симфонічного оркестру з найвідомішими саундтреками у виконанні музикантів Одеської філармонії.",
                    Price = 750,
                    Duration = new TimeSpan(2, 0, 0),
                    Place = places[0],
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
