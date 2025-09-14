# Visitor Registration (Azure)

This starter kit gives you:
- `frontend/index.html` — Static Web App form (Name + optional Email)
- `function/` — Azure Functions (.NET 8 isolated) with `checkin` endpoint that writes to Azure SQL
- SQL table schema and step-by-step setup in Azure

## SQL Table
Run this in your Azure SQL database (Query Editor in portal):

```sql
CREATE TABLE dbo.VisitorLog (
  Id INT IDENTITY(1,1) PRIMARY KEY,
  Name NVARCHAR(200) NOT NULL,
  Email NVARCHAR(256) NULL,
  TimestampUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

## Local run (optional)
1) Install .NET 8 SDK and Azure Functions Core Tools.
2) `cd function && dotnet restore && dotnet build`
3) Add your SQL connection string to `local.settings.json` (`SqlConnectionString`).
4) `func start`

## Deploy
- Create an Azure **Function App** (dotnet isolated), enable Application Insights.
- In Function App **Configuration**, add `SqlConnectionString` app setting.
- Deploy `function/` folder (VS Code Azure Functions extension is easiest).
- Create a **GitHub Page** and deploy the `docs/` folder (via GitHub).
- In `frontend/index.html`, set `API_BASE` to your Function App URL.
- In Function App **CORS**, add your Static Web App URL.
