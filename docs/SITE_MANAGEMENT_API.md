# Site Management API Documentation

## Overview

The Site Management API provides endpoints for creating, managing, and provisioning multi-tenant CMS sites in the MOSAIC platform. Each site represents an isolated workspace where users can create and manage content.

## Base URL

```
/api/site
```

## Authentication

All endpoints except listing sites require authentication. Use the user's email address for site filtering and ownership validation.

## Endpoints

### 1. Create Site

Creates and provisions a new CMS site for a user.

**Endpoint:** `POST /api/site`

**Request Body:**
```json
{
  "name": "My Awesome Site",
  "description": "A portfolio and blog site",
  "purpose": "I want to showcase my work and share my thoughts",
  "themeId": 1,
  "adminEmail": "user@example.com"
}
```

**Request Fields:**
- `name` (required): Site name (string)
- `description` (optional): Site description (string)
- `purpose` (optional): Purpose/goal of the site (string)
- `themeId` (optional): ID of the theme to apply (integer)
- `adminEmail` (required): Email of the site administrator (string)

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Site 'My Awesome Site' created successfully!",
  "site": {
    "id": 1,
    "name": "My Awesome Site",
    "description": "A portfolio and blog site",
    "url": "my-awesome-site",
    "themeId": 1,
    "themeName": "Modern Business",
    "logoUrl": null,
    "faviconUrl": null,
    "adminEmail": "user@example.com",
    "isActive": true,
    "defaultLanguage": "en-US",
    "createdOn": "2025-12-10T19:30:00Z",
    "modifiedOn": null,
    "status": "active"
  },
  "cmsDashboardUrl": "/admin?site=1",
  "errorDetails": null
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Failed to create site",
  "site": null,
  "cmsDashboardUrl": null,
  "errorDetails": "Site name is required"
}
```

### 2. Get User Sites

Retrieves all sites associated with a user.

**Endpoint:** `GET /api/site?userEmail={email}`

**Query Parameters:**
- `userEmail` (optional): Filter sites by user email. If omitted, returns all sites.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "My Portfolio",
    "description": "Personal portfolio site",
    "url": "my-portfolio",
    "themeId": 1,
    "themeName": null,
    "logoUrl": null,
    "faviconUrl": null,
    "adminEmail": "user@example.com",
    "isActive": true,
    "defaultLanguage": "en-US",
    "createdOn": "2025-12-10T10:00:00Z",
    "modifiedOn": null,
    "status": "active"
  },
  {
    "id": 2,
    "name": "Business Site",
    "description": "Corporate website",
    "url": "business-site",
    "themeId": 2,
    "themeName": null,
    "logoUrl": null,
    "faviconUrl": null,
    "adminEmail": "user@example.com",
    "isActive": true,
    "defaultLanguage": "en-US",
    "createdOn": "2025-12-10T11:00:00Z",
    "modifiedOn": null,
    "status": "active"
  }
]
```

### 3. Get Site by ID

Retrieves details for a specific site.

**Endpoint:** `GET /api/site/{id}`

