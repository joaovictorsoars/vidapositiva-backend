using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250520222400)]
public class AddUserTable : Migration {
    private const string TableName = "users";
    
    public override void Up()
    {
        Create
            .Table(TableName)
            .WithColumn("user_id").AsInt32().Identity().PrimaryKey("pk_users")
            .WithColumn("email").AsString(255).Unique("uk_users_email").NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("google_id").AsString(255).Nullable()
            .WithColumn("public_id").AsString(255).NotNullable().WithDefaultValue("(gen_random_uuid())")
            .WithColumn("picture_url").AsString(255).Nullable()
            .WithColumn("last_login").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("created_at").AsCustom("timestamp with time zone").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("timestamp with time zone").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete
            .Table(TableName);
    }
}