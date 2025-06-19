using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250616230700)]
public class AddCategoryTable : Migration {
    private const string TableName = "categories";
    
    public override void Up()
    {
        Create
            .Table(TableName)
            .WithColumn("category_id").AsInt32().NotNullable().PrimaryKey("pk_categories").Identity()
            .WithColumn("name").AsString(150).NotNullable()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("picture_url").AsString(255).Nullable()
            .WithColumn("parent_id").AsInt32().Nullable()
            .WithColumn("pote_id").AsInt32().Nullable()
            .WithColumn("user_id").AsInt32().Nullable();
        
        Create
            .ForeignKey("fk_categories_potes")
            .FromTable(TableName)
            .ForeignColumn("pote_id")
            .ToTable("potes")
            .PrimaryColumn("pote_id");
        
        Create
            .ForeignKey("fk_categories_users")
            .FromTable(TableName)
            .ForeignColumn("user_id")
            .ToTable("users")
            .PrimaryColumn("user_id");
    }

    public override void Down()
    {
        Delete.Table(TableName);
    }
}