using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250616230650)]
public class AddPoteTable : Migration {
    private const string TableName = "potes";
    
    public override void Up()
    {
        Create
            .Table(TableName)
            .WithColumn("pote_id").AsInt32().PrimaryKey("pk_potes").Identity()
            .WithColumn("name").AsString(150).NotNullable()
            .WithColumn("picture_url").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Table(TableName);
    }
}