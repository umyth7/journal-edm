using EDM.Application.Models;

namespace EDM.Application.Interfaces.Infrastructure;

public interface IFilenameParser
{
    FilenameParseResult Parse(string rawFileName);
}
