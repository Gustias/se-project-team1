## Unit Tests

### Running Tests
from `../backend/tests/BookClub.Api.Tests` run `dotnet test`

### Generating Coverage reports
from `../backend/tests/BookClub.Api.Tests` run:
- `dotnet test --collect:"XPlat Code Coverage"`
- `reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:coveragereport -classfilters:"-BookClub.Api.Migrations.*;-Program" -filefilters:"-*/obj/*"`


`Program.cs`, `Migrations` and code generated in `obj/` are excluded from the total coverage %.

