ensure you have a pg up with 5432 and a db named test then you can run the next scripts

```shell
dotnet tool restore
dotnet ef database update -p ./DapperBug/DapperBug.csproj
```
