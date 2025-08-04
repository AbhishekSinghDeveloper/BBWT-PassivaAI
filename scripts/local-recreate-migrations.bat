DEL /F /Q /S "..\project\BBWT.Data.MySQL\Migrations\*.*"

dotnet ef migrations add Initial -p ../project/bbwt.data.mysql -s ../project/bbwt.server -c DataContext

DEL /F /Q /S "..\project\BBWT.Data.SqlServer\Migrations\*.*"

dotnet ef migrations add Initial -p ../project/BBWT.Data.SqlServer -s ../project/bbwt.server -c DataContext
