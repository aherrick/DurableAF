namespace DurableAF
{
    public class DurableResponse
    {
        public string ErrorMsg { get; set; }

        public bool IsError
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ErrorMsg);
            }
        }

        public string XMLResult { get; set; }
    }
}