# QRAPI - QR Code Generation API

## Overview
QRAPI is a RESTful API that allows users to generate QR codes in various formats, including **SVG, PNG, and JPEG**. The API supports customization features such as colors, error correction levels, and embedded icons (logos), with access controlled by specific permissions.

## Features
- Generate QR codes in **SVG, PNG, and JPEG** formats.
- Customizable colors for dark and light modules.
- Support for embedded icons inside the QR code.
- Error correction level adjustment.
- Role-based access control using JWT claims.

## Authentication & Permissions
QRAPI uses **JWT-based authentication**. Certain features require specific permissions granted via JWT claims:

- **`qr-use-colors`**: Allows users to customize QR code colors.
- **`qr-use-icon`**: Allows users to embed an icon/logo inside the QR code (only SVG).

### Example Payload
```json
{
  "url": "https://joseenrique.dev",
  "size": 10,
  "darkColorHex":"#8332ac",
  "lightColorHex": "#e7ecf2",
  "eccLevel": 1,
  "logourl":"",
  "logosvgbase64":"PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMDAiIGhlaWdodD0iMTAwIj48Y2lyY2xlIGN4PSI1MCIgY3k9IjUwIiByPSI0MCIgZmlsbD0icmVkIiAvPjwvc3ZnPg=="
}
```

## API Endpoints

### Generate QR Code (SVG)
**Endpoint:**
```
POST /generate/qr/svg
```
**Request Body:**
```json
{
  "url": "https://example.com",
  "size": 30,
  "darkColorHex": "#000000",
  "lightColorHex": "#FFFFFF",
  "ECCLevel": "M",
  "border": true,
  "logoUrl": "https://example.com/logo.png",
  "logoSvgBase64": "<base64-encoded-svg>"
}
```
**Response:**
Returns an SVG string containing the generated QR code.

### Generate QR Code (PNG)
**Endpoint:**
```
POST /generate/qr/png
```
**Request Body:** *(Same as SVG endpoint)*

**Response:**
Returns a PNG image of the generated QR code.

### Generate QR Code (JPEG)
**Endpoint:**
```
POST /generate/qr/jpeg
```
**Request Body:** *(Same as SVG endpoint)*

**Response:**
Returns a JPEG image of the generated QR code.

## Usage Examples

### 1️⃣ Generate a Basic QR Code (No Colors, No Icon)
```bash
curl -X POST "https://qr.joseenrique.dev/generate/qr/svg" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -d '{ "url": "https://example.com", "size": 25 }'
```

### 2️⃣ Generate a QR Code with Custom Colors (Requires `qr-use-colors` Permission)
```bash
curl -X POST "https://qr.joseenrique.dev/generate/qr/png" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -d '{ "url": "https://example.com", "size": 30, "darkColorHex": "#FF0000", "lightColorHex": "#FFFF00" }'
```

### 3️⃣ Generate a QR Code with an Icon (Requires `qr-use-icon` Permission)
```bash
curl -X POST "https://qr.joseenrique.dev/generate/qr/svg" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -d '{ "url": "https://example.com", "size": 30, "logoUrl": "https://example.com/logo.png" }'
```

## Error Responses
| Status Code | Description |
|------------|-------------|
| **400** | Invalid request (e.g., missing `url` field) |
| **401** | Unauthorized (Missing or invalid JWT token) |
| **403** | Forbidden (User lacks required permission) |
| **500** | Internal server error |

## Setup & Deployment
1. Clone the repository:
   ```sh
   git clone https://github.com/HenryBJ/qrapi.git
   cd qrapi
   ```
2. Configure the `appsettings.json` with your jwt key in `apikey`.
3. Run the API:
   ```sh
   dotnet run
   ```

## License
This project is licensed under the MIT License.

