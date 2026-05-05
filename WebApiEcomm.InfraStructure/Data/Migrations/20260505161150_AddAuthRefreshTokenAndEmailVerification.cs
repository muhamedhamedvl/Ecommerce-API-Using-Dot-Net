using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiEcomm.InfraStructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthRefreshTokenAndEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op migration.
            // These tables already exist in environments where previous migrations were applied or the schema was created manually.
            // This migration exists to align the EF model snapshot with the database without re-creating tables.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op
        }
    }
}
