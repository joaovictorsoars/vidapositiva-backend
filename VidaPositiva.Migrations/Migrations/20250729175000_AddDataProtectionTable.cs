using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250729175000)]
public class AddDataProtectionTable : Migration {
    public override void Up()
    {
        Create.Table("DataProtectionKeys")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("FriendlyName").AsString(int.MaxValue).Nullable()
            .WithColumn("Xml").AsString(int.MaxValue).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("DataProtectionKeys");
    }
}