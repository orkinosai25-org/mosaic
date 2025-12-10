# Zoota AI Assistant - Admin User Guide

## Overview

Zoota is your AI-powered administrative assistant for OrkinosaiCMS. Available exclusively in the admin panel, Zoota helps you manage your CMS through conversational interactions and provides intelligent assistance for common administrative tasks.

**Version:** 2.0.0  
**Last Updated:** December 9, 2025

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Accessing the Admin Panel](#accessing-the-admin-panel)
3. [Using Zoota](#using-zoota)
4. [CMS API Endpoints](#cms-api-endpoints)
5. [Common Tasks](#common-tasks)
6. [Security & Access Control](#security--access-control)
7. [Troubleshooting](#troubleshooting)

---

## Getting Started

### Prerequisites

- Administrator role in OrkinosaiCMS
- Valid admin credentials
- Access to the CMS admin panel

### First-Time Setup

1. **Create Admin User** (if not already created):
   ```csharp
   // This should be done via database seeding or admin tool
   // Default credentials for demo: admin / Admin@123
   ```

2. **Configure Azure OpenAI** (optional, uses mock responses by default):
   - Open `appsettings.json`
   - Update `AzureOpenAI` section with your credentials
   - See [ZOOTA-INTEGRATION-GUIDE.md](./ZOOTA-INTEGRATION-GUIDE.md) for details

---

## Accessing the Admin Panel

### Login

1. Navigate to: `https://your-cms-url/admin/login`
2. Enter your admin credentials:
   - **Username**: admin (or your assigned username)
   - **Password**: Your secure password
3. Click "Sign In"

**Demo Credentials:**
- Username: `admin`
- Password: `Admin@123`

### Admin Dashboard

After login, you'll see the Admin Dashboard with:
- Overview cards (Pages, Content, Media, Users)
- Quick action buttons
- Zoota banner highlighting AI assistant features
- Chat button in bottom-right corner (Zoota)

---

## Using Zoota

### Activating Zoota

1. Look for the **floating chat button** in the bottom-right corner of the admin panel
2. The button shows the Zoota parrot logo with a subtle pulse animation
3. Click the button to open the chat panel

### Chat Interface

**Welcome Screen:**
- Zoota logo and greeting
- Quick suggestion buttons for common questions
- Professional Azure-themed design

**Chat Features:**
- **Message History**: All your conversations with Zoota
- **Typing Indicator**: Shows when Zoota is processing your request
- **Timestamps**: Each message shows the time sent
- **Quick Suggestions**: Pre-defined buttons for common queries

### Asking Questions

Zoota can help with:

**General CMS Questions:**
```
- "What is OrkinosaiCMS?"
- "How do I create a new page?"
- "Tell me about content management"
- "What features are available?"
```

**Navigation Help:**
```
- "Where can I manage users?"
- "How do I access media library?"
- "Show me the settings page"
```

**Documentation:**
```
- "Explain master pages"
- "What are modules?"
- "How does the permission system work?"
```

**Workflow Guidance:**
```
- "Walk me through creating a blog post"
- "How do I publish a page?"
- "What's the best way to organize content?"
```

### Language Support

Zoota supports both English and Turkish:
- Automatically detects your language
- Responds in the language you use
- Switch languages anytime by typing in your preferred language

---

## CMS API Endpoints

Zoota has access to the following admin-only API endpoints for future direct integrations:

### Pages Management

**List all pages:**
```http
GET /api/zoota/cms/pages
Authorization: Bearer {admin-token}
```

**Create a page:**
```http
POST /api/zoota/cms/pages
Content-Type: application/json

{
  "title": "About Us",
  "path": "/about",
  "metaDescription": "Learn more about our company",
  "isPublished": true
}
```

**Update a page:**
```http
PUT /api/zoota/cms/pages/{id}
Content-Type: application/json

{
  "title": "Updated Title",
  "isPublished": true
}
```

**Delete a page:**
```http
DELETE /api/zoota/cms/pages/{id}
```

### Content Management

**List all content:**
```http
GET /api/zoota/cms/content
```

**Create content:**
```http
POST /api/zoota/cms/content
Content-Type: application/json

{
  "title": "Welcome Post",
  "contentType": "Document",
  "body": "This is the content body..."
}
```

**Update content:**
```http
PUT /api/zoota/cms/content/{id}
Content-Type: application/json

{
  "title": "Updated Title",
  "body": "Updated content..."
}
```

**Delete content:**
```http
DELETE /api/zoota/cms/content/{id}
```

### User Management

**List all users:**
```http
GET /api/zoota/cms/users
```

---

## Common Tasks

### Task 1: Creating a New Page

**Via Admin Panel:**
1. Click "Create New Page" button on dashboard
2. Fill in page details
3. Click "Save"

**Asking Zoota:**
```
"How do I create a new page?"
```

Zoota will guide you through:
- Required fields
- Best practices
- SEO considerations
- Publishing workflow

### Task 2: Managing Content

**Via Admin Panel:**
1. Navigate to "Content" section
2. Click "Add Content"
3. Select content type
4. Fill in details

**Asking Zoota:**
```
"Explain content types"
"What's the difference between documents and media?"
```

### Task 3: User Management

**Via Admin Panel:**
1. Go to "Users" section
2. View list of users
3. Manage roles and permissions

**Asking Zoota:**
```
"How do roles work?"
"What permissions does the Editor role have?"
```

### Task 4: Finding Help

**Quick Tips:**
```
"Show me keyboard shortcuts"
"What are quick actions?"
"How do I navigate faster?"
```

**Documentation:**
```
"Where is the documentation?"
"Link me to the architecture guide"
"Show me deployment instructions"
```

---

## Security & Access Control

### Admin-Only Access

✅ **Zoota is ONLY available to administrators:**
- Requires valid admin authentication
- Not visible on public pages
- Session-based security
- Role verification on each request

### Data Security

- All CMS operations require admin authorization
- API endpoints are protected with `[Authorize(Roles = "Administrator")]`
- Session data is encrypted using ASP.NET Core Protected Browser Storage
- No sensitive data is logged in chat history

### Best Practices

1. **Always log out** when finished using the admin panel
2. **Use strong passwords** for admin accounts
3. **Don't share credentials** with unauthorized users
4. **Review permissions** regularly
5. **Monitor user activity** through logs

---

## Troubleshooting

### Zoota Not Appearing

**Problem:** Chat button doesn't show in admin panel

**Solutions:**
1. Verify you're logged in as an administrator
2. Check that you're in the admin panel (URL starts with `/admin`)
3. Refresh the page
4. Clear browser cache
5. Check browser console for JavaScript errors

### Zoota Not Responding

**Problem:** Messages sent but no response

**Solutions:**
1. Check Python backend is running (port 8000)
2. Verify `appsettings.json` configuration
3. Check backend logs: `/home/LogFiles/python_error.log`
4. Ensure Azure OpenAI is configured (or accept mock responses)
5. Check network connectivity

### Login Issues

**Problem:** Cannot log in to admin panel

**Solutions:**
1. Verify credentials are correct
2. Check that user account is active
3. Ensure user has "Administrator" role
4. Check database connection
5. Review authentication logs

### API Endpoint Errors

**Problem:** API calls return 401 Unauthorized

**Solutions:**
1. Verify authentication token is valid
2. Check user has Administrator role
3. Ensure session hasn't expired
4. Re-login to refresh authentication

---

## FAQ

### Q: Can I use Zoota from my mobile device?
**A:** Yes! The admin panel and Zoota are fully responsive and work on tablets and smartphones.

### Q: Does Zoota have access to my data?
**A:** Zoota can view CMS content you have access to as an admin, but all data stays within your CMS instance. If using Azure OpenAI, conversations are processed by Azure's AI service.

### Q: Can I customize Zoota's responses?
**A:** Yes! Edit the `SystemPrompt` in `appsettings.json` to customize Zoota's personality and capabilities.

### Q: Does Zoota work offline?
**A:** Zoota requires an active connection to the Python backend. If Azure OpenAI is not configured, it will use local mock responses.

### Q: Can multiple admins use Zoota at the same time?
**A:** Yes! Each admin has their own chat session and conversation history.

### Q: How do I update Zoota?
**A:** Zoota is part of OrkinosaiCMS. Update the CMS to get the latest Zoota features.

---

## Additional Resources

- **Integration Guide**: [ZOOTA-INTEGRATION-GUIDE.md](./ZOOTA-INTEGRATION-GUIDE.md)
- **Implementation Summary**: [ZOOTA-IMPLEMENTATION-SUMMARY.md](./ZOOTA-IMPLEMENTATION-SUMMARY.md)
- **Backend Deployment**: [PYTHON-BACKEND-DEPLOYMENT.md](./PYTHON-BACKEND-DEPLOYMENT.md)
- **CMS Architecture**: [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Database Guide**: [DATABASE.md](./DATABASE.md)

---

## Support

For issues or questions:
1. Check this guide first
2. Review the troubleshooting section
3. Check [GitHub Issues](https://github.com/orkinosai25-org/orkinosaiCMS/issues)
4. Contact your system administrator

---

**Last Updated:** December 9, 2025  
**Version:** 2.0.0  
**Status:** Production Ready
