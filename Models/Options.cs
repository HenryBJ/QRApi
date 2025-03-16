namespace QRApi.Models;

public class PNGOptions
{
    public string Url { get; set; }
    public int Size { get; set; } = 20;
    public string DarkColorHex { get; set; } = "#000000";
    public string LightColorHex { get; set; } = "#ffffff";
    public int ECCLevel { get; set; } = 2;
    public bool Border { get; set; } = false;
}

public class BitmapOptions
{
    public string Url { get; set; }
    public int Size { get; set; } = 20;
    public string DarkColorHex { get; set; } = "#000000";
    public string LightColorHex { get; set; } = "#ffffff";
    public int ECCLevel { get; set; } = 2;
}

public class SVGOptions
{
    public string Url { get; set; }
    public int Size { get; set; } = 20;
    public string DarkColorHex { get; set; } = "#000000";
    public string LightColorHex { get; set; } = "#ffffff";
    public int ECCLevel { get; set; } = 2;
    public string? LogoUrl { get; set; }
    public string? LogoSvgBase64 { get; set; }
    public bool Border { get; set; } = false;
}
