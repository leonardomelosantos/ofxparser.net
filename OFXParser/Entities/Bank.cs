namespace OFXParser.Entities
{
    public class Bank
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public Bank(int code, string name)
        {
            this.Code = code;
            this.Name = name;
        }
    }
}
