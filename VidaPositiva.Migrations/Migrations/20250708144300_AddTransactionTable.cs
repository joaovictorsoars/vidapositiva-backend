using FluentMigrator;

namespace Migrations.Migrations;

[Migration(20250708144300)]
public class AddTransactionTable : Migration {
    private const string TableName = "transactions";
    
    public override void Up()
    {
        Create
            .Table(TableName)
            .WithColumn("transaction_id").AsInt32().PrimaryKey("pk_transactions").Identity()
            .WithColumn("public_id").AsString(100).NotNullable()
            .WithColumn("type").AsString(50).NotNullable()
            .WithColumn("title").AsString(150).NotNullable()
            .WithColumn("description").AsString(500).Nullable()
            .WithColumn("accrual_date").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("cash_date").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("amount").AsDecimal(13, 2).NotNullable()
            .WithColumn("installments").AsInt32().NotNullable()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("pote_id").AsInt32().NotNullable()
            .WithColumn("category_id").AsInt32().NotNullable()
            .WithColumn("subcategory_id").AsInt32().NotNullable();

        Create
            .ForeignKey("fk_transactions_users")
            .FromTable(TableName)
            .ForeignColumn("user_id")
            .ToTable("users")
            .PrimaryColumn("user_id");

        Create
            .ForeignKey("fk_transactions_potes")
            .FromTable(TableName)
            .ForeignColumn("pote_id")
            .ToTable("potes")
            .PrimaryColumn("pote_id");
        
        Create
            .ForeignKey("fk_transactions_categories")
            .FromTable(TableName)
            .ForeignColumn("category_id")
            .ToTable("categories")
            .PrimaryColumn("category_id");
        
        Create
            .ForeignKey("fk_transactions_subcategories")
            .FromTable(TableName)
            .ForeignColumn("subcategory_id")
            .ToTable("categories")
            .PrimaryColumn("category_id");
    }

    public override void Down()
    {
        Delete.Table(TableName);
    }
}