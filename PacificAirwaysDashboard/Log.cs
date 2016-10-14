using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Serialization;

namespace PacificAirwaysDashboard
{
    public class Log
    {
        /// <summary>
        /// (string) pid - Pilot's PID
        /// </summary>
        public string PID { get; set; }

        /// <summary>
        /// (string) f_pid - Filed Pilot ID. The PID that was used when the PIREP was originally logged
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// (date & time) date - Date and time filed
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// (date & time) event_date - Date and time of departure
        /// </summary>
        public DateTime Event_Date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UTC { get; set; }

        /// <summary>
        /// (string) dep - Departure airport ICAO
        /// </summary>
        public string Dep { get; set; }

        /// <summary>
        /// (string) arr - Arrival airport ICAO
        /// </summary>
        public string Arr { get; set; }

        /// <summary>
        /// (string) air - Airline code, AFA or PAY
        /// </summary>
        public string Air { get; set; }

        /// <summary>
        /// (string) code - Flight code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// (string) payload - Flight payload - A for passengers, C for Cargo
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// (string) ac_air - Which airline aircraft belongs to, PAY or AFA
        /// </summary>
        public string Ac_air { get; set; }

        /// <summary>
        /// (string) ac_code - Aircraft code
        /// </summary>
        public string Ac_code { get; set; }

        /// <summary>
        /// (float) time - Flight time
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// (int) atc - 1 = With online ATC, 2 = FS ATC, 3 = no ATC
        /// </summary>
        public int Atc { get; set; }

        /// <summary>
        /// (string) comment - Comments
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// (string) FlightSim (fsx, fs2004)
        /// </summary>
        /// <returns></returns>
        public string FlightSim { get; set; }

        /// <summary>
        /// (string) type - type: Returns type of log. |||  F = Flight Report, C = Logbook Credit, B = Bonus, S = Status change, T = Hub Transfer
        /// </summary>
        public char Type { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        public bool Error { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
