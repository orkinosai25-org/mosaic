# Application Settings Configuration

## Connection String Configuration

**IMPORTANT**: Never commit actual connection strings with credentials to source control!

### Development (Local)
Connection string is configured in `appsettings.Development.json` (uses LocalDB by default).

### Production (Azure)
Connection strings should be configured via:

1. **Azure App Service Configuration** (Recommended)
   - Go to Azure Portal → Your Web App → Configuration → Connection strings
   - Add: Name: `DefaultConnection`, Type: `SQLAzure`, Value: `<your-connection-string>`

2. **Environment Variables**
   ```bash
   export ConnectionStrings__DefaultConnection="<your-connection-string>"
   ```

3. **Azure Key Vault** (Most Secure)
   - Store connection string in Key Vault
   - Reference in appsettings: `@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/ConnectionString/)`

### Connection String Format

#### LocalDB (Windows Development)
```
Server=(localdb)\\mssqllocaldb;Database=OrkinosaiCMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

#### SQL Server (Windows Authentication)
```
Server=localhost;Database=OrkinosaiCMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

#### SQL Server (SQL Authentication)
```
Server=localhost;Database=OrkinosaiCMS;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=True
```

#### Azure SQL Database
```
Server=tcp:{your-server}.database.windows.net,1433;Initial Catalog=OrkinosaiCMS;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Security Best Practices

1. ✅ Use Azure Key Vault for production secrets
2. ✅ Use Managed Identity when possible (eliminates credentials)
3. ✅ Never commit `appsettings.Production.json` with real connection strings
4. ✅ Use environment-specific configuration files that are not tracked by git
5. ✅ Rotate credentials regularly
6. ✅ Use principle of least privilege for database users

## See Also

- [Setup Guide](../../docs/SETUP.md) - Complete setup instructions
- [Azure Deployment Guide](../../docs/AZURE_DEPLOYMENT.md) - Production deployment
- [Database Guide](../../docs/DATABASE.md) - Database architecture
