using Discord.WebSocket;
using FloraCSharp.Extensions;
using FloraCSharp.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    enum Scheduler
    {
        EveryMinutes,
        EveryHour,
        EveryHalfDay,
        EveryDay,
        EveryWeek,
        EveryMonth,
        EveryYear,
    }

    public class BirthdayService
    {
        CancellationTokenSource m_ctSource;
        DiscordSocketClient _client;
        Configuration _config;
        FloraDebugLogger _logger = new FloraDebugLogger();
        FloraRandom _random;

        private string[] birthdayString = new string[7]
        {
            "{userName} is celebrating their bithday today! 🎂",
            "Happy Birthday {userName}, you're {age} now! 🎂",
            "How does it feel to be {age} today, {userName}? 🎂",
            "Happy Birthday {userName}! 🎂",
            "User {userName} is now level {age} in the real world. 🎂",
            "Ew, you're old now {userName}, Happy Birthday! 🎂",
            "You've just turned {age}, {userName}, do you feel old? 🎂"
        };

        public void StartBirthdays(DateTime time, DiscordSocketClient client, Configuration config, FloraRandom random)
        {
            _client = client;
            _config = config;
            _random = random;

            var nextDay = getNextDate(time, Scheduler.EveryDay);
        
            _logger.Log($"Next Day {time}", "Birthday Service");

            birthdayHandler(time, Scheduler.EveryDay);
        }

        private void birthdayHandler(DateTime date, Scheduler scheduler)
        {
            m_ctSource = new CancellationTokenSource();

            var dateNow = DateTime.Now;
            TimeSpan ts;
            if (date > dateNow)
            {
                ts = date - dateNow;
            }
            else
            {
                date = getNextDate(date, scheduler);
                ts = date - dateNow;
            }

            _logger.Log($"Time to wait: {ts}", "Birthday Service");

            Task.Delay(ts).ContinueWith(async (x) =>
            {
                List<Birthday> todaysBirthdays = GetBirthdays();

                if (todaysBirthdays != null)
                {
                    _logger.Log($"There are birthdays today!", "Birthday Service");

                    foreach (Birthday birthday in todaysBirthdays)
                    {
                        ISocketMessageChannel channel = (ISocketMessageChannel)_client.GetChannel(_config.BirthdayChannel);
                        SocketUser user = _client.GetUser(birthday.UserID);

                        string useString = birthdayString[_random.Next(birthdayString.Length)];
                        useString = useString.Replace("{userName}", user.Username).Replace("{age}", birthday.Age.ToString());

                        await channel.SendSuccessAsync(useString);

                        using (var uow = DBHandler.UnitOfWork())
                        {
                            birthday.Age += 1;
                            uow.Birthdays.Update(birthday);
                            await uow.CompleteAsync();
                        }
                    }
                }

                birthdayHandler(getNextDate(date, scheduler), scheduler);
            }, m_ctSource.Token);
        }

        private DateTime getNextDate(DateTime date, Scheduler scheduler)
        {
            switch (scheduler)
            {
                case Scheduler.EveryMinutes:
                    return date.AddMinutes(1);
                case Scheduler.EveryHour:
                    return date.AddHours(1);
                case Scheduler.EveryHalfDay:
                    return date.AddHours(12);
                case Scheduler.EveryDay:
                    return date.AddDays(1);
                case Scheduler.EveryWeek:
                    return date.AddDays(7);
                case Scheduler.EveryMonth:
                    return date.AddMonths(1);
                case Scheduler.EveryYear:
                    return date.AddYears(1);
                default:
                    throw new Exception("Invalid scheduler");
            }
        }

        private List<Birthday> GetBirthdays()
        {
            var curDate = DateTime.Now;
            List<Birthday> userBirthdays;

            using (var uow = DBHandler.UnitOfWork())
            {
                userBirthdays = uow.Birthdays.GetAllBirthdays(curDate);
            }

            return userBirthdays;
        }
    }
}
