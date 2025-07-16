using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250710223000)]
public class AddIsFavoriteColumnToCategoryTable : Migration
{
    private const string TableName = "categories";
    private const string ColumnName = "is_favorite";
    
    public override void Up()
    {
        Alter
            .Table(TableName)
            .AddColumn(ColumnName).AsBoolean().NotNullable().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete
            .Column(ColumnName)
            .FromTable(TableName);
    }
}