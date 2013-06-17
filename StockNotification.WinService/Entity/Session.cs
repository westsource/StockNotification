namespace StockNotification.WinService.Entity
{
    public class Session
    {
        public string Id { get; set; }
        public string StockId { get; set; }
        public string Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public ulong Volume { get; set; }
        public double AdjClose { get; set; }
    }
}