**Path Parameters:**
- `id` (required): Site ID (integer)

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "My Portfolio",
  "description": "Personal portfolio site",
  "url": "my-portfolio",
  "themeId": 1,
  "themeName": null,
  "logoUrl": null,
  "faviconUrl": null,
  "adminEmail": "user@example.com",
  "isActive": true,
  "defaultLanguage": "en-US",
  "createdOn": "2025-12-10T10:00:00Z",
  "modifiedOn": null,
  "status": "active"
}
```

**Error Response (404 Not Found):**
```json
{
  "message": "Site with ID 999 not found"
}
```

### 4. Update Site

Updates an existing site's information.

**Endpoint:** `PUT /api/site/{id}`

**Path Parameters:**
- `id` (required): Site ID (integer)

**Request Body:**
```json
{
  "id": 1,
  "name": "Updated Site Name",
  "description": "Updated description",
  "url": "updated-site-name",
  "themeId": 2,
  "logoUrl": "/uploads/logo.png",
  "faviconUrl": "/uploads/favicon.ico",
  "defaultLanguage": "en-US"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Updated Site Name",
  "description": "Updated description",
  "url": "updated-site-name",
  "themeId": 2,
  "themeName": null,
  "logoUrl": "/uploads/logo.png",
  "faviconUrl": "/uploads/favicon.ico",
  "adminEmail": "user@example.com",
  "isActive": true,
  "defaultLanguage": "en-US",
  "createdOn": "2025-12-10T10:00:00Z",
  "modifiedOn": "2025-12-10T15:00:00Z",
  "status": "active"
}
```

**Error Response (400 Bad Request):**
```json
{
  "message": "Site URL 'existing-url' is already in use"
}
```

### 5. Delete Site

Soft deletes a site (marks as inactive).

**Endpoint:** `DELETE /api/site/{id}`

**Path Parameters:**
- `id` (required): Site ID (integer)

**Response (204 No Content):**

Empty response body.

**Error Response (404 Not Found):**
```json
{
  "message": "Site with ID 999 not found"
}
```

### 6. Check URL Availability

Checks if a site URL is available for use.

**Endpoint:** `GET /api/site/check-url?url={url}`

**Query Parameters:**
- `url` (required): URL slug to check (string)

**Response (200 OK):**
```json
{
  "url": "my-new-site",
  "isAvailable": true
}
```

## Site Provisioning Process

When a new site is created via `POST /api/site`, the following steps occur automatically:

1. **URL Generation**: A unique URL slug is generated from the site name
2. **Site Creation**: The site record is created in the database
3. **Theme Application**: If a theme ID is provided, the theme is applied to the site
4. **Content Initialization**: A default home page is created for the site
5. **Dashboard URL**: A CMS dashboard URL is generated for immediate access

## URL Slug Generation Rules

- Converts site name to lowercase
- Replaces spaces with hyphens
- Removes invalid URL characters
- Ensures uniqueness by appending numbers if needed
- Example: "My Awesome Site" → "my-awesome-site"
- If URL exists: "my-awesome-site-1", "my-awesome-site-2", etc.

## Multi-Tenant Isolation

Each site operates in complete isolation:

- **Data Separation**: Site data is filtered by `SiteId`
- **Content Isolation**: Pages, modules, and content are site-specific
- **Theme Independence**: Each site can have its own theme
- **User Access**: Users can only access sites they own or have permissions for

## Status Values

Sites can have the following status values:

- `active`: Site is operational and accessible
- `inactive`: Site has been soft-deleted
- `staging`: Site is in development/testing (future feature)

## Error Codes

| HTTP Code | Description |
|-----------|-------------|
| 200 | Success |
| 201 | Resource created successfully |
| 204 | Success with no content |
| 400 | Bad request (validation error) |
| 404 | Resource not found |
| 500 | Internal server error |

## Rate Limiting

Currently no rate limiting is applied. Future versions may implement rate limiting based on subscription tier.

## Example Usage

### Creating a Site with cURL

```bash
curl -X POST http://localhost:5055/api/site \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Portfolio",
    "description": "Personal portfolio and blog",
    "purpose": "Showcase my work and share insights",
    "themeId": 1,
    "adminEmail": "user@example.com"
  }'
```

### Fetching User Sites

```bash
curl -X GET "http://localhost:5055/api/site?userEmail=user@example.com"
```

### Updating a Site

```bash
curl -X PUT http://localhost:5055/api/site/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Updated Portfolio",
    "description": "Updated description",
    "url": "updated-portfolio",
    "themeId": 2,
    "defaultLanguage": "en-US"
  }'
```

## Frontend Integration

The frontend uses the `CreateSiteDialog` component which integrates with these APIs:

1. **Step 1-2**: User provides site name and purpose
2. **Step 3**: User selects a theme (fetched from theme API)
3. **Step 4**: Review and submit via `POST /api/site`
4. **Step 5**: Success screen with link to CMS dashboard

## Notes

- All dates are in ISO 8601 format (UTC)
- Theme selection is optional; sites can be created without a theme
- The `purpose` field is used by AI assistants for better recommendations
- Site URLs must be unique across the entire platform
- Soft delete allows for site recovery if needed
