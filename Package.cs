namespace PackageTransferSystem
{
    public class Package
    {
        private static int _id = 0;

        public string? SourceLine { get; set; }
        public int Id { get; private set; }
        public DateTime CreationDate { get; private set; }

        public Package()
        {
            Id = _id++;
            CreationDate = DateTime.Now;
        }
    }
}
