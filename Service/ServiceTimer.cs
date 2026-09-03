using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto.SMS
{
    public class ServiceTimer
    {
        private static ServiceTimer instance;
        public static ServiceTimer Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServiceTimer();

                return instance;
            }
        }
        DateTime startedon;
        DateTime firstend;
        System.Timers.Timer timer = new System.Timers.Timer();
        private ServiceTimer()
        { 

            CultureInfo enUS = new CultureInfo("en-US");

            DateTime dt;
            DateTime.TryParseExact(System.Configuration.ConfigurationManager.AppSettings["ScheduledOn"], "hh:mm tt", enUS, DateTimeStyles.None , out dt);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            startedon = TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            if(DateTime.UtcNow> startedon.ToUniversalTime())
            {
                startedon = startedon.AddDays(1);
            }
           
        }

       public void Start()
        {

            timer.Interval = startedon.ToUniversalTime().Subtract(DateTime.UtcNow).TotalMilliseconds;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = false;
            timer = new System.Timers.Timer();
            timer.Interval = DateTime.Now.AddDays(1).Subtract(DateTime.Now).TotalMilliseconds;
            timer.Elapsed += Actual;
      
            timer.Start();
            TriggerService();
        }

        private void Actual(object sender, System.Timers.ElapsedEventArgs e)
        {
            TriggerService();
        }

        private void TriggerService()
        {
            startedon = startedon.AddDays(1);
            ServiceProxy.Instance.TriggerSchedule(startedon);
        }
    }
}
