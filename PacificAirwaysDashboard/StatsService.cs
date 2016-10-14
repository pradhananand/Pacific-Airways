using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacificAirwaysDashboard
{
    public class StatsService
    {

        // Filter on Hub, Start and End dates
        public List<Log> GetFilteredLogs(IEnumerable<Log> hubLogBooks, string hub, DateTime startDate, DateTime endDate)
        {
            string result = hub.Equals("Seattle", StringComparison.Ordinal) ? "PAY1"
              : hub.Equals("Miami", StringComparison.Ordinal) ? "PAY2"
              : hub.Equals("Denver", StringComparison.Ordinal) ? "PAY3"
              : hub.Equals("Chicago", StringComparison.Ordinal) ? "PAY5"
              : hub.Equals("NewYork", StringComparison.Ordinal) ? "PAY6"
              : hub.Equals("LosAngeles", StringComparison.Ordinal) ? "PAY7"
              : hub.Equals("PacificEurope", StringComparison.Ordinal) ? "PAY8"
              : "PAY";

            var currentMonthLogs =
                from logs in hubLogBooks
                where logs.Event_Date >= startDate && logs.Event_Date.Date <= endDate && logs.PID.StartsWith(result)
                select logs;
            return currentMonthLogs.ToList();
        }


        // Sum of hours
        public decimal GetTotalHours(List<Log> hubLogBooks)
        {
            var categories11 =
               from prod in hubLogBooks.AsParallel()
               select new { Duration = prod.Time };
            decimal totalFlightHours = categories11.Sum(item => item.Duration);
            return totalFlightHours;
        }

        // Sum of VATSIM hours
        public decimal GetTotalVatsimHours(List<Log> hubLogBooks)
        {
            var categories14 =
                from prod in hubLogBooks
                where prod.Atc == 1
                select new { Duration = prod.Time };
            decimal totalVatsimFlightHours = categories14.Sum(item => item.Duration);
            return totalVatsimFlightHours;
        }


        // Count of flights
        public int GetTotalFlights(List<Log> hubLogBooks)
        {
            var result = from element in hubLogBooks
                         select element;
            return (result.Count());
        }


        // Count of VATSIM flights
        public int GetTotalVastimFlights(List<Log> hubLogBooks)
        {
            var result1 = from element in hubLogBooks
                          where element.Atc == 1
                          select element;
            var totalVATSIMFlightsThisMonth = result1.Count();
            return totalVATSIMFlightsThisMonth;
        }


        // Count of Charters
        public int GetCountOfTotalChartersFlown(List<Log> hubLogBooks)
        {
            var result2 = from element in hubLogBooks
                          where element.Code == "C"
                          select element;
            var totalCharterFlightsThisMonth = result2.Count();
            return totalCharterFlightsThisMonth;
        }

        // Count of Special Events flown
        public int GetCountOfTotalSpecialEventsParticipation(List<Log> hubLogBooks)
        {
            var result3 = from element in hubLogBooks
                          where element.Code == "S"
                          select element;
            var totalSpecialFlightsThisMonth = result3.Count();
            return totalSpecialFlightsThisMonth;
        }


        // Get Top Pilots by Flights
        public List<dynamic> GetTopPilotByFlights(List<Log> hubLogBooks)
        {
            var categoryCounts = (
                from prod in hubLogBooks
                let pkey = new { c1 = prod.PID, c2 = prod.Name }
                group prod by pkey into prodGroup
                orderby prodGroup.Count() descending
                select new { Pid = prodGroup.Key.c1, Name = prodGroup.Key.c2, ProductCount = prodGroup.Count() }).Take(5);
            return categoryCounts.ToList<dynamic>();
        }


        // Total Hours per pilot
        public List<dynamic> GetTop5PilotsByHours(List<Log> hubLogBooks)
        {
            var categories10 = (
                from prod in hubLogBooks
                let pkey = new { c1 = prod.PID, c2 = prod.Name }
                orderby prod.Time
                group prod by pkey into prodGroup
                orderby prodGroup.Sum(p => p.Time) descending
                select new { Pid = prodGroup.Key.c1, Name = prodGroup.Key.c2, TotalFlyingTime = prodGroup.Sum(p => p.Time) }).Take(5);
            return categories10.ToList<dynamic>();
        }

        // Total VATSIM Hours per pilot
        public List<dynamic> GetTop5PilotsByVatsimHours(List<Log> hubLogBooks)
        {
            var top5PilotsByVatsimHours = (
                from prod in hubLogBooks
                let pkey = new { c1 = prod.PID, c2 = prod.Name }
                orderby prod.Time
                where prod.Atc == 1
                group prod by pkey into prodGroup
                orderby prodGroup.Sum(p => p.Time) descending
                select new { Pid = prodGroup.Key.c1, Name = prodGroup.Key.c2, TotalFlyingTime = prodGroup.Sum(p => p.Time) }).Take(5);
            return top5PilotsByVatsimHours.ToList<dynamic>();
        }

        // Total charters per Pilot
        public List<dynamic> GetTop5PilotsByNumberOfCharterFlown(List<Log> hubLogBooks)
        {
            var charterFlownCounts = (
                from prod in hubLogBooks
                let pkey = new { c1 = prod.PID, c2 = prod.Name }
                where prod.Code == "C"
                group prod by pkey into prodGroup
                orderby prodGroup.Count() descending
                select new { Pid = prodGroup.Key.c1, Name = prodGroup.Key.c2, ProductCount = prodGroup.Count() }).Take(5);
            return charterFlownCounts.ToList<dynamic>();
        }

        // Total Special flights per Pilot
        public List<dynamic> GetTop5PilotsByNumberOfSpecialEventsFlown(List<Log> hubLogBooks)
        {
            var specialEventsFlownCounts = (
                from prod in hubLogBooks
                let pkey = new { c1 = prod.PID, c2 = prod.Name }
                where prod.Code == "S"
                group prod by pkey into prodGroup
                orderby prodGroup.Count() descending
                select new { Pid = prodGroup.Key.c1, Name = prodGroup.Key.c2, ProductCount = prodGroup.Count() }).Take(5);
            return specialEventsFlownCounts.ToList<dynamic>();
        }



        // Most flown routes
        public List<dynamic> GetTop10MostFlownRoutes(List<Log> hubLogBooks)
        {

            var Top10MostFlownRoutes = (
                from prod in hubLogBooks
                group prod.Dep by new { prod.Dep, prod.Arr } into newProd
                select new
                {
                    Dep = newProd.Key.Dep,
                    Arr = newProd.Key.Arr,
                    NumberOfTimesFlown = newProd.Count()
                }).OrderByDescending(newProd => newProd.NumberOfTimesFlown).Take(10);
            return Top10MostFlownRoutes.ToList<dynamic>();
        }

        // Hub top rank pilot
        public List<dynamic> GetHubTop10PilotsByFlights(List<Log> hubLogBooks)
        {
            var Top10PilotsByFlights = (
                hubLogBooks.GroupBy(PilotID => PilotID.PID)
                .OrderByDescending(PilotID => PilotID.Count())
                .Select((PilotID, index) => new
                {
                    pid = PilotID.Key,
                    rank = index + 1,
                    count = PilotID.Count()
                })).Take(10);
            return Top10PilotsByFlights.ToList<dynamic>();
        }
    }
}
