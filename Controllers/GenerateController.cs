using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRApi.Models;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace QRApi.Controllers;

[ApiController]
[Route("[controller]/qr")]
[Authorize]
public class GenerateController : ControllerBase
{
    [HttpPost("png")]
    public IActionResult QRCodePNG([FromBody] PNGOptions request)
    {
        if (string.IsNullOrEmpty(request.Url))
            return BadRequest(new { error = "URL cannot be empty." });

        // Check permissions
        var user = HttpContext.User;
        bool canUseColors = user.Identity!.IsAuthenticated &&
                          user.HasClaim(c => c.Type == "qr-use-colors" && c.Value == "true");

        if (!canUseColors)
        {
            if(!string.IsNullOrEmpty(request.LightColorHex) || 
                request.LightColorHex.ToLower().Trim() != "#000000")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });

            if (!string.IsNullOrEmpty(request.DarkColorHex) ||
                request.DarkColorHex.ToLower().Trim() != "#ffffff")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });
        }

        var bytes = PngByteQRCodeHelper.GetQRCode(
            request.Url, 
            request.Size, 
            HexToRgb(request.DarkColorHex), 
            HexToRgb(request.LightColorHex), 
            ToECCLevel(request.ECCLevel),
            drawQuietZones:request.Border);

        Response.Headers.Add("Content-Disposition", "inline; filename=qrcode.png");
        return File(bytes, "image/png");
    }


    [HttpPost("jpeg")]
    public IActionResult QRCodeJPEG([FromBody] BitmapOptions request)
    {
        if (string.IsNullOrEmpty(request.Url))
            return BadRequest(new { error = "URL cannot be empty." });

        // Check permissions
        var user = HttpContext.User;
        bool canUseColors = user.Identity!.IsAuthenticated &&
                          user.HasClaim(c => c.Type == "qr-use-colors" && c.Value == "true");

        if (!canUseColors)
        {
            if (!string.IsNullOrEmpty(request.LightColorHex) ||
                request.LightColorHex.ToLower().Trim() != "#000000")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });

            if (!string.IsNullOrEmpty(request.DarkColorHex) ||
                request.DarkColorHex.ToLower().Trim() != "#ffffff")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });
        }

        var bytes = BitmapByteQRCodeHelper
            .GetQRCode(
            request.Url,
            request.Size,
            request.DarkColorHex,
            request.LightColorHex,
            ToECCLevel(request.ECCLevel));

        Response.Headers.Add("Content-Disposition", "inline; filename=qrcode.jpeg");
        return File(bytes, "image/jpeg");
    }

    [HttpPost("svg")]
    public async Task<IActionResult> QRCodeSVG([FromBody] SVGOptions request)
    {
        if (string.IsNullOrEmpty(request.Url))
            return BadRequest(new { error = "URL cannot be empty." });

        // Check permissions
        var user = HttpContext.User;
        bool canUseIcon = user.Identity!.IsAuthenticated &&
                          user.HasClaim(c => c.Type == "qr-use-icon" && c.Value == "true");
        bool canUseColors = user.Identity!.IsAuthenticated &&
                          user.HasClaim(c => c.Type == "qr-use-colors" && c.Value == "true");

        if (!canUseColors)
        {
            if (!string.IsNullOrEmpty(request.LightColorHex) ||
                request.LightColorHex.ToLower().Trim() != "#000000")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });

            if (!string.IsNullOrEmpty(request.DarkColorHex) ||
                request.DarkColorHex.ToLower().Trim() != "#ffffff")
                return BadRequest(new { error = "You are not allowed to use colors. Obtain permission" });
        }

        if (!canUseIcon)
        {
            if (!string.IsNullOrEmpty(request.LogoSvgBase64))
            return BadRequest(new { error = "You are not allowed to use icon. Obtain permission" });
        }

        string svg = string.Empty;
        if (!String.IsNullOrEmpty(request.LogoUrl))
        {
            var logo = await DownloadPngAsImage(request.LogoUrl);
            svg = SvgQRCodeHelper.GetQRCode(request.Url,
            request.Size,
            request.DarkColorHex,
            request.LightColorHex,
            ToECCLevel(request.ECCLevel),
            drawQuietZones: request.Border,
            logo: new SvgQRCode.SvgLogo(iconRasterized: logo));

        }
        else if (!String.IsNullOrEmpty(request.LogoSvgBase64))
        {
            byte[] svgBytes = Convert.FromBase64String(request.LogoSvgBase64);
            string svgString = Encoding.UTF8.GetString(svgBytes);

            svg = SvgQRCodeHelper.GetQRCode(request.Url,
            request.Size,
            request.DarkColorHex,
            request.LightColorHex,
            ToECCLevel(request.ECCLevel),
            drawQuietZones: request.Border,
            logo: new SvgQRCode.SvgLogo(iconVectorized: svgString));
        }
        else
        {
            svg = SvgQRCodeHelper.GetQRCode(request.Url,
            request.Size,
            request.DarkColorHex,
            request.LightColorHex,
            ToECCLevel(request.ECCLevel),
            drawQuietZones: request.Border);
        }
        return Ok(svg);
    }

    private QRCodeGenerator.ECCLevel ToECCLevel(int level)
    {
        return level switch
        {
            0 => QRCodeGenerator.ECCLevel.L, // Baja (recupera hasta el 7% de los datos)
            1 => QRCodeGenerator.ECCLevel.M, // Media (recupera hasta el 15%)
            2 => QRCodeGenerator.ECCLevel.Q, // Cuasi-alta (recupera hasta el 25%)
            3 => QRCodeGenerator.ECCLevel.H, // Alta (recupera hasta el 30%)
            _ => QRCodeGenerator.ECCLevel.Q,
        };
    }

    private byte[] HexToRgb(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || !System.Text.RegularExpressions.Regex.IsMatch(hex, "^#?([A-Fa-f0-9]{6})$"))
            throw new ArgumentException("Invalid HEX color format. It should be like #RRGGBB.", nameof(hex));

        // Remueve el símbolo '#' si está presente
        hex = hex.TrimStart('#');

        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);

        return new byte[] { r, g, b };
    }

    private async Task<byte[]> DownloadPngAsImage(string imageUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var image = await Image.LoadAsync(stream);
                using (MemoryStream ms = new MemoryStream())
                {
                    await image.SaveAsPngAsync(ms); // O usa image.SaveAsJpegAsync(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
