using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250625005827)]
public class RemovePictureUrlFromCategoryTable : Migration {
    private const string TableName = "categories";
    
    public override void Up()
    {
        Delete
            .Column("picture_url")
            .FromTable(TableName);
    }

    public override void Down()
    {
        Alter
            .Table(TableName)
            .AddColumn("picture_url").AsString(255).Nullable();
    }
}