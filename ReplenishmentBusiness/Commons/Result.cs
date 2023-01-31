namespace Business.Commons
{
    public class Result
    {
        public bool ResultIsUse { get; set; }

        public string ResultMsg { get; set; }

        public string Index { get; set; }
        public string No { get; set; }
    }

    public class ResponseViewModel
    {
        public string status { get; set; }
        public ResponseMessage message { get; set; }
    }

    public class ResponseMessage
    {
        public string description { get; set; }
    }
}
