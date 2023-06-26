class User
{
    public string? _id { get; set; }
    public string? display_name { get; set; }
    public string? username { get; set; }

    public Dictionary<string, double> positionIn8Dimension = new Dictionary<string, double>();
}