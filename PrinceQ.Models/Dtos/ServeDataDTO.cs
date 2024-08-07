
namespace PrinceQ.Models.DTOs
{
    public class ServeDataDTO
    {
        public string GenerateDate { get; set; }
        public string ClerkId { get; set; }
        public int CategoryId { get; set; }
        public int QueueNumber { get; set; }
        public int Total_Cheque { get; set; }
        public TimeSpan ServeStart { get; set; }
        public TimeSpan ServeEnd { get; set; }
    }
}
