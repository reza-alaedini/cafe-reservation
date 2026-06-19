using System.Globalization;
using System.Text;

namespace Cafe.Helpers
{
    public static class JalaliDate
    {
        private static readonly PersianCalendar Calendar = new();
        private static readonly string[] MonthNames =
        {
            "فروردین",
            "اردیبهشت",
            "خرداد",
            "تیر",
            "مرداد",
            "شهریور",
            "مهر",
            "آبان",
            "آذر",
            "دی",
            "بهمن",
            "اسفند"
        };

        private static readonly string[] DayNames =
        {
            "یکشنبه",
            "دوشنبه",
            "سه‌شنبه",
            "چهارشنبه",
            "پنجشنبه",
            "جمعه",
            "شنبه"
        };

        public static string ToShortDateTime(DateTime dateTime)
        {
            return ToPersianDigits($"{ToDate(dateTime)} - {dateTime:HH:mm}");
        }

        public static string ToFullDateTime(DateTime dateTime)
        {
            var dayName = DayNames[(int)dateTime.DayOfWeek];
            var day = Calendar.GetDayOfMonth(dateTime);
            var month = MonthNames[Calendar.GetMonth(dateTime) - 1];
            var year = Calendar.GetYear(dateTime);

            return ToPersianDigits($"{dayName}، {day} {month} {year} - {dateTime:HH:mm}");
        }

        public static string ToInputValue(DateTime dateTime)
        {
            return ToPersianDigits($"{Calendar.GetYear(dateTime):0000}/{Calendar.GetMonth(dateTime):00}/{Calendar.GetDayOfMonth(dateTime):00} {dateTime:HH:mm}");
        }

        public static bool TryParseInput(string? value, out DateTime dateTime)
        {
            dateTime = default;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = NormalizeDigits(value.Trim())
                .Replace("-", "/")
                .Replace(".", "/");

            var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var dateParts = parts[0].Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (dateParts.Length != 3 ||
                !int.TryParse(dateParts[0], out var year) ||
                !int.TryParse(dateParts[1], out var month) ||
                !int.TryParse(dateParts[2], out var day))
            {
                return false;
            }

            var hour = 0;
            var minute = 0;

            if (parts.Length > 1)
            {
                var timeParts = parts[1].Split(':', StringSplitOptions.RemoveEmptyEntries);

                if (timeParts.Length < 2 ||
                    !int.TryParse(timeParts[0], out hour) ||
                    !int.TryParse(timeParts[1], out minute))
                {
                    return false;
                }
            }

            try
            {
                dateTime = Calendar.ToDateTime(year, month, day, hour, minute, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ToDate(DateTime dateTime)
        {
            return $"{Calendar.GetYear(dateTime):0000}/{Calendar.GetMonth(dateTime):00}/{Calendar.GetDayOfMonth(dateTime):00}";
        }

        private static string NormalizeDigits(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(character switch
                {
                    >= '۰' and <= '۹' => (char)('0' + character - '۰'),
                    >= '٠' and <= '٩' => (char)('0' + character - '٠'),
                    _ => character
                });
            }

            return builder.ToString();
        }

        private static string ToPersianDigits(string value)
        {
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(character is >= '0' and <= '9'
                    ? (char)('۰' + character - '0')
                    : character);
            }

            return builder.ToString();
        }
    }
}
