using Cafe.Enums;
using Cafe.Models;

namespace Cafe.Data;

public static class DbInitializer
{
    public static void Seed(DataContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Name = "رضا علاالدینی - سوپر ادمین",
                    UserName = "reza_alaedini",
                    Email = "reza.alaedini.ra@gmail.com",
                    Password = "123456",
                    Role = Role.ADMIN,
                    Phone = "09104588844",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    UserName = "parsa alaei",
                    Email = "parsa.alaei.pa@gmail.com",
                    Password = "123456",
                    Role = Role.USER,
                    CreatedAt = DateTime.Now
                }
            );
        }

        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product("اسپرسو", "120000", "قهوه اسپرسو تک شات", "0bd6075a-5aaa-4571-9245-7fb0df4f897d_Cappuccino_at_Sightglass_Coffee.jpg")
                {
                    IsAvalible = true
                },
                new Product("کاپوچینو", "150000", "کاپوچینو با فوم شیر", "414ee6d1-dd9b-49f1-aaa4-57e2539e9800_download.jpg")
                {
                    IsAvalible = true
                },
                new Product("لاته", "160000", "قهوه لاته با شیر", "88793a04-d378-44c9-bf83-64355f83e20c_images.jpg")
                {
                    IsAvalible = true
                },
                new Product("موکا", "180000", "قهوه موکا شکلاتی", "5808381d-6025-4bd5-aeac-518f7a687503_mocka-coffee.webp")
                {
                    IsAvalible = true
                },
                new Product("چیزکیک", "220000", "چیزکیک مخصوص کافه", "pistachio-cheesecake-recipe-5-1-1024x683.webp")
                {
                    IsAvalible = true
                },
                new Product("آیس لته فندوق", "240000", "آیس لته با سیروپ فندوق همراه با یخ فراوان", "82382791-a8c2-41f3-8637-5c7ff1caac4e_iced-latte.jpg")
                {
                    IsAvalible = true
                },
                new Product("شیر چای عسل", "250000", "شیر + چای + عسل همراه با یه شکلات دلپذیر", "07a08076-9016-4979-bde1-683fe89c7d51_tea-honey.jpg")
                {
                    IsAvalible = false
                }
            );
        }

        if (!context.ReserveTables.Any())
        {
            context.ReserveTables.AddRange(
                new ReserveTable { Price = "300000", IsAvalible = true },
                new ReserveTable { Price = "400000", IsAvalible = true },
                new ReserveTable { Price = "500000", IsAvalible = true },
                new ReserveTable { Price = "600000", IsAvalible = true },
                new ReserveTable { Price = "120000", IsAvalible = false }
            );
        }

        context.SaveChanges();
    }
}