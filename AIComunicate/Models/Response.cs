namespace AIComunicate.Models
{
    public class Response<T>
    {
        public string Code {  get; set; }
        public string Message { get; set; }
        public T Value { get; set; }
        public int Records { get; set; }
    }
}
