using Microsoft.EntityFrameworkCore.Migrations;

public partial class ConvertWorkflowFunctionToTask : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO ""WorkflowTasks"" (
                ""Id"", ""Name"", ""Description"", ""Type"", ""Url"", ""HttpVerb"",
                ""Config"", ""Data"", ""Status"", ""CreatedAt"", ""IsActive"",
                ""StateId"", ""WorkflowDefinitionId"", ""ResponseMapping"",
                ""EnrichStateData"", ""Order"", ""DeletedAt"", ""IsDeleted"",
                ""Trigger"", ""CompletedAt"", ""Result"",
                ""BindingName"", ""Operation"", ""PubSubName"", ""Topic"",
                ""AppId"", ""MethodName""
            )
            SELECT 
                ""Id"", ""Name"", ""Description"", CASE 
                    WHEN ""Type"" = 'Http' THEN 5
                    WHEN ""Type"" = 'DaprBinding' THEN 2
                    WHEN ""Type"" = 'DaprService' THEN 4
                    WHEN ""Type"" = 'DaprPubSub' THEN 3
                    ELSE 6 END,
                ""Url"", ""Method"",
                ""Headers"", ""RequestBody"", 'Pending', ""CreatedAt"", ""IsActive"",
                ""StateId"", ""WorkflowDefinitionId"", ""ResponseMapping"",
                ""EnrichStateData"", ""Order"", NULL, false,
                0, NULL, NULL,
                ""BindingName"", ""Operation"", ""PubSubName"", ""Topic"",
                ""AppId"", ""MethodName""
            FROM ""WorkflowFunctions""
        ");

        migrationBuilder.DropTable(name: "WorkflowFunctions");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This is a one-way migration as we're consolidating the models
        throw new NotImplementedException("This migration cannot be reverted");
    }
} 