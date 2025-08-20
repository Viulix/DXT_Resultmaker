public class Franchiselist
{
    public class Body
    {
        public bool success { get; set; }
        public string message { get; set; }
        public FranchiseData[] data { get; set; }
    }

    public class FranchiseData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string prefix { get; set; }
        public string logo { get; set; }
        public string banner { get; set; }
        public string emoji { get; set; }
        public object decal { get; set; }
        public string discord { get; set; }
        public string twitch { get; set; }
        public string twitter { get; set; }
        public string instagram { get; set; }
        public string tiktok { get; set; }
        public string youtube { get; set; }
        public object active { get; set; }
        public string Hex1 { get; set; }
        public string Hex2 { get; set; }
        public string Hex3 { get; set; }
    }

}
