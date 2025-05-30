using FluentMigrator.Runner.VersionTableInfo;

namespace VidaPositiva.Api.Configuration.FluentMigrator;

[VersionTableMetaData]
public class VersionTableMetaDataConfiguration : DefaultVersionTableMetaData
{
    public override string TableName => "version_info";

    public override string ColumnName => "version";

    public override string UniqueIndexName => "uk_version";

    public override string AppliedOnColumnName => "applied_on";

    public override string DescriptionColumnName => "description";
}