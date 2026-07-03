using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixWaiverPersonFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Waivers_Persons_PersonId')
                BEGIN
                    ALTER TABLE [Waivers] DROP CONSTRAINT [FK_Waivers_Persons_PersonId];
                END
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Waivers') AND name = 'PersonId')
                BEGIN
                    ALTER TABLE [Waivers] ADD CONSTRAINT [FK_Waivers_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]) ON DELETE NO ACTION;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Waivers_Persons_PersonId1')
                BEGIN
                    ALTER TABLE [Waivers] DROP CONSTRAINT [FK_Waivers_Persons_PersonId1];
                END
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Waivers') AND name = 'PersonId1')
                BEGIN
                    ALTER TABLE [Waivers] ADD CONSTRAINT [FK_Waivers_Persons_PersonId1] FOREIGN KEY ([PersonId1]) REFERENCES [Persons] ([Id]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Waivers_Persons_PersonId')
                BEGIN
                    ALTER TABLE [Waivers] DROP CONSTRAINT [FK_Waivers_Persons_PersonId];
                END
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Waivers') AND name = 'PersonId')
                BEGIN
                    ALTER TABLE [Waivers] ADD CONSTRAINT [FK_Waivers_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]) ON DELETE CASCADE;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Waivers_Persons_PersonId1')
                BEGIN
                    ALTER TABLE [Waivers] DROP CONSTRAINT [FK_Waivers_Persons_PersonId1];
                END
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Waivers') AND name = 'PersonId1')
                BEGIN
                    ALTER TABLE [Waivers] ADD CONSTRAINT [FK_Waivers_Persons_PersonId1] FOREIGN KEY ([PersonId1]) REFERENCES [Persons] ([Id]) ON DELETE CASCADE;
                END
            ");
        }
    }
}
